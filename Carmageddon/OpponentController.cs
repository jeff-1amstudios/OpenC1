using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using OneAmEngine;

namespace Carmageddon
{
    static class OpponentController
    {
        public static List<OpponentPathNode> Nodes;


        //public static OpponentPathNode GetClosestRaceNode(Vector3 currentPosition)
        //{
        //    float leastDistance = int.MaxValue;
        //    OpponentPathNode leastDistantNode = null;

        //    foreach (OpponentPathNode node in Nodes)
        //    {
        //        float thisDistance = Vector3.Distance(currentPosition, node.Position);
        //        if (thisDistance < leastDistance)
        //        {
        //            if (node.Paths.Exists(a => a.Type == PathType.Race))
        //            {
        //                leastDistance = thisDistance;
        //                leastDistantNode = node;
        //            }
        //        }
        //    }

        //    return leastDistantNode;
        //}

        public static OpponentPath GetClosestPath(Vector3 point)
        {
            float closestDist = float.MaxValue;
            OpponentPath closestPath = null;
            foreach (OpponentPathNode node in Nodes)
            {
                foreach (OpponentPath path in node.Paths)
                {
                    Vector3 closestPoint = Helpers.GetClosestPointOnLine(path.Start.Position, path.End.Position, point);
                    float dist = Vector3.Distance(point, closestPoint);
                    if (dist < closestDist)
                    {
                        closestPath = path;
                        closestDist = dist;
                    }
                }
            }

            return closestPath;
        }


        public static OpponentPathNode GetNodeCloseToPlayer()
        {
            Vector3 playerPos = Race.Current.PlayerVehicle.Position;

            int j = Engine.Random.Next(Nodes.Count);
            for (int i = 0; i < Nodes.Count; i++)
            {
                float dist = 0;
                Vector3.Distance(ref playerPos, ref Nodes[j].Position, out dist);
                if (dist > 100 && dist < 280 && Helpers.HasTimePassed(5, Nodes[j].LastUsedTime))
                {
                    Nodes[j].LastUsedTime = Engine.TotalSeconds;
                    return Nodes[j];
                }
                j++;
                j %= Nodes.Count;
            }

            return Nodes[j];
        }

        public static OpponentPathNode GetClosestNode(Vector3 currentPosition)
        {
            float leastDistance = int.MaxValue;
            OpponentPathNode leastDistantNode = null;

            foreach (OpponentPathNode node in Nodes)
            {
                float distance;
                Vector3.Distance(ref currentPosition, ref node.Position, out distance);
                if (distance < leastDistance)
                {
                    leastDistance = distance;
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
