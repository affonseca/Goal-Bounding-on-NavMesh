using System;
using System.Collections.Generic;
using RAIN.Navigation.Graph;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class NodeRecordArray : IOpenSet, IClosedSet
    {
        public NodeRecord[] NodeRecords { get; private set; }
        private List<NodeRecord> SpecialCaseNodes { get; set; } 
        private NodePriorityHeap Open { get; set; }

       

        public NodeRecordArray(List<NavigationGraphNode> nodes, BoundingBox mapSize)
        {
            //this method creates and initializes the NodeRecordArray for all nodes in the Navigation Graph
            this.NodeRecords = new NodeRecord[nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                #pragma warning disable 0618  //NodeIndex is deprecated
                node.NodeIndex = i; //we're setting the node Index because RAIN does not do this automatically
                #pragma warning restore 0618

                NodeRecord nodeRecord = new NodeRecord { node = node,
                                                         status = NodeStatus.Unvisited
                                                       };
                nodeRecord.edgeBounds = new BoundingBox[node.OutEdgeCount];
                for (int j = 0; j < nodeRecord.edgeBounds.Length; j++)
                {
                    BoundingBox boundingBox = new BoundingBox();
                    boundingBox.maxX = mapSize.minX;
                    boundingBox.maxZ = mapSize.minZ;
                    boundingBox.minX = mapSize.maxX;
                    boundingBox.minZ = mapSize.maxZ;

                    nodeRecord.edgeBounds[j] = boundingBox;
                }

                this.NodeRecords[i] = nodeRecord;
            }

            this.SpecialCaseNodes = new List<NodeRecord>();

            this.Open = new NodePriorityHeap();
        }

        public NodeRecord GetNodeRecord(NavigationGraphNode node)
        {
            //do not change this method
            //here we have the "special case" node handling

            #pragma warning disable 0618  //NodeIndex is deprecated
            if (node.NodeIndex == -1)
            #pragma warning restore 0618
            {
                for (int i = 0; i < this.SpecialCaseNodes.Count; i++)
                {
                    if (node == this.SpecialCaseNodes[i].node)
                    {
                        return this.SpecialCaseNodes[i];
                    }
                }
                return null;
            }
            else
            {
                #pragma warning disable 0618  //NodeIndex is deprecated
                return this.NodeRecords[node.NodeIndex];
                #pragma warning restore 0618
            }
        }

        public void AddSpecialCaseNode(NodeRecord node)
        {
            this.SpecialCaseNodes.Add(node);
        }

        void IOpenSet.Initialize()
        {
            this.Open.Initialize();
            //we want this to be very efficient (that's why we use for)
            for (int i = 0; i < this.NodeRecords.Length; i++)
            {
                this.NodeRecords[i].status = NodeStatus.Unvisited;
            }

            this.SpecialCaseNodes.Clear();
        }

        void IClosedSet.Initialize()
        {
            this.Open.Initialize();
            //we want this to be very efficient (that's why we use for)
            for (int i = 0; i < this.NodeRecords.Length; i++)
            {
                this.NodeRecords[i].status = NodeStatus.Unvisited;
            }

            this.SpecialCaseNodes.Clear();
        }

        public void AddToOpen(NavigationGraphNode key, NodeRecord nodeRecord)
        { 
            nodeRecord.status = NodeStatus.Open;
            Open.AddToOpen(key, nodeRecord);
        }

        public void AddToClosed(NavigationGraphNode key, NodeRecord nodeRecord)
        {
            nodeRecord.status = NodeStatus.Closed;
            Open.RemoveFromOpen(key, nodeRecord);
        }

        public NodeRecord SearchInOpen(NavigationGraphNode key, NodeRecord nodeRecord)
        {
            return Open.SearchInOpen(key, nodeRecord);
        }

        public NodeRecord SearchInClosed(NavigationGraphNode key, NodeRecord nodeRecord)
        {
            //errado
            return Open.SearchInOpen(key, nodeRecord);
        }

        public NodeRecord GetBestAndRemove()
        {
            NodeRecord aux = Open.GetBestAndRemove();
            aux.status = NodeStatus.Closed;
            return aux;
        }

        public NodeRecord PeekBest()
        {
            return Open.PeekBest();
        }

        public void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace)
        {
            nodeToBeReplaced.status = NodeStatus.Closed;
            nodeToReplace.status = NodeStatus.Open;
            this.Open.Replace(nodeToBeReplaced, nodeToReplace);
        }

        public void RemoveFromOpen(NavigationGraphNode key, NodeRecord nodeRecord)
        {
            nodeRecord.status = NodeStatus.Closed;
            this.Open.RemoveFromOpen(key, nodeRecord);
        }

        public void RemoveFromClosed(NavigationGraphNode key, NodeRecord nodeRecord)
        {
            nodeRecord.status = NodeStatus.Unvisited;
            this.Open.RemoveFromOpen(key, nodeRecord);
        }

        ICollection<NodeRecord> IOpenSet.All()
        {
           return this.Open.All();
        }

        ICollection<NodeRecord> IClosedSet.All()
        {
            ICollection<NodeRecord> aux = new List<NodeRecord>();
            for (int i = 0; i < NodeRecords.Length; i++)
            {
                if (NodeRecords[i].status == NodeStatus.Closed)
                {
                    aux.Add(NodeRecords[i]);
                    
                }
            }
            return aux;
        }

        public int CountOpen()
        {
            return this.Open.CountOpen();
        }

        public void clearRecord() {
            for (int i = 0; i < NodeRecords.Length; i++)
            {
                this.NodeRecords[i].status = NodeStatus.Unvisited;
            }
            this.Open = new NodePriorityHeap();
        }
    }
}
