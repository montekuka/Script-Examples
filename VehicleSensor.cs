using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VehicleSensor : MonoBehaviour
{
	public           List<Transform> collidingObjects = new List<Transform>();
	public           float           maxDistance;
	private          bool            hitDetected;
	public           Vector3         boxCastSize;
	public           Vector3         boxCastCenter;
	public           Transform       vehicleTransform;
	public           float           carHitDistance;
	private          Vehicle         vehicle;
	private readonly RaycastHit[]    obstacleHits = new RaycastHit[25];

	private void Awake() { vehicle = GetComponentInParent<Vehicle>(); }
	private void Start() { StartCoroutine(CheckForObstacles()); }

	private void ResetAll()
	{
		collidingObjects.Clear();
		hitDetected = false;
	}

	private IEnumerator CheckForObstacles()
	{
		while (true)
		{
			ResetAll();
			hitDetected = CheckForVehiclesInFront();
			yield return new WaitForSeconds(0.15f);
		}
	}

	public bool ObstacleDetected()
	{
		return hitDetected;
	}

	private int     hitCount;
	private Vector3 one;
	private Vector3 two;
	public LayerMask castingLayers;

	private bool CheckForVehiclesInFront()
	{
		var vehicleLayer = vehicleTransform.gameObject.layer;
		vehicleTransform.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		hitCount                          = Physics.BoxCastNonAlloc(vehicleTransform.position + vehicleTransform.TransformVector(boxCastCenter), boxCastSize / 2, vehicleTransform.forward, obstacleHits, vehicleTransform.rotation, maxDistance, castingLayers);
		vehicleTransform.gameObject.layer = vehicleLayer;

		for (int i = 0; i < hitCount; i++)
		{
			collidingObjects.Add(obstacleHits[i].transform);
			
			if (obstacleHits[i].transform.CompareTag($"Vehicle"))
			{
				if (Vector3.Distance(vehicleTransform.position, obstacleHits[i].point) > carHitDistance) continue;
				var   collidingCarDir = obstacleHits[i].transform.forward;
				float direction       = Vector3.Dot(collidingCarDir, vehicleTransform.forward);
				if (direction < 0) continue;
				var otherVehicle = obstacleHits[i].transform.GetComponent<Vehicle>();
				if (otherVehicle.currentTarget != vehicle.currentTarget && otherVehicle.currentTarget != vehicle.futureTarget) continue;

				if (Selection.activeGameObject == vehicle.gameObject)
				{
					Debug.Log(direction);
					Math3d.ClosestPointsOnTwoLines(out one, out two, transform.position, transform.forward.normalized, obstacleHits[i].transform.position, obstacleHits[i].transform.forward.normalized);
					Debug.Log(Vector3.Distance(one, two));
					Debug.Log("Hitting " + obstacleHits[i].collider.name, obstacleHits[i].transform);
				}

				return true;
			}

			if (obstacleHits[i].transform.CompareTag("Unit"))
			{
				if (!obstacleHits[i].transform.GetComponent<Pedestrian>().IsCrossing) continue;
				if (Selection.activeGameObject == gameObject) Debug.Log("Hitting " + obstacleHits[i].collider.name, obstacleHits[i].transform);
				return true;
			}
		}

		return false;
	}

	void OnDrawGizmos()
	{
		if (Selection.activeTransform != transform.parent && Selection.activeTransform != transform) return;
		Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
		DrawCarDistance();
		DrawBoxCast();
		Gizmos.matrix = oldGizmosMatrix;
		DrawHits();
	}

	private void DrawHits()
	{
		for (int i = 0; i < hitCount; i++)
		{
			Gizmos.color = hitDetected ? Color.red : Color.yellow;
			Gizmos.DrawRay(vehicleTransform.position, obstacleHits[i].point - vehicleTransform.position);
			Gizmos.DrawSphere(obstacleHits[i].point, 0.25f);
			var boxStart = vehicleTransform.position + vehicleTransform.TransformVector(boxCastCenter) + vehicleTransform.forward * obstacleHits[i].distance;
			Gizmos.matrix = Matrix4x4.TRS(boxStart, vehicleTransform.rotation, vehicleTransform.lossyScale);
			Gizmos.DrawWireCube(Vector3.zero, boxCastSize);
		}
	}

	private void DrawBoxCast()
	{
		Gizmos.color = Color.grey;
		var test = boxCastCenter;
		test.z *= maxDistance / 2;
		var test2 = boxCastSize;
		test2.z *= maxDistance;
		var boxStart2 = vehicleTransform.position + vehicleTransform.TransformVector(test); // + vehicleTransform.forward;
		Gizmos.matrix = Matrix4x4.TRS(boxStart2, vehicleTransform.rotation, vehicleTransform.lossyScale);
		Gizmos.DrawWireCube(Vector3.zero, test2);
	}

	private void DrawCarDistance()
	{
		var vehicleForward = vehicleTransform.forward.normalized;
		vehicleForward   *= carHitDistance;
		vehicleForward.y =  0.25f;
		Gizmos.DrawRay(transform.position, vehicleForward);
	}
}