using UnityEngine;
using System.Collections;

    public class BoundingBox
    {

        public float minX { get; set; }
        public float maxX { get; set; }
        public float minZ { get; set; }
        public float maxZ { get; set; }
        public BoundingBox() {
            this.minX = 0;
            this.minZ = 0;
            this.maxX = 0;
            this.maxZ = 0;
        }
}
