using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RoadObject : MonoBehaviour {
	//This class is on the road object

	public RoadType roadType;
	public RoadPoint[] roadPoints;

	public enum RoadType {
		Straight,
		Corner,
		Intersection,
		TIntersection
	}

	private void Awake() { roadPoints = GetComponentsInChildren<RoadPoint>(); }

	List<RoadObject> roadObject = new List<RoadObject>();

	//Connecting the roads to its neighbors, this is getting REALLY boring now
	public void UpdateRoad(bool remove) {
		switch (roadType) {
			case RoadType.Straight:
				roadObject.Add(CheckForNeighborRoad(transform.forward));
				roadObject.Add(CheckForNeighborRoad(-transform.forward));
				break;
			case RoadType.Corner:
				roadObject.Add(CheckForNeighborRoad(transform.forward));
				roadObject.Add(CheckForNeighborRoad(-transform.right));
				break;
			case RoadType.Intersection:
				roadObject.Add(CheckForNeighborRoad(transform.forward));
				roadObject.Add(CheckForNeighborRoad(-transform.forward));
				roadObject.Add(CheckForNeighborRoad(-transform.right));
				roadObject.Add(CheckForNeighborRoad(transform.right));
				break;
			case RoadType.TIntersection:
				roadObject.Add(CheckForNeighborRoad(transform.forward));
				roadObject.Add(CheckForNeighborRoad(-transform.forward));
				roadObject.Add(CheckForNeighborRoad(-transform.right));
				break;
		}
		List<RoadPoint> plusConnection = new List<RoadPoint>(), minusConnection = new List<RoadPoint>();
		for (int k = 0; k < roadObject.Count; k++) {
			if (roadObject[k] != null) {
				RoadPoint[] lastClosest = new RoadPoint[8];
				for (int i = 0; i < 2; i++) {
					//Debug.Log("IN", this);
					if (i == 1) i           = 4;
					RoadPoint neighborPoint = GetClosestRoadPoint(roadObject[k].roadPoints, gameObject, RoadPoint.State.Out, ref lastClosest);
					lastClosest[i] = neighborPoint;
					var point = GetClosestRoadPoint(roadPoints, roadObject[k].gameObject, RoadPoint.State.In, ref lastClosest);
					lastClosest[i + 1] = point;
					if (Selection.activeGameObject == gameObject) Debug.Log(point + " " + neighborPoint, point);
					if (remove) neighborPoint.connectedPoints.Remove(point);
					else neighborPoint.connectedPoints.Add(point);

					neighborPoint      = GetClosestRoadPoint(roadObject[k].roadPoints, gameObject, RoadPoint.State.In, ref lastClosest);
					lastClosest[i + 2] = neighborPoint;
					point              = GetClosestRoadPoint(roadPoints, roadObject[k].gameObject, RoadPoint.State.Out, ref lastClosest);
					lastClosest[i + 3] = point;

					if (remove) point.connectedPoints.Remove(neighborPoint);
					else point.connectedPoints.Add(neighborPoint);
				}
				if (Selection.activeGameObject == gameObject)
					for (int i = 0; i < lastClosest.Length; i++) { Debug.Log(lastClosest[i].name + " " + i); }
			}
			plusConnection.Clear();
			minusConnection.Clear();
		}
	}

	//I guess the method name makes sense here
	RoadObject CheckForNeighborRoad(Vector3 side) {
		Vector3      pos         = transform.position + (side * 50);
		RoadObject[] roadObjects = FindObjectsOfType<RoadObject>();
		for (int i = 0; i < roadObjects.Length; i++) {
			if (Vector3.Distance(roadObjects[i].transform.position, pos) < 2) { return roadObjects[i].GetComponent<RoadObject>(); }
		}
		return null;
	}

	//The roads have points that the vehicles follow, this method gets the closest point to connect to, stupid solution i think
	RoadPoint GetClosestRoadPoint(RoadPoint[] roadPoints, GameObject closestTo, RoadPoint.State state, ref RoadPoint[] checkCollection) {
		float     dist      = Mathf.Infinity;
		RoadPoint tempPoint = null;
		for (int i = 0; i < roadPoints.Length; i++) {
			if (Vector3.Distance(roadPoints[i].transform.position, closestTo.transform.position) < dist && roadPoints[i].state == state && !checkCollection.Contains(roadPoints[i])) {
				dist      = Vector3.Distance(roadPoints[i].transform.position, closestTo.transform.position);
				tempPoint = roadPoints[i];
			}
		}
		//Debug.Log(tempPoint + " L" + state, tempPoint);
		return tempPoint;
	}
}