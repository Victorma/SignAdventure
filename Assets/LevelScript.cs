using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelScript : MonoBehaviour {

	public Image starPrefab;

	public Text title;
	public Image lockLogo;
	public GameObject starHolder;

	public Level level;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(level == null)
			return;


		title.text = level.levelName;

		var locked = PlayerPrefs.GetString(level.levelName, "") == "";

		lockLogo.gameObject.SetActive(locked);

		if(!locked){
			level.score = PlayerPrefs.GetFloat(level.levelName + "_score", 0.0f);

			if(level.score == 0.0f) {
			
				starHolder.SetActive(false);

			}else{

				var starNum = Mathf.CeilToInt(level.score * 3);

				for(int i = 0; i<starNum; i++){

					var startInstance = GameObject.Instantiate(starPrefab);
					startInstance.transform.parent = starHolder.transform;

				}

			}
		}
	}
}
