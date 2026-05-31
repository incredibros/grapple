using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    SpriteRenderer sprite;
    [SerializeField] float alphaTime;



    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        StartCoroutine(FadeAlphaOverTime(0f, alphaTime));
    }

    IEnumerator FadeAlphaOverTime(float targetAlpha, float duration)
    {
        Color currentColor = sprite.color;
        float startAlpha = currentColor.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);

            if (newAlpha < 0.1f)
                break;

            currentColor.a = newAlpha;
            sprite.color = currentColor;

            yield return null; 
        }

        currentColor.a = targetAlpha;
        sprite.color = currentColor;
        Destroy(gameObject);
    }
}
