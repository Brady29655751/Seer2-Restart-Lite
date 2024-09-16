using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RM = ResourceManager;

namespace UnityEngine.UI
{
    public static class TransformHelper
    {
        public static void DestoryChildren(this Transform transform)
        {
            foreach (Transform t in transform)
            {
                GameObject.Destroy(t.gameObject);
            }
        }

        public static void Flip(Transform objTransform)
        {
            float screenWidth = Screen.width;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(objTransform.position);
            // 计算相对于屏幕中轴线的水平距离
            float distanceFromCenter = screenPosition.x - screenWidth / 2;
            // 计算对称位置
            float mirroredPositionX = screenWidth / 2 - distanceFromCenter;
            // 将屏幕坐标转换回世界坐标
            Vector3 mirroredWorldPosition =
                Camera.main.ScreenToWorldPoint(new Vector3(mirroredPositionX, screenPosition.y, screenPosition.z));
            // 设置游戏对象的新位置
            objTransform.position = mirroredWorldPosition;
            // 左右翻转游戏对象
            Vector3 localScale = objTransform.localScale;
            localScale.x *= -1;
            objTransform.localScale = localScale;
        }

        public static void DisableAllChild(Transform transform)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public static class SpriteSize
    {
        public static Vector2 GetTextureSize(this Texture2D texture)
        {
            return new Vector2(texture.width, texture.height);
        }

        public static Vector2 GetSpriteSize(this Sprite sprite)
        {
            return sprite.texture.GetTextureSize();
        }

        public static float GetResizedWidth(this Texture2D texture, float newHeight)
        {
            return texture.width * newHeight / texture.height;
        }

        public static float GetResizedWidth(this Sprite sprite, float newHeight)
        {
            return GetResizedWidth(sprite.texture, newHeight);
        }

        public static float GetResizedHeight(this Texture2D texture, float newWidth)
        {
            return texture.height * newWidth / texture.width;
        }

        public static float GetResizedHeight(this Sprite sprite, float newWidth)
        {
            return GetResizedWidth(sprite.texture, newWidth);
        }

        public static KeyValuePair<RectTransform.Axis, float> GetResizedSize(this Texture2D texture, Vector2 size,
            bool shrink = true)
        {
            if (size == Vector2.zero)
                return new KeyValuePair<RectTransform.Axis, float>(RectTransform.Axis.Horizontal, 0);
            if (size.x == 0)
            {
                return new KeyValuePair<RectTransform.Axis, float>(RectTransform.Axis.Vertical,
                    texture.GetResizedWidth(size.y));
            }

            if (size.y == 0)
            {
                return new KeyValuePair<RectTransform.Axis, float>(RectTransform.Axis.Horizontal,
                    texture.GetResizedWidth(size.x));
            }

            float width = texture.GetResizedWidth(size.y);
            float height = texture.GetResizedHeight(size.x);
            var resizeX = new KeyValuePair<RectTransform.Axis, float>(RectTransform.Axis.Horizontal, width);
            var resizeY = new KeyValuePair<RectTransform.Axis, float>(RectTransform.Axis.Horizontal, height);
            if (shrink)
            {
                return (width <= size.x) ? resizeX : resizeY;
            }

            return (width >= size.x) ? resizeX : resizeY;
        }

        public static KeyValuePair<RectTransform.Axis, float> GetResizedSize(this Sprite sprite, Vector2 size,
            bool shrink = true)
        {
            return GetResizedSize(sprite.texture, size, shrink);
        }

        public static void ResetAllTriggers(this Animator anim)
        {
            foreach (var param in anim.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger)
                {
                    anim.ResetTrigger(param.name);
                }
            }
        }
    }

    public static class SpriteSet
    {
        public static Sprite Empty => RM.instance.Get<Sprite>("Sprites/Empty");
        public static Sprite GetDefaultIconSprite(bool bold = false) =>
            RM.instance.Get<Sprite>("Panels/PetBag/Sprites/" + (bold ? 87 : 85));

        public static bool TryCreateSpriteFromBytes(byte[] bytes, out Sprite sprite)
        {
            sprite = null;
            Texture2D texture2D = new Texture2D(2, 2);

            if (!texture2D.LoadImage(bytes))
                return false;

            sprite = texture2D.ToSprite();
            return true;
        }

