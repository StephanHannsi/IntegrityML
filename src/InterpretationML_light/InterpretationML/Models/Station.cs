using System;
using System.Collections.Generic;
using System.Text;

namespace InterpretationML.Models
{
    public class Station
    {
        public string _id { get; set; }
        public string PointId { get; set; }
        public TerrestrialPosition Position { get; set; }
        public string ReferenceRound { get; set; }
        public Dictionary<string, RoundParams> TerrestrialRounds { get; set; }
        public double AbsThreshold { get; set; } = double.NaN;
        public Dictionary<string, TargetParams> Targets { get; set; }
    }

    public class RoundParams
    {
        public string ID { get; set; }
        public DateTime RoundStartUtc { get; set; }
        public DateTime? RoundEndUtc { get; set; }
        public bool Flagged { get; set; } = false;
        public List<string> FlaggedObs { get; set; }
    }

    public class TargetParams
    {
        public string ID { get; set; }
        public string RoundProcessed { get; set; }
        public AllTimeMean AllTimeMean { get; set; }
        public Dictionary<string, TempMean> TempMean { get; set; }

    }

    public class AllTimeMean
    {
        public int count { get; set; }
        public LocalPoint Position { get; set; }
    }

    public class TempMean
    {
        public int Temperature { get; set; }
        public int count { get; set; }
        public LocalPoint Position { get; set; }
    }
}
