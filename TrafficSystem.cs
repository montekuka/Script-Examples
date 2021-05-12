using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum TrafficType {
	Pedestrian,
	Vehicle
}

public class TrafficSystem : MonoBehaviour {
	// -------------------------------------------------------------------
	// Singleton
	private static TrafficSystem instance;
	public static TrafficSystem Instance {
		get {
			if (instance == null) instance = FindObjectOfType<TrafficSystem>();
			return instance;
		}
	}

	// -------------------------------------------------------------------
	// Properties

	public bool drawGizmos;
	public GameObject pedestrianPrefab; // TODO - Get from object pool
	public GameObject vehiclePrefab;    // TODO - Get from object pool
	public Transform pool;              // TODO - Get from object pool
	public bool spawnOnStart = true;
	public int maxRoadVehicles = 100;
	public int maxPedestrians = 100;
	public NavConnection[] connections;
	public NavSection[] navSections;
	private RoadPoint[] roadPoints;

	// -------------------------------------------------------------------
	// Initialization

	private List<Road> m_Roads = new List<Road>();

	private void Start() {
		Instance.LinkConnections();
		var roadsFound = FindObjectsOfType<Road>();
		roadPoints = FindObjectsOfType<RoadPoint>();
		foreach (var r in roadsFound) m_Roads.Add(r);

		if (spawnOnStart) {
			StartCoroutine(SpawnVehiclesAsync());
			StartCoroutine(SpawnPedestrianAsync());
		}
	}

	IEnumerator SpawnPedestrianAsync() {
		for (var i = 0; i < maxPedestrians; i++) {
			SpawnPedestrian(true);
			yield return null;
		}
	}

	IEnumerator SpawnVehiclesAsync() {
		for (var i = 0; i < maxRoadVehicles; i++) {
			SpawnRoadVehicle(true);
			yield return null;
		}
	}

	public void LinkConnections() {
		var roadObjects = FindObjectsOfType<RoadObject>();
		foreach (var roadObject in roadObjects) { roadObject.UpdateRoad(false); }
	}

	// -------------------------------------------------------------------
	// Update

	private void Update() {
		if (Input.GetKeyUp(KeyCode.Backspace)) SpawnPedestrian(true);
		if (Input.GetKeyUp(KeyCode.Return)) SpawnRoadVehicle(true);
	}

	// -------------------------------------------------------------------
	// Spawn

	private int roadVehicleSpawnAttempts;
	private int trainSpawnAttempts;
	private int pedestrianSpawnAttempts;

	private int vehicleCount;
	private readonly Dictionary<NavSection, List<int>> usedSpawns = new Dictionary<NavSection, List<int>>();

	private void SpawnRoadVehicle(bool reset) {
		while (true) {
			if (reset) roadVehicleSpawnAttempts = 0;
			var index                           = Random.Range(0, m_Roads.Count);
			var road                            = m_Roads[index];
			if (!road.TryGetVehicleSpawn(out var spawn)) {
				roadVehicleSpawnAttempts++;
				//if (roadVehicleSpawnAttempts >= m_Roads.Count) return;
				reset = false;
				continue;
			}
			var newVehicle = Instantiate(vehiclePrefab, spawn.spawn.position, spawn.spawn.rotation, pool.transform).GetComponent<Vehicle>();
			newVehicle.SetInitialTarget(spawn.destination);
			newVehicle.name += " " + vehicleCount;
			vehicleCount++;

			break;
		}
	}

	public NavConnection GetVehicleDestination(NavConnection spawnDest) {
		var randomDest = Random.Range(0,                                                                                                                              connections.Length);
		while (connections[randomDest] == spawnDest || connections[randomDest].smoothingType == NavConnection.SmoothingType.Smoothing) { randomDest = Random.Range(0, connections.Length); }
		return connections[randomDest];
	}

	public int pedestrianCount;

	private void SpawnPedestrian(bool reset) {
		while (true) {
			if (reset) pedestrianSpawnAttempts = 0;
			var index                          = Random.Range(0, m_Roads.Count);
			var road                           = m_Roads[index];
			if (!road.TryGetPedestrianSpawn(out var spawn)) {
				pedestrianSpawnAttempts++;
				if (pedestrianSpawnAttempts >= m_Roads.Count) return;
				reset = false;
				continue;
			}
			var newAgent = ModelSupply.Instance.GetRandomPedestrian(spawn.position, spawn.rotation, pool.transform).GetComponent<Pedestrian>();
			newAgent.transform.GetChild(0).GetComponent<CharacterCustomization>().Init();
			newAgent.animator =  newAgent.transform.GetChild(0).GetComponent<CharacterCustomization>().animators[1];
			newAgent.name     += " " + pedestrianCount;
			pedestrianCount++;
			newAgent.Initialize();
			break;
		}
	}

	// -------------------------------------------------------------------
	// Navigation

	public Transform GetPedestrianDestination() {
		while (true) {
			var index = Random.Range(0, m_Roads.Count);
			var road  = m_Roads[index];
			if (!road.TryGetPedestrianSpawn(out var destination)) continue;
			return destination;
		}
	}

	public RoadPoint GetNewVehicleDestination(Vector3 vehiclePosition)
	{
		RoadPoint nearestPoint;
		if (roadPoints.Length > 0)
		{
			float dist = 0;
			dist = Vector3.Distance(vehiclePosition, roadPoints[0].transform.position);
			nearestPoint = roadPoints [0];
			for (int i = 1; i < roadPoints.Length; i++)
			{
				if (Vector3.Distance(vehiclePosition, roadPoints[i].transform.position) < dist)
				{
					dist = Vector3.Distance(vehiclePosition, roadPoints[i].transform.position);
					nearestPoint = roadPoints [i];
				}
			}
		}
		else
		{
			return null;
		}

		return nearestPoint.GetComponent<RoadPoint> ();
	}

	// -------------------------------------------------------------------
	// Static

	public static float GetAgentSpeedFromKph(int kph) { return kph / 3.6f; }
}