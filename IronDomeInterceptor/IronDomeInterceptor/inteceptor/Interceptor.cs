using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronDomeInterceptor.inteceptor
{
    internal class Interceptor : FlyingEntity
    {
        private double interceptionRange;
        private FlyingEntity target;
        private double speed;
        private bool isEngaged;
        private bool HasIntercepedTarget;

        public Interceptor(double x, double y, string name, double Vx, double Vy, double size, double interceptionRange)
            : base(x, y, name, Vx, Vy, size)
        {
            this.interceptionRange = interceptionRange;
            target = null;
            speed = Math.Sqrt(Vx * Vx + Vy * Vy);
            isEngaged = false;
            HasIntercepedTarget = false;
        }
        public double InterceptionRange()
        {
            return interceptionRange;
        }
        public void SetHasInterceptedTarget()
        {
            HasIntercepedTarget = true;
        }
        public bool GetHasInterceptedTarget()
        {
            return HasIntercepedTarget;
        }
        public FlyingEntity GetTarget()
        {
            return target;
        }
        public virtual void EngageTarget(FlyingEntity target)
        {
            if (target != null && !isEngaged)
            {
                this.target = target;
                isEngaged = true;
                Console.WriteLine($"{name} has engaged target {target.getName()}");
            }
        }
        public override void UpdatePosition(double time)
        {
            if (isEngaged && target != null)
            {
                double targetX = target.getX();
                double targetY = target.getY();

                double directionX = targetX - x;
                double directionY = targetY - y;

                double length = Math.Sqrt(directionX * directionX + directionY * directionY);

                if (length > 0)
                {
                    directionX /= length;
                    directionY /= length;

                    Vx = directionX * speed;
                    Vy = directionY * speed;
                }

                x += Vx * time;
                y += Vy * time;

                if (length < 100)
                {
                    HasIntercepedTarget = true;
                    isEngaged = false;
                }
            }
        }
        public double TimeToIntercept()
        {
            if (target != null)
            {
                double targetX = target.getX();
                double targetY = target.getY();
                double directionX = targetX - x;
                double directionY = targetY - y;
                double distance = Math.Sqrt(directionX * directionX + directionY * directionY);
                if (speed > 0)
                {
                    double timeToIntercept = distance / speed;
                    return timeToIntercept;
                }
            }
            return -1.0;
        }
    }
}
