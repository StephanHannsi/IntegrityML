using System;
using System.Collections.Generic;
using System.Text;

namespace Calculation.Models
{
    public class Results
    {
        public Dictionary<string, LocalPoint> LocalCoordinates { get; set; }
        public Dictionary<string, ProjectCoordinates> ProjectCoordinates { get; set; }
    }

    public class ProjectCoordinates
    {
        public string PointId { get; set; }

        public double? Northing { get; set; }
        public double? Easting { get; set; }
        public double? Elevation { get; set; }

        public double? NorthingStdDev { get; set; }
        public double? EastingStdDev { get; set; }
        public double? ElevationStdDev { get; set; }
    }
}
