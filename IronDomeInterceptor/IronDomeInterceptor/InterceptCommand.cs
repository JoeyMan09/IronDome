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

        public EntityType.Entitytype EntityType { get; set; }
        public bool isFriendly { get; set; }

        // חשוב ל-JSON
        public InterceptCommand()
        {
        }


        public InterceptCommand(
            int commandId,
            int targetId,
            double targetX,
            double targetY,
            double targetVx,
            double targetVy,
            EntityType.Entitytype entityType,bool isFriendly)
        {
            CommandId = commandId;

            TargetId = targetId;

            TargetX = targetX;
            TargetY = targetY;

            TargetVx = targetVx;
            TargetVy = targetVy;

            EntityType = entityType;
            this.isFriendly = isFriendly;
        }
    }
}