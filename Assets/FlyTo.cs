using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FlyTo : MonoBehaviour {

	public float randomness = 1f;
	public float resistance = .98f;
	public float speedCap = 2f;

	private Vector3 direction;
	public Transform destination;

    public Color destinationColor;
    public Vector3 destinationScale;

	public float speed = 0.1f;
	public float distanceToEnd = 0f;

	// Use this for initialization
	void Start () {

		direction = Vector3.back;

	}
	
	// Update is called once per frame
	void Update () {

		Vector3 desiredDirection = destination.position - transform.position;
		distanceToEnd = desiredDirection.magnitude;
		if (desiredDirection.magnitude < 0.1f && speed < 0.1f)
			return;
		else if (desiredDirection.magnitude > 0.1f && speed < 0.1f) {
			speed = 0.1f;
		}

		direction = Vector3.Lerp (desiredDirection.normalized, direction, Mathf.Lerp(1f, resistance, speed / speedCap));

		speed *= 1.01f;
		if (speed > speedCap) {
			speed = speedCap;
		}

		speed = Mathf.Lerp (0f, speed, Mathf.Clamp01 (desiredDirection.magnitude * 2f));
        this.transform.localScale = Vector3.Lerp(destinationScale, this.transform.localScale, Mathf.Clamp01(desiredDirection.magnitude / 200f));
        this.GetComponent<Text>().color = Color.Lerp(destinationColor, this.GetComponent<Text>().color, Mathf.Clamp01(desiredDirection.magnitude / 200f));
        speed = Mathf.Lerp(0f, speed, Mathf.Clamp01(desiredDirection.magnitude * 2f));

        this.transform.position = this.transform.position + direction * speed;
	}
}
