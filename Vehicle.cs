using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Vehicle : MonoBehaviour
{
	[Header("Wheels")]
	public WheelCollider[] frontWheels;

	public WheelCollider[] backWheels;

	[Header("Misc")]
	public MeshRenderer body;

	[Range(20, 45)]
	public int steerAngle;

	public   int       power, brakeTorque, bodyMaterialIndex;
	internal RoadPoint currentTarget;
	internal RoadPoint futureTarget;
	private RoadPoint stoppingPoint;
	private  Rigidbody rigid;
	private VehicleSensor vehicleSensor;
	private  Ray       ray;
	private  float     zVelocity;
	private bool      stop;
	private  bool      initialized;
	
	[Header("Debugging")]
	public List<Collider> colliders = new List<Collider>(10);
	

	public Vehicle()
	{
		WheelControlling = new WheelControlling(this);
	}

	private WheelControlling WheelControlling { get; }

	public bool Stop
	{
		get => stop;
		set
		{
			//if (Selection.activeGameObject == gameObject) Debug.Log(new StackTrace().GetFrame(1).GetMethod().Name + " " + value);
			stop = value;
		}
	}

	private void Awake()
	{
		body.materials[bodyMaterialIndex] = new Material(body.materials[bodyMaterialIndex])
		{
			color = new Color(Random.Range(0.4f, 0.8f), Random.Range(0.4f, 0.8f), Random.Range(0.4f, 0.8f))
		};
		rigid = GetComponent<Rigidbody>();
		vehicleSensor = GetComponentInChildren<VehicleSensor>();
	}

	public void SetInitialTarget(RoadPoint target)
	{
		currentTarget = target;
		if (currentTarget != null)
		{
			futureTarget = currentTarget.connectedPoints.Count > 0 ? currentTarget.connectedPoints[Random.Range(0, currentTarget.connectedPoints.Count)] : null;
		}

		initialized = true;
	}

	private void Update()
	{
		zVelocity = transform.InverseTransformDirection(rigid.velocity).z;
		if (!initialized) return;
		if (currentTarget != null)
		{
			if (Vector3.Distance(transform.position, currentTarget.transform.position) < 0.5f + (zVelocity / 6))
			{
				SwitchToNextTarget();
			}
			HandleStopPoint();
			SetSteeringAimTransform();
		}
		else
		{
			FindNewTarget();
		}

		EvaluateStop();

	}

	private void FixedUpdate()
	{
		WheelControlling.UpdateSteerAngle();
	}

	private void EvaluateStop()
	{
		if (stoppingPoint != null)
		{
			EvaluateStoppingPoint();
			if (Selection.activeGameObject == gameObject && !Stop) Debug.Log("1");
			if (Vector3.Distance(transform.position, stoppingPoint.transform.position) > 4)
			{
				stoppingPoint = null;
			}
		}

		if (stoppingPoint == null)
		{
			Stop = vehicleSensor.ObstacleDetected();
		}

		if (currentTarget == null)
		{
			Stop = true;
		}
	}

	private void FindNewTarget()
	{
		currentTarget = TrafficSystem.Instance.GetNewVehicleDestination(transform.position);
		if (currentTarget != null)
		{
			futureTarget = currentTarget.connectedPoints.Count > 0 ? currentTarget.connectedPoints[Random.Range(0, currentTarget.connectedPoints.Count)] : null;
		}
	}

	private  Quaternion targetSteering;
	private  Quaternion steeringSmoothed;
	internal Vector3    steerAngles;
	public   float      steerSmoothingSpeed = 1.5f;

	private void SetSteeringAimTransform()
	{
		var position = currentTarget.transform.position;
		targetSteering   = Quaternion.LookRotation(position - transform.position, transform.up);
		targetSteering.x = 0;
		steeringSmoothed = Quaternion.Lerp(steeringSmoothed, targetSteering, steerSmoothingSpeed * Time.deltaTime * Time.timeScale);
		steerAngles      = (Quaternion.Inverse(transform.rotation) * steeringSmoothed).eulerAngles;
	}

	private void HandleStopPoint()
	{
		if (currentTarget.roadState == RoadPoint.PointState.Stop)
		{
			if (stoppingPoint == null)
			{
				stoppingPoint = currentTarget;
			}
		}
	}

	private void SwitchToNextTarget()
	{
		if (currentTarget.connectedPoints.Count > 0)
		{
			currentTarget = futureTarget;
			if (currentTarget != null)
			{
				if (currentTarget.connectedPoints.Count > 0)
				{
					futureTarget = currentTarget.connectedPoints[Random.Range(0, currentTarget.connectedPoints.Count)];
				}
				else
				{
					futureTarget = null;
				}
			}
		}
	}

	private  void EvaluateStoppingPoint()
	{
		if (stoppingPoint.roadState == RoadPoint.PointState.Stop && Vector3.Distance(transform.position, stoppingPoint.transform.position) < 4)
		{
			// if (transform.InverseTransformPoint(stoppingPoint.transform.position).normalized.z > 0.2f)
			// {
			// 	return Vector3.Distance(transform.position, stoppingPoint.transform.position) < 0.2f + (zVelocity);
			// }

			Stop = true;
		}
		else
		{
			
			Stop = false;
		}
	}

	void OnDrawGizmos()
	{
		if (Selection.activeGameObject != gameObject) return;
		if (currentTarget != null)
		{
			var targetPosition = currentTarget.transform.position;
			var futurePosition = Vector3.zero;
			if (futureTarget != null)
			{
				futurePosition = futureTarget.transform.position;
				Gizmos.color   = Color.red;
				Gizmos.DrawWireSphere(futurePosition, 0.5f);
			}

			targetPosition.y += 0.1f;
			futurePosition.y += 0.1f;
			Gizmos.color     =  Color.red;
			Gizmos.DrawLine(transform.position, targetPosition);
			Gizmos.DrawLine(targetPosition,     futurePosition);
			Gizmos.DrawWireSphere(currentTarget.transform.position, 0.5f);
		}

		// Gizmos.color = Color.green;
		//
		// var transformForward = (Quaternion.Euler(steerAngles) * transform.forward * (1 + (zVelocity * 2)));
		// //Gizmos.DrawLine(transform.position, transform.position + transformForward);
		// Gizmos.DrawLine(transform.position, transform.position + transform.forward * 4);
		// Gizmos.DrawWireSphere(transform.position + transform.forward * 4, 1f);

		if (stoppingPoint != null)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(stoppingPoint.transform.position, 0.2f + (zVelocity / 2));
		}
	}
}

