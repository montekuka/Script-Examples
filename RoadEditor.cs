using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Road))]
public class RoadEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		if (GUILayout.Button("Smooth road (needs to be ordered!)")) {
			//((Road) target).SmoothRoad();
		}
	}
}