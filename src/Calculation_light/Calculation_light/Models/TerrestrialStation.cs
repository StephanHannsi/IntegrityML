using System;
using System.Collections.Generic;
using System.Text;

namespace Calculation.Models
{
    public class TerrestrialStation
    {
        public string SerialNumber { get; set; }
        public string PointId { get; set; }
        public TerrestrialPosition Position { get; set; }
    }
}
