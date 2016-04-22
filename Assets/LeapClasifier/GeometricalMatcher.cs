using UnityEngine;
using System.Collections.Generic;

public class GeometricalMatcher : TemplateMatcher {

	public int e = 0;

	/**
	* An implementation of the geometric template mathcing algorithm.
	*/
		
	int pointCount = 25; 			// Gestures are resampled to this number of points
	Point origin = new Point (0f, 0f, 0f, 0); // Gestures are translated to be centered on this point
		
	/**
	 * Prepares a recorded gesture for template matching - resampling, scaling, and translating the gesture to the 
	 * origin.  Gesture processing ensures that during recognition, apples are compared to apples - all gestures are the 
	 * same (resampled) length, have the same scale, and share a centroid.
	 * 
	 * @param gesture
	 * @returns
	 */
	public List<Point> process(List<Point> gesture) { 
			
		List<Point> points = new List<Point>();
		int stroke = 1;

		foreach (var v in gesture) {
			points.Add(new Point(v.x, v.y, v.z, stroke));
		}
		
		return this.translateTo(this.scale(this.resample(points, this.pointCount)), this.origin);	
	}
		
	/**
	 * This is the primary correlation function, called in the LeapTrainer.Controller above in order to compare an detected 
	 * gesture with known gestures.  
	 * 
	 * @param gesture
	 * @param trainingGesture
	 * @returns
	 */
	public float match (List<Point> gesture, List<Point> trainingGesture) {
			
		int l = gesture.Count;
		int step = Mathf.FloorToInt (Mathf.Pow (l, 1 - this.e));
		float min = float.PositiveInfinity;
		
		for (var i = 0; i < l; i += step) {
			
			min = Mathf.Min(min, Mathf.Min(this.gestureDistance(gesture, trainingGesture, i), this.gestureDistance(trainingGesture, gesture, i)));
		}
		
		return min;
	}
		
	/**
	 * Calculates the geometric distance between two gestures.
	 * 
	 * @param gesture1
	 * @param gesture2
	 * @param start
	 * @returns {Number}
	 */
	float gestureDistance (List<Point> gesture1, List<Point>gesture2, int start) {
			
		var p1l = gesture1.Count;
		
		var matched = new List<bool>(p1l);
		int i = start, index;
		float sum = 0, min, d;
		
		do {
			
			index = -1;
			min = float.NegativeInfinity;
			
			for (var j = 0; j < p1l; j++) {
				
				if (!matched[j]) {
					
					//if (gesture1[i] == null || gesture2[j] == null) { continue; }
					
					d = Point.Distance(gesture1[i], gesture2[j]);
					
					if (d < min) { min = d; index = j; }
				}
			}
			
			matched[index] = true;
			
			sum += (1 - ((i - start + p1l) % p1l) / p1l) * min;
			
			i = (i + 1) % p1l;
			
		} while (i != start);
		
		return sum;
	}
		
		/**
	 * Resamples a gesture in order to create gestures of homogenous lengths.  The second parameter indicates the length to 
	 * which to resample the gesture.
	 * 
	 * This function is used to homogenize the lengths of gestures, in order to make them more comparable. 
	 * 
	 * @param gesture
	 * @param newLength
	 * @returns {Array}
	 */
	List<Point> resample (List<Point> gesture, float newLength) {
			
		float target = newLength - 1f;

		Point p, pp, q;
		float interval = this.pathLength(gesture)/target, dist = 0.0f, d, ppx, ppy, ppz;
		List<Point> resampledGesture = new List<Point>();
			
		for (int i = 1, l = gesture.Count; i < l; i++) {
			
			p	= gesture[i];
			pp	= gesture[i - 1];
			
			if (p.stroke == pp.stroke) {
				
				d = Point.Distance(pp, p);
				
				if ((dist + d) >= interval) {
					
					ppx = pp.x;
					ppy = pp.y;
					ppz = pp.z;
					
					q = new Point((ppx + ((interval - dist) / d) * (p.x - ppx)), 
		                          (ppy + ((interval - dist) / d) * (p.y - ppy)),
		                          (ppz + ((interval - dist) / d) * (p.z - ppz)), p.stroke);
					
					resampledGesture.Add(q);
					
					gesture.Insert(i, q);
					
					dist = 0.0f;
					
				} else { 
					
					dist += d;
				}
			}
		}
			
		/*
		 * Rounding errors will occur short of adding the last point - in which case the array is padded by 
		 * duplicating the last point
		 */
		if (resampledGesture.Count != target) {
			
			p = gesture[gesture.Count - 1];
			
			resampledGesture.Add(new Point(p.x, p.y, p.z, p.stroke));
		}
		
		return resampledGesture;
	}
		
