using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGame
{
    public class Bird : Module
    {
        public event Action<Collision2D> onCollisionEnter2DEvent;

        private void OnCollisionEnter2D(Collision2D other)
        {
            onCollisionEnter2DEvent?.Invoke(other);
        }

    }
}
