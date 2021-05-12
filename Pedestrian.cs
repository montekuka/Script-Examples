using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Pedestrian : Agent {
	private List<WaitZone> crossingZones;

	private WaitZone nextZone;
	private Vector3 targetPosition;
	private Transform finalDestination;
	public LayerMask targetMask;
	public bool IsCrossing { get; private set; }
	public LayerMask crossingLayer;
	public Animator animator;

	public override void Initialize() {
		base.Initialize();
		agent                = GetComponent<NavMeshAgent>();
		agent.enabled        = true;
		agent.speed          = Speed;
		agent.updateRotation = false;
		finalDestination     = TrafficSystem.Instance.GetPedestrianDestination();
		agent.destination    = finalDestination.position;
		StartCoroutine(WaitForPathFound());
	}

	private float debug;

	protected override void Update() {
		base.Update();
		CheckDestinationReached();
		var shouldMove = agent.velocity.magnitude > 0.1f && agent.remainingDistance > agent.radius;
		debug = agent.velocity.magnitude;
		// Update animation parameters
		animator.SetBool(Walk, !IsWaiting);
		animator.speed = !IsWaiting ? agent.desiredVelocity.magnitude * 1.15f : 0.2f;
	}

	private void LateUpdate() {
		if (agent.velocity.sqrMagnitude > 0.5f) { transform.rotation = Quaternion.LookRotation(agent.velocity.normalized); }
	}

	private IEnumerator WaitForPathFound() {
		while (agent.pathPending || !agent.isOnNavMesh || !agent.hasPath) yield return new WaitForEndOfFrame();
		path = agent.path.corners;
		EvaluateCrossings();
		InitializeDebugLineRenderer(agent.path.corners);
	}

	private void EvaluateCrossings() {
		path          = agent.path.corners;
		crossingZones = new List<WaitZone>();
		for (var i = 0; i < path.Length - 1; i++) {
			RaycastHit hit;
			if (Physics.Raycast(path[i], path[i + 1] - path[i], out hit, Vector3.Distance(path[i + 1], path[i]), targetMask)) {
				var waitZone = hit.collider.GetComponent<WaitZone>();
				if (waitZone.type == type && !crossingZones.Contains(waitZone)) crossingZones.Add(waitZone);
			}
		}
		//GetNextZone();
	}

	private void CheckDestinationReached() {
		//if (!(Vector3.Distance(transform.position, targetPosition) < 0.4f)) return;
		if (!agent.pathPending)
		{
			if (agent.remainingDistance <= agent.stoppingDistance)
			{
				if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
				{
					GetNewDestination();
				}
			}
		}
	}

	private void GetNewDestination() {
		targetPosition    = Vector3.zero;
		finalDestination  = TrafficSystem.Instance.GetPedestrianDestination();
		agent.destination = finalDestination.position;
		StartCoroutine(WaitForPathFound());
	}

	private WaitZone[] currentZones;
	private static readonly int Walk = Animator.StringToHash("walk");

	private void OnTriggerEnter(Collider other) {
		if ((crossingLayer.value & 1 << other.gameObject.layer) != 0) {
			if (crossingZones is null) return;
			if (crossingZones.Any(s => other.GetComponent<CrossingZone>().waitZones.Contains(s))) {
				currentZones = other.GetComponent<CrossingZone>().waitZones;
				if (!IsWaiting && !currentZones[0].CanPass) {
					IsWaiting = true;
					StartCoroutine(CheckWhileWaiting());
				}
				if (!IsCrossing && currentZones[0].CanPass) { IsCrossing = true; }
			}
		}
	}

	IEnumerator CheckWhileWaiting() {
		while (!currentZones[0].CanPass) { yield return new WaitForSeconds(1f); }
		IsWaiting  = false;
		IsCrossing = true;
	}

	// private void OnTriggerStay(Collider other) {
	// 	if ((crossingLayer.value & 1 << other.gameObject.layer) == 0 || !IsWaiting) return;
	// 	if (!currentZones[0].CanPass) return;
	// 	IsWaiting  = false;
	// 	IsCrossing = true;
	// }

	private void OnTriggerExit(Collider other) {
		if ((crossingLayer.value & 1 << other.gameObject.layer) != 0 && IsCrossing) {
			if (crossingZones is null) return;
			if (crossingZones.Any(s => other.GetComponent<CrossingZone>().waitZones.Contains(s))) {
				if (IsCrossing) IsCrossing = false;
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Debug.Log("Distance to target: " + agent.remainingDistance);
		Gizmos.color = Color.yellow;
		Gizmos.DrawCube(agent.destination, new Vector3(0.2f,10f,0.2f));
	}
}