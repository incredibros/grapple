using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : PlayerSystem
{
    // This system script controls player death and running the death animation

    Transform cam;

    bool died = false;

    float shakeDuration = 0.2f;
    float shakeMagnitude = 0.15f;

    void Start()
    {
        cam = Camera.main.transform;
    }

    #region Event Handlers
    void PlayDeathFX()
    {
        if (died)
            return;

        GameObject effect = Instantiate(player.data.gunShotEffectPrefab, transform.position, Quaternion.identity);
        Destroy(effect, 1f);

        StartCoroutine(ScreenShake());
        StartCoroutine(OnDeath());
    }

    void PlayRespawnFX()
    {
        GameObject effect = Instantiate(player.data.groundEffectPrefab, transform.position, Quaternion.identity);
        Destroy(effect, 1f);
    }

    IEnumerator ScreenShake()
    {
        Vector3 originalCamPos = cam.localPosition;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            Camera.main.transform.localPosition = new Vector3(originalCamPos.x + x, originalCamPos.y + y, originalCamPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalCamPos;
    }

    IEnumerator OnDeath()
    {
        died = true;
        SetPlayerVisibility(false);

        yield return new WaitForSeconds(0.5f);

        if (player.tempData.LastCheckpoint != null)
        {
            transform.position = (Vector3) player.tempData.LastCheckpoint;
        }
        else
        {
            transform.position = Vector2.zero;
        }

        SetPlayerVisibility(true);
        died = false;
        
        player.events.OnRespawn?.Invoke();
    }

    void SetPlayerVisibility(bool visible)
    {
        SpriteRenderer[] renderers = player.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            sr.enabled = visible;
        }
    }
    #endregion

    #region Events
    void OnEnable()
    {
        player.events.OnDeath += PlayDeathFX;
        player.events.OnRespawn += PlayRespawnFX;
    }

    void OnDisable()
    {
        player.events.OnDeath -= PlayDeathFX;
        player.events.OnRespawn -= PlayRespawnFX;
    }
    #endregion
}
