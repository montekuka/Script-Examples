

using UnityEngine;

namespace Pathfinding {
public class Node : IHeapItem<Node> {
	private int gridX;
	private int gridY;
	private Vector3 worldPosition;
	private bool walkable;
	private int movementPenalty;

	private readonly int gCost;
	private readonly int hCost;
	public Node parent;

	public Node( bool walkable, Vector3 worldPos, int gridX, int gridY, int penalty, int gCost, int hCost) {
		this.walkable        = walkable;
		worldPosition   = worldPos;
		this.gridX           = gridX;
		this.gridY           = gridY;
		movementPenalty = penalty;
		this.gCost = gCost;
		this.hCost = hCost;
	}

	private int fCost => gCost + hCost;

	public int HeapIndex { get; set; }

	public int CompareTo( Node nodeToCompare ) {
		var compare = fCost.CompareTo(nodeToCompare.fCost);
		if ( compare == 0 ) {
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}
}
}