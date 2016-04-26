using UnityEngine;
using Leap;
using System;
using System.Collections;
using System.Collections.Generic;

/**
 * Now we get to defining the base LeapTrainer Controller.  This class contains the default implementations of all functions.
 * 
 * The constructor accepts an options parameter, which is then passed to the initialize in order to set up the object.
 * 
 */
public class LeapTrainer : MonoBehaviour {

	public HandController handController;

	/**
	 * Events Adapted to C#
	 */

	public delegate void StartedRecordingDelegate();
	public delegate void EndedRecordingDelegate();
	public delegate void GestureDetectedDelegate(List<Point> points, int frameCount);
	public delegate void GestureCreatedDelegate(string name, bool trainingSkipped);
	public delegate void GestureRecognizedDelegate(string name, float value, Dictionary<string,float> allHits);
	public delegate void GestureUnknownDelegate(Dictionary<string,float> allHits);
	public delegate void TrainingCountdownDelegate(int countdown);
	public delegate void TrainingStartedDelegate(string name);
	public delegate void TrainingCompleteDelegate(string name, List<List<Point>> gestures, bool isPose);
	public delegate void TrainingGestureSavedDelegate(string name, List<List<Point>> gestures);

	public event StartedRecordingDelegate 		OnStartedRecording;
	public event EndedRecordingDelegate 		OnEndedRecording;
	public event GestureDetectedDelegate 		OnGestureDetected;
	public event GestureCreatedDelegate 		OnGestureCreated;
	public event GestureRecognizedDelegate 		OnGestureRecognized;
	public event GestureUnknownDelegate 		OnGestureUnknown;
	public event TrainingCountdownDelegate 		OnTrainingCountdown;
	public event TrainingStartedDelegate 		OnTrainingStarted;
	public event TrainingCompleteDelegate 		OnTrainingComplete;
	public event TrainingGestureSavedDelegate 	OnTrainingGestureSaved;
	
	/**
	 * Attribs
	 */
		
	private TemplateMatcher templateMatcher;

	private Controller controller = null;	// An instance of Leap.Controller from the leap.js library.  This will be created if not passed as an option
	private TrainerListener listener = null;

	bool pauseOnWindowBlur = false; // If this is TRUE, then recording and recognition are paused when the window loses the focus, and restarted when it's regained
		
	public float minRecordingVelocity = 300f; // The minimum velocity a frame needs to clock in at to trigger gesture recording, or below to stop gesture recording (by default)
	public float maxRecordingVelocity = 30f;  // The maximum velocity a frame can measure at and still trigger pose recording, or above which to stop pose recording (by default)
		
	public int minGestureFrames = 5;	// The minimum number of recorded frames considered as possibly containing a recognisable gesture 
	public int minPoseFrames = 75;	    // The minimum number of frames that need to hit as recordable before pose recording is actually triggered
	public int frameCount = 0; 		// The actual frame count	

	int recordedPoseFrames = 0;	// A counter for recording how many pose frames have been recorded before triggering
	bool recordingPose = false; // A flag to indicate if a pose is currently being recorded
	bool recording = false; 	// Variable to know if there is recording a gesture
		
	public float hitThreshold = 0.65f;	// The correlation output value above which a gesture is considered recognized. Raise this to make matching more strict
		
	int trainingCountdown = 3;	// The number of seconds after startTraining is called that training begins. This number of 'training-countdown' events will be emit.
	int trainingGestures = 1;	// The number of gestures samples that collected during training
	int convolutionFactor = 0;	// The factor by which training samples will be convolved over a gaussian distribution to expand the available training data
		
	public float downtime = 1000f; // The number of milliseconds after a gesture is identified before another gesture recording cycle can begin
	public float lastHit = 0;	 // The timestamp at which the last gesture was identified (recognized or not), used when calculating downtime
		
	private Dictionary<string, List<List<Point>>> gestures = new Dictionary<string, List<List<Point>>>(); // The current set of recorded gestures - names mapped to convolved training data
	private Dictionary<string, bool> poses = new Dictionary<string, bool>();	  // Though all gesture data is stored in the gestures object, here we hold flags indicating which gestures were recorded as poses
	private List<Point> gesture = null;				      	      // Actual recording gesture
		
	string trainingGesture	= ""; // The name of the gesture currently being trained, or null if training is not active
	//listeners				= {};	// Listeners registered to receive events emit from the trainer - event names mapped to arrays of listener functions
		
