using UnityEngine;
using System.Collections;

public class Destroyer : MonoBehaviour {

	void Destroy(){
		GameObject.Destroy (this.gameObject);
	}
}
