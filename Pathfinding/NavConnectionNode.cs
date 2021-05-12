using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding {
[System.Serializable]
public class NavConnectionNode : IHeapItem<NavConnectionNode> {
	private int heapIndex;
	public Vector3 worldPosition;
	public List<NavConnectionNode> neighbours = new List<NavConnectionNode>();
	public int movementPenalty;
	public NavConnectionNode parent;
	public NavConnection correspondingConnection;
	public bool walkable = true;

	public int HeapIndex { get => heapIndex; set => heapIndex = value; }

	private  int FCost => GCost + HCost;
	internal int HCost { get; set; }
	internal int GCost { get; set; }

	public int CompareTo(NavConnectionNode nodeToCompare) {
		var compare = FCost.CompareTo(nodeToCompare.FCost);
		if (compare == 0) {
			compare = HCost.CompareTo(nodeToCompare.HCost);
		}
		return -compare;
	}
}
}