	public bool paused	= false; // This variable is set by the pause() method and unset by the resume() method - when true it disables frame monitoring temporarily.
		
	//renderableGesture = null; // Implementations that record a gestures for graphical rendering should store the data for the last detected gesture in this array.
		
	/**
	 * The controller initialization function - this is called just after a new instance of the controller is created to parse the options array, 
	 * connect to the Leap Motion device (unless an existing Leap.Controller object was passed as a parameter), and register a frame listener with 
	 * the leap controller.
	 * 
	 * @param options
	 */
	void Start() {
			
		/*
		 * The options array overrides all parts of this object - so any of the values above or any function below can be overridden by passing it as a parameter.
		 */
		//if (options) { for (var optionName in options) { if (options.hasOwnProperty(optionName)) { this[optionName] = options[optionName]; };};}
			
		/*
		 * The current DEFAULT recognition algorithm is geometric template matching - which is initialized here.
		 */
		this.templateMatcher = new GeometricalMatcher();
			
		/*
		 * Getting Leap.Controller reference from the hand controller.
		 */
		this.controller = handController.GetLeapController ();

		/*
		 * The bindFrameListener attaches a function to the leap controller frame event below.
		 */
		this.bindFrameListener();
			

	}

	private float time;
	void Update(){
		time = Time.time;
	}

	private class TrainerListener : Listener {

		private LeapTrainer lt;
		private Controller c;

		public TrainerListener(LeapTrainer lt, Controller c){
			this.lt = lt;
			this.c = c;
			c.AddListener(this);
		}

		public void Release(){
			c.RemoveListener (this);
		}

		public override void OnFrame (Controller arg0)
		{
			lt.onFrame (arg0.Frame ());
		}

	}
		
	/**
	 * The onFrame function is defined below in the bindFrameListener function in order to allow locally scoped variables be 
	 * defined for use on each frame.
	 */
	private void onFrame(Frame frame) {		
		
		/*
 		 * The pause() and resume() methods can be used to temporarily disable frame monitoring.
 		 */
		if (this.paused) { return; }
		
		/*
 		 * Frames are ignored if they occur too soon after a gesture was recognized.
 		 */
		if (this.time - this.lastHit < this.downtime) { return; }
		
		/*
		 * The recordableFrame function returns true or false - by default based on the overall velocity of the hands and pointables in the frame.  
		 * 
		 * If it returns true recording should either start, or the current frame should be added to the existing recording.  
		 * 
		 * If it returns false AND we're currently recording, then gesture recording has completed and the recognition function should be 
		 * called to see what it can do with the collected frames.
		 * 
		 */

		if (this.recordableFrame (frame, this.minRecordingVelocity, this.maxRecordingVelocity)) {
			
			/*
			 * If this is the first frame in a gesture, we clean up some running values and fire the 'started-recording' event.
			 */
			if (!recording) { 
				
				recording 				= true; 
				frameCount 				= 0; 
				gesture 				= new List<Point>(); 
				this.recordedPoseFrames = 0;
				
				if(OnStartedRecording != null) OnStartedRecording();
			}
			
			/*
			 * We count the number of frames recorded in a gesture in order to check that the 
			 * frame count is greater than minGestureFrames when recording is complete.
			 */
			frameCount++;
			
			/*
			 * The recordFrame function may be overridden, but in any case it's passed the current frame, the previous frame, and 
			 * utility functions for adding vectors and individual values to the recorded gesture activity.
			 */
			this.recordFrame(frame, this.controller.Frame());
			
			/*
		     * Since renderable frame data is not necessarily the same as frame data used for recognition, a renderable frame will be 
			 * recorded here IF the implementation provides one.
			 */
			//this.recordRenderableFrame(frame, this.controller.Frame(1));
			
		} else if (recording) {

			/*
			 * If the frame should not be recorded but recording was active, then we deactivate recording and check to see if enough 
			 * frames have been recorded to qualify for gesture recognition.
			 */
			recording = false;
			
			/*
			 * As soon as we're no longer recording, we fire the 'stopped-recording' event
			 */
			if(OnEndedRecording != null) OnEndedRecording();
			
			if (this.recordingPose || frameCount >= this.minGestureFrames) {
				
				/*
				 * If a valid gesture was detected the 'gesture-detected' event fires, regardless of whether the gesture will be recognized or not.
				 */
				if(OnGestureDetected != null) OnGestureDetected(gesture, frameCount);
				
				/*
				 * Finally we pass the recorded gesture frames to either the saveTrainingGesture or recognize functions (either of which may also 
				 * be overridden) depending on whether we're currently training a gesture or not.
				 * the time of the last hit.
				 */

				if (trainingGesture != "") { 
					//this.saveTrainingGesture(trainingGesture, gesture, this.recordingPose);
				} else { 
					this.recognize(gesture, frameCount); 
				}
				
				this.lastHit = this.time;
			}
		}
	}

