using UnityEngine;

public class WaitZone : MonoBehaviour {
	// -------------------------------------------------------------------
	// Properties

	public TrafficType type;
	public WaitZone opposite;

	// -------------------------------------------------------------------
	// State

	public bool CanPass { get; set; } = false;

	public Vector3 RandomPointInBounds() {
		var bounds = GetComponent<Collider>().bounds;
		return new Vector3(
			Random.Range(bounds.min.x, bounds.max.x),
			Random.Range(bounds.min.y, bounds.max.y),
			Random.Range(bounds.min.z, bounds.max.z)
		);
	}
}