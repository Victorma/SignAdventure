using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Play : MonoBehaviour {

	public MenuReferences menu;

	public AudioSource yay;
	private Animator animator;
	private CanvasGroup group;

	public Level level;
	public GameObject floatingTitle;
	public GameObject wordsPositions;
	public Dictionary<Exercise, GameObject> floatingExercises = new Dictionary<Exercise, GameObject> ();
	public Transform center;
	public ResultsFiller exam;

	public Tutorial tutorial;
	public LeapTrainer trainer;

	private bool training = false;
	public float maxScore = 0;
	public float totalScore = 0;

	private int failCount = 0;

	private Animator tutoanimator;

	// Use this for initialization
	void Start () {
		menu = GetComponentInParent<MenuReferences> ();
		animator = GetComponent<Animator> ();
		animator.SetBool ("Show", true);
		tutoanimator = tutorial.GetComponent<Animator> ();
		group = GetComponent<CanvasGroup> ();

		trainer.OnGestureRecognized += (name, value, allHits) => {
			maxScore = value;
		};

		trainer.OnGestureUnknown += (allHits) => {
			failCount++;
			if(failCount == 15){

				tutorial.playing = true;
				tutoanimator.SetBool ("Show", true);
			}
		};

	}
	
	// Update is called once per frame
	void Update () {


		if (group.alpha == 0) {
			animator.SetBool ("Show", true);
			tutorial.playing = false;
			if(floatingTitle)
				DestroyImmediate (floatingTitle);
			foreach (var v in floatingExercises) {
				DestroyImmediate (v.Value);
			}
			for (int i = wordsPositions.transform.childCount-1; i >= 0; i--) {
				GameObject.DestroyImmediate (wordsPositions.transform.GetChild (i).gameObject);
			}
			floatingExercises.Clear ();
		}
	}

	public void StartTraining(){
		aborted = false;
		training = true;
		StartCoroutine (StartPlay ());

	}

	public void StartExam(){
		aborted = false;
		training = false;
		StartCoroutine (StartPlay ());
	}

	private bool aborted = false;

	public void Abort(){
		animator.SetBool ("Show", false);
		trainer.Clean ();
		trainer.paused = true;
		StopAllCoroutines ();

		tutorial.GetComponent<Animator> ().SetBool ("Show", false);
		exam.GetComponent<Animator> ().SetBool ("Show", false);
	}

	private IEnumerator StartPlay(){


		foreach (var e in level.exercises) {
			FlyTo ft = null;
			Transform exercisePos = null;
			try{
				GameObject floatingExercise = floatingExercises [e];

				ft = floatingExercise.GetComponent<FlyTo> ();
				exercisePos =  ft.destination;

				ft.destination = center;
			}catch(System.Exception){return false;}
			yield return new WaitForSeconds (2f);
			try{
				if (aborted)
					return false;

				tutorial.gesture = e;

				if (training) {
					tutorial.playing = true;
					tutorial.GetComponent<Animator> ().SetBool ("Show", true);
				}

				tutorial.UpdateGesture ();

			}catch(System.Exception){return false;}

			yield return new WaitForSeconds (1f);
			if (aborted)
				return false;
			try{
				trainer.Clean ();
				trainer.loadFromFrames (e.exerciseName, tutorial.GetFrames (), false);

				trainer.paused = false;
			}catch(System.Exception){return false;}

			yield return new WaitUntil (() => maxScore > 0.3f);
			if (aborted)
				return false;
		
			try{
				trainer.paused = true;

				if (!training && maxScore > e.score)
					e.score = maxScore;

				// Save score
				// Give feedback

				totalScore += maxScore;
				maxScore = 0;

				ft.destination = exercisePos;
				ft.destinationColor = Color.green;

				tutorial.GetComponent<Animator> ().SetBool ("Show", false);
			}catch(System.Exception){return false;}
		}

		yield return new WaitForSeconds (2f);
		tutorial.playing = false;

		try{
			if (!training) {
				yay.PlayDelayed (.5f);
				float score = 0;
				foreach (var e in level.exercises) {
					score += e.score;
				}
				score /= (level.exercises.Length * 1f);
				if (score > level.score)
					level.score = score;

				exam.level = level;
				exam.GetComponent<Animator> ().SetBool ("Show", true);
			} else {
				menu.Menu = 3;
				Abort ();
			}
		}catch(UnityException){}


	}

}
