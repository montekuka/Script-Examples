using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utility;
using Random = UnityEngine.Random;

public class Road : NavSection {
	// -------------------------------------------------------------------
	// Properties

	[Header("Road")]
	public Transform[] pedestrianSpawns;

	public NavConnection.NavConnectionWrapper[] navConnectionsToSmooth;
	public Dictionary<NavConnection, Vector3[]> smoothedPaths = new Dictionary<NavConnection, Vector3[]>();

	// -------------------------------------------------------------------
	// Get Data

	public bool TryGetPedestrianSpawn(out Transform spawn) {
		if (pedestrianSpawns.Length > 0) {
			var index = Random.Range(0, pedestrianSpawns.Length);
			spawn = pedestrianSpawns[index];
			return true;
		}
		spawn = null;
		return false;
	}

	private void SmoothRoad(IReadOnlyList<NavConnection> navConnections) {
		var endConnections   = new List<NavConnection>();
		var otherConnections = new List<NavConnection>();
		var startConnection  = navConnections[0];
		for (var i = 1; i < navConnections.Count; i++) {
			if (navConnections[i].smoothingType == NavConnection.SmoothingType.Smoothing) otherConnections.Add(navConnections[i]);
			else if (navConnections[i].smoothingType == NavConnection.SmoothingType.SmoothingEnd) endConnections.Add(navConnections[i]);
			if (Selection.activeTransform == transform) Debug.Log(i);
		}
		var finalEnd = endConnections.Count > 1 ? Vector3.Lerp(endConnections[0].transform.position, endConnections[1].transform.position, 0.5f) - endConnections[0].transform.forward * 8f : endConnections[0].transform.position;
		var toSmooth = new Vector3[otherConnections.Count + 2];
		toSmooth[0]                   = startConnection.transform.position;
		toSmooth[toSmooth.Length - 1] = finalEnd;
		for (var i = 0; i < otherConnections.Count; i++) { toSmooth[i + 1] = otherConnections[i].transform.position; }

		smoothedPaths[startConnection] = SmoothCurveUtility.GetSmoothCurve(toSmooth, 3f);
	}

	protected virtual void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		foreach (var smoothedPathsKey in smoothedPaths.Keys) {
			for (int j = 0; j < smoothedPaths[smoothedPathsKey].Length - 1; j++) { Gizmos.DrawLine(smoothedPaths[smoothedPathsKey][j], smoothedPaths[smoothedPathsKey][j + 1]); }
		}
		//foreach (var position in smoothedPaths.Values.SelectMany(positionArray => positionArray)) { Gizmos.DrawWireSphere(position, 0.025f); }
	}
}