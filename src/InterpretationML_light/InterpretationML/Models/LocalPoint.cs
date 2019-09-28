using System;
using System.Collections.Generic;
using System.Text;

namespace InterpretationML.Models
{
    public class LocalPoint
    {
        public string PointId { get; set; }

        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Z { get; set; }
    }
}
