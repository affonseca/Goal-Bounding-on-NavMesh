using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Assets.Scripts.IAJ.Unity.Utils.FileUtils
{
    class FileWriter
    {
        public string fileName { get; set; }
        public NodeRecordArray data { get; set; }

        public void writeFile()
        {
         

            StreamWriter sw = new StreamWriter(fileName);

            foreach(var nodeRecord in data.NodeRecords) {
                sw.WriteLine("<nodeRecord>");
                foreach (var boundingBox in nodeRecord.edgeBounds) {
                    sw.WriteLine("  <bounds>");
                    sw.WriteLine("      <minX>" + boundingBox.minX + "</minX>");
                    sw.WriteLine("      <minZ>" + boundingBox.minZ + "</minZ>");
                    sw.WriteLine("      <maxX>" + boundingBox.maxX + "</maxX>");
                    sw.WriteLine("      <maxZ>" + boundingBox.maxZ + "</maxZ>");
                    sw.WriteLine("  </bounds>");
                }
                sw.WriteLine("</nodeRecord>");
            }
            sw.Close();
            
        }

    }
}
