using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using UnityEngine;
using RAIN.Navigation;
using RAIN.Navigation.NavMesh;
using RAIN.Navigation.Graph;
using Assets.Scripts.IAJ.Unity.Pathfinding.PreComputing;

public class PathfindingManager : MonoBehaviour {

	//public fields to be set in Unity Editor
	public GameObject startDebugSphere;
	public GameObject endDebugSphere;
	public Camera myCamera;

    //goal bounding variables
    bool isPreComputing = false;
    bool usingPartial = false;

	//private fields for internal use only
	private Vector3 startPosition;
	private Vector3 endPosition;
	private NavMeshPathGraph navMesh;
	private ushort currentClickNumber;
    private GoalBounding goalbounding;

    private NodeArrayAStarPathFinding aStarPathFinding;
    //private AStarPathfinding aStarPathFinding;
    private Path currentSolution;

    private bool draw;

	// Use this for initialization
	void Awake ()
	{
	    this.draw = false;
		this.currentClickNumber = 1;
		this.navMesh = NavigationManager.Instance.NavMeshGraphs [0];
        
        BoundingBox mapSize = new BoundingBox();
        mapSize.minX = -200;
        mapSize.minZ = -200;
        mapSize.maxX = 200;
        mapSize.maxZ = 200;
        this.goalbounding = new GoalBounding(navMesh, mapSize);

        this.aStarPathFinding = new NodeArrayAStarPathFinding(this.navMesh,new SimpleHeuristic(),true);
	    this.aStarPathFinding.NodesPerSearch = 100;
	}
	
	// Update is called once per frame
	void Update () 
    {
		Vector3 position;

        if (Input.GetKeyDown(KeyCode.A)) {
            this.isPreComputing = !this.isPreComputing;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            this.usingPartial = !this.usingPartial;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            this.aStarPathFinding.UsingGoalBounding = !this.aStarPathFinding.UsingGoalBounding;
        }

        if(this.isPreComputing)
            goalbounding.update();

        if (Input.GetMouseButtonDown(0)) 
		{
			//if there is a valid position
			if(this.MouseClickPosition(out position))
			{
				
				//if this is the first click we're setting the start point
				if(this.currentClickNumber == 1)
				{
					//show the start sphere, hide the end one
					//this is just a small adjustment to better see the debug sphere
					this.startDebugSphere.transform.position = position + Vector3.up;
					this.startDebugSphere.SetActive(true);
					this.endDebugSphere.SetActive(false);
					this.currentClickNumber = 2;
					this.startPosition = position;
				    this.currentSolution = null;
				    this.draw = false;
				}
				else 
				{
					//we're setting the end point
					//this is just a small adjustment to better see the debug sphere
					this.endDebugSphere.transform.position = position + Vector3.up;
					this.endDebugSphere.SetActive(true);
					this.currentClickNumber = 1;
					this.endPosition = position;
				    this.draw = true;
                    //initialize the search algorithm
                    this.aStarPathFinding.InitializePathfindingSearch(this.startPosition,this.endPosition);
				}
			}
		}

        //call the pathfinding method if the user specified a new goal
	    if (this.aStarPathFinding.InProgress)
	    {
	        var finished = this.aStarPathFinding.Search(out this.currentSolution, usingPartial);
	        if (finished && this.currentSolution != null)
	        {
	            //here I would make a character follow the path   
	        }
	    }
	}

    public void OnGUI()
    {
        if (this.currentSolution != null)
        {
            var time = this.aStarPathFinding.TotalProcessingTime*1000;
            float timePerNode;
            if (this.aStarPathFinding.TotalProcessedNodes > 0)
            {
                timePerNode = time/this.aStarPathFinding.TotalProcessedNodes;
            }
            else
            {
                timePerNode = 0;
            }
            var text = "Nodes Visited: " + this.aStarPathFinding.TotalProcessedNodes
                       + "\nMaximum Open Size: " + this.aStarPathFinding.MaxOpenNodes
                       + "\nProcessing time (ms): " + time
                       + "\nTime per Node (ms):" + timePerNode;
            GUI.contentColor = Color.black;
            GUI.Label(new Rect(10,10,200,100),text);
        }

        var onWord = "On";
        var offWord = "Off";

        var precomputeWord = this.isPreComputing ? onWord : offWord;
        var usingPartialWord = this.usingPartial ? onWord : offWord;
        var usingGoalBoundingWord = this.aStarPathFinding.UsingGoalBounding ? onWord : offWord;

        var instructions = "Options:\n" +
                            "A - Precompute Goal Bounds (" +precomputeWord + ")\n" +
                            "S - Use partial Solutions in A* (" +usingPartialWord + ")\n" +
                            "D - Use Goal Bounding in A* (" +usingGoalBoundingWord + ")\n";

        GUI.contentColor = Color.black;
        GUI.Label(new Rect(800, 675, 250, 100), instructions);


    }

    public void OnDrawGizmos()
    {
        if (this.draw)
        {
            //draw the current Solution Path if any (for debug purposes)
            if (this.currentSolution != null)
            {
                var previousPosition = this.startPosition;
                foreach (var pathPosition in this.currentSolution.PathPositions)
                {
                    Debug.DrawLine(previousPosition, pathPosition, Color.red);
                    previousPosition = pathPosition;
                }
            }

            //draw the nodes in Open and Closed Sets
            if (this.aStarPathFinding != null)
            {
                Gizmos.color = Color.cyan;

                if (this.aStarPathFinding.Open != null)
                {
                    foreach (var nodeRecord in this.aStarPathFinding.Open.All())
                    {
                        Gizmos.DrawSphere(nodeRecord.node.LocalPosition, 1.0f);
                    }
                }

                Gizmos.color = Color.blue;

                if (this.aStarPathFinding.Closed != null)
                {
                    foreach (var nodeRecord in this.aStarPathFinding.Closed.All())
                    {
                        Gizmos.DrawSphere(nodeRecord.node.LocalPosition, 1.0f);
                    }
                }
            }
        }
    }

	private bool MouseClickPosition(out Vector3 position)
	{
		RaycastHit hit;

		var ray = this.myCamera.ScreenPointToRay (Input.mousePosition);
		//test intersection with objects in the scene
		if (Physics.Raycast (ray, out hit)) 
		{
			//if there is a collision, we will get the collision point
			position = hit.point;
			return true;
		}

		position = Vector3.zero;
		//if not the point is not valid
		return false;
	}
}
