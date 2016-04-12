using UnityEngine;
using System.Collections;

public class SoftMovement : MonoBehaviour {

	public float timeElapsed = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		float spin = 30f * Mathf.Sin (timeElapsed);

		timeElapsed += Time.deltaTime;

		transform.localRotation = Quaternion.Euler (0, 0, spin);

	}
}
