﻿using RunGame.SelectStage;
using RunGame.Title;
using System.Collections;   // コルーチンのため
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RunGame.Stage
{
    /// <summary>
    /// 『ステージ画面』のシーン遷移を制御します。
    /// </summary>
    /// 
    public class SceneController : MonoBehaviour
    {
        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得します。
        /// </summary>
        public static SceneController Instance {
            get { return instance; }
        }
        static SceneController instance = null;

        /// <summary>
        /// Start()より先に実行されます。
        /// </summary>
        private void Awake()
        {
            instance = this;
        }
        #endregion

        /// <summary>
        /// 起動するシーン番号を取得または設定します。
        /// </summary>
        public static int StageNo {
            get { return stageNo; }
            set { stageNo = value; }
        }
        private static int stageNo = 0;

        public AudioClip normalStageBgm;
        public AudioClip gameOverBgm;

        public GameObject mole;

        /// <summary>
        /// プレハブからステージを生成する場合はtrueを指定します。
        /// </summary>
        /// <remarks>ステージ開発用のシーンではfalseに設定します。</remarks>
        [SerializeField]
        private bool instantiateStage = true;

        /// <summary>
        /// ステージ開始からの経過時間(秒)を取得します。
        /// </summary>
        public float PlayTime { get; private set; }
        //public float PlayTime {
        //    get { return playTime; }
        //    private set { playTime = value; }
        //}
        //float playTime = 0;

        // 起動しているOnPlay()コルーチン
        Coroutine playState = null;
        // 外部のゲームオブジェクトの参照変数
        Player player;

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            // 他のゲームオブジェクトを参照
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

            // データ読み込み
            var pos = player.transform.position;
            switch (PlayerPrefs.GetInt("isContinue")) {
                case 1:
                    pos.x = PlayerPrefs.GetFloat("PlayerPosX", 0.0f);
                    pos.y = PlayerPrefs.GetFloat("PlayerPosY", 3.0f);
                    break;
                default:
                case 0:
                    pos.x = 0.0f;
                    pos.y = 3.0f;
                    break;
            }
            player.transform.position = pos;

            // ステージプレハブを読み込む
            if (instantiateStage)
            {
                var stageName = string.Format("Stage {0}", stageNo);
                var stage = Resources.Load<GameObject>(stageName);
                Instantiate(stage);
            }

            var bgmAudio = Camera.main.GetComponent<AudioSource>();
            bgmAudio.clip = normalStageBgm;
            bgmAudio.Play();

//            StartCoroutine(OnStart());
        }

        /// <summary>
        /// コルーチンを使ったカウントダウン演出
        /// </summary>
        //IEnumerator OnStart()
        //{
        //    yield return new WaitForSeconds(1); // 1秒待機

        //    const float showTimeout = 0.6f;

        //    UiManager.Instance.ShowMessage("3");
        //    yield return new WaitForSeconds(showTimeout);
        //    UiManager.Instance.HideMessage();
        //    yield return new WaitForSeconds(1 - showTimeout);

        //    UiManager.Instance.ShowMessage("2");
        //    yield return new WaitForSeconds(showTimeout);
        //    UiManager.Instance.HideMessage();
        //    yield return new WaitForSeconds(1 - showTimeout);

        //    UiManager.Instance.ShowMessage("1");
        //    yield return new WaitForSeconds(showTimeout);
        //    UiManager.Instance.HideMessage();
        //    yield return new WaitForSeconds(1 - showTimeout);

        //    UiManager.Instance.ShowMessage("GO!");

        //    // ステージをプレイ開始
        //    playState = StartCoroutine(OnPlay());

        //    yield return new WaitForSeconds(1); // 1秒待機
        //    UiManager.Instance.HideMessage();
        //}

        /// <summary>
        /// Playステートの際のフレーム更新処理です。
        /// </summary>
        IEnumerator OnPlay()
        {
            player.IsActive = true;

            while (true)
            {
                PlayTime += Time.deltaTime;

#if UNITY_EDITOR
                // 「Enter」キーが押された場合『リザルト画面』へ
                if (Input.GetKeyUp(KeyCode.Return))
                {
                    StageClear();
                    break;
                }
                // 'O'キーが押された場合「GameOver」を表示
                else if (Input.GetKeyUp(KeyCode.O))
                {
                    GameOver();
                    break;
                }
#endif
                yield return null;
            }
        }

        /// <summary>
        /// ステージをクリアーさせます。
        /// </summary>
        public void StageClear()
        {
            // 現在のコルーチンを停止
            if (playState != null)
            {
                StopCoroutine(playState);
            }

            player.IsActive = false;

            // ステージクリアー演出のコルーチンを開始
            StartCoroutine(OnStageClear());
        }

        /// <summary>
        /// ゲームオーバーさせます。
        /// </summary>
        public void GameOver()
        {
            // 現在のコルーチンを停止
            if (playState != null)
            {
                StopCoroutine(playState);
            }

            var bgmAudio = Camera.main.GetComponent<AudioSource>();
            bgmAudio.clip = gameOverBgm;
            bgmAudio.loop = false;
            bgmAudio.Play();
            player.IsActive = false;
            UiManager.Instance.GameOver.Show();
        }

        /// <summary>
        /// StageClearステートの際のフレーム更新処理です。
        /// </summary>
        IEnumerator OnStageClear()
        {
            // ベストタイムを更新
            //if (PlayTime < GameController.Instance.BestTime)
            //{
            //    GameController.Instance.BestTime = PlayTime;
            //}
            UiManager.Instance.ShowMessage("CLEAR!");
            yield return new WaitForSeconds(1);
            // 入力を待ち受けるために無限ループ
            while (true)
            {
                // 「Enter」キーが押された場合
                if (Input.GetKeyUp(KeyCode.Return))
                {
                    // ステージ番号を伝えてから「Result」を読み込む
                    //Result.SceneController.StageNo = StageNo;
                    //Result.SceneController.ClearTime = PlayTime;
                    SceneManager.LoadScene( "HappyEndMovie" );
                    break;
                }
                yield return null;  // 次のフレームまで待機
            }
        }
    }
}