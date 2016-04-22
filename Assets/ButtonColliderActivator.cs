using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonColliderActivator : MonoBehaviour {

	public Button b;

	// Use this for initialization
	void Start () {
		if (b == null) {
			b = GetComponent<Button> ();
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other){
		if (other.tag == "Pointer") {
			if (b != null) {
				Debug.Log ("Selecting");
				b.OnPointerClick (null);
			}
		}
	}
}
