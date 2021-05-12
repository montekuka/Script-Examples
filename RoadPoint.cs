using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RoadPoint : MonoBehaviour
{
	public enum PointState
	{
		None,
		Go,
		Ready,
		Stop
	}

	public List<RoadPoint> connectedPoints = new List<RoadPoint>();
	public State           state;
	public int             speed;

	public PointState roadState = PointState.None;

	public enum State
	{
		In,
		Out,
		Middle
	}

	public void SetRoadState(PointState state)
	{
		roadState = state;
	}

	void OnDrawGizmos()
	{
		//Label for Road Point
		//if (gameObject.name.Contains("(") && gameObject.name.StartsWith("RoadPoint")) { Handles.Label(transform.position, gameObject.name.Substring(11, 4)); }
		Gizmos.color = roadState switch
		{
			PointState.Go    => Color.green,
			PointState.Ready => Color.yellow,
			PointState.Stop  => Color.red,
			PointState.None  => Color.gray,
			_                => Gizmos.color
		};
		if (connectedPoints.Count > 0)
		{
			foreach (var roadPoint in connectedPoints.Where(roadPoint => roadPoint != null))
			{
				Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.2f);
				Gizmos.DrawLine(transform.position, roadPoint.transform.position);
			}
		}

		Gizmos.DrawCube(transform.position, new Vector3(0.4f,0.4f, 0.4f));

		if (Selection.activeGameObject == gameObject)
		{
			Gizmos.color = Color.yellow;
			foreach (var connectedPoint in connectedPoints.Where(connectedPoint => connectedPoint))
			{
				Gizmos.DrawSphere(connectedPoint.transform.position, 0.6f);
			}
		}
	}
}