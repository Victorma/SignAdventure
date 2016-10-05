using UnityEngine;
using System.Collections;
using Leap.Unity;
using Leap;
using System;

public class RecordedServiceProvider : LeapProvider
{
    private LeapRecorder recorder;
    public LeapRecorder GetLeapRecorder()
    {
        return recorder;
    }

    public override Frame CurrentFixedFrame
    {
        get
        {
            return recorder.GetCurrentFrame();
        }
    }

    public override Frame CurrentFrame
    {
        get
        {
            return recorder.GetCurrentFrame();
        }
    }

    public override Image CurrentImage
    {
        get
        {
            return null;
        }
    }

    // Use this for initialization
    void Start () {
        recorder = new LeapRecorder();
	}
	
	// Update is called once per frame
	void Update () {
        if(recorder.state == RecorderState.Playing)
        {
            this.DispatchUpdateFrameEvent(recorder.NextFrame());
        }
	}
}
