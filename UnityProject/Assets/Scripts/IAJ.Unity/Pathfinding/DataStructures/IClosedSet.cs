using RAIN.Navigation.Graph;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public interface IClosedSet
    {
        void Initialize();
        void AddToClosed(NavigationGraphNode key, NodeRecord value);
        void RemoveFromClosed(NavigationGraphNode key, NodeRecord nodeRecord);
        //should return null if the node is not found
        NodeRecord SearchInClosed(NavigationGraphNode key, NodeRecord nodeRecord);
        ICollection<NodeRecord> All();
    }
}
