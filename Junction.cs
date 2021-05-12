using System;
using System.Linq;
using UnityEngine;

public class Junction : Road {
	// -------------------------------------------------------------------
	// Enum

	public enum PhaseType {
		Timed,
		OnDemand
	}

	// -------------------------------------------------------------------
	// Properties

	[Header("Junction")]
	public PhaseType type = PhaseType.Timed;
	public Phase[] phases;
	public float phaseInterval = 30f;
	public float intervalYellowIn = 3f;
	public float intervalYellowOut = 22f;
	public float intervalRed = 26f;

	// -------------------------------------------------------------------
	// Initialization

	public void Start() {
		foreach (var item in phases) { item.Init(this); }
		if (phases.Length > 0) phases[0].Enable();
	}

	// -------------------------------------------------------------------
	// Update

	private void Update() {
		if (type != PhaseType.Timed) return;
		phaseTimer += Time.deltaTime;
		if (!phaseEnded && phaseTimer > intervalYellowIn) phases[currentPhase].GoTraffic();
		if (!phaseEnded && phaseTimer > intervalYellowOut) phases[currentPhase].PrepareEnd();
		if (!phaseEnded && phaseTimer > intervalRed) EndPhase();
		if (phaseTimer > phaseInterval) ChangePhase();
	}

	// -------------------------------------------------------------------
	// Phase

	private float phaseTimer;
	private bool phaseEnded;
	private int currentPhase;

	private void EndPhase() {
		phaseEnded = true;
		phases[currentPhase].End();
	}

	private void ChangePhase() {
		phaseTimer = 0;
		phaseEnded = false;
		if (currentPhase < phases.Length - 1) currentPhase++;
		else currentPhase = 0;
		phases[currentPhase].Enable();
	}

	// -------------------------------------------------------------------
	// Debug

	private Mesh cube;
	private Mesh Cube {
		get {
			if (cube != null) return cube;
			var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube = go.GetComponent<MeshFilter>().sharedMesh;
			DestroyImmediate(go);
			return cube;
		}
	}

	protected override void OnDrawGizmos() {
		base.OnDrawGizmos();
		if (!TrafficSystem.Instance.drawGizmos) return;
		if (allZones == null) return;
		foreach (var zone in allZones) {
			Gizmos.color = zone.CanPass ? Color.green : Color.red;
			DrawAreaGizmo(zone.transform);
		}
	}

	private void DrawAreaGizmo(Transform t) {
		var rotationMatrix1 = Matrix4x4.TRS(t.position - new Vector3(0, t.localScale.y * 0.5f, 0) - t.right * 0.3f, t.rotation, Vector3.Scale(t.lossyScale, new Vector3(6f, 1f, 1f)));
		Gizmos.matrix = rotationMatrix1;
		Gizmos.DrawMesh(Cube, Vector3.zero, Quaternion.identity);
	}

	// -------------------------------------------------------------------
	// Data Classes
	public WaitZone[] allZones;
	public TrafficLight[] allLights;
	public RoadPoint[] allPoints;

	[Serializable]
	public class Phase
	{
		public bool test;
		public WaitZone[] positiveZones;
		public TrafficLight[] positiveLights;
		public RoadPoint[] positivePoints;
		private Junction junction;

		public void Enable() {
			if (junction.allZones == null) return;
			foreach (var zone in junction.allZones) zone.CanPass = positiveZones.Contains(zone);
			foreach (var light in junction.allLights)
				if (positiveLights.Contains(light)) {
					if (light.type == TrafficType.Pedestrian) light.SetLight(LightColor.Green);
					else light.SetLight(LightColor.Yellow);
				} else { light.SetLight(LightColor.Red); }
			foreach (var roadPoint in junction.allPoints) {
				if (positivePoints.Contains(roadPoint)) { roadPoint.SetRoadState(RoadPoint.PointState.Ready); } else { roadPoint.SetRoadState(RoadPoint.PointState.Stop); }
			}
		}

		public void GoTraffic() {
			foreach (var light in positiveLights)
				if (light.type == TrafficType.Vehicle)
					light.SetLight(LightColor.Green);
			foreach (var positivePoint in positivePoints) { positivePoint.SetRoadState(RoadPoint.PointState.Go); }
		}

		public void PrepareEnd() {
			foreach (var light in positiveLights)
				if (light.type == TrafficType.Vehicle)
					light.SetLight(LightColor.Yellow);
			foreach (var positivePoint in positivePoints) { positivePoint.SetRoadState(RoadPoint.PointState.Ready); }
		}

		public void End() {
			foreach (var zone in positiveZones) zone.CanPass = false;
			foreach (var light in positiveLights) light.SetLight(LightColor.Red);
			foreach (var positivePoint in positivePoints) { positivePoint.SetRoadState(RoadPoint.PointState.Stop); }
		}

		internal void Init(Junction junction2) { junction = junction2; }
	}
}