using UnityEngine;
using System.Collections.Generic;

public interface TemplateMatcher  {

	List<Point> process(List<Point> gesture);
	float match (List<Point> gesture, List<Point> trainingGesture);

}
