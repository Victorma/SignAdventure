using UnityEngine;
using System.Collections;

public class AudioVolumeController : MonoBehaviour {

	public AnimationCurve curve;
	private AudioSource audios;
	// Use this for initialization
	void Start () {
		audios = this.GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (audios != null) {

			audios.volume = curve.Evaluate (audios.time / audios.clip.length);
		}
	}
}
