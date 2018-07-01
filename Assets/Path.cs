using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    //public Vector3 currentVector = new Vector3(0,0.9f,0); //Vector3.up; //Vector3.forward;
    public LineRenderer lines;
    public List<float> velocities;
    public int currentIndex;
    public static float MAX_VELOCITY = 7.0f;
    public static float MIN_VELOCITY = 6.0f;

	// Use this for initialization
	public Path () {
        this.currentIndex = 0;
        this.lines = GameObject.Find("LinePath").GetComponent<LineRenderer>();
        this.velocities = new List<float>();
        for (int i=0; i<10; ++i) {
            Vector3 v = this.lines.GetPosition(i);
            //var cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //cube.transform.localScale = new Vector3(1f, 1f, 1f);
            //cube.transform.position = v;
            this.velocities.Add(Random.Range(MIN_VELOCITY, MAX_VELOCITY));
        }
	}

    public Vector3 GetDirection()
    {
        Vector3 v;
        if (this.currentIndex < this.lines.positionCount - 1) {
            v = this.GetNextPosition() - this.GetPosition();
        } else {
            v = this.GetPosition() - this.GetPreviousPosition();
        }
        return v.normalized;
    }

    public Vector3 GetPosition()
    {
        return this.lines.GetPosition(this.currentIndex);
    }

    public Vector3 GetPreviousPosition()
    {
        if (this.currentIndex - 1 < 0) {
            return this.lines.GetPosition(this.currentIndex);
        } else {
            return this.lines.GetPosition(this.currentIndex - 1);
        }
    }

    public Vector3 GetNextPosition()
    {
        if (this.currentIndex > this.lines.positionCount) {
            return this.lines.GetPosition(this.currentIndex);
        }
        else {
            return this.lines.GetPosition(this.currentIndex + 1);
        }
    }

    public float GetCurrentVelocity() { return this.velocities[this.currentIndex]; }

    public int GetCount() { return this.lines.positionCount; }
}