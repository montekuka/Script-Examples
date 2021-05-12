using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding {
public class PathRequestManager : MonoBehaviour {
	private readonly Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
	private PathRequest currentPathRequest;

	private static PathRequestManager instance;
	private Pathfinding pathfinding;

	private bool isProcessingPath;

	private void Awake() {
		instance    = this;
		pathfinding = GetComponent<Pathfinding>();
	}

	public static void RequestPath(NavConnection pathStart, NavConnection pathEnd, Action<Vector3[], NavConnection[], bool> callback) {
		var newRequest = new PathRequest(pathStart, pathEnd, callback);
		instance.pathRequestQueue.Enqueue(newRequest);
		instance.TryProcessNext();
	}

	private void TryProcessNext() {
		if (isProcessingPath || pathRequestQueue.Count <= 0) return;
		currentPathRequest = pathRequestQueue.Dequeue();
		isProcessingPath   = true;
		pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
	}

	public void FinishedProcessingPath(Vector3[] path, NavConnection[] pathConnections, bool success) {
		currentPathRequest.callback(path, pathConnections, success);
		isProcessingPath = false;
		TryProcessNext();
	}

	private readonly struct PathRequest {
		public readonly NavConnection pathStart;
		public readonly NavConnection pathEnd;
		public readonly Action<Vector3[], NavConnection[], bool> callback;

		public PathRequest(NavConnection start, NavConnection end, Action<Vector3[], NavConnection[], bool> callback) {
			pathStart     = start;
			pathEnd       = end;
			this.callback = callback;
		}
	}
}
}