        public static Sprite ToSprite(this Texture2D texture2D) {
            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0, 0),
                100f, 0, SpriteMeshType.Tight);
        }

        public static void SetSprite(this Image image, Sprite sprite)
        {
            if (image == null)
                return;

            image.sprite = sprite;
        }

        /// <summary>
        /// Get icon according to the special pattern. <br/>
        /// Eg. pet[60] returns the icon of the pet with id 60. <br/>
        /// Eg. item[10001] returns the icon of the item with id 10001. <br/>
        /// Eg. buff[10] returns the icon of the buff with id 10.
        /// </summary>
        /// <param name="param">The pattern. Basically, type[id]</param>
        /// <returns>The icon sprite.</returns>
        public static Sprite GetIconSprite(string param) 
        {
            if (string.IsNullOrEmpty(param))
                return null;

            Sprite icon = null;
            var splitIndex = param.IndexOf('[');
            if (splitIndex != -1) {
                var category = param.Substring(0, splitIndex).ToLower();
                if (int.TryParse(param.TrimParentheses(), out int iconId)) {
                    icon = category switch {
                        "pet" =>    Pet.GetPetInfo(iconId)?.ui.icon,
                        "emblem" => Pet.GetPetInfo(iconId)?.ui.emblemIcon,
                        "item" =>   Item.GetItemInfo(iconId)?.icon,
                        "buff" =>   Buff.GetBuffInfo(iconId)?.icon,
                        "npc"  =>   NpcInfo.GetIcon(iconId.ToString()),
                        _ =>   NpcInfo.GetIcon(param),
                    };
                }
            }
            icon ??= NpcInfo.GetIcon(param);
            return icon;
        }

        public static void SetElementSprite(this Image image, Element element)
        {
            Sprite sprite = element.GetSprite();
            float width = sprite.GetResizedWidth(image.rectTransform.rect.size.y);
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            image.sprite = sprite;
        }

        public static void SetGenderSprite(this Image image, int gender)
        {
            if (gender < 0)
                return;

            image.sprite = ResourceManager.instance.GetSprite("Genders/" + gender.ToString());
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (gender == 1) ? 24 : 35);
        }

        public static void SetSkillBackgroundSprite(this Image image, bool isChosen, bool isSuper,
            SecretType secretType)
        {
            int index = 0;

            if (secretType != SecretType.Others)
                index = (isSuper ? (isChosen ? 3 : 2) : (isChosen ? 1 : 0));
            else
                index = isChosen ? 3 : 8;

            image.sprite = ResourceManager.instance.GetSprite("Skills/" + index);
        }

        public static void SetDamageBackgroundSprite(this Image image, float elementRelation, bool isHit, bool isAbsorb)
        {
            if (!isHit)
            {
                image.sprite = GetDamageBackgroundSprite("miss");
                image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 128);
                image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 43);
                return;
            }

            if (isAbsorb)
            {
                image.sprite = GetDamageBackgroundSprite("absorb");
                image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 116);
                image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 60);
                return;
            }

            string type = (elementRelation > 1) ? "weak" : ((elementRelation < 1) ? "resist" : "normal");
            image.sprite = GetDamageBackgroundSprite(type);
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 225);
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 158);
        }

        private static Sprite GetDamageBackgroundSprite(string type)
        {
            return ResourceManager.instance.GetSprite("Fight/damage " + type);
        }
    }

    public static class TextHelper
    {
        public static void SetText(this Text text, string content)
        {
            if (text == null)
                return;

            text.text = content;
        }
    }
}

namespace UnityEngine
{
    public static class ColorHelper
    {
        // public static Color32 chosen = new Color32(3, 109, 159, 255);
        public static Color32 gold => new Color32(255, 187, 0, 255);
        public static Color32 red => Color.red;
        public static Color32 green => new Color32(119, 226, 12, 255);
        public static Color32 secretSkill => new Color32(252, 237, 105, 255);
        public static Color32 normalSkill => new Color32(82, 229, 249, 255);
    }

    public static class EventHelper
    {
        public static void SetListener(this Events.UnityEvent unityEvent, Events.UnityAction unityCall)
        {
            if (unityCall == null)
                return;

            unityEvent.RemoveAllListeners();
            unityEvent.AddListener(unityCall);
        }
    }
}