	/*
	 * These two utility functions are used to push a vector (a 3-variable array of numbers) into the gesture array - which is the 
	 * array used to store activity in a gesture during recording. NaNs are replaced with 0.0, though they shouldn't occur!
	 */

	private void recordVector (Vector v) { 
		this.gesture.Add (new Point (v.x, v.y, v.z, 0));
	}
	
	/**
 	 * This function binds a listener to the Leap.Controller frame event in order to monitor activity coming from the device.
 	 * 
 	 * This bound frame listener function fires the 'gesture-detected', 'started-recording', and 'stopped-recording' events.
	 * 
	 */
	private void bindFrameListener() {

		/**
	 	 * This is the frame listening function, which will be called by the Leap.Controller on every frame.
	 	 */
		listener = new TrainerListener (this, controller);
			
		/*
		 * If pauseOnWindowBlur is true, then we bind the pause function to the controller blur event and the resume 
		 * function to the controller focus event
		 */
		/*if (this.pauseOnWindowBlur) {
			this.controller.on("blur",	this.pause.bind(this));
			this.controller.on("focus",	this.resume.bind(this)); 			
		}*/
	}
		
	/**
	 * This function returns TRUE if the provided frame should trigger recording and FALSE if it should stop recording.  
	 * 
	 * Of course, if the system isn't already recording, returning FALSE does nothing, and vice versa.. So really it returns 
	 * whether or not a frame may possibly be part of a gesture.
	 * 
	 * By default this function makes its decision based on one or more hands or fingers in the frame moving faster than the 
	 * configured minRecordingVelocity, which is provided as a second parameter.
	 * 
	 * @param frame
	 * @param min
	 * @returns {Boolean}
	 */
	private bool recordableFrame(Frame frame, float min, float max) {
			
		float palmVelocity, tipVelocity;
		bool poseRecordable = false;
		
		foreach (var hand in frame.Hands) {

			palmVelocity = hand.PalmVelocity.Magnitude;
			
			/*
			 * We return true if there is a hand moving above the minimum recording velocity
			 */
			if (palmVelocity >= min) { return true; }
			if (palmVelocity <= max) { poseRecordable = true; break; }
			
			foreach (var finger in hand.Fingers) {
				
				tipVelocity = finger.TipVelocity.Magnitude;
				
				/*
				 * Or if there's a finger tip moving above the minimum recording velocity
				 */
				if (tipVelocity >= min) { return true; }
				if (tipVelocity <= max) { poseRecordable = true; break; }
			}	
		}
		
		/*
		 * A configurable number of frames have to hit as pose recordable before actual recording is triggered.
		 */
		if (poseRecordable) {
			
			this.recordedPoseFrames++;
			
			if (this.recordedPoseFrames >= this.minPoseFrames) {

				this.recordingPose = true;
				return true;
			}
			
		} else {
			this.recordedPoseFrames = 0;
		}

		return false;
	}
		
	/**
	 * This function is called for each frame during gesture recording, and it is responsible for adding values in frames using the provided 
	 * recordVector and recordValue functions (which accept a 3-value numeric array and a single numeric value respectively).
	 * 
	 * This function should be overridden to modify the quality and quantity of data recorded for gesture recognition.
	 * 
	 * @param frame
	 * @param lastFrame
	 * @param recordVector
	 * @param recordValue
	 */
	private void recordFrame(Frame frame, Frame lastFrame) {
		
		foreach(var hand in frame.Hands) {
			
			recordVector(hand.StabilizedPalmPosition);
			
			foreach (var finger in hand.Fingers) {

				recordVector(finger.StabilizedTipPosition);	
			}
		}
	}
		
