﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class LifeSupportSystemEtherManager : MonoBehaviour, IInteractable,IDamageable
{
    [SerializeField] private float initEther;
    [SerializeField] private float etherReductionRate;
    [SerializeField] private float etherChargeValue;

    [SerializeField] private GameObject etherObject;
    [SerializeField] private float emitPower;
    [SerializeField] private float damagePower;

    private Subject<bool> deathStream;

    private LifeSupportSystemModel lifeSupportSystemModel;

    void Start()
    {
        lifeSupportSystemModel = GetComponent<LifeSupportSystemModel>();
        lifeSupportSystemModel.ether.Value = initEther;

        Observable.Interval(TimeSpan.FromMilliseconds(1000))
            .Subscribe(_ =>
            {
                ReduceEther(etherReductionRate);
            }).AddTo(this);
    }

    private void ReduceEther(float ether)
    {
        lifeSupportSystemModel.ether.Value -= ether;
    }

    private void AcquireEther(float ether)
    {
        lifeSupportSystemModel.ether.Value += ether;
    }

    public void Interact(PlayerManager playerManager)
    {
        playerManager.playerEtherManager.EmitEther(etherChargeValue);
        AcquireEther(etherChargeValue);
    }

    public void SetDamage(Damage damage){
        float LSSemithigh = 0.0f;
        float damageAmount = damage.amount * damagePower;
        var singleEtherValue = damage.amount;

        while (damageAmount > 0)
        {
            var emittedEtherObject = Instantiate(etherObject, transform.position + Vector3.up * LSSemithigh, transform.rotation);

            if (damageAmount < singleEtherValue) singleEtherValue = damageAmount;

            damageAmount -= singleEtherValue;
            emittedEtherObject.GetComponent<EtherObject>().Init(singleEtherValue);
            LSSemithigh += emittedEtherObject.transform.localScale.y;

            var emitDirestion = Vector3.up + new Vector3(UnityEngine.Random.Range(-emitPower, emitPower), 0, UnityEngine.Random.Range(-emitPower, emitPower));
            emittedEtherObject.GetComponent<Rigidbody>().AddForce(emitDirestion, ForceMode.Impulse);
        }
    }

    public Subject<bool> GetDeathStream()
    {
        return deathStream;
    }
}
