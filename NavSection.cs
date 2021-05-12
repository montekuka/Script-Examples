using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavSection : MonoBehaviour {
	// -------------------------------------------------------------------
	// Properties

	[Header("Nav Section")]
	public VehicleSpawn[] vehicleSpawns;
	public NavConnection[] connections;
	public int speedLimit = 20;

	// -------------------------------------------------------------------
	// Initialization

	int[] usedIndexes;
	private int usedIndexIndex;

	public void Awake() { usedIndexes = new int[vehicleSpawns.Length]; }

	// -------------------------------------------------------------------
	// Get Data

	public bool TryGetVehicleSpawn(out VehicleSpawn spawn) {
		if (m_CurrentVehicles.Count < vehicleSpawns.Length) {
			var index = UnityEngine.Random.Range(0,                                   vehicleSpawns.Length);
			while (usedIndexes.Contains(index)) { index = UnityEngine.Random.Range(0, vehicleSpawns.Length); }
			usedIndexes[usedIndexIndex] = index;
			usedIndexIndex++;
			spawn = vehicleSpawns[index];
			return true;
		}
		spawn = null;
		return false;
	}

	// -------------------------------------------------------------------
	// Vehicle Management

	private List<Vehicle> m_CurrentVehicles = new List<Vehicle>();

	public void RegisterVehicle(Vehicle input, bool isAdd) {
		if (isAdd) m_CurrentVehicles.Add(input);
		else {
			if (m_CurrentVehicles.Contains(input)) m_CurrentVehicles.Remove(input);
			else Debug.LogWarning("Traffic: Attempted to remove non-existing vehicle from Road: " + gameObject.name);
		}
	}
}

[Serializable]
public class VehicleSpawn {
	public Transform spawn;
	public RoadPoint destination;
}