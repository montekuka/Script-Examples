using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrafficSystem))]
public class TrafficSystemEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		var myScript = (TrafficSystem)target;
		if ( GUILayout.Button("Link Connections") ) {
			myScript.LinkConnections();
		}
	}
}