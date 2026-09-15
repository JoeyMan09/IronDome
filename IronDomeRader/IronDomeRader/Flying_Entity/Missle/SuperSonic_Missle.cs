using System;

namespace IronDomeRader.Flying_Entity.Missle
{
    internal class SupersonicMissile : BallisticMissile
    {
        private double traveledDistance;
        private double MaxRange;
        private const double SpeedOfSound = 343;

       public SupersonicMissile(
    double x,
    double y,
    string name,
    double vx,
    double vy,
    double explosionRadius,
    double explosivePayload,
    double launchAngle,
    ThreatLevel.threatLevel threatLevel,
    double size,
    double maxAltitude,
    double supersonicAltitude,double maxrange)
    : base(
        x,
        y,
        name,
        vx,
        vy,
        explosionRadius,
        explosivePayload,
        launchAngle,
        threatLevel,
        size,
        maxAltitude,
        EntityType.Entitytype.supersonic)
        {
            traveledDistance = 0;
            this.MaxRange = maxrange;
            NormalizeSpeed(); // 🔥 חשוב מאוד
        }

        // 🔥 מונע מהירות מטורפת
        private void NormalizeSpeed()
        {
            double speed = getSpeed();

            double maxSpeed = 450; // supersonic limit (אפשר לשנות)
            if (speed > maxSpeed)
            {
                double scale = maxSpeed / speed;
                Vx *= scale;
                Vy *= scale;
            }
        }

        public override void UpdatePosition(double dt)
        {
            double oldX = x;
            double oldY = y;

            base.UpdatePosition(dt);

            double dx = x - oldX;
            double dy = y - oldY;

            traveledDistance += Math.Sqrt(dx * dx + dy * dy);

            if (traveledDistance > MaxRange)
            {
                Vx = 0;
                Vy = 0;
            }
        }

        public double CalculateMachNumber()
        {
            double speed = getSpeed();
            return speed / SpeedOfSound;
        }

        public bool IsHypersonic()
        {
            return CalculateMachNumber() >= 5;
        }

        public bool CanReachTarget(double distance)
        {
            return distance <= MaxRange;
        }

        public double GetMaxRange()
        {
            return MaxRange;
        }

        public double GetMaxAltitude()
        {
            return MaxAltitude;
        }
    }
}