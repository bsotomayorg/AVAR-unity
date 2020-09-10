using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class NavigationReactorUI : NavigationReactor
{
    public UnityEngine.UI.Selectable M_SelectHandler { get; protected set; }

    public abstract void changeState(bool isOn);
}
