using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronDomeCommandCenter
{
    internal class InterceptResult
    {
            public int CommandId { get; set; }
            public int TargetId { get; set; }
            public bool Success { get; set; }
        
    }
}
