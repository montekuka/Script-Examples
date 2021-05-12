using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding {
public class Pathfinding : MonoBehaviour {
	private PathRequestManager requestManager;
	public NavConnection startPoint;
	public NavConnection endPoint;
	private NavConnection[] navConnections;

	private void Awake() {
		navConnections = FindObjectsOfType<NavConnection>();
		requestManager = GetComponent<PathRequestManager>();
		//
	}

	public void StartFindPath(NavConnection startPos, NavConnection targetPos) { StartCoroutine(FindPath(startPos, targetPos)); }

	private NavConnection[] waypointConnections;

	private IEnumerator FindPath(NavConnection startPos, NavConnection targetPos) {
		Vector3[] waypoints;
		var       pathSuccess = false;

		var startNode  = startPos.node;
		var targetNode = targetPos.node;

		//EditorGUIUtility.PingObject(startNode.correspondingConnection);
		//Debug.Log("CURRENT: " + startNode.correspondingConnection.name);
		var closedSet = new HashSet<NavConnectionNode>();
		if (startNode.walkable && targetNode.walkable) {
			var openSet = new Heap<NavConnectionNode>(navConnections.Length);
			openSet.Add(startNode);

			while (openSet.Count > 0) {
				var currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);

				if (currentNode == targetNode) {
					pathSuccess = true;
					break;
				}
				//Debug.Log(currentNode.neighbours.Count);
				foreach (var neighbour in currentNode.neighbours) {
					//Debug.Log(neighbour.connection.name);
					if (!neighbour.walkable || closedSet.Contains(neighbour)) { continue; }
					var newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;
					if (newMovementCostToNeighbour >= neighbour.GCost && openSet.Contains(neighbour)) continue;
					neighbour.GCost  = newMovementCostToNeighbour;
					neighbour.HCost  = GetDistance(neighbour, targetNode);
					neighbour.parent = currentNode;
					if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
					else openSet.UpdateItem(neighbour);
				}
			}
		}
		yield return null;
		if (pathSuccess) { waypoints = RetracePath(startNode, targetNode); } else {
			//Debug.Log(startPos + " " + targetPos);
			//TODO: Angriff starten, falls kein path verfügbar
			waypoints = closedSet.Select(item => item.worldPosition).ToArray();
		}
		requestManager.FinishedProcessingPath(waypoints, waypointConnections, pathSuccess);
	}

	private Vector3[] RetracePath(NavConnectionNode startNode, NavConnectionNode endNode) {
		var path        = new List<NavConnectionNode>();
		var currentNode = endNode;
		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		path.Add(startNode);
		path = RemoveSmoothedCorners(path);
		//Debug.Log("PATH START " + path[0].worldPosition + "\nPATH END " + path[path.Count - 1].worldPosition);
		//Debug.Log(path.Count);
		var waypoints = new Vector3[path.Count]; //= SimplifyPath(path);
		waypointConnections = new NavConnection[path.Count];
		//waypoints = new Vector3[path.Count];
		for (var i = 0; i < waypoints.Length; i++) {
			waypoints[i]           = path[i].worldPosition;
			waypointConnections[i] = path[i].correspondingConnection;
		}
		//Debug.Log(waypoints.Length);
		Array.Reverse(waypoints);
		Array.Reverse(waypointConnections);
		//lineRenderer.positionCount = waypoints.Length;
		//lineRenderer.SetPositions(waypoints);
		return waypoints;
	}

	private static List<NavConnectionNode> RemoveSmoothedCorners(IEnumerable<NavConnectionNode> nodes) { return nodes.Where(node => node.correspondingConnection.smoothingType != NavConnection.SmoothingType.Smoothing).ToList(); }

	private static int GetDistance(NavConnectionNode nodeA, NavConnectionNode nodeB) { return Mathf.RoundToInt(Vector3.Distance(nodeA.worldPosition, nodeB.worldPosition)); }
}
}