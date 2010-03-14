using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;

namespace Carmageddon
{
    static class OpponentController
    {
        public static List<OpponentPathNode> Nodes;


        public static OpponentPathNode GetClosestRaceNode(Vector3 currentPosition)
        {
            float leastDistance = int.MaxValue;
            OpponentPathNode leastDistantNode = null;

            foreach (OpponentPathNode node in Nodes)
            {
                float thisDistance = Vector3.Distance(currentPosition, node.Position);
                if (thisDistance < leastDistance)
                {
                    if (node.Paths.Exists(a => a.Type == PathType.Race))
                    {
                        leastDistance = thisDistance;
                        leastDistantNode = node;
                    }
                }
            }

            return leastDistantNode;
        }
        public static OpponentPathNode GetClosestNode(Vector3 currentPosition)
        {
            float leastDistance = int.MaxValue;
            OpponentPathNode leastDistantNode = null;

            foreach (OpponentPathNode node in Nodes)
            {
                float thisDistance = Vector3.Distance(currentPosition, node.Position);
                if (thisDistance < leastDistance)
                {
                    leastDistance = thisDistance;
                    leastDistantNode = node;
                }
            }

            return leastDistantNode;
        }

        public static OpponentPathNode GetRandomNode()
        {
            return Nodes[Engine.Random.Next(Nodes.Count)];
        }

        public static OpponentPath GetNextPath(OpponentPathNode currentNode)
        {
            if (currentNode.Paths.Count == 0) return null;

            //return currentNode.Paths[Engine.RandomNumber.Next(currentNode.Paths.Count)];
            
            foreach (OpponentPath path in currentNode.Paths)
            {
                if (path.Type == PathType.Race)
                    return path;
            }
            return currentNode.Paths[0];
        }
    }
}
