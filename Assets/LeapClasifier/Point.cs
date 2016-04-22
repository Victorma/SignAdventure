using UnityEngine;

public struct Point {

	private Vector3 v;

	public float x { get{ return v.x; } set{ v.x = value; } }
	public float y { get{ return v.y; } set{ v.y = value; } }
	public float z { get{ return v.z; } set{ v.z = value; } }

	public int stroke;

	public static float Distance(Point p1, Point p2){
		return Vector3.Distance(p1.v, p2.v);
	}

	/**
	* A basic holding class for a 3D point. Note the final parameter, stroke, intended to indicate with which 
	* stroke in a multi-stroke gesture a point is associated - even if multi-stroke gestures are not yet supported 
	* by the framework.
	* 
	* @param x
	* @param y
	* @param z
	* @param stroke
	* @returns {LeapTrainer.Point}
	*/
	public Point(float x, float y, float z, int stroke) {

		this.v.x = x;
		this.v.y = y;
		this.v.z = z;

		this.stroke = stroke; // stroke ID to which this point belongs (1,2,...)
	}
}
