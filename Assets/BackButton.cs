using UnityEngine;
using System.Collections;

public class BackButton : MonoBehaviour {

    public MenuReferences menu;

    void OnMouseUpAsButton()
    {
		if (menu.Menu == 4) {
			menu.playAnimator.gameObject.GetComponent<Play> ().Abort ();
		}
		menu.setMenu(menu.Menu -= 1);

    }


}
