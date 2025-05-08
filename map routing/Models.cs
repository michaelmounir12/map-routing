using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Models
{
    public class Result
    {
        public List<int> Path { get; set; } = new List<int>();
        public double Time { get; set; }
        public double TotalDist { get; set; }
        public double TotalWalk { get; set; }
        public double VehicleDist { get; set; }
    }

    public record Node(int VertexId, double X, double Y,
                       double WalkDistance, double NonWalkDistance, double TotalTime,
                       int PrevId);
}