	/**
	 * Scales gestures to homogenous variances in order to provide for detection of the same gesture at different scales.
	 * 
	 * @param gesture
	 * @returns {Array}
	 */
	List<Point> scale(List<Point> gesture) {

		float minX = float.PositiveInfinity, 
			  maxX = float.NegativeInfinity, 
			  minY = float.PositiveInfinity, 
			  maxY = float.NegativeInfinity,
			  minZ = float.PositiveInfinity, 
			  maxZ = float.NegativeInfinity,
			  x, y, z;

		int l = gesture.Count;
		Point g;
			
		for (int i = 0; i < l; i++) {
			
			g = gesture[i];
			
			x = g.x;
			y = g.y;
			z = g.z;
			
			minX = Mathf.Min(minX, x);
			minY = Mathf.Min(minY, y);
			minZ = Mathf.Min(minZ, z);
			
			maxX = Mathf.Max(maxX, x);
			maxY = Mathf.Max(maxY, y);
			maxZ = Mathf.Max(maxZ, z);
		}
		
		var size = Mathf.Max(maxX - minX, maxY - minY, maxZ - minZ);
		
		for (int i = 0; i < l; i++) {
			
			g = gesture[i];
			
			gesture[i] = new Point((g.x - minX)/size, (g.y - minY)/size, (g.z - minZ)/size, g.stroke);
		}
		
		return gesture;
	}
		
	/**
	 * Translates a gesture to the provided centroid.  This function is used to map all gestures to the 
	 * origin, in order to recognize gestures that are the same, but occurring at at different point in space.
	 * 
	 * @param gesture
	 * @param centroid
	 * @returns {Array}
	 */
	List<Point> translateTo(List<Point> gesture, Point centroid) {
			
		Point center = this.centroid(gesture);
		
		for (int i = 0, l = gesture.Count; i < l; i++) {

			float x = gesture[i].x + centroid.x - center.x,
				  y = gesture[i].y + centroid.y - center.y,
				  z = gesture[i].z + centroid.z - center.z;

			gesture[i] = new Point(x,y,z, gesture[i].stroke);
		}
		
		return gesture;
	}
		
	/**
	 * Finds the center of a gesture by averaging the X and Y coordinates of all points in the gesture data.
	 * 
	 * @param gesture
	 * @returns {LeapTrainer.Point}
	 */
	Point centroid(List<Point> gesture) {
			
		float x = 0.0f, y = 0.0f, z = 0.0f, l = gesture.Count;
		
		foreach (Point g in gesture) {
			
			x += g.x;
			y += g.y;
			z += g.z;
		}
		
		return new Point(x/l, y/l, z/l, 0);
	}
		
	/**
	 * Calculates the average distance between corresponding points in two gestures
	 * 
	 * @param gesture1
	 * @param gesture2
	 * @returns {Number}
	 */
	float pathDistance(List<Point> gesture1, List<Point> gesture2) {
			
		float d = 0.0f;
		int l = gesture1.Count;
		
		/*
		 * Note that resampling has ensured that the two gestures are both the same length
		 */
		for (var i = 0; i < l; i++) { d += Point.Distance(gesture1[i], gesture2[i]); }
		
		return d/l;
	}
		
	/**
	 * Calculates the length traversed by a single point in a gesture
	 * 
	 * @param gesture
	 * @returns {Number}
	 */
	float pathLength (List<Point> gesture) {
			
		float d = 0.0f;
		Point g, gg;
		
		for (int i = 1, l = gesture.Count; i < l; i++) {
			
			g	= gesture[i];
			gg 	= gesture[i - 1];
			
			if (g.stroke == gg.stroke) { d += Point.Distance(gg, g); }
		}
		
		return d;
	}
}
