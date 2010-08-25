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

        public static OpponentPathNode GetNodeCloseToPlayer()
        {
            float leastDistance = int.MaxValue;
            OpponentPathNode leastDistantNode = null;
            Vector3 playerPos = Race.Current.PlayerVehicle.Position;

            foreach (OpponentPathNode node in Nodes)
            {
                float thisDistance = Vector3.Distance(playerPos, node.Position);
                if (thisDistance < leastDistance)
                {
                    leastDistance = thisDistance;
                    leastDistantNode = node;
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

        /// <summary>
        /// Choose a random path. More likely to choose a race path than a general path
        /// </summary>
        /// <param name="currentNode"></param>
        /// <returns></returns>
        public static OpponentPath GetNextPath(OpponentPathNode currentNode)
        {
            if (currentNode.Paths.Count == 0) return null;

            foreach (OpponentPath path in currentNode.Paths)
            {
                if (path.Type == PathType.Cheat)
                    return path;
            }

            int choosenPath = Engine.Random.Next(currentNode.Paths.Count);
            if (currentNode.Paths[choosenPath].Type == PathType.Race)
                return currentNode.Paths[choosenPath];

            // 1/3 times, we re-choose the race path
            if (Engine.Random.Next() % 3 == 0)
            {
                foreach (OpponentPath path in currentNode.Paths)
                {
                    if (path.Type == PathType.Race)
                        return path;
                }
            }

            // if we cant find a race path, go with our first choice
            return currentNode.Paths[choosenPath];
        }
    }
}
