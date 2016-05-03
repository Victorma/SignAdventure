using UnityEngine;
using System.Collections;

public class BackButton : MonoBehaviour {

    public MenuReferences menu;

    void OnMouseUpAsButton()
    {
		if (menu.Menu == 4) {
			menu.playAnimator.gameObject.GetComponent<Play> ().Abort ();
		}
		if (menu.Menu == -1) 
		{
			menu.Menu = 0;
		} else {
			menu.setMenu(menu.Menu -= 1);
		}

    }


}
