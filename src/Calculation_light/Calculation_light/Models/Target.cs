using System;
using System.Collections.Generic;
using System.Text;

namespace Calculation.Models
{
    public class Target
    {
        public TerrestrialObservation Face1 { get; set; }
        public TerrestrialObservation Face2 { get; set; }
        public TerrestrialObservation MeanF1F2 { get; set; }
    }
}
