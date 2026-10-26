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

        // Target impact coordinates
        public double ImpactX { get; set; }
        public double ImpactY { get; set; }

        public bool HasImpactPoint { get; set; }

        public Entitytype.EntityType EntityType { get; set; }

        public bool isFriendly { get; set; }

        // Required for JSON deserialization
        public InterceptCommand()
        {
        }

        public InterceptCommand(TargetData target)
        {
            CommandId = ClassId++;

            TargetId = target.Id;

            TargetX = target.X;
            TargetY = target.Y;

            TargetVx = target.Vx;
            TargetVy = target.Vy;

            ImpactX = target.impactX;
            ImpactY = target.impactY;

            HasImpactPoint = true;

            EntityType = target.EntityType;
            isFriendly = target.isFriendly;
        }
    }
}