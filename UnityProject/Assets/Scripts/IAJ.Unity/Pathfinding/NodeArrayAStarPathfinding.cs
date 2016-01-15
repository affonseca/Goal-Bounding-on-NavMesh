using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Utils.FileUtils;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class NodeArrayAStarPathFinding : AStarPathfinding
    {
        protected NodeRecordArray NodeRecordArray { get; set; }
        private FileReader fileReader;
        public bool UsingGoalBounding { get; set; }
        int partialMaxOpenNodes = 0;
        public NodeArrayAStarPathFinding(NavMeshPathGraph graph, IHeuristic heuristic, bool usingGoalBounding) : base(graph, null, null, heuristic)
        {
            //do not change this
            var nodes = this.GetNodesHack(graph);
            this.NodeRecordArray = new NodeRecordArray(nodes, new BoundingBox());
            this.Open = this.NodeRecordArray;
            this.Closed = this.NodeRecordArray;
            this.UsingGoalBounding = usingGoalBounding;
            this.fileReader = new FileReader();
            if (usingGoalBounding)
            {
                fileReader.fileName = "goalBoundingInfo.xml";
                NodeRecordArray = fileReader.readFile(this.NodeRecordArray);

            }

        }

        #pragma warning disable 108 //Hiding inherited member
        public void InitializePathfindingSearch(Vector3 startPosition, Vector3 goalPosition)
        #pragma warning restore 108
        {

            NodeRecordArray.clearRecord();

   //NodeIndex is deprecated
            this.StartPosition = startPosition;
            this.GoalPosition = goalPosition;

            if (this.StartNode != null) {
                ((NavMeshPoly)this.StartNode).RemoveConnectedPoly();
            }

            if (this.GoalNode != null)
            {
                ((NavMeshPoly)this.GoalNode).RemoveConnectedPoly();
            }


            this.StartNode = this.Quantize(this.StartPosition);
            this.GoalNode = this.Quantize(this.GoalPosition);

            //if it is not possible to quantize the positions and find the corresponding nodes, then we cannot proceed
            if (this.StartNode == null || this.GoalNode == null) return;

            //I need to do this because in Recast NavMesh graph, the edges of polygons are considered to be nodes and not the connections.
            //Theoretically the Quantize method should then return the appropriate edge, but instead it returns a polygon
            //Therefore, we need to create one explicit connection between the polygon and each edge of the corresponding polygon for the search algorithm to work
            ((NavMeshPoly)this.StartNode).AddConnectedPoly(this.StartPosition);
            ((NavMeshPoly)this.GoalNode).AddConnectedPoly(this.GoalPosition);

            this.InProgress = true;
            this.TotalProcessedNodes = 0;
            this.TotalProcessingTime = 0.0f;
            this.MaxOpenNodes = 0;

            var initialNode = new NodeRecord
            {
                gValue = 0,
                hValue = this.Heuristic.H(this.StartNode, this.GoalNode),
                node = this.StartNode
            };

            initialNode.fValue = AStarPathfinding.F(initialNode);


            this.NodeRecordArray.AddToOpen(initialNode.node, initialNode);
            
            
            }



        //don't forget to add the override keyword here if you define a ProcessChildNode method in the base class
        protected void ProcessChildNode(NodeRecord bestNode, NavigationGraphEdge connectionEdge)
        {

            var childNode = connectionEdge.ToNode;
            var childNodeRecord = this.NodeRecordArray.GetNodeRecord(childNode);

            if (childNodeRecord == null)
            {
                //this piece of code is used just because of the special start nodes and goal nodes added to the RAIN Navigation graph when a new search is performed.
                //Since these special goals were not in the original navigation graph, they will not be stored in the NodeRecordArray and we will have to add them
                //to a special structure
                //it's ok if you don't understand this, this is a hack and not part of the NodeArrayA* algorithm, just do NOT CHANGE THIS, or you algorithm wont work
                childNodeRecord = new NodeRecord
                {
                    node = childNode,
                    parent = bestNode,
                    status = NodeStatus.Unvisited
                };
                this.NodeRecordArray.AddSpecialCaseNode(childNodeRecord);
            }

            var nodeInOpen = NodeRecordArray.SearchInOpen(childNode, childNodeRecord);

            float g = bestNode.gValue + connectionEdge.Cost;
            float h = this.Heuristic.H(childNode, this.GoalNode);
            float f = g + h;

            if (childNodeRecord.status == NodeStatus.Unvisited || childNodeRecord.fValue > f)
            {
                childNodeRecord.gValue = g;
                childNodeRecord.hValue = h;
                childNodeRecord.fValue = f;
                childNodeRecord.parent = bestNode;

                if (childNodeRecord.status == NodeStatus.Unvisited)
                {
                    this.NodeRecordArray.AddToOpen(childNode, childNodeRecord);
                    partialMaxOpenNodes++;
                }
                else if (childNodeRecord.status == NodeStatus.Open)
                {
                    this.NodeRecordArray.Replace(nodeInOpen, childNodeRecord);
                }
                else if (childNodeRecord.status == NodeStatus.Closed)
                {
                    this.NodeRecordArray.RemoveFromClosed(childNode, childNodeRecord);
                    this.NodeRecordArray.AddToOpen(childNode, childNodeRecord);
                }

            }

        }

        public override bool Search(out Path solution, bool returnPartialSolution = false)
        {

            float startTime = Time.realtimeSinceStartup;
            float endTime;
            TotalProcessedNodes = 0;
            while (this.NodeRecordArray.CountOpen() > 0) 
            {
                var currentNode = NodeRecordArray.GetBestAndRemove();


                if (currentNode.node.Equals(GoalNode))
                {
                    endTime = Time.realtimeSinceStartup;
                    TotalProcessingTime = endTime - startTime;
                    solution = CalculateSolution(currentNode, false);
                    this.InProgress = false;
                    return true;
                }

                if (returnPartialSolution && TotalProcessedNodes >= this.NodesPerSearch)
                {
                    endTime = Time.realtimeSinceStartup;
                    TotalProcessingTime = endTime - startTime;
                    solution = CalculateSolution(currentNode, true);
                    return false;
                }

                TotalProcessedNodes++;
                this.NodeRecordArray.AddToClosed(currentNode.node, currentNode);

                var outConnections = currentNode.node.OutEdgeCount;
                for (int i = 0; i < outConnections; i++)
                {

                    var childNode = currentNode.node.EdgeOut(i);

                    
                    if (UsingGoalBounding && 
                        currentNode.node != this.StartNode &&
                        childNode.ToNode != StartNode &&
                        childNode.ToNode != GoalNode && 
                        !withinGoalBounds(currentNode, i)
                    )
                        continue;
                    
                    this.ProcessChildNode(currentNode, childNode);
                }
            }

            endTime = Time.realtimeSinceStartup;
            TotalProcessingTime = endTime - startTime;
            solution = null;
            return true;
        }


        private List<NavigationGraphNode> GetNodesHack(NavMeshPathGraph graph)
        {
            //this hack is needed because in order to implement NodeArrayA* you need to have full acess to all the nodes in the navigation graph in the beginning of the search
            //unfortunately in RAINNavigationGraph class the field which contains the full List of Nodes is private
            //I cannot change the field to public, however there is a trick in C#. If you know the name of the field, you can access it using reflection (even if it is private)
            //using reflection is not very efficient, but it is ok because this is only called once in the creation of the class
            //by the way, NavMeshPathGraph is a derived class from RAINNavigationGraph class and the _pathNodes field is defined in the base class,
            //that's why we're using the type of the base class in the reflection call
            return (List<NavigationGraphNode>)Utils.Reflection.GetInstanceField(typeof(RAINNavigationGraph), graph, "_pathNodes");
        }

        private bool withinGoalBounds(NodeRecord currentNode, int edgeIndex)
        {
            var currentEdgeBounds = currentNode.edgeBounds[edgeIndex];

            if (  GoalNode.Position.x < currentEdgeBounds.maxX &&
                  GoalNode.Position.x > currentEdgeBounds.minX &&
                  GoalNode.Position.z < currentEdgeBounds.maxZ &&
                  GoalNode.Position.z > currentEdgeBounds.minZ)
            {
                
                return true;
            }

            return false;
        }

    }
}
