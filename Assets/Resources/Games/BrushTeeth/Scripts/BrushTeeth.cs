using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGame
{
    public class BrushTeeth : MiniGame
    {
        [SerializeField] private int time = 60;
        [SerializeField] private Texture2D cursor;
        [SerializeField] private IText recordText;
        [SerializeField] private SpriteRenderer spray;
        [SerializeField] private List<GameObject> germObjects;

        protected override void Start()
        {
            data.SetData("germ", "0");

            var hintbox = Hintbox.OpenHintbox();
            hintbox.SetSize(480, 280);
            hintbox.SetContent("利牙鱼的口腔里有很多的蛀牙菌哟，让我们一起来为它清理一下吧！\n\n清洗药剂是由鼠标控制移动的，让喷雾对准蛀牙菌，\n\n点击鼠标把它们一扫而光！", 15, FontOption.Arial);
            hintbox.SetOptionNum(1);
            hintbox.SetOptionCallback(() =>
            {
                Cursor.SetCursor(cursor, cursor.GetTextureSize(), CursorMode.Auto);
                timer.SetTimer(time);
                StartCoroutine(GenerateGerm());
            });
        }

        private void Update()
        {
            CheckSpray();
        }

        private IEnumerator GenerateGerm()
        {
            while (!timer.isDone)
            {
                var germs = germObjects.FindAllIndex(x => !x.activeSelf);
                while (ListHelper.IsNullOrEmpty(germs))
                {
                    yield return new WaitForSeconds(2);
                    germs = germObjects.FindAllIndex(x => !x.activeSelf);
                }

                germObjects[germs.ToList().Random()]?.SetActive(true);
                yield return new WaitForSeconds(timer.currentTime / 15 + 1);
            }
        }

        private void CheckSpray()
        {
            if (timer.isPaused)
                return;

            if (!Input.GetMouseButtonDown(0))
                return;

            StartCoroutine(Spray());
        }

        private IEnumerator Spray()
        {
            spray.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spray.transform.Translate(-spray.sprite.texture.width / 2 / 100, 0, -spray.transform.position.z);
            spray.gameObject.SetActive(true);

            var rect = new Rect(spray.transform.position, Vector2.one);
            rect.position -= Vector2.one / 2;

            var killedGerm = germObjects.FirstOrDefault(germ => germ.activeSelf && rect.Contains(germ.transform.position));
            if (killedGerm != null)
            {
                killedGerm.SetActive(false);
                data.SetData("germ", data.GetData<int>("germ") + 1);
                recordText?.SetText(data.GetData("germ"));
            }

            yield return new WaitForSeconds(0.5f);

            spray.gameObject.SetActive(false);
        }

        public override void OnFinish()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            var germ = data.GetData<int>("germ");
            var hintbox = Hintbox.OpenHintbox();
            hintbox.SetContent($"你帮助利牙鱼清除了<color=#ffbb33>{germ}</color>个细菌\n它变得更干净啦！", 16, FontOption.Arial);
            hintbox.SetOptionNum(1);
            hintbox.SetOptionCallback(() =>
            {
                var item = new Item(10232, germ / 2);
                Item.Add(item);

                var itemHintbox = Item.OpenHintbox(item);
                itemHintbox.SetOptionCallback(CloseMiniGame);
            });
        }

    }   
}
