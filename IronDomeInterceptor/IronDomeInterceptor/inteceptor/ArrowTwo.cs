using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace IronDomeInterceptor.inteceptor
{
    internal class ArrowTwo : IronDome
    {
        private double ReactionTime; // Time it takes for the interceptor to react and launch after detecting a target
        public enum WarheadType { Explosive = 1, Fragmentation = 2 }
        private WarheadType warheadType;

        public ArrowTwo(double x, double y, string name, double Vx, double Vy, double size, double interceptionRange, double batteryLocationX, double batteryLocationY, double reactionTime, WarheadType warheadType, EntityType.Entitytype type)
            : base(x, y, name, Vx, Vy, size, interceptionRange, batteryLocationX, batteryLocationY,type)
        {
            ReactionTime = reactionTime;
            this.warheadType = warheadType;
        }
        public override void UpdatePosition(double time)
        {
            base.UpdatePosition(time);
            if (ReactionTime > 0)
            {
                Console.WriteLine($"{name} is reacting to target. Time remaining: {ReactionTime} seconds.");
                ReactionTime -= time;
                if (ReactionTime <= 0)
                {
                    Console.WriteLine($"{name} has launched towards the target with {warheadType} warhead.");
                }
            }
        }
        public override void EngageTarget(FlyingEntity target)
        {
            if (target != null && !GetHasInterceptedTarget())
            {
                //Console.WriteLine($"{name} is preparing to intercept {target.getName()} with {warheadType} warhead.");
                base.EngageTarget(target);
            }
        }
        public double GetReactionTime()
        {
            return ReactionTime;
        }
        public WarheadType GetWarheadType()
        {
            return warheadType;
        }
        public override string ToString()
        {
            return $"ArrowTwo: {name}, Position: ({x}, {y}), Velocity: ({getSpeed()}), Interception Range: {InterceptionRange()}\n, Reaction Time: {ReactionTime} seconds, Warhead Type: {warheadType} Time To Interception {TimeToIntercept()}";
        }
    }
}
