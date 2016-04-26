using UnityEngine;
using System.Collections;

public class Level : ScriptableObject {

	public string levelName;
	public Exercise[] exercises;
	[Range(0f,1f)]
	public float score; 
	public Level[] unlocks;


}
