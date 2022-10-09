using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    //public bool onlyDisplayPathGizmos;
    public bool displayGridGizmos;
    public Transform player;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    //how much individual node cover
    public float nodeRadius;
   

    Node[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Awake(){
        nodeDiameter = nodeRadius*2;
        //calculate how many node that can fit in the gridWorldSize.x/nodeDiameter;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
        CreateGrid();
    }

    public int MaxSize{
        get{
            return gridSizeX * gridSizeY;
        }
    }
    void CreateGrid(){
     
        grid = new Node[gridSizeX,gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;

        for(int x = 0; x< gridSizeX; x++)
        {
            for(int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right*
                (x*nodeDiameter + nodeRadius) + Vector3.forward * (y*nodeDiameter+nodeRadius);
                //collision check
                //if there collison(CheckSphere) check walkable to
                bool walkable = !(Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask));
                grid[x,y] = new Node(walkable,worldPoint,x,y);
            
            }
        }
    }

    //return list of Node
    public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbours.Add(grid[checkX,checkY]);
				}
			}
		}

		return neighbours;
	}

    //the player position in the grid
    public Node NodeFromWorldPoint(Vector3 worldPosition) {
        float percentX =  (worldPosition.x + gridWorldSize.x/2)/ gridWorldSize.x;
        float percentY =  (worldPosition.z + gridWorldSize.y/2)/ gridWorldSize.y;
        //Calmp01: if player ourside the grid, it does not give invalid
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX-1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY-1) * percentY);

        return grid[x,y];

    }

    //public List<Node> path;
  
    

    void OnDrawGizmos(){
        //reason theres y because we will use it for z
        // y represent z
        Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,1,gridWorldSize.y));
		if (grid != null && displayGridGizmos) {
            //Node playerNode = NodeFromWorldPoint(player.position);
            foreach(Node n in grid){
                //is n walkable
                Gizmos.color = (n.walkable)?Color.white:Color.red;
                /*if(playerNode == n){
                    Gizmos.color = Color.cyan;
                }
                if(path!= null){
                    if(path.Contains(n)){
                        Gizmos.color = Color.black;
                    }
                }*/
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter-.1f));

            }
        }
        
        
    }
}
