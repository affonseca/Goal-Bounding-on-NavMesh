using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using RAIN.Navigation.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Utils.FileUtils
{
    class FileReader
    {
        public string fileName { get; set; }

        public NodeRecordArray readFile(NodeRecordArray nodeRecordArray)
        {
            StreamReader sr = new StreamReader(fileName);
            string line = sr.ReadLine();
            int i = -1;
            int j = -1;
            //Continue to read until you reach end of file
            while (line != null)
            {
                string[] lineAux = null;

                //Read the next line
                switch (line) {
                    case "<nodeRecord>":
                        i = -1;
                        j++;
                        break;
                    case "  <bounds>":
                        i++;
                        break;
                    case "</nodeRecord>":
                        break;
                    case "  </bounds>":
                        break;
                    default:
                        lineAux = line.Split('<', '>');
                        break;
                }
                
                if (line.Contains("<minX>"))
                {

                    nodeRecordArray.NodeRecords[j].edgeBounds[i].minX=float.Parse(lineAux[2]);
                }
                else if (line.Contains("<minZ>"))
                {
                    nodeRecordArray.NodeRecords[j].edgeBounds[i].minZ = float.Parse(lineAux[2]);
                }
                else if (line.Contains("<maxX>"))
                {
                    nodeRecordArray.NodeRecords[j].edgeBounds[i].maxX = float.Parse(lineAux[2]);
                }
                else if (line.Contains("<maxZ>"))
                {
                    nodeRecordArray.NodeRecords[j].edgeBounds[i].maxZ = float.Parse(lineAux[2]);
                }

                line = sr.ReadLine();

            }
            sr.Close();
            return nodeRecordArray;    
        }
        
    }
}
