namespace IronDomeRader.Flying_Entity.AirCraft
{
    internal class Drone : AirCraft
    {
        protected double SurveillanceRange;
        protected double PayloadCapacity;

        private bool isScanning;

        public Drone(
            double x,
            double y,
            string name,
            double vx,
            double vy,
            double size,
            double surveillanceRange,
            double payloadCapacity)
            : base(x, y, name, vx, vy, size, 50, 100, false,EntityType.Entitytype.drone)
        {
            SurveillanceRange = surveillanceRange;
            PayloadCapacity = payloadCapacity;
            isScanning = true;
        }

        public override void UpdatePosition(double dt)
        {
            if (fuel <= 0) return;

            base.UpdatePosition(dt);
        }

        public bool CanDetectTarget(double distance)
        {
            return isScanning && distance <= SurveillanceRange;
        }

        public bool CanCarryPayload()
        {
            return PayloadCapacity > 0;
        }

        public void UsePayload()
        {
            if (PayloadCapacity > 0)
                PayloadCapacity--;
        }

        public void StartScanning()
        {
            isScanning = true;
        }

        public void StopScanning()
        {
            isScanning = false;
        }
    }
}