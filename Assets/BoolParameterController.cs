using UnityEngine;
using System.Collections;

public class BoolParameterController : MonoBehaviour {

	private Animator animator;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();
	}
	
	public void SetParameterTrue(string parameter){
		animator.SetBool (parameter, true);
	}

	public void SetParameterFalse(string parameter){
		animator.SetBool (parameter, false);
	}
}
