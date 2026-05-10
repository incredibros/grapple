using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffects : PlayerSystem
{
    #region Collision Effects
    void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 contactPoint = collision.GetContact(0).point; 
        GameObject effect = Instantiate(player.data.effectPrefab, contactPoint, Quaternion.identity);
        Destroy(effect, 1f);
    }

    void OnGrapple(Vector2 point)
    {
        GameObject effect = Instantiate(player.data.effectPrefab, point, Quaternion.identity);
        Destroy(effect, 1f);
    }
    #endregion

    #region Events
    void OnEnable()
    {
        player.events.OnGrapple += OnGrapple;
    }

    void OnDisable()
    {
        player.events.OnGrapple -= OnGrapple;
    }
    #endregion
}
