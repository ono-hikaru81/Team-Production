﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RunGame.Stage
{
    /// <summary>
    /// 『ダンゴムシ』を表します。
    /// </summary>
    public class Player : MonoBehaviour
    {
        // 通常の移動速度を指定します。
        [SerializeField]
        private float speed = 4;
        // ダッシュ時の移動速度を指定します。
        [SerializeField]
        private float dashSpeed = 8;
        // ジャンプの力を指定します。
        [SerializeField]
        private float jumpPower = 150;
        // 設置判定の際に判定対象となるレイヤーを指定します。
        [SerializeField]
        private LayerMask groundLayer = 0;
        // ダッシュの際のサウンドを指定します。
        [SerializeField]
        private AudioClip soundOnDash = null;

        // コロコロモードのスタミナ
        float stamina = 10.0f;

        /// <summary>
        /// プレイ中の場合はtrue、ステージ開始前またはゲームオーバー時にはfalse
        /// </summary>
        public bool IsActive {
            get { return isActive; }
            set { isActive = value; }
        }
        bool isActive = false;

        // 着地している場合はtrue、ジャンプ中はfalse
        [SerializeField]
        bool isGrounded = false;

        // AnimatorのパラメーターID
        static readonly int dashId = Animator.StringToHash("isDash");

        // コロコロモード状態の場合はtrue
        public bool RotationMode {
            get { return rotationMode; }
            private set {
                rotationMode = value;
                // ダッシュ状態への遷移時
                if (value)
                {
                    // ダッシュアニメーションへ切り替え
                    animator.SetBool(dashId, true);
                    // サウンド再生
                    if (audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }
                    audioSource.clip = soundOnDash;
                    audioSource.loop = true;
                    audioSource.Play();
                }
                // 通常状態へ遷移する場合
                else
                {
                    // 通常アニメーションへ切り替え
                    animator.SetBool(dashId, false);
                    // サウンド停止
                    if (audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }
                }
            }
        }
        bool rotationMode = false;

        // 設置判定用のエリア
        Vector3 groundCheckA, groundCheckB;

        // コンポーネントを事前に参照しておく変数
        Animator animator;
        new Rigidbody2D rigidbody;
        // サウンドエフェクト再生用のAudioSource
        AudioSource audioSource;

        // Start is called before the first frame update
        void Start()
        {
            // 事前にコンポーネントを参照
            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody2D>();
            audioSource = GetComponent<AudioSource>();

            // Box Collider 2Dの判定エリアを取得
            var collider = GetComponent<BoxCollider2D>();
            // コライダーの中心座標へずらす
            groundCheckA = collider.offset;
            groundCheckB = collider.offset;
            // コライダーのbottomへずらす
            groundCheckA.y += -collider.size.y * 0.5f;
            groundCheckB.y += -collider.size.y * 0.5f;
            // 範囲を決定
            Vector2 size = collider.size;
            size.x *= 0.75f;    // 横幅
            size.y *= 0.25f;    // 高さは4分の1
            // コライダーの横幅方向へ左右にずらす
            groundCheckA.x += -size.x * 0.5f;
            groundCheckB.x += size.x * 0.5f;
            // コライダーの高さ方向へ上下にずらす
            groundCheckA.y += -size.y * 0.5f;
            groundCheckB.y += size.y * 0.5f;
        }

        // Update is called once per frame
        void Update()
        {
            if (IsActive)
            {
                // 転倒判定
                // 1. スピードが０になったとき
                // 2. かつ、着地できていない場合

                // 回転角度を取得[-360, 360]
                var rotationZ = transform.eulerAngles.z;
                // 角度が[-180, 180]で表されるように補正
                if (rotationZ > 180)
                {
                    rotationZ -= 360;
                }
                else if (rotationZ < -180)
                {
                    rotationZ += 360;
                }
                // 転倒条件フラグ(転倒状態:true)
                var isTumbled = (rotationZ > 70) || (rotationZ < -70);
                if (rigidbody.velocity.magnitude < 0.1f &&
                    !isGrounded && isTumbled)
                {
                    IsActive = false;
                    SceneController.Instance.GameOver();
                }
            }

            // 接地している場合
            if (isGrounded)
            {
                // '十字'キーが押されているときの移動させる処理
                if(Input.GetKey(KeyCode.RightArrow))
                {
                    var velocity = rigidbody.velocity;
                    velocity.x = speed;
                    rigidbody.velocity = velocity;
                }
                else if (Input.GetKey(KeyCode.LeftArrow))
                {
                    var velocity = rigidbody.velocity;
                    velocity.x = -speed;
                    rigidbody.velocity = velocity;
                }
                // 'D'キーが押し下げられている場合はダッシュ処理(コロコロモード)
                if (Input.GetKey(KeyCode.D) && stamina >= 0)
                {
                    // スタミナが減少
                    stamina -= Time.deltaTime;
                    // x軸方向の移動
                    var velocity = rigidbody.velocity;
                    if(Input.GetKey(KeyCode.RightArrow))
                    {
                        velocity.x = dashSpeed;
                    }
                    else if(Input.GetKey(KeyCode.LeftArrow))
                    {
                        velocity.x = -dashSpeed;
                    }
                    rigidbody.velocity = velocity;
                    // 通常状態からダッシュ状態に切り替える場合
                    if (!RotationMode)
                    {
                        RotationMode = true;
                    }
                }
                // 'W'キーが押された場合はジャンプ処理
                else if (Input.GetKeyDown(KeyCode.W))
                {
                    //RotationMode = false;
                    //rigidbody.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
                    //// ジャンプ状態に設定
                    //isGrounded = false;
                }
                else
                {
                    RotationMode = false;
                //    // x軸方向の移動
                //    var velocity = rigidbody.velocity;
                //    velocity.x = -speed;
                //    rigidbody.velocity = velocity;
                }
            }
            // 空中状態の場合
            else
            {
                if (RotationMode)
                {
                    RotationMode = false;
                }
            }
        }

        /// <summary>
        /// 固定フレームレートで実行されるフレーム更新処理
        /// </summary>
        private void FixedUpdate()
        {
            // 着地判定
            // ワールド空間の位置へ移動
            var minPosition = groundCheckA + transform.position;
            var maxPosition = groundCheckB + transform.position;
            // minPositionとmaxPositionで指定した範囲内に
            // コライダーが存在するかどうかを判定
            isGrounded = Physics2D.OverlapArea(
                minPosition, maxPosition, groundLayer);
#if UNITY_EDITOR
            // デバッグ用にテストでラインを描画する
            Vector2 start, end;

            // TOP
            start.x = minPosition.x;
            end.x = maxPosition.x;
            start.y = maxPosition.y;
            end.y = maxPosition.y;
            Debug.DrawLine(start, end, Color.yellow);
            // BOTTOM
            start.x = minPosition.x;
            end.x = maxPosition.x;
            start.y = minPosition.y;
            end.y = minPosition.y;
            Debug.DrawLine(start, end, Color.yellow);
            // LEFT
            start.x = minPosition.x;
            end.x = minPosition.x;
            start.y = minPosition.y;
            end.y = maxPosition.y;
            Debug.DrawLine(start, end, Color.yellow);
            // RIGHT
            start.x = maxPosition.x;
            end.x = maxPosition.x;
            start.y = minPosition.y;
            end.y = maxPosition.y;
            Debug.DrawLine(start, end, Color.yellow);
#endif
        }

        /// <summary>
        /// このプレイヤーが他のオブジェクトのトリガー内に侵入した際に
        /// 呼び出されます。
        /// </summary>
        /// <param name="collider">侵入したトリガー</param>
        private void OnTriggerEnter2D(Collider2D collider)
        {
            // ゴール判定
            if (collider.tag == "Finish")
            {
                SceneController.Instance.StageClear();
            }
            // ゲームオーバー判定
            else if (collider.tag == "GameOver")
            {
                SceneController.Instance.GameOver();
            }
            // アイテムを取得
            else if (collider.tag == "Item")
            {
                // 取得したアイテムを削除
                Destroy(collider.gameObject);
            }
        }
    }
}