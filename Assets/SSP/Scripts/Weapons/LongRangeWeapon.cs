﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using UniRx.Triggers;

public class LongRangeWeapon : NetworkBehaviour, IAttackable
{
    [SerializeField] private float coolTime, bulletSpeed;
    [SerializeField] GameObject bulletPrefab, muzzle;
    [SerializeField] private float bulletDamageAmount, bulletDeathTime;
    private BulletModel bulletModel;
    private bool canAttack = true;
    [SerializeField, SyncVar] private int playerId, teamId;
    private RaycastHit hit;
    private int layerMask = ~(1 << LayerMap.LocalPlayer);
    private float time = 0;
    private Transform cameraTransform;

    public void Init(PlayerModel playerModel)
    {
        playerId = playerModel.playerId;
        teamId = playerModel.teamId;
        cameraTransform = Camera.main.transform;

        this.UpdateAsObservable()
            .Where(_ => this.gameObject.activeSelf)
            .Subscribe(_ =>
            {
                time += Time.deltaTime;
                if (time >= coolTime)
                    canAttack = true;
            });
    }

    public void NormalAttack()
    {
        if (canAttack)
        {
            CmdShoot(cameraTransform.position, cameraTransform.forward, cameraTransform.rotation);
            time = 0;
            canAttack = false;
        }
    }

    [Command]
    private void CmdShoot(Vector3 castPosition, Vector3 castDirection, Quaternion uncastableDirection)
    {
        RpcShoot(castPosition, castDirection, uncastableDirection);
    }

    [ClientRpc]
    private void RpcShoot(Vector3 castPosition, Vector3 castDirection, Quaternion uncastableDirection)
    {
        var bullet = Instantiate(bulletPrefab, muzzle.transform.position, muzzle.transform.rotation);
        //レイが衝突すればその点へ飛ばし、衝突しなければそれっぽいところへ飛ばす。
        if (Physics.Raycast(castPosition, castDirection, out hit, 1000, layerMask))
        {
            bullet.transform.LookAt(hit.point);
        }
        else
        {
            bullet.transform.rotation = uncastableDirection;
        }

        bullet.GetComponent<BulletModel>().SetProperties(playerId, teamId, bulletDamageAmount, bulletDeathTime);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletSpeed);
    }
}
