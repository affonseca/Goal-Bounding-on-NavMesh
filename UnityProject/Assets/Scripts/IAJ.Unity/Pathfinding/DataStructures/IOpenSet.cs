using RAIN.Navigation.Graph;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public interface IOpenSet
    {
        void Initialize();
        void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace);
        NodeRecord GetBestAndRemove();
        NodeRecord PeekBest();
        void AddToOpen(NavigationGraphNode key, NodeRecord value);
        void RemoveFromOpen(NavigationGraphNode key, NodeRecord value);
        //should return null if the node is not found
        NodeRecord SearchInOpen(NavigationGraphNode key, NodeRecord value);
        ICollection<NodeRecord> All();
        int CountOpen();
    }
}
