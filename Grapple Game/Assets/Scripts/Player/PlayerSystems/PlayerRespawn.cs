using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : PlayerSystem
{
    // This system script controls player death, keeping track of the last respawn point and running the death animation
    
    Vector2 lastCheckpoint;
    
    bool canMove = true;
    
    void Update()
    {
        if (!canMove)
            return;
        
        Collider2D checkpoint = Physics2D.OverlapBox(transform.position, player.data.checkpointCheckSize, 0f, player.data.checkpointLayer);
        if (checkpoint != null)
        {
            lastCheckpoint = checkpoint.transform.position;
            Debug.Log("Checkpoint reached");
        }
        Collider2D hazard = Physics2D.OverlapBox(transform.position, player.data.hazardCheckSize, 0f, player.data.hazardLayer);
        if (hazard != null)
        {
            StartCoroutine(OnDeath());
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            player.events.OnDeath?.Invoke();
            player.events.OnRespawn?.Invoke();
            LoadNextScene();
        }
    }

    IEnumerator OnDeath()
    {
        canMove = false;
        player.events.OnDeath?.Invoke();

        yield return new WaitForSeconds(0.5f);

        if (lastCheckpoint != null)
        {
            transform.position = lastCheckpoint;
        }
        else
        {
            transform.position = Vector2.zero;
        }
        
        player.events.OnRespawn?.Invoke();
        canMove = true;
    }

    void LoadNextScene()
    {
        int nextScene = (SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene(nextScene);
    }
}
