﻿using RunGame.Stage;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// <summary>
/// 敵の『ケムシ』を表します。
/// </summary>
public class Caterpiller : MonoBehaviour
{
    GameObject playerObj;

    Player player;
    Rigidbody2D rigidbody;

    public AudioClip SE_death;

    public enum ActionPart
    {
        Wait, // 待機モーション
        Raid, // 戦闘モーション
        Death // 死亡モーション
    }
    ActionPart Action = ActionPart.Wait;

    // ケムシの速度
    float speed_x = -0.7f;
    float speed_y = 0.0f;
    float speed_z = 0.0f;

    // 時間管理変数
    float motiontimer = 0.0f;
    float pausetimer = 0.0f;

    GameObject needle;

    SpriteRenderer sprite;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        playerObj = GameObject.Find("Player");
        player = playerObj.GetComponent<Player>();
        needle = (GameObject)Resources.Load("Prefabs/needle");
    }

    // Update is called once per frame
    void Update()
    {
        switch (Action)
        {
            case ActionPart.Wait:
                WaitAction();
                break;
            case ActionPart.Raid:
                RaidAction();
                break;
            case ActionPart.Death:
                DeathAction();
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if ( tag == "Enemy" ) {
            if ( collision.gameObject.tag == "Player" ) {
                if ( player.RotationMode == true ) {
                    GetComponent<AudioSource>().clip = SE_death;
                    GetComponent<AudioSource>().Play();
                    tag = "Dead";
                    GetComponent<Animator>().SetBool( "isDead", true );
                    GetComponent<BoxCollider2D>().isTrigger = true;
                    Action = ActionPart.Death;
                }
            }
        }

        if ( collision.gameObject.tag == "Wall" ||
             collision.gameObject.tag == "Enemy" ) {
            speed_x *= -1;
            var scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag == "Grounds") {
            speed_x *= -1;
            var scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    void WaitAction()
    {
        motiontimer += Time.deltaTime;
        if(motiontimer > Random.Range(1.5f, 2.0f)) {
            motiontimer = 0;
            Action = ActionPart.Raid;
        }
        var velocity = rigidbody.velocity;
        velocity.x = speed_x;
        rigidbody.velocity = velocity;
    }

    void RaidAction()
    {
        if (needle != null) {
            Instantiate(needle, new Vector3( transform.position.x, transform.position.y - 0.3f ), Quaternion.Euler(0, 0, 180));
            Instantiate(needle, new Vector3( transform.position.x, transform.position.y - 0.3f ), Quaternion.Euler(0, 0, 150));
            Instantiate(needle, new Vector3( transform.position.x, transform.position.y - 0.3f ), Quaternion.Euler(0, 0, 210));
        }
        Action = ActionPart.Wait;
    }

    void DeathAction()
    {
        sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color( sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a - 0.01f );
        if ( sprite.color.a < 0 ) {
            Destroy( gameObject );
        }
    }

    private void OnBecameVisible() {
        Action = ActionPart.Wait;
    }
}
