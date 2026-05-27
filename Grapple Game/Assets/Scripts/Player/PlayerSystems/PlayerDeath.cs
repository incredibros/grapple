using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : PlayerSystem
{
    float shakeDuration = 0.2f;
    float shakeMagnitude = 0.15f;

    Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void PlayDeathFX()
    {
        GameObject effect = Instantiate(player.data.gunShotEffectPrefab, transform.position, Quaternion.identity);
        Destroy(effect, 1f);

        StartCoroutine(ScreenShake());
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
