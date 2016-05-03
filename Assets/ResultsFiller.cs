using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResultsFiller : MonoBehaviour {

	private const string fullStar = "";
	private const string emptyStar = "";
	private const int numStars = 3;

	public GameObject exercisePrefab;

	public Level level;
	private Level previousLevel;

	public Text textName;
	public GameObject exercises;
	public Text score;

	public Text unlocks;
	public Text unlocksTitle;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (level != previousLevel) {

			textName.text = level.levelName;
			PlayerPrefs.SetFloat(level.levelName + "_score", level.score);
			score.text = toStars (level.score);

			while (exercises.transform.childCount > 0) {
				GameObject.DestroyImmediate (exercises.transform.GetChild (0).gameObject);
			}

			foreach (var e in level.exercises) {
				var ei = GameObject.Instantiate (exercisePrefab);

				PlayerPrefs.SetFloat(e.exerciseName + "_ex_score", e.score);
				ei.transform.FindChild ("ExName").GetComponent<Text> ().text = e.exerciseName;
				ei.transform.FindChild ("ExScore").GetComponent<Text> ().text = toStars(e.score);

				ei.transform.parent = exercises.transform;
				ei.transform.localPosition = new Vector3 (0, 0, 0);
				ei.transform.localRotation = Quaternion.Euler(0, 0, 0);
				ei.transform.localScale = new Vector3 (1, 1, 1);
			}


			if (level.unlocks.Length > 0 && PlayerPrefs.GetInt (level.unlocks [0].levelName, 0) == 0) {
				// Unlock the level
				PlayerPrefs.SetInt (level.unlocks [0].levelName, 1);
				unlocks.gameObject.SetActive (true);
				unlocksTitle.gameObject.SetActive (true);
				unlocksTitle.text = level.unlocks [0].levelName;
			} else {
				unlocks.gameObject.SetActive (false);
				unlocksTitle.gameObject.SetActive (false);
			}

			previousLevel = level;
		}
	}

	private string toStars(float score){
		// Convert score into stars
		string ts = "";
		float cs = score;
		while (ts.Length != numStars) {
			// .1f is the extra help to get all stars
			cs -= (1f - .1f) / (numStars * 1f);

			if (cs >= 0) ts += fullStar;
			else 		 ts += emptyStar;
		}
		return ts;
	}
}
