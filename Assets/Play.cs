using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Play : MonoBehaviour {

	public MenuReferences menu;

	public Level level;
	public GameObject floatingTitle;
	public Dictionary<Exercise, GameObject> floatingExercises = new Dictionary<Exercise, GameObject> ();
	public Transform center;

	public Tutorial tutorial;

	private bool training = false;
	public float maxScore = 0;

	// Use this for initialization
	void Start () {
		menu = GetComponentInParent<MenuReferences> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void StartTraining(){
		
		training = true;
		StartCoroutine (StartPlay ());

	}

	public void StartExam(){

	}

	private IEnumerator StartPlay(){

		foreach (var e in level.exercises) {

			GameObject floatingExercise = floatingExercises [e];

			FlyTo ft = floatingExercise.GetComponent<FlyTo> ();
			Transform exercisePos = ft.destination;

			ft.destination = center;

			yield return new WaitForSeconds (2f);

			tutorial.playing = true;

			yield return new WaitUntil (() => maxScore > 0.3f);

			// Save score
			// Give feedback

			maxScore = 0;

			ft.destination = exercisePos;
			ft.destinationColor = Color.green;
		}


	}

}
