using System.Collections.Generic;
using RAIN.Navigation.Graph;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class Path
    {
        public List<NavigationGraphNode> PathNodes { get; set; }
        public List<Vector3> PathPositions { get; set; } 
        public bool IsPartial { get; set; }

        public Path()
        {
            this.PathNodes = new List<NavigationGraphNode>();
            this.PathPositions = new List<Vector3>();
        }
    }
}
