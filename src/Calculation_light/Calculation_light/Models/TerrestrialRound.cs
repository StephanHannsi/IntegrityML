using System;
using System.Collections.Generic;
using System.Text;

namespace Calculation.Models
{
    public class TerrestrialRound
    {
        public string _id { get; set; }
        public string DeviceId { get; set; }
        public string StationId { get; set; }
        public string RefRound { get; set; }
        public DateTime RoundStartUtc { get; set; }
        public DateTime? RoundEndUtc { get; set; }
        public TerrestrialStation Station { get; set; }
        public Dictionary<string, Target> Observations { get; set; }
        public Results Results { get; set; }
        public IntegrityData IntegrityData { get; set; }
    }
}