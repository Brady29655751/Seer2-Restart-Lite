using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading.Tasks;
using System.Linq;

public class NpcView : Module
{
    private ResourceManager RM => ResourceManager.instance;
    [SerializeField] private RectTransform rect;
    [SerializeField] private Image image;
    [SerializeField] private IButton button;
    [SerializeField] private Text nameText;
    [SerializeField] private AnimCamera animCamera;
    [SerializeField] private UniGifImage gif;

    private GameObject currentAnim;
    private Vector3 visualBaseScale = Vector3.one;
    private Vector2 visualBasePosition;
    private Vector3 animationBaseLocalPosition;
    private Vector3 animationFlipPositionOffset;
    private bool hasAnimationFlipPositionOffset;
    private bool currentVisualFlipped;

    public object GetIdentifier(string id)
    {
        return id switch
        {
            "color" => button?.image.color ?? Color.clear,
            _ => null,
        };
    }

    public void SetNpcInfo(NpcInfo info)
    {
        SetRaycastTarget(info.raycastTarget);
        SetRect(info.pos, info.size, info.rotation);
        SetName(info.name);
        SetNamePos(info.namePos);
        SetNameSize(info.nameSize);
        SetNameColor(info.nameColor);
        SetNameFont(info.nameFont);
        SetIcon(info.resId);
        SetColor(info.color);
        SetAnim(info.anim, info.animInfo);
        SetGif(info.gifInfo);
    }

    public void SetRaycastTarget(bool isRaycastTarget)
    {
        if (button?.image == null)
            return;

        button.image.raycastTarget = isRaycastTarget;
    }

    public void SetPosition(Vector2 pos) => rect.anchoredPosition = pos;
    public void SetRotation(Vector3 rotation) => button.rect.rotation = Quaternion.Euler(rotation);
    public void SetSize(Vector2 size) => rect.SetSize(size);

    public void SetRect(Vector2 pos, Vector2 size, Quaternion rotation)
    {
        rect.SetAsLastSibling();
        SetPosition(pos);
        SetSize(size);
        button.rect.rotation = rotation;
    }

    public void SetColor(Color color)
    {
        if (button?.image == null)
            return;

        button.image.color = color;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            nameText.text = name;
            return;
        }

        if (name.TryTrimStart("[expr]", out var expr))
            name = Parser.ParseOperation(expr).ToString();

