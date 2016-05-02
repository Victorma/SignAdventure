using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Leap;

public class Tutorial : MonoBehaviour {

	public bool playing;
	private bool previousPlaying;
	public Exercise gesture;

	public LeapRecorder player;
	public HandController controller;
	public UnityEngine.UI.Image teacherImage;

	public LeapTrainer trainer;
	private bool trained = false;

	// Use this for initialization
	void Start () {
		player = controller.GetLeapRecorder ();
		trainer = GetComponent<LeapTrainer> ();

		if (trainer != null) {
			trainer.OnEndedRecording 		+= () 						=> Debug.Log ("OnEndedRecording");
			trainer.OnGestureCreated 		+= (name, trainingSkipped) 	=> Debug.Log ("OnGestureCreated");
			trainer.OnGestureDetected 		+= (points, frameCount) 	=> Debug.Log ("OnGestureDetected");
			trainer.OnGestureRecognized 	+= (name, value, allHits) => {

				Debug.Log ("OnGestureRecognized");
				foreach(var v in allHits)
					Debug.Log(" ==> " + v.Key + " : " + v.Value);

			};
			trainer.OnGestureUnknown += (allHits) => {

				Debug.Log ("OnGestureUnknown");
				foreach(var v in allHits)
					Debug.Log(" ==> " + v.Key + " : " + v.Value);

			};
			trainer.OnStartedRecording 		+= () 						=> Debug.Log ("OnStartedRecording");
			trainer.OnTrainingComplete 		+= (name, gestures, isPose) => Debug.Log ("OnTrainingComplete");
			trainer.OnTrainingCountdown 	+= (countdown) 				=> Debug.Log ("OnTrainingCountdown");
			trainer.OnTrainingGestureSaved 	+= (name, gestures) 		=> Debug.Log ("OnTrainingGestureSaved");
			trainer.OnTrainingStarted 		+= (name) 					=> Debug.Log ("OnTrainingStarted");

		}
	}

	// Update is called once per frame
	void Update () {

		if (playing) {
			if (!previousPlaying) {
				controller.gameObject.SetActive (true);

				Debug.Log ("Loaded FrameCount: " + player.GetFramesCount ());
				player.Play ();
				player.loop = true;
			}

			teacherImage.gameObject.SetActive (true);
			teacherImage.sprite = gesture.teacherFrames [Mathf.FloorToInt (Time.time) % 2];

		} else {
			teacherImage.gameObject.SetActive (false);
			controller.gameObject.SetActive (false);
		}

		previousPlaying = playing;
	}

	public void UpdateGesture(){
		
		player.Load (gesture.recording);

		Debug.Log (player.GetFramesCount ());
		var newFrames = player.GetFrames ();
		newFrames.RemoveRange (player.GetFramesCount () - 200, 199);
		player.Reset ();
		foreach (var f in newFrames)
			player.AddFrame (f);

	}

	public List<Frame> GetFrames(){
		return player.GetFrames ();
	}
}