	/**
	 * This function records a single frame in a format suited for graphical rendering.  Since the recordFrame function will capture 
	 * data suitable for whatever recognition algorithm is implemented, that data is not necessarily relating to geometric positioning 
	 * of detected hands and fingers.  Consequently, this function should capture this geometric data.
	 * 
	 * Currently, only the last recorded gesture is stored - so this function should just write to the renderableGesture array.
	 * 
	 * Any format can be used - but the format expected by the LeapTrainer UI is - for each hand:
	 * 
	 * 	{	position: 	[x, y, z], 
	 * 	 	direction: 	[x, y, z], 
	 * 	 	palmNormal	[x, y, z], 
	 * 
	 * 		fingers: 	[ { position: [x, y, z], direction: [x, y, z], length: q },
	 * 					  { position: [x, y, z], direction: [x, y, z], length: q },
	 * 					  ... ]
	 *  }
	 *  
	 *  So a frame containing two hands would push an array with two objects like that above into the renderableGesture array.
	 * 
	 * @param frame
	 * @param lastFrame
	 * @param recordVector
	 * @param recordValue
	 */
	/*void recordRenderableFrame(Frame frame, Frame lastFrame) {
			
		var frameData = [];
		
		var hands		= frame.hands;
		var handCount 	= hands.length;
		
		var hand, finger, fingers, fingerCount, handData, fingersData;
		
		for (var i = 0, l = handCount; i < l; i++) {
			
			hand = hands[i];
			
			handData = {position: hand.stabilizedPalmPosition, direction: hand.direction, palmNormal: hand.palmNormal};
			
			fingers 	= hand.fingers;
			fingerCount = fingers.length;
			
			fingersData = [];
			
			for (var j = 0, k = fingerCount; j < k; j++) {
				
				finger = fingers[j];
				
				fingersData.push({position: finger.stabilizedTipPosition, direction: finger.direction, length: finger.length});
			}
			
			handData.fingers = fingersData;
			
			frameData.push(handData);
		}
		
		this.renderableGesture.push(frameData);
	}*/
		
	/**
	 * This function is called to create a new gesture, and - normally - trigger training for that gesture.  
	 * 
	 * The parameter gesture name is added to the gestures array and unless the trainLater parameter is present, the startRecording 
	 * function below is triggered.
	 * 
	 * This function fires the 'gesture-created' event.
	 * 
	 * @param gestureName
	 * @param trainLater
	 */
	public void Create(string gestureName, bool skipTraining) {
			
		this.gestures.Add(gestureName, new List<List<Point>>());
		
		if(OnGestureCreated != null)
			OnGestureCreated(gestureName, skipTraining);
		
		if (!skipTraining) { 
			StartCoroutine(StartTraining(gestureName, this.trainingCountdown)); 
		}
	}
		
	/**
	 * This function sets the object-level trainingGesture variable. This modifies what happens when a gesture is detected 
	 * by determining whether we save it as a training gesture or attempting to recognize it.
	 * 
	 * Since training actually starts after a countdown, this function will recur a number of times before the framework enters 
	 * training mode.  Each time it recurs it emits a 'training-countdown' event with the number of recursions still to go.  Consequently, 
	 * this function is normally initially called by passing this.trainingCountdown as the second parameter.
	 * 
	 * This function fires the 'training-started' and 'training-countdown' events.
	 * 
	 * @param gestureName
	 * @param countdown
	 */
	IEnumerator StartTraining(string gestureName, int countdown) { 
			
		if(OnTrainingCountdown != null)
			OnTrainingCountdown(countdown);

		this.pause ();

		yield return new WaitForSeconds (countdown);
		
		this.trainingGesture = gestureName; 
		StartCoroutine(StartTraining(gestureName, countdown)); 
		
		this.resume();

		if(OnTrainingStarted != null)
			OnTrainingStarted(gestureName);

	}
		
	/**
	 * Deletes the set of training gestures associated with the provided gesture name, and re-enters training mode for that gesture. 
	 * 
	 * If the provided name is unknown, then this function will return FALSE.  Otherwise it will call the 
	 * startTraining function (resulting in a 'training-started' event being fired) and return TRUE.
	 * 
	 * @param gestureName
	 * @returns {Boolean}
	 */
	public bool Retrain(string gestureName) { 
			
		if (this.gestures.ContainsKey(gestureName)) {
			
			this.gestures[gestureName].Clear();
			StartCoroutine(StartTraining(gestureName, this.trainingCountdown));
			
			return true;
		}
		
		return false;
	}
		