        nameText.text = name;
    }

    public void SetNamePos(Vector2 namePos)
    {
        nameText.rectTransform.anchoredPosition = namePos;
    }

    public void SetNameSize(int nameSize)
    {
        if (nameSize <= 0)
            return;

        nameText.fontSize = nameSize;
        nameText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(30, nameSize + 10));
    }

    public void SetNameColor(Color nameColor)
    {
        nameText.color = nameColor;
    }

    public void SetNameFont(string fontOption)
    {
        if (string.IsNullOrEmpty(fontOption))
            return;

        nameText.font = ResourceManager.instance.GetFont(fontOption);
    }

    public void SetIcon(string resId)
    {
        SetSprite(NpcInfo.GetIcon(resId));
    }

    public void SetSprite(Sprite sprite)
    {
        gif.Clear();
        button.SetSprite(sprite);
        if (currentAnim == null)
            CaptureVisualBase();
    }

    public void SetAnim(string animId)
    {
        var anim = NpcInfo.GetAnim(animId);
        SetAnim(anim);
    }

    public void SetAnim(GameObject prefab, AnimInfo animInfo = null)
    {
        currentAnim = null;
        foreach (Transform t in animCamera.transform)
        {
            if (t.name == "Pet Anim")
                continue;

            Destroy(t);
        }

        if (prefab == null)
        {
            CaptureVisualBase();
            return;
        }

        SetColor(Color.clear);

        currentAnim = Instantiate(prefab, animCamera.transform);
        if (animInfo == null)
        {
            CaptureVisualBase();
            return;
        }

        currentAnim.transform.localScale = animInfo.AnimScale / (animCamera.canvas?.scaleFactor ?? 1);
        currentAnim.transform.localPosition = animInfo.AnimPos / (animCamera.canvas?.scaleFactor ?? 1);
        currentAnim.transform.localRotation = animInfo.AnimRot;
        CaptureVisualBase();
    }

    public void SetVisualFacing(float directionX, bool originalFacesRight)
    {
        if (Mathf.Abs(directionX) <= 0.05f)
            return;

        bool faceRight = directionX > 0f;
        bool shouldFlip = faceRight != originalFacesRight;
        if (currentVisualFlipped == shouldFlip)
            return;

        if (currentAnim != null)
        {
            SetAnimationFacing(shouldFlip);
            currentVisualFlipped = shouldFlip;
            return;
        }

        Transform facingTarget = button?.rect;
        if (facingTarget == null)
            return;

        Vector3 scale = visualBaseScale;
        if (shouldFlip)
            scale.x *= -1f;
        facingTarget.localScale = scale;
        currentVisualFlipped = shouldFlip;
    }

    private void SetAnimationFacing(bool shouldFlip)
    {
        if (currentAnim == null)
            return;

        Transform animTransform = currentAnim.transform;
        animTransform.localPosition = animationBaseLocalPosition;
        animTransform.localScale = visualBaseScale;

        if (!shouldFlip)
            return;

        if (!hasAnimationFlipPositionOffset)
        {
            bool hasCenterBefore = TryGetAnimationVisualCenter(out Vector3 centerBefore);
            Vector3 flippedScale = visualBaseScale;
            flippedScale.x *= -1f;
            animTransform.localScale = flippedScale;

            if (hasCenterBefore && TryGetAnimationVisualCenter(out Vector3 centerAfter))
            {
                Transform parent = animTransform.parent;
                animationFlipPositionOffset = parent.InverseTransformVector(centerBefore - centerAfter);
            }

            hasAnimationFlipPositionOffset = true;
        }
        else
        {
            Vector3 flippedScale = visualBaseScale;
            flippedScale.x *= -1f;
            animTransform.localScale = flippedScale;
        }

        animTransform.localPosition = animationBaseLocalPosition + animationFlipPositionOffset;
    }

    private bool TryGetAnimationVisualCenter(out Vector3 center)
    {
        center = Vector3.zero;
        if (currentAnim == null)
            return false;

        Renderer[] renderers = currentAnim.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return false;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        center = bounds.center;
        return true;
    }

    public void SetVisualOffset(Vector2 offset)
    {
        RectTransform offsetTarget = GetVisualOffsetTarget();
        if (offsetTarget != null)
            offsetTarget.anchoredPosition = visualBasePosition + offset;
    }

    private void CaptureVisualBase()
    {
        Transform facingTarget = currentAnim != null ? currentAnim.transform : button?.rect;
        if (facingTarget != null)
            visualBaseScale = facingTarget.localScale;

        if (currentAnim != null)
            animationBaseLocalPosition = currentAnim.transform.localPosition;

        animationFlipPositionOffset = Vector3.zero;
        hasAnimationFlipPositionOffset = false;

        RectTransform offsetTarget = GetVisualOffsetTarget();
        if (offsetTarget != null)
            visualBasePosition = offsetTarget.anchoredPosition;

        currentVisualFlipped = false;
    }

    private RectTransform GetVisualOffsetTarget()
    {
        return currentAnim != null ? animCamera?.displayRect : button?.rect;
    }

    public void SetGif(AnimInfo gifInfo = null)
    {
        if (gifInfo == null)
            return;

        switch (gifInfo.id)
        {
            default:
                break;

            case "stop":
                gif.Stop();
                return;
            
            case "reset":
                gif.Reset();
                return;
        }

        if (gifInfo.GifPath == null)
            return;

        gif.SetGifFromUrl(gifInfo.GifPath, speed: gifInfo.AnimSpeed, useGifSize: gifInfo.UseAnimSize);
        rect.localScale = gifInfo.AnimScale;
    }

    public void SetBGM(AudioClip bgm)
    {
        button.SetBGM(bgm);
    }

    public void SetAction(NpcController npc, Dictionary<int, NpcController> npcList, InfoPrompt infoPrompt)
    {
        NpcInfo info = npc.GetInfo();
        if (info.description != null)
        {
            button.onPointerEnterEvent.AddListener(() => infoPrompt.SetActive(true));
            button.onPointerExitEvent.AddListener(() => infoPrompt.SetActive(false));
            button.onPointerOverEvent.AddListener(() => infoPrompt.SetInfoPromptWithAutoSize(info.description, TextAnchor.MiddleLeft));
        }

        Action onArrive = (() => { });
        foreach (var handler in info.eventHandler)
        {
            UnityEvent pointerEvent = NpcHandler.GetButtonEvent(button, handler);
            Action handlerAction = NpcHandler.GetNpcEntity(npc, handler, npcList);
            if (handler.type != ButtonEventType.OnPointerClick)
            {
                pointerEvent?.AddListener(handlerAction.Invoke);
                continue;
            }
            Action onOldArrive = onArrive;
            onArrive = () => { onOldArrive?.Invoke(); handlerAction?.Invoke(); };
        }
        Action onClick = (info.transport == null) ? onArrive : (() => TeleportHandler.Transport(info.transportPos, onArrive));
        button.onPointerClickEvent.AddListener(onClick.Invoke);
    }

    public void SetFarmAction(NpcController npc, Dictionary<int, NpcController> npcList, InfoPrompt infoPrompt)
    {
        var info = npc.GetInfo();
        var activity = Activity.Find("farm");

        button.onPointerEnterEvent.AddListener(() => infoPrompt.SetActive(true));
        button.onPointerExitEvent.AddListener(() => infoPrompt.SetActive(false));
        button.onPointerOverEvent.AddListener(() => infoPrompt.SetPlant(Plant.LoadData(info.id)));
        button.onPointerClickEvent.AddListener(() => Plant.OnClick(info.id, npc, npcList));
    }

    public void SetAnimalAction(NpcController npc, Dictionary<int, NpcController> npcList, InfoPrompt infoPrompt)
    {
        var info = npc.GetInfo();
        button.onPointerEnterEvent.AddListener(() => infoPrompt.SetActive(true));
        button.onPointerExitEvent.AddListener(() => infoPrompt.SetActive(false));
        button.onPointerOverEvent.AddListener(() => infoPrompt.SetAnimal(Animal.LoadData(info.id)));
        button.onPointerClickEvent.AddListener(() => Animal.OnClick(info.id));
    }
}
