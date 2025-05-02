using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MiniGame 
{
    public class PlayerController : Module
    {
        [SerializeField] protected const float Speed = 3f;
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Sprite[] playerDirectionSprite = new Sprite[4];

        public Vector3 Position
        {
            get => this.transform.position;
            set => this.transform.position = value;

        }

        private void Update()
        {
            Move();
        }

        private void Move() 
        {
            bool[] direction = new bool[4];
            direction[0] = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W); // 上
            direction[1] = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S); // 下
            direction[2] = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A); // 左
            direction[3] = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D); // 右

            float[] move = direction.Select(x => x ? Speed * Time.deltaTime : 0).ToArray();
            this.Position += move[0] * Vector3.up + move[1] * Vector3.down + move[2] * Vector3.left + move[3] * Vector3.right;

            SetDirection(direction);
        }

        public void SetDirection(bool[] direction)
        {
            bool isMove = direction[0] || direction[1] || direction[2] || direction[3];//上下
        }

    }

}
