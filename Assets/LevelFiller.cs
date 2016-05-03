using UnityEngine;
using System.Collections.Generic;

public class LevelFiller : MonoBehaviour {

    public PageFiller pageFiller;
    public MenuReferences menu;

    public GameObject levelPrefab;

	public Level firstLevel;
    public Level previousValue;

	public void Refresh(){
		previousValue = null;
	}


    // Use this for initialization
    void Start () {


	
	}
	
	// Update is called once per frame
	void Update () {
        if(previousValue != firstLevel)
        {
            previousValue = firstLevel;

            while(this.transform.childCount > 0)
            {
                GameObject.DestroyImmediate(this.transform.GetChild(0).gameObject);
            }

            List<Level> levels = new List<Level>();
            Queue<Level> toExpand = new Queue<Level>();

            toExpand.Enqueue(firstLevel);

            while (toExpand.Count > 0)
            {

                var l = toExpand.Dequeue();

                levels.Add(l);

                foreach (var sublevel in l.unlocks)
                    toExpand.Enqueue(sublevel);

            }

            foreach (var l in levels)
            {

                var instance = GameObject.Instantiate(levelPrefab);
                instance.transform.parent = this.transform;

                var ls = instance.GetComponent<LevelScript>();
                ls.level = l;
                ls.menu = menu;
                ls.pageFiller = pageFiller;

                var r = instance.GetComponent<RectTransform>();

                var pos = r.anchoredPosition3D;
                r.anchoredPosition3D = new Vector3(pos.x, pos.y, 0);

            }
        }
    }
}
