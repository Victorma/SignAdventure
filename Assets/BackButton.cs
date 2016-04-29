using UnityEngine;
using System.Collections;

public class BackButton : MonoBehaviour {

    public MenuReferences menu;

    void OnMouseUpAsButton()
    {

        menu.setMenu(menu.Menu -= 1);

    }


}
