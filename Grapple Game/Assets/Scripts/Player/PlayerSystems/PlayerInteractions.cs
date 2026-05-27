using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractions : PlayerSystem
{
    // This system script checks if the player interacts with an orb

    List<LayerMask> layerMask = new List<LayerMask>();
    List<Vector2> checkSize = new List<Vector2>();
    Vector2? lastCheckpoint;
    
    bool canMove = true;

    protected override void Awake()
    {
        base.Awake();

        layerMask.Add(player.data.checkpointLayer);
        layerMask.Add(player.data.hazardLayer);
        layerMask.Add(player.data.orbLayer);
        layerMask.Add(player.data.springLayer);
        //layerMask.Add(player.data.pitonLayer);

        checkSize.Add(player.data.checkpointCheckSize);
        checkSize.Add(player.data.hazardCheckSize);
        checkSize.Add(player.data.orbCheckSize);
        checkSize.Add(player.data.springCheckSize);
        //checkSize.Add(player.data.pitonCheckSize);
    }

    void Update()
    {
        if (!canMove) return;

        for (int i = 0; i < layerMask.Count; i++)
        {
            Collider2D hitCollider = Physics2D.OverlapBox(transform.position, checkSize[i], 0f, layerMask[i]);

            // Check Point
            if (i == 0 && hitCollider != null)
            {
                lastCheckpoint = hitCollider.transform.position;
            }
            // Hazard
            else if (i == 1 && hitCollider != null)
            {
                StartCoroutine(OnDeath());
            }
            // Orb
            else if (i == 2 && hitCollider != null)
            {
                player.events.OnOrbPickUp?.Invoke(hitCollider.transform.parent.gameObject);
            }
            // Spring
            else if (i == 3 && hitCollider != null)
            {
                player.events.OnSpringActivated?.Invoke();
            }
        }
    }

    #region Death
    IEnumerator OnDeath()
    {
        canMove = false;
        player.events.OnDeath?.Invoke();

        yield return new WaitForSeconds(0.5f);

        if (lastCheckpoint != null)
        {
            transform.position = (Vector3) lastCheckpoint;
        }
        else
        {
            transform.position = Vector2.zero;
        }
        
        player.events.OnRespawn?.Invoke();
        canMove = true;
    }
    #endregion
}
