using System;

namespace IronDomeInterceptor
{
    internal class FlyingEntity
    {
        protected static int ClassId = 0;

        protected int id;

        protected double x;
        protected double y;

        protected string name;

        protected double Vx;
        protected double Vy;

        protected double size;

        protected EntityType.Entitytype type;

        protected bool isDestroyed;
        protected bool isFriendly;

        // Impact location
        private double impactX;
        private double impactY;
        private bool hasImpactPoint;

        private const double ImpactRadius = 100.0;

        public FlyingEntity(
            double x,
            double y,
            string name,
            double Vx,
            double Vy,
            double size,
            EntityType.Entitytype type,
            bool isFriendly)
        {
            id = ClassId++;

            this.x = x;
            this.y = y;

            this.name = name;

            this.Vx = Vx;
            this.Vy = Vy;

            this.size = size;
            this.type = type;

            this.isFriendly = isFriendly;

            isDestroyed = false;
            hasImpactPoint = false;
        }

        public FlyingEntity(FlyingEntity entity)
        {
            id = ClassId++;

            x = entity.x;
            y = entity.y;

            name = entity.name;

            Vx = entity.Vx;
            Vy = entity.Vy;

            size = entity.size;
            type = entity.type;

            isFriendly = entity.isFriendly;
            isDestroyed = entity.isDestroyed;

            impactX = entity.impactX;
            impactY = entity.impactY;
            hasImpactPoint = entity.hasImpactPoint;
        }

        public void SetImpactPoint(double x, double y)
        {
            impactX = x;
            impactY = y;
            hasImpactPoint = true;
        }

        public string getName() => name;

        public double getX() => x;

        public double getY() => y;

        public double getVx() => Vx;

        public double getVy() => Vy;

        public int getId() => id;

        public bool getIsFriendly() => isFriendly;

        public EntityType.Entitytype GetEntityType() => type;

        public double getSpeed()
        {
            return Math.Sqrt(Vx * Vx + Vy * Vy);
        }

        public virtual void UpdatePosition(double dt)
        {
            if (isDestroyed)
                return;

            double previousX = x;
            double previousY = y;

            x += Vx * dt;
            y += Vy * dt;

            if (!hasImpactPoint || isFriendly)
                return;

            // Check whether the movement segment reached
            // the impact point, even between timer ticks.
            double dx = x - previousX;
            double dy = y - previousY;

            double lengthSquared = dx * dx + dy * dy;

            double t = 0;

            if (lengthSquared > 0)
            {
                t = (
                    (impactX - previousX) * dx +
                    (impactY - previousY) * dy
                ) / lengthSquared;

                t = Math.Max(0, Math.Min(1, t));
            }

            double closestX = previousX + t * dx;
            double closestY = previousY + t * dy;

            double distanceX = impactX - closestX;
            double distanceY = impactY - closestY;

            double distanceSquared =
                distanceX * distanceX +
                distanceY * distanceY;

            if (distanceSquared <= ImpactRadius * ImpactRadius)
            {
                isDestroyed = true;
            }
        }

        public bool HasImpacted()
        {
            return isDestroyed;
        }

        public double TimeToImpact()
        {
            if (!hasImpactPoint)
                return double.PositiveInfinity;

            double speed = getSpeed();

            if (speed <= 0)
                return double.PositiveInfinity;

            double dx = impactX - x;
            double dy = impactY - y;

            // A target moving away from its impact point
            // should not have a valid ETA.
            if (dx * Vx + dy * Vy <= 0)
                return double.PositiveInfinity;

            return Math.Sqrt(dx * dx + dy * dy) / speed;
        }

        public override string ToString()
        {
            return $"{name} at ({x:F0}, {y:F0}) speed {getSpeed():F0}";
        }
    }
}