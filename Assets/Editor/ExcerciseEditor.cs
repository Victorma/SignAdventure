using UnityEngine;
using UnityEditor;
using System.Collections;

public class ExcerciseEditor : Editor {

	[MenuItem("Assets/Create/Exercise")]
	public static Exercise CreateExerciseAsset(){

		ScriptableObject so = ScriptableObject.CreateInstance<Exercise> ();
		ProjectWindowUtil.CreateAsset(so, "New Excercise.asset");
		Selection.activeObject = so;  

		return so as Exercise;
	}

	[MenuItem("Assets/Create/Level")]
	public static Level CreateLevelAsset(){

		ScriptableObject so = ScriptableObject.CreateInstance<Level> ();
		ProjectWindowUtil.CreateAsset(so, "New Level.asset");
		Selection.activeObject = so;  

		return so as Level;
	}
}
