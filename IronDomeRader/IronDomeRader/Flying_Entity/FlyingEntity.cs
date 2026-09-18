using System;

namespace IronDomeRader.Flying_Entity
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

        protected ThreatLevel.threatLevel threat;
        protected EntityType.Entitytype type;
        protected bool isFriendly;
        protected bool isDestroyed;
        protected double impactX;
        protected double impactY;
        protected ImpactLocation.ImpactLocations impactLocation;
        public FlyingEntity(
            double x,
            double y,
            string name,
            double Vx,
            double Vy,
            double size,
            ThreatLevel.threatLevel threatLevel,
            EntityType.Entitytype type,
            bool isFriendly)
        {
            this.id = ClassId++;

            this.x = x;
            this.y = y;

            this.name = name;

            this.Vx = Vx;
            this.Vy = Vy;

            this.size = size;

            this.threat = threatLevel;
            this.type = type;

            isDestroyed = false;
            this.isFriendly = isFriendly;
        }
        public void SetImpactPoint(double impactX, double impactY)
        {
            this.impactX = impactX;
            this.impactY = impactY;
        }
        public void SetImpactLocation(
    ImpactLocation.ImpactLocations location)
        {
            impactLocation = location;
        }

        public ImpactLocation.ImpactLocations GetImpactLocation()
        {
            return impactLocation;
        }
        public double GetImpactX()
        {
            return impactX;
        }

        public double GetImpactY()
        {
            return impactY;
        }
        public string getName()
        {
            return name;
        }
        public bool getIsFriendly()
        {
            return isFriendly;
        }
        public double getX()
        {
            return x;
        }

        public double getY()
        {
            return y;
        }

        public double getVx()
        {
            return Vx;
        }

        public double getVy()
        {
            return Vy;
        }

        public int getId()
        {
            return id;
        }

        public ThreatLevel.threatLevel getThreatLvl()
        {
            return threat;
        }

        public EntityType.Entitytype GetEntityType()
        {
            return type;
        }

        public virtual void UpdatePosition(double dt)
        {
            if (isDestroyed)
                return;

            x += Vx * dt;
            y += Vy * dt;

            if (y == 0)
            {
                y = 0;
                isDestroyed = true;
            }
        }

        public bool HasImpacted()
        {
            return isDestroyed;
        }

        public double getSpeed()
        {
            return Math.Sqrt(
                Vx * Vx +
                Vy * Vy
            );
        }

        public double TimeToImpact()
        {
            double dx = impactX - x;
            double dy = impactY - y;

            double distance =
                Math.Sqrt(dx * dx + dy * dy);

            double speed = getSpeed();

            if (speed == 0)
                return double.PositiveInfinity;

            return distance / speed;
        }
        public bool IsFriendly()
        {
            return isFriendly;
        }

        public override string ToString()
        {
            return $"{name} at ({x}, {y}) speed {getSpeed()}";
        }
    }
}