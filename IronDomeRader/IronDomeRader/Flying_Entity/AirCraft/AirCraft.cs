namespace IronDomeRader.Flying_Entity.AirCraft
{
    internal class AirCraft : FlyingEntity
    {
        protected double altitude;
        protected double fuel;

        public AirCraft(
            double x,
            double y,
            string name,
            double vx,
            double vy,
            double size, double altitude, double fuel, bool isFriendly, EntityType.Entitytype type)
            : base(
                x,
                y,
                name,
                vx,
                vy,
                size,
                ThreatLevel.threatLevel.no,
                type,isFriendly)
        {
            this.altitude = altitude;
            this.fuel = fuel;
        }

        public override void UpdatePosition(double dt)
        {
            if (fuel <= 0) return;

            x += Vx * dt;
            y += Vy * dt;

            fuel -= getSpeed() * dt * 0.01;

            if (fuel < 0)
                fuel = 0;
        }

        public double GetAltitude()
        {
            return altitude;
        }

        public double GetFuel()
        {
            return fuel;
        }

        

        public bool IsOutOfFuel()
        {
            return fuel <= 0;
        }
    }
}