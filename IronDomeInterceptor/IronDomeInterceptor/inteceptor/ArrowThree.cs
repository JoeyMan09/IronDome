using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronDomeInterceptor.inteceptor
{
    internal class ArrowThree : ArrowTwo
    {
        private double minExoAtmosphereicAltitude; // Minimum altitude at which the interceptor can operate effectively
        private bool boostPhaseCapable; // Indicates if the interceptor can operate during the boost phase of a missile's flight
        public ArrowThree(double x, double y, string name, double Vx, double Vy, double size, double interceptionRange, double batteryLocationX, double batteryLocationY, double reactionTime, WarheadType warheadType, double minExoAtmosphericAltitude, bool boostPhaseCapable,EntityType.Entitytype type)
            : base(x, y, name, Vx, Vy, size, interceptionRange, batteryLocationX, batteryLocationY, reactionTime, warheadType, type)
        {
            minExoAtmosphereicAltitude = minExoAtmosphericAltitude;
            this.boostPhaseCapable = boostPhaseCapable;
        }
        public override void UpdatePosition(double time)
        {
            base.UpdatePosition(time);
            if (boostPhaseCapable)
            {
                Console.WriteLine($"{name} is capable of engaging targets during the boost phase.");
            }
            if (minExoAtmosphereicAltitude > 0)
            {
                Console.WriteLine($"{name} is designed to operate effectively above {minExoAtmosphereicAltitude} meters.");
            }
        }
        public override void EngageTarget(FlyingEntity target)
        {
            if (boostPhaseCapable)
            {
                Console.WriteLine($"{name} is engaging {target.getName()} during the boost phase.");
            }
            else
            {
                Console.WriteLine($"{name} is engaging {target.getName()} outside of the boost phase.");
            }
            base.EngageTarget(target);
        }
        public override string ToString()
        {
            return $"ArrowThree: {name}, Position: ({x}, {y}), Velocity: ({getSpeed()}), Interception Range: {InterceptionRange()}, Reaction Time: {GetReactionTime()} seconds, Warhead Type: {GetWarheadType()},\n Min Exo-Atmospheric Altitude: {minExoAtmosphereicAltitude} meters, Boost Phase Capable: {boostPhaseCapable} Time To Interception {TimeToIntercept()}";
        }
    }
}
