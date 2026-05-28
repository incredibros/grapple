using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractions : PlayerSystem
{
    // This system script checks if the player interacts with an orb

    List<LayerMask> layerMask = new List<LayerMask>();
    List<Vector2> checkSize = new List<Vector2>();

    protected override void Awake()
    {
        base.Awake();

        layerMask.Add(player.data.checkpointLayer);
        layerMask.Add(player.data.hazardLayer);
        layerMask.Add(player.data.orbLayer);
        layerMask.Add(player.data.springLayer);

        checkSize.Add(player.data.checkpointCheckSize);
        checkSize.Add(player.data.hazardCheckSize);
        checkSize.Add(player.data.orbCheckSize);
        checkSize.Add(player.data.springCheckSize);
    }

    void Update()
    {
        for (int i = 0; i < layerMask.Count; i++)
        {
            Collider2D hitCollider = Physics2D.OverlapBox(transform.position, checkSize[i], 0f, layerMask[i]);

            // Check Point
            if (i == 0 && hitCollider != null)
            {
                player.saveData.LastCheckpoint = hitCollider.transform.position;
            }
            // Hazard
            else if (i == 1 && hitCollider != null)
            {
                player.events.OnDeath?.Invoke();
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
    
    #endregion
}
