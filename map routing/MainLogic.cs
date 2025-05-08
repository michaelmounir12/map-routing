using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MainLogic
{
    const double walkSpeed = 5.0d;

    public static double EuclideanDistance(double x1, double y1, double x2, double y2)
    {
        double dx = x2 - x1;
        double dy = y2 - y1;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public static Models.Result Solver(
        Tuple<double, double> point1, Tuple<double, double> point2, long R,
        int N, Dictionary<int, (double, double)> IdToCoor,
        Dictionary<int, List<Tuple<int, double, double>>> edges)
    {
        double startX = point1.Item1, startY = point1.Item2;
        double destX = point2.Item1, destY = point2.Item2;

        var prev = Enumerable.Repeat(-1, N).ToList(); // Fixed initialization
        var bestTime = Enumerable.Repeat(double.PositiveInfinity, N).ToList();

        int lastVertexBeforeDest = -1;
        var pq = new PriorityQueue<Models.Node, double>();

        double Rkm = R / 1000.0;

        // Check if we can directly walk from start to destination (special case)
        double directDistance = EuclideanDistance(startX, startY, destX, destY);
        if (directDistance <= Rkm)
        {
            // Return direct walking path with no vertices
            double directTime = directDistance / walkSpeed;
            return new Models.Result
            {
                Path = new List<int>(), // Empty path - direct walk
                Time = directTime * 60, // Convert to minutes
                TotalDist = directDistance,
                TotalWalk = directDistance,
                VehicleDist = 0
            };
        }

        foreach (var kvp in IdToCoor)
        {
            var (x, y) = kvp.Value;
            int vid = kvp.Key;
            double d = EuclideanDistance(x, y, startX, startY);
            if (d <= Rkm)
            {
                double t = d / walkSpeed;
                bestTime[vid] = t;
                prev[vid] = -1;
                pq.Enqueue(new Models.Node(vid, x, y, d, 0.0, t, -1), t);
            }
        }

        double totalTime = double.PositiveInfinity;  // Initialize to infinity instead of 0
        double minWalk = 0, minNonWalk = 0;

        while (pq.Count > 0)
        {
            var cur = pq.Dequeue();

            // If we've already reached destination with a better path, skip
            if (cur.TotalTime >= totalTime && cur.VertexId != -1)
                continue;

            if (cur.VertexId == -1)  // We've reached the destination
            {
                totalTime = cur.TotalTime;
                minWalk = cur.WalkDistance;
                minNonWalk = cur.NonWalkDistance;
                lastVertexBeforeDest = cur.PrevId;
                break;
            }

            int curId = cur.VertexId;
            if (bestTime[curId] < cur.TotalTime) continue;
            bestTime[curId] = cur.TotalTime;

            // Check if we can walk to destination from here
            double walkDist = EuclideanDistance(cur.X, cur.Y, destX, destY);
            if (walkDist <= Rkm)
            {
                double newWalk = cur.WalkDistance + walkDist;
                double newTime = cur.TotalTime + walkDist / walkSpeed;

                if (newTime < totalTime)
                {
                    pq.Enqueue(new Models.Node(-1, destX, destY,
                                               newWalk, cur.NonWalkDistance,
                                               newTime, curId), newTime);
                }
            }

            if (edges.ContainsKey(curId))  
            {
                foreach (var edge in edges[curId])
                {
                    int nid = edge.Item1;
                    double dist = edge.Item2;
                    double speed = edge.Item3;
                    double tEdge = dist / speed;
                    double newTime = cur.TotalTime + tEdge;

                    if (newTime < bestTime[nid])
                    {
                        bestTime[nid] = newTime;
                        prev[nid] = curId;
                        var (nx, ny) = IdToCoor[nid];
                        pq.Enqueue(new Models.Node(nid, nx, ny,
                                                   cur.WalkDistance,
                                                   cur.NonWalkDistance + dist,
                                                   newTime, curId), newTime);
                    }
                }
            }
        }

        // If no path was found
        if (double.IsPositiveInfinity(totalTime))
        {
            return new Models.Result
            {
                Path = new List<int>(), // Empty path
                Time = double.PositiveInfinity,
                TotalDist = 0,
                TotalWalk = 0,
                VehicleDist = 0
            };
        }

        var pathVerts = new List<int>();
        for (int v = lastVertexBeforeDest; v != -1; v = prev[v])
            pathVerts.Add(v);
        pathVerts.Reverse();

        double totalDistance = minWalk + minNonWalk;

        return new Models.Result
        {
            Path = pathVerts,
            Time = totalTime * 60, // Convert hours to minutes for output
            TotalDist = totalDistance,
            TotalWalk = minWalk,
            VehicleDist = Math.Round(minNonWalk, 2, MidpointRounding.AwayFromZero)
        };
    }
}