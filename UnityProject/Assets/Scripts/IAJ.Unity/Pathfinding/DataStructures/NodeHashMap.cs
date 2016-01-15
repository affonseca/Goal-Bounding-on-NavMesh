using RAIN.Navigation.Graph;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{

    //very simple (and unefficient) implementation of the open/closed sets
    public class NodeHashMap : IClosedSet
    {
        private Dictionary<NavigationGraphNode, NodeRecord> NodeRecords { get; set; }

        public NodeHashMap()
        {

            this.NodeRecords = new Dictionary<NavigationGraphNode, NodeRecord>();
            this.NodeRecords.Clear();

        }

        public void Initialize()
        {
            this.NodeRecords.Clear();
        }

        public int Count()
        {
            return this.NodeRecords.Count;
        }

        public void AddToClosed(NavigationGraphNode key, NodeRecord value)
        {
            this.NodeRecords.Add(key, value);
        }

        public void RemoveFromClosed(NavigationGraphNode key, NodeRecord nodeRecord)
        {
            this.NodeRecords.Remove(key);
        }

        public NodeRecord SearchInClosed(NavigationGraphNode key, NodeRecord nodeRecord)
        {
            if (this.NodeRecords.Keys.Contains(key))
                return (NodeRecord)this.NodeRecords[key];
            else
                return null;
        }
        public ICollection<NodeRecord> All()
        {

            return this.NodeRecords.Values;
        }
    }
}

