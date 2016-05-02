using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Feedback : MonoBehaviour {

	public string best, high, medium, low, fail, nothing;

	public GameObject feedbackPrefab;

	public LeapTrainer trainer;

	private string text;
	private bool instanciateLater;

	// Use this for initialization
	void Start () {
		
		if (trainer != null) {
			trainer.OnEndedRecording 		+= () 						=> Debug.Log ("OnEndedRecording");
			trainer.OnGestureCreated 		+= (name, trainingSkipped) 	=> Debug.Log ("OnGestureCreated");
			trainer.OnGestureDetected 		+= (points, frameCount) 	=> Debug.Log ("OnGestureDetected");
			trainer.OnGestureRecognized 	+= (name, value, allHits) => {

				string text = nothing;
				Debug.Log ("OnGestureRecognized");
				foreach(var v in allHits){


					Debug.Log(" ==> " + v.Key + " : " + v.Value);
					if(v.Value > 0.2f){
						text = low;
					}
					if(v.Value > 0.5f){
						text = medium;
					}
					if(v.Value > 0.7f){
						text = high;
					}
					if(v.Value > 0.85f){
						text = best;
					}

				}

				instanciateLater = true;
				this.text = text;

			};
			trainer.OnGestureUnknown += (allHits) => {

				string text = nothing;

				Debug.Log ("OnGestureUnknown");
				foreach(var v in allHits){
					Debug.Log(" ==> " + v.Key + " : " + v.Value);
					if(v.Value > 0.1){
						text = fail;
					}
				}

				instanciateLater = true;
				this.text = text;
			};
			trainer.OnStartedRecording 		+= () 						=> Debug.Log ("OnStartedRecording");
			trainer.OnTrainingComplete 		+= (name, gestures, isPose) => Debug.Log ("OnTrainingComplete");
			trainer.OnTrainingCountdown 	+= (countdown) 				=> Debug.Log ("OnTrainingCountdown");
			trainer.OnTrainingGestureSaved 	+= (name, gestures) 		=> Debug.Log ("OnTrainingGestureSaved");
			trainer.OnTrainingStarted 		+= (name) 					=> Debug.Log ("OnTrainingStarted");

		}
	}

	void Update(){
		if (instanciateLater) {

			var i = GameObject.Instantiate(feedbackPrefab);
			i.GetComponent<Text>().text = text;
			i.transform.parent = this.transform;
			i.transform.position = this.transform.position;

			instanciateLater = false;
		}
	}
}
