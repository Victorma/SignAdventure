using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelScript : MonoBehaviour {

	private const string fullStar = "";
	private const string emptyStar = "";
	private const int numStars = 3;

    public PageFiller pageFiller;
    public MenuReferences menu;

	public Text title;
	public Image lockLogo;
	public Text stars;

    public Button button;

	public Level level;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(level == null)
			return;


		title.text = level.levelName;

		var locked = PlayerPrefs.GetInt(level.levelName, 0) == 0;

		lockLogo.gameObject.SetActive(locked);

		if(!locked){
			level.score = PlayerPrefs.GetFloat(level.levelName + "_score", 0.0f);

            button.onClick.AddListener(() =>
            {
                pageFiller.level = this.level;
                menu.setMenu(3);

            });

			var starNum = Mathf.CeilToInt(level.score * 3);

			stars.text = level.score > 0 ? toStars (level.score) : "";

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
