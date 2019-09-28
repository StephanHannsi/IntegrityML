using System;
using System.Collections.Generic;
using System.Text;

namespace InterpretationML.Models
{
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
        public double? CompensatorTiltX { get; set; }
        public double? CompensatorTiltY { get; set; }
        public double? Pressure { get; set; }
        public double? Temperature { get; set; }
        public int Lockmode { get; set; }
        public int RoundSet { get; set; }
        public bool Flagged { get; set; } = false;
        public TerrestrialPosition Position { get; set; }
    }
}
