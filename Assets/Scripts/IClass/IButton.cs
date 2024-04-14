using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button), typeof(Image))]
public class IButton : IMonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
    IPointerDownHandler, IPointerUpHandler
{   
    protected Vector3 initPos;
    public RectTransform rect { get; protected set; } = null;
    public Image image {get; protected set;} = null;
    public Button button {get; protected set;} = null;
    private Sprite initSprite;

    [SerializeField] protected bool playSoundWhenHover = false;
    [SerializeField] protected bool playSoundWhenClick = true;
    [SerializeField] protected AudioClip sound = null;
    [SerializeField] public UnityEvent onPointerClickEvent = new UnityEvent();
    [SerializeField] public UnityEvent onPointerOverEvent = new UnityEvent();
    [SerializeField] public UnityEvent onPointerEnterEvent = new UnityEvent();
    [SerializeField] public UnityEvent onPointerExitEvent = new UnityEvent();
    [SerializeField] public UnityEvent onPointerHoldEvent = new UnityEvent();


    public bool isPointerOver { get; private set; } = false;
    public bool isHold { get; private set; } = false;
    public float totalHoldSeconds { get; private set; }
    private float currentHoldSeconds = 0;
    
    public float holdThreshold { get; set; } = 1;
    public float holdSecondsDelta { get; set; } = 0;

    protected override void Awake() {
        base.Awake();
        rect = gameObject.GetComponent<RectTransform>();
        button = gameObject.GetComponent<Button>();
        image = gameObject.GetComponent<Image>();
        initSprite = image.sprite;
        initPos = rect.anchoredPosition;
    }

    protected virtual void Update() {
        if (isPointerOver) {
            OnPointerOver();
        }
        if (isHold) {
            OnPointerHold();
        }
    }

    protected void OnDestroy() {
        onPointerClickEvent.RemoveAllListeners();
        onPointerEnterEvent.RemoveAllListeners();
        onPointerExitEvent.RemoveAllListeners();
        onPointerHoldEvent.RemoveAllListeners();
        onPointerOverEvent.RemoveAllListeners();
    }

    /* Pointer In, Over, and Out */
    public virtual void OnPointerEnter(PointerEventData eventData) {
        if (!button.interactable)
            return;

        isPointerOver = true;
        if (playSoundWhenHover)
            AudioSystem.instance.PlaySound(sound);

        if (button.transition == Selectable.Transition.SpriteSwap) {
            if (button.interactable)
                button.image.sprite = button.spriteState.highlightedSprite;
        }
        onPointerEnterEvent?.Invoke();
    }

    public virtual void OnPointerOver() {
        if (!button.interactable)
            return;

        onPointerOverEvent?.Invoke();
    }

    public virtual void OnPointerExit(PointerEventData eventData) {
        isPointerOver = false;
        if (button.transition == Selectable.Transition.SpriteSwap) {
            button.image.sprite = initSprite;
        }
        onPointerExitEvent?.Invoke();
    }

    /* Pointer Hold */
    public virtual void OnPointerDown(PointerEventData eventData) {
        if (!button.interactable)
            return;

        isHold = true;
        totalHoldSeconds = 0;
        currentHoldSeconds = 0;
    }

    public virtual void OnPointerHold() {
        if (!button.interactable)
            return;

        totalHoldSeconds += Time.deltaTime;
        currentHoldSeconds += Time.deltaTime;
        if (totalHoldSeconds >= holdThreshold) {
            if (currentHoldSeconds >= holdSecondsDelta) {
                currentHoldSeconds = 0;
                onPointerHoldEvent?.Invoke();
            }   
        }
    } 

    public virtual void OnPointerUp(PointerEventData eventData) {
        if (!button.interactable)
            return;

        if ((button.interactable) && (totalHoldSeconds < holdThreshold)) {
            OnPointerClick();
        }
        isHold = false;
        totalHoldSeconds = 0;
        currentHoldSeconds = 0;
    }

    /* Pointer Click */
    public virtual void OnPointerClick() {
        if (!button.interactable)
            return;
            
        if (playSoundWhenClick)
            AudioSystem.instance.PlaySound(sound);
        
        onPointerClickEvent?.Invoke();
    }

    /* Interactable */
    protected virtual void OnInteractable(bool interactable) {
        var a = image.color.a;
        image.color = interactable ? new Color(1, 1, 1, a) : new Color(0.5f ,0.5f, 0.5f, a);
    }

    public void SetInteractable(bool interactable, bool grayWhenDisabled = true) {
        if (button == null)
            return;
            
        button.interactable = interactable;
        OnInteractable(interactable);
        
        if (!grayWhenDisabled)
            image.color = new Color(1, 1, 1, image.color.a);
    }
    
    public virtual void SetPosition(Vector2 pos) {
        rect.anchoredPosition = pos;
    }

    public virtual void SetPosition(int pos, RectTransform.Axis axis) {
        if (axis == RectTransform.Axis.Horizontal) {
            rect.anchoredPosition = new Vector2(pos, rect.anchoredPosition.y);
        } else if (axis == RectTransform.Axis.Vertical) {
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, pos);
        }
    }

    public virtual void SetSize(Vector2 size) {
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    public virtual void SetSprite(Sprite sprite) {
        if (image != null) {
            image.sprite = sprite;
        }
    }
}