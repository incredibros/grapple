using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Orb : MonoBehaviour
{
    SpriteRenderer sP;

    public float travelTime;
    public float respawnTime;

    void Awake()
    {
        sP = GetComponent<SpriteRenderer>();
    }
    
    public void OnPickUp(Transform player)
    {
        transform.GetChild(0).gameObject.SetActive(false);
        StartCoroutine(GoToPlayer(player));
    }

    IEnumerator GoToPlayer(Transform player)
    {
        float time = 0f;
        Vector2 startingPos = (Vector2) transform.position;
        while (time < travelTime)
        {
            transform.position = Vector2.Lerp(startingPos, player.position, time / travelTime);
            time += Time.deltaTime;
            yield return new WaitForSeconds(0);
        }

        transform.position = startingPos;
        sP.color = Color.gray;

        yield return new WaitForSeconds(respawnTime);

        sP.color = Color.white;
        transform.GetChild(0).gameObject.SetActive(true);
    }
}
