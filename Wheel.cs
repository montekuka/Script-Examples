using UnityEngine;

public class Wheel : MonoBehaviour
{
	public  WheelCollider wheelCollider;
	private Vector3       wheelPosition;
	private Quaternion    wheelRotation;

	private void Update()
	{
		wheelCollider.GetWorldPose(out wheelPosition, out wheelRotation);
		transform.position = wheelPosition;
		transform.rotation = wheelRotation;
		wheelCollider.ConfigureVehicleSubsteps(5, 12, 15);
	}
}