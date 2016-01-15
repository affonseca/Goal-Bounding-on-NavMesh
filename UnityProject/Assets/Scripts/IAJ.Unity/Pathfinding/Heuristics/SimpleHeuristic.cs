using RAIN.Navigation.Graph;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class SimpleHeuristic : IHeuristic
    {
        public float H(NavigationGraphNode node, NavigationGraphNode goalNode)
        {
            return (goalNode.Position - node.Position).magnitude;
        }
    }
}
