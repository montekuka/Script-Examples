using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public class Agent : MonoBehaviour {
	protected NavMeshAgent agent;

	[Header("Agent")]
	public TrafficType type = TrafficType.Pedestrian;
	public int maxSpeed = 20;
	public float rotationSpeed;
	protected float Speed { get; set; }
	public LineRenderer lineRenderer;
	private bool isWaiting;
	public bool IsWaiting {
		get => isWaiting;
		set {
			//if (Selection.activeGameObject == gameObject) Debug.Log(value);
			isWaiting = value;
		}
	}

	protected Vector3[] path;

	protected virtual bool CheckStop() { return IsWaiting; }

	public virtual void Initialize() { Speed = TrafficSystem.GetAgentSpeedFromKph(maxSpeed); }

	protected virtual void Update() {
		if (CheckStop()) ReduceSpeed();
		ToggleLineRenderer();
	}

	protected virtual void ReduceSpeed() => agent.velocity *= 0.9f;

	#region Debugging
	protected void InitializeDebugLineRenderer(Vector3[] positions) {
		if (!lineRenderer) lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = positions.Length;
		lineRenderer.SetPositions(positions);
	}

	protected void ToggleLineRenderer() {
		if (path == null) return;
		if (Selection.activeGameObject == gameObject) { lineRenderer.enabled = true; } else if (lineRenderer) { lineRenderer.enabled = false; }
	}
	#endregion
}