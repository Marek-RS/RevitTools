using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTRevitTools.Pathfinder
{
    public class GraphAnalyzer
    {
        PipingSystemPath _pipingSystemPath;
        private int v;

        private List<int>[] adjList;

        public GraphAnalyzer(int vertices, PipingSystemPath pipingSystemPath)
        {
            _pipingSystemPath = pipingSystemPath;
            // initialise vertex count
            this.v = vertices;

            // initialise adjacency list
            initAdjList();
        }

        private void initAdjList()
        {
            adjList = new List<int>[v];

            for (int i = 0; i < v; i++)
            {
                adjList[i] = new List<int>();
            }
        }

        public void AddEdge(int u, int v)
        {
            adjList[u].Add(v);
        }

        public void GetAllPaths(int s, int d)
        {
            bool[] isVisited = new bool[v];
            List<int> pathList = new List<int>();
            pathList.Add(s);
            GetAllPathsReccurent(s, d, isVisited, pathList);
        }

        private void GetAllPathsReccurent(int u, int d, bool[] isVisited, List<int> localPathList)
        {

            if (u.Equals(d))
            {
                _pipingSystemPath.PathfinderViewModel.AddNewDatagridItem(localPathList);
                return;
            }

            isVisited[u] = true;

            foreach (int i in adjList[u])
            {
                if (!isVisited[i])
                {
                    localPathList.Add(i);
                    GetAllPathsReccurent(i, d, isVisited, localPathList);
                    localPathList.Remove(i);
                }
            }
            isVisited[u] = false;
        }
    }
}
