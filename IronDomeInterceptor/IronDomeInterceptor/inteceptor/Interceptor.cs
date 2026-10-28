using System;

namespace IronDomeInterceptor.inteceptor
{
    internal class Interceptor : FlyingEntity
    {
        private const double SuccessProbability = 0.80;

        private readonly double interceptionRange;
        private readonly double speed;

        private FlyingEntity? target;

        private bool isEngaged;
        private bool hasInterceptedTarget;
        private bool hasFailedInterception;
        private bool isFalling = false;
        private const double Gravity = 300.0;
        private const double GroundY = -25000.0;

        public bool IsFalling()
        {
            return isFalling;
        }

        public bool HasFinishedFalling()
        {
            return isFalling && y <= GroundY;
        }
        public Interceptor(
            double x,
            double y,
            string name,
            double Vx,
            double Vy,
            double size,
            double interceptionRange,
            EntityType.Entitytype type)
            : base(x, y, name, Vx, Vy, size, type, true)
        {
            this.interceptionRange = interceptionRange;
            speed = Math.Sqrt(Vx * Vx + Vy * Vy);
        }

        public double InterceptionRange()
        {
            return interceptionRange;
        }

        public void SetHasInterceptedTarget()
        {
            hasInterceptedTarget = true;
            hasFailedInterception = false;
            isEngaged = false;
        }

        public bool GetHasInterceptedTarget()
        {
            return hasInterceptedTarget;
        }

        public bool GetHasFailedInterception()
        {
            return hasFailedInterception;
        }

        public FlyingEntity? GetTarget()
        {
            return target;
        }

        public virtual void EngageTarget(FlyingEntity newTarget)
        {
            if (newTarget == null || isEngaged)
                return;

            target = newTarget;
            isEngaged = true;

            hasInterceptedTarget = false;
            hasFailedInterception = false;
        }

        public virtual void DisEngageTarget()
        {
            target = null;
            isEngaged = false;

            Vx = 0;
            Vy = 0;
        }

        public override void UpdatePosition(double time)
        {
            if (isFalling)
            {
                Vy -= Gravity * time;

                x += Vx * time;
                y += Vy * time;

                return;
            }
            if (!isEngaged || target == null || time <= 0)
                return;

            double dx = target.getX() - x;
            double dy = target.getY() - y;

            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance <= interceptionRange)
            {
                AttemptInterception();
                return;
            }

            if (speed <= 0)
                return;

            double directionX = dx / distance;
            double directionY = dy / distance;

            Vx = directionX * speed;
            Vy = directionY * speed;

            double movement = speed * time;

            if (movement >= distance)
            {
                x = target.getX();
                y = target.getY();

                AttemptInterception();
                return;
            }

            x += directionX * movement;
            y += directionY * movement;

            double remainingX = target.getX() - x;
            double remainingY = target.getY() - y;

            double remainingDistance = Math.Sqrt(
                remainingX * remainingX +
                remainingY * remainingY
            );

            if (remainingDistance <= interceptionRange)
            {
                AttemptInterception();
            }
        }

        private void AttemptInterception()
        {
            if (!isEngaged)
                return;

            isEngaged = false;

            bool success =
                Random.Shared.NextDouble() < SuccessProbability;

            if (success)
            {
                hasInterceptedTarget = true;

                Vx = 0;
                Vy = 0;
            }
            else
            {
                hasFailedInterception = true;
                isFalling = true;

                // Keep the current velocity so it
                // continues forward before falling.
            }
        }

        public double TimeToIntercept()
        {
            if (target == null || speed <= 0)
                return -1;

            double dx = target.getX() - x;
            double dy = target.getY() - y;

            double distance = Math.Sqrt(dx * dx + dy * dy);

            return Math.Max(
                0,
                (distance - interceptionRange) / speed
            );
        }
    }
}