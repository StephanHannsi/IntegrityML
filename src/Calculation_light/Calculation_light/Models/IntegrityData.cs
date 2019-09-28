using System;
using System.Collections.Generic;
using System.Text;

namespace Calculation.Models
{
    public class IntegrityData
    {
        public int IngerityState { get; set; } = 1;
        public Dictionary<string, ObservationIntegrity> ObservationIntegrity { get; set; }
        public Dictionary<string, ObservationIntegrityTime> ObservationIntegrityTime { get; set; }
        public Dictionary<string, EstimatedDist> EstimatedDistToRef { get; set; }
        public RoundIntegrity RoundIntegrity { get; set; }
    }

    public class ObservationIntegrity
    {
        public string PointId { get; set; }
        public bool Flag { get; set; } = false;
        public double HzDiff { get; set; }
        public double VzDiff { get; set; }
        public double EdmDiff { get; set; }
        public double CompTiltXDiff { get; set; }
        public double CompTiltYDiff { get; set; }
    }

    public class ObservationIntegrityTime
    {
        public string PointId { get; set; }
        public string Face { get; set; }
        public bool Flag { get; set; } = false;
        public double DistToRef { get; set; }
        public double DistToLast { get; set; }
        public double DistToAllTimeMean { get; set; }
        public double DistToTempMean { get; set; }
    }

    public class EstimatedDist
    {
        public string PointId { get; set; }
        public bool Flag { get; set; } = false;
        public double EstimatedDistToRef { get; set; }
    }

    public class RoundIntegrity
    {

    }
}
