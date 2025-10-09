using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGame
{
    public class MiniGame : Module
    {
        public string gameName => data?.id;
        public Activity data => Player.instance.currentMiniGame;

        [SerializeField] protected Timer timer;

        public static T LoadMiniGame<T>() where T : MiniGame {
            string name = typeof(T).Name.TrimEnd("Game");
            return (T)LoadMiniGame(name);
        }       

        public static MiniGame LoadMiniGame(string miniGameName) {
            if (string.IsNullOrEmpty(miniGameName))
                return null;

            GameObject prefab = ResourceManager.instance.GetMiniGame(miniGameName);
            if (prefab == null)
                return null;

            GameObject obj = Instantiate(prefab);
            MiniGame game = obj?.GetComponent<MiniGame>();
            return game;
        }

        public static void CloseMiniGame()
        {
            SceneLoader.instance.ChangeScene(SceneId.Map);
        }

        protected override void Awake()
        {
            if (timer != null)
                timer.onDoneEvent += (time) => OnFinish();
        }

        public virtual void OnFinish()
        {

        }
    }
    
}
