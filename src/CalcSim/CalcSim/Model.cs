using System;
using System.Collections.Generic;

namespace CalcSim
{
    public class Config
    {
        public string FailSavePath { get; set; }
        public string DeviceId { get; set; }
        public DateTime StartTime { get; set; }
        public int Interval { get; set; }
        public int Days { get; set; }
        public double AddOutliers {get; set; }
        public double Temperature { get; set; }
        public TerrestrialStation Station{ get; set; }
        public Dictionary<string,Target> Targets{ get; set; }
    }

    public class Target
    {
        public string id { get; set; }
        public double Abplattung { get; set; }
        public double MoveScale { get; set; }
        public double RandomScale { get; set; }
        public TerrestrialPosition Position { get; set; }
    }

    public class TerrestrialRoundMessage
    {
        public string DeviceId { get; set; }
        public string RoundId { get; set; }
        public DateTime RoundStartUtc { get; set; }
        public DateTime? RoundEndUtc { get; set; }
        public TerrestrialStation Station { get; set; }
        public TerrestrialObservation[] Observations { get; set; }
    }

    public class TerrestrialStation
    {
        public string SerialNumber { get; set; }
        public string PointId { get; set; }
        public TerrestrialPosition Position { get; set; }
    }

    public class TerrestrialPosition
    {
        public double Easting { get; set; } = double.NaN;
        public double Northing { get; set; } = double.NaN;
        public double Elevation { get; set; } = double.NaN;
    }

    public class TerrestrialObservation
    {
        public string PointId { get; set; }
        public DateTime TimeUtc { get; set; }
        public double TargetHeight { get; set; }
        public double PrismConstant { get; set; }
        public double? EdmDistance { get; set; }
        public double? HorizontalCircleReading { get; set; }
        public double? VerticalAngle { get; set; }
        public string ObservationType { get; set; }
        public double? Pressure { get; set; }
        public double? Temperature { get; set; }
        public int Lockmode { get; set; }
        public int RoundSet { get; set; }
        public double? CompensatorTiltX { get; set; }
        public double? CompensatorTiltY { get; set; }
        public TerrestrialPosition Position { get; set; }
    }
}
