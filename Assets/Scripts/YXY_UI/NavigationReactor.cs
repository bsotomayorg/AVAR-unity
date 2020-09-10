using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NavigationReactor:MonoBehaviour
{
    public abstract void NavigationStart(Vector3 startPos, GameObject origin);
    public abstract void NavigationUpdate(Vector3 deltaPos, GameObject origin);
    public abstract void NavigationEnd(Vector3 endPos, GameObject origin);
}
