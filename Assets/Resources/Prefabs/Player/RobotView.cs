using System;
// using Script.Map;
// using Script.Map.Room;
// using Script.Panel.Hintbox;
using UnityEngine;

    public class RobotView : MonoBehaviour
    {
        [SerializeField] private Animator animationController;
        [SerializeField] private Rigidbody2D rg2d;
        [SerializeField] private const float Speed = 3.0f;
        [SerializeField] private GameObject playerAnim;

        public Vector3 Position
        {
            get => this.transform.position;
            set => this.transform.position = value;

        }

        private void Update()
        {
            bool[] direction = new bool[4];
            direction[0] = Input.GetKey(KeyCode.W); // 上
            direction[1] = Input.GetKey(KeyCode.S); // 下
            direction[2] = Input.GetKey(KeyCode.A); // 左
            direction[3] = Input.GetKey(KeyCode.D); // 右
            this.rg2d.velocity = new Vector2(direction[3] ? Speed : direction[2] ? -Speed : 0, direction[0] ? Speed : direction[1] ? -Speed : 0);
            SetDirection(direction);
        }

        public void SetDirection(bool[] direction)
        {
            bool isMove = direction[0] || direction[1] || direction[2] || direction[3];//上下左右
            if (!isMove)
            {
                animationController.Play("Idle");
                return;
            }
            if (direction[0])//上
            {
                if (direction[2] || direction[3])
                {
                    this.playerAnim.transform.localScale = new Vector3(direction[2] ? 1 : -1, 1, 1);
                    animationController.Play("SidelongBack");
                }else
                {
                    animationController.Play("Backwards");
                }
            }else if (direction[1])//下
            {
                if (direction[2] || direction[3])
                {
                    this.playerAnim.transform.localScale = new Vector3(direction[2] ? 1 : -1, 1, 1);
                    animationController.Play("Sidelong");
                }
                else
                {
                    animationController.Play("Front");
                }
            }
            else
            {
                this.playerAnim.transform.localScale = new Vector3(direction[2] ? 1 : -1, 1, 1);
                animationController.Play("LeftRight");
            }
            
        }
    }
