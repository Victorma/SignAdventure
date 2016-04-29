using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PageFiller : MonoBehaviour {

	private const string fullStar = "";
	private const string emptyStar = "";
	private const int numStars = 3;

    public MenuReferences menu;

    public GameObject exPrefab;

	public GameObject play;
	public GameObject playTitle;
    public GameObject playExercises;

    public GameObject exercisePrefab;

	public Level level;

	public Text title;
	public Image image;
	public Text score;
	public GameObject exercisesPanel;
	public Button train;
	public Button exam;

	private Level lastLevel;

	// Update is called once per frame
	void Update () {
		if (lastLevel != level) {
			lastLevel = level;

			title.text = level.levelName;
			image.sprite = null; // TODO Level.image


			// Convert score into stars
			string ts = "";
			float cs = level.score;
			while (ts.Length != numStars) {
				// .1f is the extra help to get all stars
				cs -= (1f - .1f) / (numStars * 1f);

				if (cs >= 0) ts += fullStar;
				else 		 ts += emptyStar;
			}

			score.text = ts;

			GameObject exins = null;

			foreach (var e in level.exercises) {
				exins = GameObject.Instantiate (exercisePrefab);
				exins.GetComponent<Text> ().text = e.exerciseName;
				exins.GetComponent<Button> ().onClick.AddListener(() => Debug.Log("trainExercise"));
				exins.transform.parent = exercisesPanel.transform;

				// instanciation fixes
				exins.transform.localRotation = Quaternion.Euler (0, 0, 0);
				exins.transform.localPosition = new Vector3 (0, 0, 0);
				exins.transform.localScale = new Vector3 (1, 1, 1);
			}

			train.onClick.RemoveAllListeners ();
			train.GetComponent<Button> ().onClick.AddListener(() => {

				StartCoroutine(StartLevel(true));
				Debug.Log("trainlevel");

			});

			exam.onClick.RemoveAllListeners ();
			exam.GetComponent<Button> ().onClick.AddListener(() => Debug.Log("examlevel"));
		}
	}

	private IEnumerator StartLevel(bool train){

		GameObject titleClone = GameObject.Instantiate (title.gameObject);
		titleClone.transform.parent = play.transform;
		titleClone.transform.position = title.gameObject.transform.position;
		titleClone.transform.rotation = title.gameObject.transform.rotation;
		titleClone.transform.localScale = title.transform.lossyScale;
		title.gameObject.SetActive (false);

		FlyTo ft = titleClone.AddComponent<FlyTo> ();
		ft.destination = playTitle.transform;
        ft.destinationColor = Color.white;
        ft.destinationScale = new Vector3(1.5f, 1.5f, 1.5f);

        ParticleSystem.EmissionModule emission = titleClone.GetComponent<ParticleSystem> ().emission;
		emission.enabled = true;


        int i = 0;
        foreach(var e in level.exercises)
        {
            yield return new WaitForSeconds(0.25f);

            GameObject destination = GameObject.Instantiate(exPrefab);
            destination.transform.parent = playExercises.transform;
            destination.transform.localPosition = new Vector3(0, 0, 0);
            destination.transform.localRotation = Quaternion.Euler(0, 0, 0);
            destination.transform.localScale = new Vector3(1, 1, 1);


            GameObject exerciseGO = exercisesPanel.transform.GetChild(i).gameObject;
            GameObject exerciseTitleClone = GameObject.Instantiate(exerciseGO);
            exerciseTitleClone.transform.parent = play.transform;
            exerciseTitleClone.transform.position = exerciseGO.gameObject.transform.position;
            exerciseTitleClone.transform.rotation = exerciseGO.gameObject.transform.rotation;
            exerciseTitleClone.transform.localScale = exerciseGO.transform.lossyScale;
            exerciseTitleClone.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

            exerciseGO.GetComponent<Text>().enabled = (false);

            FlyTo fly = exerciseTitleClone.AddComponent<FlyTo>();
            fly.destination = destination.transform;
            fly.destinationColor = Color.white;
            fly.destinationScale = new Vector3(1.5f, 1.5f, 1.5f);

            // ParticleSystem.EmissionModule emission = titleClone.GetComponent<ParticleSystem>().emission;
            // emission.enabled = true;
            i++;
        }


        menu.setMenu(4);

        yield return new WaitForEndOfFrame();
	}
}
