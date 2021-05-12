using System;
using Pathfinding;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;

[Serializable]
public class NavConnection : MonoBehaviour {
	public float gizmoStraight = 4.15f;
	public bool goesIntoSection;
	public NavConnection inConnection;
	[SerializeField]
	public OutLane[] outLanes;
	public TrafficType trafficType;
	public SmoothingType smoothingType;
	public bool IsSmoother { get; set; }

	public enum SmoothingType {
		NoSmoothing,
		SmoothingStart,
		Smoothing,
		SmoothingEnd
	}

	public enum TrafficType {
		Road,
		Train
	}

	[Serializable]
	public class OutLane {
		public Direction direction;
		[FormerlySerializedAs("out_Left")]
		public NavConnection outLeft;
		[FormerlySerializedAs("out_Right")]
		public NavConnection outRight;

		public enum Direction {
			Straight,
			Left,
			Right
		}
	}

	public NavSection NavSection { get; set; }

	[SerializeField]
	public NavConnectionNode node;

	private void Awake() { node = new NavConnectionNode {correspondingConnection = this, worldPosition = transform.position}; }

	// -------------------------------------------------------------------
	// Debug

	private void OnDrawGizmos() {
		if (!TrafficSystem.Instance.drawGizmos) return;
		Gizmos.color = goesIntoSection ? Color.green : Color.white;
		Gizmos.DrawSphere(transform.position, smoothingType == SmoothingType.Smoothing ? 0.4f : 0.5f);
		DrawLaneGizmo();
	}

	private void DrawLaneGizmo() {
		Gizmos.color = Color.cyan;
		var position          = transform.position;
		var adjustedTransform = new Vector3(position.x, 0.05f, position.z);

		{ }
		if (outLanes == null) return;

		DrawLines(adjustedTransform);
		//DrawArrows(adjustedTransform);

		void DrawLines(Vector3 vector3) {
			if (inConnection != null) {
				var adjustedIn = new Vector3(inConnection.transform.position.x, 0.05f, inConnection.transform.position.z);
				Gizmos.color = Color.yellow;
				Gizmos.DrawRay(adjustedIn, vector3 - adjustedIn);
				//Gizmos.DrawWireSphere(Vector3.Lerp(adjustedIn, vector3, 0.5f), 0.1f);
			}
			foreach (var lane in outLanes) {
				if (lane == null) continue;
				switch (lane.direction) {
					case OutLane.Direction.Straight:
						Gizmos.color = Color.yellow;
						if (lane.outRight != null)
							if (lane.outRight.smoothingType != SmoothingType.Smoothing && lane.outRight.smoothingType != SmoothingType.SmoothingEnd)
								Gizmos.DrawLine(transform.position, lane.outRight.transform.position);
						break;
					case OutLane.Direction.Left:
					case OutLane.Direction.Right: break;
					default: throw new ArgumentOutOfRangeException();
				}
				if (Selection.activeGameObject != gameObject) continue;
				Gizmos.color = Color.yellow;
				if (lane.outLeft != null) Gizmos.DrawSphere(lane.outLeft.transform.position,   0.1f);
				if (lane.outRight != null) Gizmos.DrawSphere(lane.outRight.transform.position, 0.1f);
			}
		}

		/*void DrawArrows(Vector3 vector3) {
			if (inConnection != null) {
				var adjustedIn = new Vector3(inConnection.transform.position.x, 0.05f, inConnection.transform.position.z);
				Gizmos.color = Color.yellow;
				Gizmos.DrawRay(adjustedIn, vector3 - adjustedIn);
				Gizmos.DrawWireSphere(Vector3.Lerp(adjustedIn, vector3, 0.5f), 0.1f);
			}
			foreach (var lane in outLanes) {
				if (lane == null) continue;
				switch (lane.direction) {
					case OutLane.Direction.Straight:
						Gizmo.DrawArrow(vector3, transform.forward * gizmoStraight);
						break;
					case OutLane.Direction.Left:
						Gizmos.DrawRay(vector3, transform.forward * 3.15f);
						Gizmo.DrawArrow(transform.forward * 3.15f + vector3, -transform.right * 2f);
						break;
					case OutLane.Direction.Right:
						Gizmos.color = Color.cyan;
						Gizmos.DrawRay(vector3, transform.forward * 1.65f);
						Gizmo.DrawArrow(transform.forward * 1.65f + vector3, transform.right);
						break;
					default: throw new ArgumentOutOfRangeException();
				}
				if (Selection.activeGameObject != gameObject) continue;
				Gizmos.color = Color.yellow;
				if (lane.outLeft != null) Gizmos.DrawSphere(lane.outLeft.transform.position,   0.1f);
				if (lane.outRight != null) Gizmos.DrawSphere(lane.outRight.transform.position, 0.1f);
			}
		}*/
	}

	[Serializable]
	public class NavConnectionWrapper {
		public NavConnection[] connections;
	}
}