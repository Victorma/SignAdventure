using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class OpenLinkScript : MonoBehaviour
{
	public String link;

	public void Launch ()
	{
		Application.OpenURL (link);
	}
}


