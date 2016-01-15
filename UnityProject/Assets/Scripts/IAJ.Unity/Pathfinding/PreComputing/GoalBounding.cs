using System;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using System.Linq;
using System.Text;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Utils.FileUtils;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.PreComputing
{
    class GoalBounding
    {
        private NodeRecordArray nodeInfos;
        private FileWriter fileWriter;
        private int numberOfProcessedNodes = 0;

        public GoalBounding(NavMeshPathGraph navMesh, BoundingBox mapSize) {

            //Hack to access private field with all the nodes of the mesh
            List<NavigationGraphNode> nodeList = (List<NavigationGraphNode>)Utils.Reflection.GetInstanceField(
                    typeof(RAINNavigationGraph), navMesh, "_pathNodes");
            nodeInfos = new NodeRecordArray(nodeList, mapSize);
        }

        public bool update() {

            if(numberOfProcessedNodes == 0)
                Debug.Log("Starting analysis of " + nodeInfos.NodeRecords.Length + " nodes...");

            if (numberOfProcessedNodes >= nodeInfos.NodeRecords.Length)
                return true;

            DijkstraFloodfill dijkstra = new DijkstraFloodfill(nodeInfos);

            var node = nodeInfos.NodeRecords[numberOfProcessedNodes];
            dijkstra.flood(node);

            ICollection<NodeRecord> closedList = ((IClosedSet)nodeInfos).All();

            foreach (var closedNode in closedList)
            {
                int startingEdge = closedNode.edgeFromStart;
                if (startingEdge < 0)
                    continue;


                if (startingEdge >= node.edgeBounds.Length)
                {
                    continue;
                }
                float nodeX = closedNode.node.Position.x;
                float nodeZ = closedNode.node.Position.z;

                if (node.edgeBounds[startingEdge].minX > nodeX)
                {
                    node.edgeBounds[startingEdge].minX = nodeX;
                }
                if (node.edgeBounds[startingEdge].maxX < nodeX)
                {
                    node.edgeBounds[startingEdge].maxX = nodeX;
                }
                if (node.edgeBounds[startingEdge].minZ > nodeZ)
                {
                    node.edgeBounds[startingEdge].minZ = nodeZ;
                }
                if (node.edgeBounds[startingEdge].maxZ < nodeZ)
                {
                    node.edgeBounds[startingEdge].maxZ = nodeZ;
                }
            }
            numberOfProcessedNodes++;
            Debug.Log("Analysis at " + (((float)numberOfProcessedNodes / nodeInfos.NodeRecords.Length) * 100).ToString("F2") + 
                "% ( " + numberOfProcessedNodes +"/" +nodeInfos.NodeRecords.Length + " nodes )");

            if (numberOfProcessedNodes == nodeInfos.NodeRecords.Length)
            {

                fileWriter = new FileWriter();
                fileWriter.fileName = "goalBoundingInfo.xml";
                fileWriter.data = nodeInfos;
                fileWriter.writeFile();
                Debug.Log("Analysis complete!");
                return true;
            }
            else
                return false;
                
        }
    }
}
