using System;
using System.Collections.Generic;
using System.Text;

namespace Calculation.Models
{
    public class TerrestrialPosition
    {
        public double Easting { get; set; } = double.NaN;
        public double Northing { get; set; } = double.NaN;
        public double Elevation { get; set; } = double.NaN;
    }
}
