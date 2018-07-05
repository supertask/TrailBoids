using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractivePath
{
    public static int MAX_BUFF = 3;
    public List<Vector3> posBuffer;
    public List<Vector3> dirBuffer;
    public List<float> velocityBuffer;
    public Vector3 lastPosition;

	// Use this for initialization
	public InteractivePath() {
        this.posBuffer = new List<Vector3>();
        this.dirBuffer = new List<Vector3>();
        this.velocityBuffer = new List<float>();
        this.posBuffer.Add(Vector3.zero);
        this.dirBuffer.Add(Vector3.forward);
        this.velocityBuffer.Add(1.0f);
        this.lastPosition = Vector3.zero;
	}

    public void SetPosition(Vector3 v) {
        if (this.posBuffer.Count > InteractivePath.MAX_BUFF) {
            this.posBuffer.RemoveAt(0);
        }
        this.posBuffer.Add(v);
    }
    public void SetDirection(Vector3 v) {
        if (this.dirBuffer.Count > InteractivePath.MAX_BUFF) {
            this.dirBuffer.RemoveAt(0);
        }
        this.dirBuffer.Add(v);
    }

    public void SetVelocity(float velocity) {
        if (this.velocityBuffer.Count > InteractivePath.MAX_BUFF) {
            this.velocityBuffer.RemoveAt(0);
        }
        this.velocityBuffer.Add(velocity);
    }

    public Vector3 GetDirection() {
        return this.dirBuffer[this.posBuffer.Count - 1];
    }

    public Vector3 GetPosition() {
        return this.posBuffer[this.posBuffer.Count - 1];
    }

    public float GetVelocity() {
        return this.velocityBuffer[this.velocityBuffer.Count - 1];
    }

    /*
    public Vector3 GetPreviousPosition() {
        return this.GetPosition(this.GetCurrentIndex() - 1); //少なくとも2つ要素がある
    }

    public Vector3 GetDirection() { return this.GetDirection(this.GetCurrentIndex()); }
    public Vector3 GetDirection(int index)
    {
        Vector3 v;
        v = this.GetPosition(index) - this.GetPosition(index - 1);
        if (v == Vector3.zero) {
            return this.GetDirection(index - 1);
        }
        else { return v.normalized; }
    }
    */
}