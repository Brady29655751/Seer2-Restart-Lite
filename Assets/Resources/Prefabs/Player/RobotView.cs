using System;
// using Script.Map;
// using Script.Map.Room;
// using Script.Panel.Hintbox;
using UnityEngine;
using UnityEngine.UI;

    public class RobotView : PlayerView
    {
        // [SerializeField] protected const float Speed = 1.5f;
        [SerializeField] protected Animator animationController;
        protected Image playerImage => playerButton.image;
        // [SerializeField] protected new RectTransform playerRect;

        public Vector3 Position
        {
            get => this.transform.position;
            set => this.transform.position = value;

        }

        /*
        private void Update()
        {
            bool[] direction = new bool[4];
            direction[0] = Input.GetKey(KeyCode.UpArrow); // 上
            direction[1] = Input.GetKey(KeyCode.DownArrow); // 下
            direction[2] = Input.GetKey(KeyCode.LeftArrow); // 左
            direction[3] = Input.GetKey(KeyCode.RightArrow); // 右
            this.Position = new Vector2(this.Position.x + (direction[3] ? Speed : direction[2] ? -Speed : 0), this.Position.y + (direction[0] ? Speed : direction[1] ? -Speed : 0));
            SetDirection(direction);
        }
        */

        public override void SetDirection(Vector2 dir)
        {
            bool[] direction = new bool[] { dir[1] > 0, dir[1] < 0, dir[0] < 0, dir[0] > 0 };
            bool isMove = direction[0] || direction[1] || direction[2] || direction[3];//上下左右

            animationController.enabled = isMove;

            if (!isMove)
            {
                // animationController.Play("Idle");
                Sprite directionSprite = GetPlayerDirectionSprite(lastDirection);
                if (directionSprite != null) {
                    playerImage?.SetSprite(directionSprite); 
                    playerRect.localScale = new Vector3((lastDirection.x < 0) ? 1 : -1, 1, 1);
                }
                
                playerImage.SetNativeSize();
                playerNameText.rectTransform.localScale = this.playerRect.localScale;

                lastDirection = dir;
                return;
            }
            if (direction[0])//上
            {
                if (direction[2] || direction[3])
                {
                    this.playerRect.localScale = new Vector3(direction[2] ? 1 : -1, 1, 1);
                    animationController.Play("SidelongBack");
                }else
                {
                    animationController.Play("Backwards");
                }
            }else if (direction[1])//下
            {
                if (direction[2] || direction[3])
                {
                    this.playerRect.localScale = new Vector3(direction[2] ? 1 : -1, 1, 1);
                    animationController.Play("Sidelong");
                }
                else
                {
                    animationController.Play("Front");
                }
            }
            else
            {
                this.playerRect.localScale = new Vector3(direction[2] ? 1 : -1, 1, 1);
                animationController.Play("LeftRight");
            }
            playerNameText.rectTransform.localScale = this.playerRect.localScale;
            achievementText.rectTransform.localScale = this.playerRect.localScale;
            playerImage.SetNativeSize();

            lastDirection = dir;
        }
    }
