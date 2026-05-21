using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffects : PlayerSystem
{
    [SerializeField] Transform gunTip;

    #region Collision Effects
    // Ground
    void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 contactPoint = collision.GetContact(0).point; 
        GameObject effect = Instantiate(player.data.groundEffectPrefab, contactPoint, Quaternion.identity);
        Destroy(effect, 1f);
    }

    // Grapple point
    void OnGrapple(Vector2 point)
    {
        GameObject effect = Instantiate(player.data.groundEffectPrefab, point, Quaternion.identity);
        Destroy(effect, 1f);
    }

    void OnGrappleButtonDown()
    {
        GameObject effect = Instantiate(player.data.gunShotEffectPrefab, gunTip.position, Quaternion.identity);
        Destroy(effect, 1f);
    }

    void OnPullButtonDown()
    {
        GameObject effect = Instantiate(player.data.gunShotEffectPrefab, gunTip.position, Quaternion.identity);
        Destroy(effect, 1f);
    }
    #endregion

    #region Events
    void OnEnable()
    {
        player.events.OnGrapple += OnGrapple;
        player.events.OnGrappleButtonDown += OnGrappleButtonDown;
        player.events.OnPullButtonDown += OnPullButtonDown;
    }

    void OnDisable()
    {
        player.events.OnGrapple -= OnGrapple;
        player.events.OnGrappleButtonDown -= OnGrappleButtonDown;
        player.events.OnPullButtonDown -= OnPullButtonDown;
    }
    #endregion
}
