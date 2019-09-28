using System;
using System.Collections.Generic;
using System.Text;

namespace Calculation.Models
{
    public class TerrestrialRoundMessage
    {
        public string RoundId { get; set; }
        public string DeviceId { get; set; }
        public string StationId { get; set; }
        public DateTime RoundStartUtc { get; set; }
        public DateTime? RoundEndUtc { get; set; }
        public TerrestrialStation Station { get; set; }
        public TerrestrialObservation[] Observations { get; set; }
    }
}