internal class WheelControlling
{
	private Vehicle vehicle;

	internal WheelControlling(Vehicle vehicle)
	{
		this.vehicle = vehicle;
	}

	internal void UpdateSteerAngle()
	{
		var steer = vehicle.steerAngles.y;
		foreach (var currentWheel in vehicle.frontWheels)
		{
			if (steer <= 360 && steer >= 360 - vehicle.steerAngle)
			{
				currentWheel.steerAngle = steer;
			}
			else if (steer <= 360 - vehicle.steerAngle && steer >= 180)
			{
				currentWheel.steerAngle = -vehicle.steerAngle;
			}
			else if (steer <= -vehicle.steerAngle && steer >= -180)
			{
				currentWheel.steerAngle = -vehicle.steerAngle;
			}
			else if (steer >= 0 && steer <= vehicle.steerAngle)
			{
				currentWheel.steerAngle = steer;
			}
			else if (steer >= vehicle.steerAngle && steer <= 180)
			{
				currentWheel.steerAngle = vehicle.steerAngle;
			}
			else
			{
				currentWheel.steerAngle = steer;
			}
		}

		foreach (var currentWheel in vehicle.backWheels)
		{
			var currentSpeed = vehicle.currentTarget == null ? 4 : vehicle.currentTarget.speed;

			if (currentWheel.rpm < (currentSpeed * 30) && vehicle.Stop == false)
			{
				currentWheel.motorTorque = vehicle.power;
				currentWheel.brakeTorque = 0;
			}
			else
			{
				currentWheel.motorTorque = 0;
				currentWheel.brakeTorque = vehicle.brakeTorque;
			}
			//if (Selection.activeGameObject == vehicle.gameObject) Debug.Log(currentWheel.name + " " + currentWheel.motorTorque);
		}
	}
}