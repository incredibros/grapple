using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{
    SpriteRenderer sP;
    Player player;
    [SerializeField] float travelTime;
    Vector2 startingPos;
    bool hasCrystal;

    void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        
        sP = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        startingPos = (Vector2) transform.position;
    }

    void Update()
    {
        if (hasCrystal && player.tempData.OnGround)
        {
            player.tempData.Crystals++;
            Destroy(gameObject);
        }
    }
    
    #region Event Handler
    public void OnCrystalPickUp(GameObject player)
    {
        sP.enabled = true;
        transform.GetChild(0).gameObject.SetActive(false);
        hasCrystal = true;
        StartCoroutine(GoToPlayer(player.transform));
    }

    IEnumerator GoToPlayer(Transform player)
    {
        float time = 0f;
        while (time < travelTime)
        {
            transform.position = Vector2.Lerp(startingPos, player.position, time / travelTime);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = startingPos;
        sP.enabled = false;
    }

    void OnRespawn()
    {
        if (hasCrystal)
        {
            hasCrystal = false;
            transform.GetChild(0).gameObject.SetActive(true);
            sP.enabled = true;
        }
    }
    #endregion

    #region Events
    void OnEnable()
    {
        player.events.OnRespawn += OnRespawn;
        player.events.OnCrystalPickUp += OnCrystalPickUp;
    }

    void OnDisable()
    {
        player.events.OnRespawn -= OnRespawn;
        player.events.OnCrystalPickUp -= OnCrystalPickUp;
    }
    #endregion
}
