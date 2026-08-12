using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronDomeInterceptor.inteceptor
{
    internal class IronDome:Interceptor
    {
        private double batteryLocationX;
        private double batteryLocationY;
        private int interceporCount;
        private const int succsessRate = 90; // 90% success rate for interception

        public IronDome(double x, double y, string name, double Vx, double Vy, double size, double interceptionRange, double batteryLocationX, double batteryLocationY)
            : base(x, y, name, Vx, Vy, size, interceptionRange)
        {
            this.batteryLocationX = batteryLocationX;
            this.batteryLocationY = batteryLocationY;
            interceporCount = 0;
        }
        public void setX(double x)
        {
            batteryLocationX = x;
        }
        public void setY(double y)
        {
            batteryLocationY = y;
        }
        public override void UpdatePosition(double time)
        {
            base.UpdatePosition(time);
            if (interceporCount > 0)
            {
                Console.WriteLine($"{name} is returning to battery after interception.");
                double directionX = batteryLocationX - getX();
                double directionY = batteryLocationY - getY();
                double length = Math.Sqrt(directionX * directionX + directionY * directionY);

                if (length > 0)
                {
                    directionX /= length;
                    directionY /= length;

                    Vx = directionX * Math.Sqrt(Vx * Vx + Vy * Vy);
                    Vy = directionY * Math.Sqrt(Vx * Vx + Vy * Vy);
                }

                setX(getX() + Vx * time);
                setY(getY() + Vy * time);

                if (length < 10)
                {
                    Console.WriteLine($"{name} has returned to battery.");
                    interceporCount--;
                }
            }
        }
        public override void EngageTarget(FlyingEntity target)
        {
            base.EngageTarget(target);
            if (target != null)
            {
                Random rand = new Random();
                int chance = rand.Next(100);
                
                while(interceporCount > 0 && GetHasInterceptedTarget())
                {
                    interceporCount--;
                    if (chance < succsessRate)
                    {
                        Console.WriteLine($"{name} successfully intercepted {target.getName()}!");
                        SetHasInterceptedTarget();
                    }
                    else
                    {
                        Console.WriteLine($"{name} failed to intercept {target.getName()} trying again {interceporCount} intercepters left.");
                    }
                }
            }
        }
        public override string ToString()
        {
            return $"{name} at ({x}, {y}) with velocity ({getSpeed()}), size {size}, interception range {InterceptionRange()},\n battery location ({batteryLocationX}, {batteryLocationY}), interceptors available: {interceporCount}Time To Interception {TimeToIntercept()}";
        }
    }
}
