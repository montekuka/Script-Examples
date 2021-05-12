using System;
using UnityEngine;
using UnityEngine.Serialization;

public class TrafficLight : MonoBehaviour {
	public TrafficType type;
	public GameObject[] redLights;
	public GameObject[] yellowLights;
	public GameObject[] greenLights;

	public void SetLight(LightColor targetColor) {
		switch (targetColor) {
			case LightColor.Green:
				foreach (var greenLight in greenLights) { greenLight.SetActive(true); }
				foreach (var yellowLight in yellowLights) { yellowLight.SetActive(false); }
				foreach (var redLight in redLights) { redLight.SetActive(false); }
				break;
			case LightColor.Yellow:
				foreach (var greenLight in greenLights) { greenLight.SetActive(false); }
				foreach (var yellowLight in yellowLights) { yellowLight.SetActive(true); }
				foreach (var redLight in redLights) { redLight.SetActive(false); }
				break;
			case LightColor.Red:
				foreach (var greenLight in greenLights) { greenLight.SetActive(false); }
				foreach (var yellowLight in yellowLights) { yellowLight.SetActive(false); }
				foreach (var redLight in redLights) { redLight.SetActive(true); }
				break;
		}
	}

	// private void OnDrawGizmos() {
	// 	Gizmos.color = Color.green;
	// 	foreach (var greenLight in greenLights) { Gizmos.DrawWireSphere(greenLight.transform.position, 0.1f); }
	// 	Gizmos.color = Color.yellow;
	// 	foreach (var yellowLight in yellowLights) { Gizmos.DrawWireSphere(yellowLight.transform.position, 0.1f); }
	// 	Gizmos.color = Color.red;
	// 	foreach (var redLight in redLights) {
	// 		Gizmos.DrawWireSphere(redLight.transform.position, 0.1f);
	// 		;
	// 	}
	// }
}

public enum LightColor {
	Green,
	Yellow,
	Red
}