using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour {

    public delegate void TurnCallback(Vector2 turnPosition, Vector2 previousDirection, Vector2 currentDirection);

    public Transform seeker, target;
	Grid grid;

	void Awake() {
		grid = GetComponent<Grid> ();
	}

	void Update() {
		FindPath (seeker.position, target.position);
	}

	void FindPath(Vector3 startPos, Vector3 targetPos) {

        Stopwatch sw = new Stopwatch();
        sw.Start();

		Node startNode = grid.NodeFromWorldPoint(startPos);
		Node targetNode = grid.NodeFromWorldPoint(targetPos);

		Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);

		while (openSet.Count > 0) {
			Node node = openSet.RemoveFirst();
			closedSet.Add(node);

			if (node == targetNode) {
                sw.Stop();
                //print("Path found: " + sw.ElapsedMilliseconds + " ms"); 
				RetracePath(startNode,targetNode);
				return;
			}

			foreach (Node neighbor in grid.GetNeighbors(node)) {
				if (!neighbor.walkable || closedSet.Contains(neighbor)) {
					continue;
				}

				int newCostToNeighbour = node.gCost + GetDistance(node, neighbor);
				if (newCostToNeighbour < neighbor.gCost || !openSet.Contains(neighbor)) {
                    neighbor.gCost = newCostToNeighbour;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = node;

					if (!openSet.Contains(neighbor))
						openSet.Add(neighbor);
                    openSet.UpdateItem(neighbor);
				}
			}
		}
	}

	void RetracePath(Node startNode, Node endNode) {
		List<Node> path = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		path.Reverse();

		grid.path = path;

	}

    public static void ForEachTurn(List<Node> path, TurnCallback callback) {
        List<Node> turns = new List<Node>();

        for (int i = 2; i < path.Count; i++) {
            Node lastNode = path[i - 2];
            Node previousNode = path[i - 1];
            Node currentNode = path[i];

            Vector2 previousDirection = new Vector2(previousNode.worldPosition.x - lastNode.worldPosition.x, previousNode.worldPosition.y - lastNode.worldPosition.y).normalized;
            Vector2 currentDirection = new Vector2(currentNode.worldPosition.x - previousNode.worldPosition.x, currentNode.worldPosition.y - previousNode.worldPosition.y).normalized;

            if (previousDirection != currentDirection) { // If direction has been changed...
                callback(previousNode.worldPosition, previousDirection, currentDirection);
            }
            else { // If direction has been changed...

            }
        }
    }

	int GetDistance(Node nodeA, Node nodeB) {
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		if (dstX > dstY)
			return 14*dstY + 10* (dstX-dstY);
		return 14*dstX + 10 * (dstY-dstX);
	}
}