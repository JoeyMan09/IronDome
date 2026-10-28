namespace IronDomeInterceptor
{
    internal class InterceptCommand
    {
        public int CommandId { get; set; }

        public int TargetId { get; set; }

        public double TargetX { get; set; }

        public double TargetY { get; set; }

        public double TargetVx { get; set; }

        public double TargetVy { get; set; }

        public double ImpactX { get; set; }

        public double ImpactY { get; set; }

        public bool HasImpactPoint { get; set; }

        public EntityType.Entitytype EntityType { get; set; }

        public bool isFriendly { get; set; }

        // Required for JSON deserialization
        public InterceptCommand()
        {
        }

        // Existing constructor, preserved for compatibility
        public InterceptCommand(
            int commandId,
            int targetId,
            double targetX,
            double targetY,
            double targetVx,
            double targetVy,
            EntityType.Entitytype entityType,
            bool isFriendly)
        {
            CommandId = commandId;
            TargetId = targetId;

            TargetX = targetX;
            TargetY = targetY;

            TargetVx = targetVx;
            TargetVy = targetVy;

            EntityType = entityType;
            this.isFriendly = isFriendly;

            HasImpactPoint = false;
        }

        // Constructor with impact coordinates
        public InterceptCommand(
            int commandId,
            int targetId,
            double targetX,
            double targetY,
            double targetVx,
            double targetVy,
            EntityType.Entitytype entityType,
            bool isFriendly,
            double impactX,
            double impactY)
            : this(
                commandId,
                targetId,
                targetX,
                targetY,
                targetVx,
                targetVy,
                entityType,
                isFriendly)
        {
            ImpactX = 0;
            ImpactY = 0;
            HasImpactPoint = true;
        }
    }
}