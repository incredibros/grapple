using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerSystem : MonoBehaviour
{
    // This script is the parent of all player system scripts, giving each system access to the main player script
    // Every player system script is a child of this script, which basically means they attach their code underneath everything
    // written here, basically an add-on to this script

    // Each system script has an OnEnable function that connects functions to actions and OnDisable function that deconnects
    // functions to actions. This makes sure an action isn't trying to call a function that doesn't currently exist
    // because that script is turned off
    
    // Protected changes the visibilty of something along with private and public. Protected means all parents and childs
    // of this script can see it
    protected Player player;

    // Virtual means that this function can be overriden. This is used when childs of this script need to use their own awake function
    protected virtual void Awake()
    {
        player = transform.root.GetComponent<Player>();
    }
}
