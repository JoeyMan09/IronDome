using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronDomeCommandCenter
{
    internal class InterceptCommand
    {
        private static int ClassId = 0;

        public int CommandId { get; set; }
        public int TargetId { get; set; }

        public double TargetX { get; set; }
        public double TargetY { get; set; }
        public double TargetVx { get; set; }
        public double TargetVy { get; set; }

        public Entitytype.EntityType EntityType { get; set; }
        public bool isFriendly {  get; set; }
        public InterceptCommand(TargetData target)
        {
            CommandId = ClassId++;

            TargetId = target.Id;

            TargetX = target.X;
            TargetY = target.Y;

            TargetVx = target.Vx;
            TargetVy = target.Vy;

            EntityType = target.EntityType;
            isFriendly = target.isFriendly;
        }
    }
}
