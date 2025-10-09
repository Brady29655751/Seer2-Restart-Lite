using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGame
{
    public class FlappyBird : MiniGame
    {
        [SerializeField] private Bird bird;
        [SerializeField] private GameObject pipePrefab;
        [SerializeField] private Transform pipeParent;
        [SerializeField] private List<Sprite> pipeSprites;
        [SerializeField] private Vector2 birdSpeed = new Vector2(2.5f, 5f);
        [SerializeField] private int pipeInterval = 3;
        [SerializeField] private float pipeMinY = -1f;
        [SerializeField] private float pipeMaxY = 2f;
        [SerializeField] private IText scoreText;

        private int score => data.GetData<int>("score", "0");
        private float speed => (score < 15) ? 1f : 1.5f;
        private Rigidbody2D birdRb;
        private List<GameObject> pipes = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();
            bird.onCollisionEnter2DEvent += (col) => OnBirdDie();
            birdRb = bird.GetComponent<Rigidbody2D>();
        }

        protected override void Start()
        {
            var hintbox = Hintbox.OpenHintbox();
            hintbox.SetSize(400, 200);
            hintbox.SetContent("点击鼠标左键让皮皮飞起来，\n躲避障碍物，获得更高的分数吧！", 16, FontOption.Arial);
            hintbox.SetOptionNum(1);
            hintbox.SetOptionCallback(() =>
            {
                SpawnPipe();     // 立即生成第一个水管
                timer.onDoneEvent += (sec) =>
                {
                    OnBirdScore();
                    SpawnPipe();
                    timer.Restart();
                    timer.SetSpeed(speed);
                };
                timer.SetTimer(pipeInterval);
                birdRb.gravityScale = 1;
            });
        }

        private void Update()
        {
            if (timer.isPaused)
                return;

            if (!bird.transform.position.y.IsWithin(6, -6))
            {
                OnBirdDie();
                return;
            }

            if (Input.GetMouseButtonDown(0)) // 点击鼠标让小鸟飞起来
            {
                birdRb.velocity = Vector2.up * birdSpeed.y;
            }

            // 移动水管
            for (int i = pipes.Count - 1; i >= 0; i--)
            {
                pipes[i].transform.Translate(Vector3.left * birdSpeed.x * Time.deltaTime);
                if (pipes[i].transform.position.x < -10f) // 水管移出屏幕后销毁
                {
                    Destroy(pipes[i].gameObject);
                    pipes.RemoveAt(i);
                }
            }
        }

        private void SpawnPipe()
        {
            float yPos = Random.Range(pipeMinY, pipeMaxY);
            var newPipe = Instantiate(pipePrefab, new Vector3(10f, yPos, 0f), Quaternion.identity, pipeParent);
            newPipe.transform.localScale = Vector3.one * Random.Range(1f, 3f);
            newPipe.GetComponent<SpriteRenderer>().sprite = pipeSprites.Get(Random.Range(0, pipeSprites.Count));
            pipes.Add(newPipe);
        }

        private void OnBirdScore()
        {
            data.SetData("score", score + 1);
            scoreText.SetText(data.GetData("score"));
        }

        private void OnBirdDie()
        {
            timer.Pause();
            birdRb.gravityScale = 0;
            birdRb.velocity = Vector2.zero;

            var hintbox = Hintbox.OpenHintboxWithContent("游戏结束！\n\n你的得分是：" + data.GetData("score", "0"), 16);
            hintbox.SetOptionCallback(() =>
            {
                var item = new Item(1, score * 100);
                Item.Add(item);
                
                var itemHintbox = Item.OpenHintbox(item);
                itemHintbox.SetOptionCallback(CloseMiniGame);
            });
        }
    }
}
