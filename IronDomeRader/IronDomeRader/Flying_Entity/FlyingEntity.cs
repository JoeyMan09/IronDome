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

        protected bool isDestroyed;

        public FlyingEntity(
            double x,
            double y,
            string name,
            double Vx,
            double Vy,
            double size,
            ThreatLevel.threatLevel threatLevel,
            EntityType.Entitytype type)
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
        }

        public string getName()
        {
            return name;
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
            double distance =
                Math.Sqrt(
                    x * x +
                    y * y
                );

            if (getSpeed() == 0)
                return double.PositiveInfinity;

            return distance / getSpeed();
        }

        public override string ToString()
        {
            return $"{name} at ({x}, {y}) speed {getSpeed()}";
        }
    }
}