	/**
	 * For recognition algorithms that need a training operation after training data is gathered, but before the 
	 * gesture can be recognized, this function can be implemented and will be called in the 'saveTrainingGesture' function 
	 * below when training data has been collected for a new gesture.
	 * 
	 * The current DEFAULT implementation of this function calls a LeapTrainer.TemplateMatcher in order to process the saved 
	 * gesture data in preparation for matching.
	 * 
	 * Sub-classes that implement different recognition algorithms SHOULD override this function.
	 * 
	 * @param gestureName
	 * @param trainingGestures
	 */
	void trainAlgorithm(string gestureName, List<List<Point>> trainingGestures) {
			
		for (int i = 0, l = trainingGestures.Count; i < l; i++) { 
			trainingGestures[i] = this.templateMatcher.process(trainingGestures[i]);
		}		
	}

	public void loadFromFrames(string gestureName, List<Frame> frames, bool isPose){
		
		List<Point> bcgesture = gesture;
		gesture = new List<Point> ();

		Frame lf = null;
		foreach (var f in frames) {
			if (lf == null) lf = f;
			else this.recordFrame (f, lf);
		}
		
		saveTrainingGesture (gestureName, gesture, isPose);

		gesture = bcgesture;
	}
		
	/**
	 * The saveTrainingGesture function records a single training gesture.  If the number of saved training gestures has reached 
	 * 'trainingGestures', the training is complete and the system switches back out of training mode.
	 * 
	 * This function fires the 'training-complete' and 'training-gesture-saved' events.
	 * 
	 * @param gestureName
	 * @param gesture
	 */
	void saveTrainingGesture(string gestureName, List<Point> gesture, bool isPose) {
			
		/*
	 	* We retrieve all gestures recorded for this gesture name so far
	 	*/

		List<List<Point>> trainingGestures = null;
		
		if (!this.gestures.TryGetValue (gestureName, out trainingGestures)) {
			trainingGestures = new List<List<Point>> ();
		}
		

		/*
		 * Save the newly recorded gesture data
		 */
		trainingGestures.Add(gesture);
		
		/*
		 * And check if we have enough saved gestures to complete training
		 */
		if (trainingGestures.Count == this.trainingGestures) { 
			
			/*
			 * We expand the training data by generating a gaussian normalized distribution around the input.  This increases the 
			 * number of training gestures used during recognition, without demanding more training samples from the user.
			 */
			this.gestures[gestureName] = this.distribute(trainingGestures);
			
			/*
			 * Whether or not the gesture was recorded as a pose is stored
			 */
			this.poses[gestureName] = isPose;
			
			/*
			 * Setting the trainingGesture variable back to NULL ensures that the system will attempt to recognize subsequent gestures 
			 * rather than save them as training data.
			 */
			this.trainingGesture = "";
			
			/*
			 * The trainAlgorithm function provides an opportunity for machine learning recognition systems to train themselves on 
			 * the full training data set before the training cycle completes.
			 */
			this.trainAlgorithm(gestureName, trainingGestures);
			
			/*
			 * Finally we fire the 'training-complete' event.
			 */
			if(OnTrainingComplete != null)
				OnTrainingComplete(gestureName, trainingGestures, isPose);
			
		} else { 
			/*
			 * If more training gestures are required we just fire the 'training-gesture-saved' event.
			 */
			if(OnTrainingGestureSaved != null)
				OnTrainingGestureSaved(gestureName, trainingGestures);
		}
	}
		
	private float distributeAux(float f){
		return Mathf.Round((UnityEngine.Random.Range(0,1)*2f - 1f) + 
							(UnityEngine.Random.Range(0,1)*2f - 1f) + 
							(UnityEngine.Random.Range(0,1)*2f - 1f) * 
		                   ((f * 10000f) / 50f) + (f * 10000f)) / 10000f;
	}

