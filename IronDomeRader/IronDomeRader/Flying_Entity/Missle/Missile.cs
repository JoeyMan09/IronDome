namespace IronDomeRader.Flying_Entity.Missle
{
    abstract class Missile : FlyingEntity
    {
        protected ThreatLevel.threatLevel threatLevel;
        protected bool isDestroyed;

        public ThreatLevel.threatLevel ThreatLevel => threatLevel;

        public bool IsDestroyed => isDestroyed;

        protected Missile(
            double x,
            double y,
            string name,
            double vx,
            double vy,
            ThreatLevel.threatLevel threatLevel,
            double size)
            : base(x, y, name, vx, vy, size)
        {
            this.threatLevel = threatLevel;
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