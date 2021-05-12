using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PedestrianSensor : MonoBehaviour {
	private Vehicle vehicle;
	public string targetTag;
	public LayerMask targetLayer;
	public float detectionInterval = 0.5f;
	private BoxCollider collider;
	private Collider[] colliders;
	private int collisionCount;
	private Transform transformParent;
	private bool colliding;

	private void Awake() {
		transformParent = transform.parent;
		vehicle         = transformParent.parent.GetComponent<Vehicle>();
		collider        = GetComponent<BoxCollider>();
		colliders       = new Collider[50];
		//StartCoroutine(CheckCollisions());
	}

	private IEnumerator CheckCollisions() {
		while (true) {
			collisionCount = Physics.OverlapBoxNonAlloc(transform.position + collider.center, collider.size / 2, colliders, transformParent.rotation, targetLayer);
			for (var i = 0; i < collisionCount; i++) { CheckCollision(colliders[i]); }
			//if (collisionCount == 0) vehicle.AdjustCurrentColliderCount(false);
			colliding = collisionCount > 0;
			yield return new WaitForSeconds(detectionInterval);
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (Selection.activeGameObject == gameObject) Debug.Log(other.name, this);
		CheckCollision(other);
	}

	private void OnTriggerStay(Collider other) { CheckCollision(other); }

	private void CheckCollision(Collider other) {
		if (!other.transform.CompareTag(targetTag)) return;
		var pedestrian = other.transform.GetComponent<Pedestrian>();
		if (pedestrian.IsWaiting) return;
		if (Selection.activeGameObject == gameObject) Debug.Log(other.name, this);
		//vehicle.SetColliding(Vehicle.ColliderType.Pedestrian, pedestrian.IsCrossing);
		colliding = true;
	}

	private void OnTriggerExit(Collider other) {
		if (!other.transform.CompareTag(targetTag)) return;
		if (Selection.activeGameObject == gameObject) Debug.Log(other.name, this);
		//vehicle.SetColliding(Vehicle.ColliderType.Pedestrian, false);
		colliding = false;
	}
}