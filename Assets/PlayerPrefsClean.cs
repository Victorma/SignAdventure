using UnityEngine;
using System.Collections;

public class PlayerPrefsClean : MonoBehaviour {

	public void Clean(){
		PlayerPrefs.DeleteAll ();
		PlayerPrefs.Save ();
	}
}
