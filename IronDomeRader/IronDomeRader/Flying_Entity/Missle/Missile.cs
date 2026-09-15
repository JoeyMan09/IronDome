namespace IronDomeRader.Flying_Entity.Missle
{
    internal class Missile : FlyingEntity
    {
        protected Missile(
            double x,
            double y,
            string name,
            double vx,
            double vy,
            ThreatLevel.threatLevel threatLevel,
            double size,
            EntityType.Entitytype type)
            : base(
                x,
                y,
                name,
                vx,
                vy,
                size,
                threatLevel,
                type)
        {
            this.isDestroyed = false;
        }


        public virtual void Destroy()
        {
            isDestroyed = true;
            Vx = 0;
            Vy = 0;
        }

        public override void UpdatePosition(double dt)
        {
            if (isDestroyed) return;

            base.UpdatePosition(dt);
        }
    }
}