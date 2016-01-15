using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using RAIN.Navigation.NavMesh;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.PreComputing
{
    class DijkstraFloodfill
    {
        public NodeRecordArray NodeRecords { get; protected set; }

        public DijkstraFloodfill(NodeRecordArray nodeRecords) {
            this.NodeRecords = nodeRecords;
        }

        public void flood(NodeRecord startingNode) {
            NodeRecords.clearRecord();

            startingNode.parent = null;
            startingNode.edgeFromStart = -1;
            startingNode.fValue = 0;
            startingNode.status = NodeStatus.Open;
                
            initializeFlood(startingNode);

            startingNode.status = NodeStatus.Closed;

            while (NodeRecords.CountOpen() > 0) {
                var currentNode = NodeRecords.GetBestAndRemove();

                for (int i = 0; i < currentNode.node.OutEdgeCount; i++)
                {
                    
                    NodeRecord childNode = this.NodeRecords.GetNodeRecord(currentNode.node.EdgeOut(i).ToNode);
                    this.pushNode(childNode, currentNode, currentNode.edgeFromStart, 
                        currentNode.fValue + (currentNode.node.Position - childNode.node.Position).magnitude);
                }
                
                currentNode.status = NodeStatus.Closed;
            }
           
        }

        private void initializeFlood(NodeRecord startingNode)
        {
            for (int i = 0; i < startingNode.node.OutEdgeCount; i++) {
                NodeRecord nodeRecord = this.NodeRecords.GetNodeRecord(startingNode.node.EdgeOut(i).ToNode);
                
                this.pushNode(nodeRecord, startingNode, i, (startingNode.node.Position - nodeRecord.node.Position).magnitude);
            }
        }

        private void pushNode(NodeRecord nodeRecord, NodeRecord parent, int edgeGraphIndex, float givenCost)
        {
            

            var nodeInOpen = NodeRecords.SearchInOpen(nodeRecord.node, nodeRecord);

            if(nodeRecord.status == NodeStatus.Unvisited || nodeRecord.fValue > givenCost) {
                
                nodeRecord.parent = parent;
                nodeRecord.edgeFromStart = edgeGraphIndex;
                nodeRecord.fValue = givenCost;

                if (nodeRecord.status == NodeStatus.Unvisited)
                {
                    this.NodeRecords.AddToOpen(nodeRecord.node, nodeRecord);
                }
                else if (nodeRecord.status == NodeStatus.Open)
                {
                    this.NodeRecords.Replace(nodeInOpen, nodeRecord);
                }
            }
        }

    }
}
