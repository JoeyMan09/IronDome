using System;

namespace IronDomeRader.Flying_Entity.Missle
{
    internal class BallisticMissile : Missile
    {
        protected double explosionRadius;
        protected double explosivePayload;
        protected double launchAngle;
        protected double MaxAltitude;

        protected bool isTracked;
        protected bool isIntercepted;
        protected bool isDestroyed;

        private double traveledDistance;

        public BallisticMissile(
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
            double maxAltitude)
            : base(x, y, name, vx, vy, threatLevel, size)
        {
            this.explosionRadius = explosionRadius;
            this.explosivePayload = explosivePayload;
            this.launchAngle = launchAngle;
            this.MaxAltitude = maxAltitude;

            isTracked = false;
            isIntercepted = false;
            isDestroyed = false;

            traveledDistance = 0;
        }

        public override void UpdatePosition(double dt)
        {
            if (isDestroyed) return;

            double oldX = x;
            double oldY = y;

            base.UpdatePosition(dt);
            double dx = x - oldX;
            double dy = y - oldY;

            traveledDistance += Math.Sqrt(dx * dx + dy * dy);

            //  altitude limit (אם Y מייצג גובה)
            if (y > MaxAltitude)
            {
                Vy = -Math.Abs(Vy); // חזרה למטה
            }
        }

        public double CalculateDamageArea()
        {
            return Math.PI * explosionRadius * explosionRadius;
        }

        public bool IsThreat()
        {
            return explosivePayload > 5;
        }

        public void MarkTracked()
        {
            isTracked = true;
        }

        public void MarkIntercepted()
        {
            isIntercepted = true;
            isDestroyed = true;

            Vx = 0;
            Vy = 0;
        }

        public bool IsDestroyed()
        {
            return isDestroyed;
        }

        public double GetTraveledDistance()
        {
            return traveledDistance;
        }

        public override string ToString()
        {
            return $"Name: {name}, Position: ({x}, {y}), Speed: {getSpeed()}" +
                   $"\nExplosion Radius: {explosionRadius}, Payload: {explosivePayload}, Launch Angle: {launchAngle}, Max Altitude: {MaxAltitude}" +
                   $"\nDestroyed: {isDestroyed}, Tracked: {isTracked}, Intercepted: {isIntercepted}";
        }
    }
}