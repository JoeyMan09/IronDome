using System;

namespace IronDomeInterceptor
{
    internal class FlyingEntity
    {
        protected static int ClassId=0;
        protected int id;
        protected double x;
        protected double y;
        protected string name;
        protected double Vx;
        protected double Vy;
        protected double size;

        protected bool isDestroyed;

        public FlyingEntity(double x, double y, string name, double Vx, double Vy, double size)
        {
            this.id = ClassId++;
            this.x = x;
            this.y = y;
            this.name = name;
            this.Vx = Vx;
            this.Vy = Vy;
            this.size = size;

            isDestroyed = false;
        }
        public FlyingEntity(FlyingEntity entity)
        {
            this.id = ClassId++;
            this.x = entity.x;
            this.y = entity.y;
            this.name = entity.name;
            this.Vx = entity.Vx;
            this.Vy = entity.Vy;
            this.size = entity.size;
            isDestroyed = false;
        }
        public string getName()
        {
            return name;
        }
        public double getX() { return x; }
        public double getY() { return y; }
        public double getVx() { return Vx; }
        public double getVy() { return Vy; }
        public int getId() { return id; }
        public virtual void UpdatePosition(double dt)
        {
            if (isDestroyed) return;


            x += Vx * dt;
            y += Vy * dt;

        }

        public bool HasImpacted()
        {
            return isDestroyed;
        }

        public double getSpeed()
        {
            return Math.Sqrt(Vx * Vx + Vy * Vy);
        }

        public double TimeToImpact()
        {
            double distance = Math.Sqrt(x * x + y * y);
            if (getSpeed() == 0)
                return double.PositiveInfinity;
            else
                return distance / getSpeed();
        }

        public override string ToString()
        {
            return $"{name} at ({x}, {y}) speed {getSpeed()}";
        }
    }
}