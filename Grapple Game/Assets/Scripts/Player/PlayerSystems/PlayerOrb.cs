using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOrb : PlayerSystem
{
    void Update()
    {
        RaycastHit2D boxcast = Physics2D.BoxCast(transform.position, player.data.orbCheckSize, 0f, Vector2.zero, 0f, player.data.orbLayer);
        if (boxcast.collider != null)
            { player.events.OnOrbPickUp?.Invoke(boxcast.collider.transform.parent.gameObject); }
    }
}
