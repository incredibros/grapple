using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public enum GrappleableTypes { None, NonGrappleable, Platform, SemiSolid, CrumblingPlatform, Peg, Piton, Flinger }
public enum PullTypes { Boost, Reel }

public class Grappleable : MonoBehaviour
{
    public GrappleableTypes type;
    public PullTypes pullType;
    [ShowIf("pullType", PullTypes.Reel)] [AllowNesting] public Vector2 boostDirection;
    public bool clampGrapple;
    [ShowIf("clampGrapple")] [AllowNesting] public Vector2 minGrappleBounds;
    [ShowIf("clampGrapple")] [AllowNesting] public Vector2 maxGrappleBounds;
}
