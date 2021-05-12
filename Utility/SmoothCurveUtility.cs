using System.Collections.Generic;
using UnityEngine;

namespace Utility {
public static class SmoothCurveUtility {
	public static Vector3[] GetSmoothCurve(Vector3[] arrayToCurve, float smoothness) {
		List<Vector3> points;
		List<Vector3> curvedPoints;
		int           pointsLength;
		int           curvedLength;

		if (smoothness < 1.0f) smoothness = 1.0f;

		pointsLength = arrayToCurve.Length;

		curvedLength = (pointsLength * Mathf.RoundToInt(smoothness)) - 1;
		curvedPoints = new List<Vector3>(curvedLength);

		var t = 0.0f;
		for (var pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++) {
			t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

			points = new List<Vector3>(arrayToCurve);

			for (var j = pointsLength - 1; j > 0; j--) {
				for (var i = 0; i < j; i++) { points[i] = (1 - t) * points[i] + t * points[i + 1]; }
			}

			curvedPoints.Add(points[0]);
		}
		//curvedPoints.RemoveAt(0);
		return (curvedPoints.ToArray());
	}
}
}