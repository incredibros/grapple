using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOrb : PlayerSystem
{
    void Update()
    {
        Collider2D hit = Physics2D.OverlapBox(transform.position, player.data.orbCheckSize, 0f, player.data.orbLayer);
        if (hit != null)
            { player.events.OnOrbPickUp?.Invoke(hit.transform.parent.gameObject);}
    }
}
