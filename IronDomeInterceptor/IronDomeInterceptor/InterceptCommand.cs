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

        public InterceptCommand(
            int commandId,
            int targetId,
            double targetX,
            double targetY,
            double targetVx,
            double targetVy)
        {
            CommandId = commandId;
            TargetId = targetId;
            TargetX = targetX;
            TargetY = targetY;
            TargetVx = targetVx;
            TargetVy = targetVy;
        }
    }
}