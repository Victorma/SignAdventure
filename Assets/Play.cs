using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Play : MonoBehaviour {

	public MenuReferences menu;

	private Animator animator;
	private CanvasGroup group;

	public Level level;
	public GameObject floatingTitle;
	public Dictionary<Exercise, GameObject> floatingExercises = new Dictionary<Exercise, GameObject> ();
	public Transform center;

	public Tutorial tutorial;
	public LeapTrainer trainer;

	private bool training = false;
	public float maxScore = 0;
	public float totalScore = 0;

	private int failCount = 0;

	// Use this for initialization
	void Start () {
		menu = GetComponentInParent<MenuReferences> ();
		animator = GetComponent<Animator> ();
		animator.SetBool ("Show", true);
		group = GetComponent<CanvasGroup> ();
	}
	
	// Update is called once per frame
	void Update () {
		trainer.OnGestureRecognized += (name, value, allHits) => {
			maxScore = value;
		};

		trainer.OnGestureUnknown += (allHits) => {
			failCount++;
			if(failCount == 15){

				tutorial.playing = true;
				tutorial.GetComponent<Animator> ().SetBool ("Show", true);
			}
		};

		if (group.alpha == 0) {
			animator.SetBool ("Show", true);
			tutorial.playing = false;
			if(floatingTitle)
				DestroyImmediate (floatingTitle);
			foreach (var v in floatingExercises) {
				DestroyImmediate (v.Value);
			}
			floatingExercises.Clear ();
		}
	}

	public void StartTraining(){
		
		training = true;
		StartCoroutine (StartPlay ());

	}

	public void StartExam(){

	}

	public void Abort(){
		animator.SetBool ("Show", false);
		trainer.Clean ();
		trainer.paused = true;
		tutorial.GetComponent<Animator> ().SetBool ("Show", false);
		StopAllCoroutines ();
	}

	private IEnumerator StartPlay(){


		foreach (var e in level.exercises) {

			GameObject floatingExercise = floatingExercises [e];

			FlyTo ft = floatingExercise.GetComponent<FlyTo> ();
			Transform exercisePos = ft.destination;

			ft.destination = center;
			yield return new WaitForSeconds (2f);

			tutorial.gesture = e;

			if (training) {
				tutorial.playing = true;
				tutorial.GetComponent<Animator> ().SetBool ("Show", true);
			}

			tutorial.UpdateGesture ();

			yield return new WaitForSeconds (1f);

			trainer.Clean ();
			trainer.loadFromFrames (e.exerciseName, tutorial.GetFrames (), false);

			trainer.paused = false;

			yield return new WaitUntil (() => maxScore > 0.3f);

			trainer.paused = true;

			// Save score
			// Give feedback

			totalScore += maxScore;
			maxScore = 0;

			ft.destination = exercisePos;
			ft.destinationColor = Color.green;

			tutorial.playing = false;
			tutorial.GetComponent<Animator> ().SetBool ("Show", false);
		}

		if (!training) {

		}

	}

}
