using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class CrumblingPlatform : MonoBehaviour
{
    public Player player;

    SpriteRenderer sP;
    BoxCollider2D bC;

    enum State { active, crumbling, deactive, reappearing }
    State currentState;
    
    [Header("Timers")]
    public int crumbleTime;
    public int deactiveTime;
    public int reappearTime;

    int timer;
    
    void Awake()
    {
        sP = GetComponent<SpriteRenderer>();
        bC = GetComponent<BoxCollider2D>();
    }
    
    void FixedUpdate()
    {
        if (currentState == State.active) return;

        timer--;

        if (timer > 0) return;

        if (currentState == State.crumbling)
        {
            currentState = State.deactive;
            timer = deactiveTime;
            
            sP.enabled = false;
            bC.enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);

            player.events.ReleaseGrapplePoint?.Invoke();
        }
        else if (currentState == State.deactive)
        {
            currentState = State.reappearing;
            timer = reappearTime;

            sP.enabled = true;
            sP.color = Color.black;
            bC.enabled = true;
            transform.GetChild(0).gameObject.SetActive(true);
        }
        else if (currentState == State.reappearing)
        {
            currentState = State.active;

            sP.color = Color.gray;
        }
    }
    
    public void ActivateCrumbling()
    {
        if (currentState == State.active)
        {
            currentState = State.crumbling;
            timer = crumbleTime;

            sP.color = Color.red;
        }
    }
}