	/**
	 * This function generates a normalized distribution of values around a set of recorded training gestures.  The objective of 
	 * this function is to increase the size of the training data without actually requiring the user to perform more training 
	 * gestures.
	 * 
	 * This implementation generates a gaussian normalized distribution.
	 * 
	 * @param trainingGestures
	 * @returns
	 */
	List<List<Point>> distribute(List<List<Point>> trainingGestures) {
			
		var factor = this.convolutionFactor;
		
		/*
		 * If the convolutionFactor is set to zero no distribution is generation.
		 */
		if (factor == 0) { return trainingGestures; }
		
		List<Point> generatedGesture;
		
		/*
		 * For convolutionFactor times
		 */
		for (int i = 0, p = factor; i < p; i++) {
			
			/*
			 * For each training gesture
			 */
			foreach (var gesture in trainingGestures) {

				generatedGesture = new List<Point>();
				
				/*
				 * For each value in the training gesture
				 */
				foreach (var point in gesture) {
					
					/*
					 * Generate a random point within a normalized gaussian distribution
					 */
					float x = distributeAux(point.x);
					float y = distributeAux(point.y);
					float z = distributeAux(point.z);

					generatedGesture.Add(new Point(x,y,z,point.stroke));
				}
				
				/*
				 * Add the generated gesture to the trainingGesture array
				 */
				trainingGestures.Add(generatedGesture);
			}	
		}
		
		/*
		 * Return the expanded trainingGestures array
		 */
		return trainingGestures;
	}
		
	/**
	 * This function matches a parameter gesture against the known set of saved gestures.  
	 * 
	 * This function does not need to return any value, but it should fire either the 'gesture-recognized' or 
	 * the 'gesture-unknown' event.  
	 * 
	 * The 'gesture-recognized' event includes a numeric value for the closest match, the name of the recognized 
	 * gesture, and a list of hit values for all known gestures as parameters.  The list maps gesture names to 
	 * hit values.
	 * 
	 * The 'gesture-unknown' event, includes a list of gesture names mapped to hit values for all known gestures 
	 * as a parameter.
	 * 
	 * If a gesture is recognized, an event with the name of the gesture and no parameters will also be fired. So 
	 * listeners waiting for a 'Punch' gestures, for example, can just register for events using: 
	 * 
	 * 		trainer.on('Punch').
	 * 
	 * @param gesture
	 * @param frameCount
	 */
	void recognize(List<Point> gesture, int frameCount) {
			
		var threshold = this.hitThreshold;
		Dictionary<string, float> allHits	= new Dictionary<string, float>();
		float hit = 0;
		float bestHit = 0;
		bool recognized = false;
		string closestGestureName = null;
		bool recognizingPose = (frameCount == 1); //Single-frame recordings are idenfied as poses
		
		/*
		 * We cycle through all known gestures
		 */
		foreach (var knownGesture in gestures) {
			
			/*
			 * We don't actually attempt to compare gestures to poses
			 */
			if (this.poses[knownGesture.Key] != recognizingPose) { 
				hit = 0.0f;
			} else {
				/*
				 * For each know gesture we generate a correlation value between the parameter gesture and a saved 
				 * set of training gestures. This correlation value is a numeric value between 0.0 and 1.0 describing how similar 
				 * this gesture is to the training set.
				 */
				hit = this.correlate(knownGesture.Key, knownGesture.Value, gesture);				
			}
			
			/*
			 * Each hit is recorded
			 */
			allHits.Add(knownGesture.Key, hit);
				
			/*
			 * If the hit is equal to or greater than the configured hitThreshold, the gesture is considered a match.
			 */
			if (hit >= threshold) { 
				recognized = true; 
			}
				
			/*
			 * If the hit is higher than the best hit so far, this gesture is stored as the closest match.
			 */
			if (hit > bestHit) { 
				bestHit = hit; 
				closestGestureName = knownGesture.Key; 
			}
		}
			
		if (recognized) { 
			if(OnGestureRecognized != null)
				OnGestureRecognized(closestGestureName, bestHit, allHits);
		} else {
			if(OnGestureUnknown != null)
				OnGestureUnknown(allHits);
		}
	}
		
	/**
	 * This function accepts a set of training gestures and a newly input gesture and produces a number between 0.0 and 1.0 describing 
	 * how closely the input gesture resembles the set of training gestures.
	 * 
	 * This DEFAULT implementation uses a LeapTrainer.TemplateMatcher to perform correlation.
	 * 
	 * @param gestureName
	 * @param trainingGestures
	 * @param gesture
	 * @returns {Number}
	 */
	float correlate(string gestureName, List<List<Point>> trainingGestures, List<Point> gesture) {
			
		List<Point> parsedGesture = this.templateMatcher.process(gesture);
		
		float nearest = float.PositiveInfinity, distance;
		bool foundMatch = false;
		
		foreach (var toMatch in trainingGestures) {
			
			distance = this.templateMatcher.match(parsedGesture, toMatch);
			
			if (distance < nearest) {
				/*
				 * 'distance' here is the calculated distance between the parameter gesture and the training 
				 * gesture - so the smallest value indicates the closest match
				 */
				nearest = distance;
				foundMatch = true;
			}
		}
		
		return (!foundMatch) ? 0f : (Mathf.Min(Mathf.Max(100f * Mathf.Max(nearest - 4f) / -4f, 0f), 100f)/100f);
	}
		
