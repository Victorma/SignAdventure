using UnityEngine;
using System.Collections.Generic;

public class LevelFiller : MonoBehaviour {

	public GameObject levelPrefab;

	public Level firstLevel;

	// Use this for initialization
	void Start () {

		List<Level> levels = new List<Level>();
		Queue<Level> toExpand = new Queue<Level>();

		toExpand.Enqueue(firstLevel);

		while(toExpand.Count > 0){

			var l = toExpand.Dequeue();

			levels.Add(l);

			foreach(var sublevel in l.unlocks)
				toExpand.Enqueue(sublevel);

		}

		foreach(var l in levels){

			var instance = GameObject.Instantiate(levelPrefab);
			instance.transform.parent = this.transform;
			instance.GetComponent<LevelScript>().level = l;

			var r = instance.GetComponent<RectTransform>();

			var pos = r.anchoredPosition3D;
			r.anchoredPosition3D = new Vector3(pos.x, pos.y, 0);

		}
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
