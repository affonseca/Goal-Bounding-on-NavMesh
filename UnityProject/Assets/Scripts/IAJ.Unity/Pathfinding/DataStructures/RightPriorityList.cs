using RAIN.Navigation.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class RightPriorityList : IOpenSet, IComparer<NodeRecord>
    {
        private List<NodeRecord> Open { get; set; }

        public RightPriorityList()
        {
            this.Open = new List<NodeRecord>();    
        }
        public void Initialize()
        {
            this.Open.Clear();
        }

        public void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace)
        {
            this.RemoveFromOpen(null, nodeToReplace);
            this.AddToOpen(null, nodeToBeReplaced);
        }

        public NodeRecord GetBestAndRemove()
        {
            var best = this.PeekBest();
            this.RemoveFromOpen(null, best);
            return best;
        }

        public NodeRecord PeekBest()
        {
            return this.Open.Last();
        }

        public void AddToOpen(NavigationGraphNode key, NodeRecord nodeRecord)
        {
            //a little help here
            //is very nice that the List<T> already implements a binary search method
            int index = this.Open.BinarySearch(nodeRecord,this);
            if (index < 0)
            {

                this.Open.Insert(~index, nodeRecord);
            }
            else
                this.Replace(nodeRecord, this.Open[index]);
        }

        public void RemoveFromOpen(NavigationGraphNode key, NodeRecord nodeRecord)
        {
            int index = this.Open.BinarySearch(nodeRecord,this);
            if (index >= 0)
            {
                this.Open.RemoveAt(index);
            }
        }

        public NodeRecord SearchInOpen(NavigationGraphNode key, NodeRecord nodeRecord)
        {
            int index = this.Open.BinarySearch(nodeRecord,this);
            if (index >= 0)
            {
                return this.Open[index];
            }
            return Open.FirstOrDefault(n => n.Equals(nodeRecord));
        }

        public ICollection<NodeRecord> All()
        {
            return this.Open;
        }

        public int CountOpen()
        {
            return Open.Count;
        }
       
        public int Compare(NodeRecord x, NodeRecord y)
        {
            return -x.CompareTo(y);
        }
    }
}