	/**
	 * These three functions are used by the training UI to select alternative strategies - sub-classes should override these functions 
	 * with names for the algorithms they implement.
	 * 
	 * Each function should return a descriptive name of the strategy implemented.
	 */
	string getRecordingTriggerStrategy() { return "Frame velocity"; }
		
	/**
	 * This is the type and format of gesture data recorded by the recordFrame function.
	 */
	string getFrameRecordingStrategy() { return "3D Geometric Positioning"; }
		
	/**
	 * This is the name of the mechanism used to recognize learned gestures.
	 */
	string getRecognitionStrategy() { return "Geometric Template Matching"; }
		
	/**
	 * This function converts the requested stored gesture into a JSON string containing the gesture name and training data.  
	 * 
	 * Gestures exported using this function can be re-imported using the fromJSON function below.
	 * 
	 * @param gestureName
	 * @returns {String}
	 */
	string toJSON(string gestureName) {
			
		var gestures = this.gestures[gestureName];

		
		SimpleJSON.JSONNode json = new SimpleJSON.JSONNode (),
							name = new SimpleJSON.JSONNode (),
							pose = new SimpleJSON.JSONNode (),
							data = new SimpleJSON.JSONNode (),
							x,y,z,stroke;

		foreach (var gesture in gestures) {

			SimpleJSON.JSONNode gesturePoints = new SimpleJSON.JSONNode();

			foreach(var point in gesture){
				SimpleJSON.JSONNode pointComponents = new SimpleJSON.JSONNode();

				x = new SimpleJSON.JSONNode (); x.AsFloat = point.x;
				y = new SimpleJSON.JSONNode (); y.AsFloat = point.y;
				z = new SimpleJSON.JSONNode (); z.AsFloat = point.z;
				stroke = new SimpleJSON.JSONNode (); stroke.AsFloat = point.stroke;

				pointComponents.Add("x", x);
				pointComponents.Add("y", y);
				pointComponents.Add("z", z);
				pointComponents.Add("stroke", stroke);

				gesturePoints.Add(pointComponents);
			}

			data.Add(gesturePoints);
		}


		name.Value = gestureName;
		pose.AsBool = this.poses [gestureName];

		json.Add ("name", name);
		json.Add ("pose", pose);
		json.Add ("data", data);

		return json.ToString ();
	}
		
	/**
	 * This is a simple import function for restoring gestures exported using the toJSON function above.
	 * 
	 * It returns the object parsed out of the JSON, so that overriding implementations can make use of this function.
	 * 
	 * @param json
	 * @returns {Object}
	 */
	Dictionary<string, object> fromJSON(string json) {
		var jo = SimpleJSON.JSON.Parse(json);

		List<List<Point>> gesture = new List<List<Point>> ();

		string gestureName = jo["name"].Value;
		foreach(var g in jo["data"].Childs){
			List<Point> subGesture = new List<Point>();

			foreach(var p in g.Childs)
				subGesture.Add(new Point(p["x"].AsFloat, p["y"].AsFloat, p["z"].AsFloat, p["stroke"].AsInt));

			gesture.Add(subGesture);
		}

		this.Create(gestureName, true);
		this.gestures.Add(gestureName, gesture);
		this.poses.Add(gestureName, jo["pose"].AsBool);

		Dictionary<string, object> parsed = new Dictionary<string, object>();

		parsed ["name"] = gestureName;
		parsed ["pose"] = poses [gestureName];
		parsed ["data"] = gesture;

		return parsed;
	}
		
	/**
	 * This function temporarily disables frame monitoring.
	 * 
	 * @returns {Object} The leaptrainer controller, for chaining.
	 */
	LeapTrainer pause() { 
		this.paused = true; 
		return this; 
	}
		
	/**
	 * This function resumes paused frame monitoring.
	 * 
	 * @returns {Object} The leaptrainer controller, for chaining.
	 */
	LeapTrainer resume() { 
		this.paused = false; 
		return this; 
	}
		
		/**
	 * This function unbinds the controller from the leap frame event cycle - making it inactive and ready 
	 * for cleanup.
	 */
	void OnDestroy() { 
		this.listener.Release();
	}
}
