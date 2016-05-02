using UnityEngine;
using System.Collections;

public class HandsFader : MonoBehaviour {

	CanvasGroup g;

	// Use this for initialization
	void Start () {
		g = GetComponent<CanvasGroup> ();
	}
	
	// Update is called once per frame
	void Update () {
	
		Renderer[] renderers = this.GetComponentsInChildren<Renderer> ();

		foreach (Renderer r in renderers) {
			Material m = r.material;

			m.SetColor("_Color", new Color(1,1,1, g.alpha * .65f));
			m.SetColor("_OutlineColor", new Color(0,0,0, g.alpha * .65f));

			r.material = m;
		}

	}
}
