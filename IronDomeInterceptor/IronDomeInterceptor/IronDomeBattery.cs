using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronDomeInterceptor
{
    public class IronDomeBattery
    {
        public string City { get; }
        public double X { get; }
        public double Y { get; }

        public IronDomeBattery(string city, double x, double y)
        {
            City = city;
            X = x;
            Y = y;
        }
    }
}
