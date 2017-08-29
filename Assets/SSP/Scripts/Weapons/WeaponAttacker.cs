﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Triggers;
using UniRx;
using System.Linq;

public class WeaponAttacker : MonoBehaviour
{
    [SerializeField] public float damageAmount;//攻撃のダメージ量
    [SerializeField] float hitDetectionTimeOffset;//攻撃開始から当たり判定が発生するまでの時間
    [SerializeField] float hitDetectionDuration;//当たり判定が発生する時間の長さ
    [SerializeField] float detectionTimer;
    bool isAttackStarted;

    void Start()
    {
        this.Init();
    }

    void Init()
    {
        isAttackStarted = false;
        detectionTimer = 0.0f;
    }

    public void NormalAttack(Animator animator)
    {
        animator.SetTrigger("Attack");
        detectionTimer = 0.0f;
        isAttackStarted = true;
    }

    void OnTriggerEnter(Collider col)
    {
        if (CanDetectObject())
        {
            var hm = col.gameObject.GetComponent<HealthModel>();
            if(hm != null)
            {
                CmdSetDamage(hm, this.damageAmount);
            }
        }
    }

    bool CanDetectObject()
    {
        if (!isAttackStarted) return false;
        if (detectionTimer < hitDetectionTimeOffset) return false;
        if (detectionTimer < hitDetectionDuration) return true;
        return false;
    }

    //今後ネットワークにするためCmd
    void CmdSetDamage(HealthModel hm,float dmgAmount)
    {
        hm.SetDamage(dmgAmount);
    }

    [SerializeField] bool hoge;

    void FixedUpdate()
    {
        if (isAttackStarted)
        {
            detectionTimer += Time.fixedDeltaTime;
        }
        if(detectionTimer > (hitDetectionTimeOffset + hitDetectionDuration))
        {
            Init();
        }
        hoge = CanDetectObject();
    }
}
