using IronDomeRader.Flying_Entity;
using IronDomeRader.Flying_Entity.AirCraft;
using IronDomeRader.Flying_Entity.Missle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Net.Sockets;
using System.Net;


namespace IronDomeRader
{
    internal class RadarSystem
    {
        private List<FlyingEntity> flyingEntitiesDetected;
        private TcpListener listener;
        private const int port = 6767;
        private const string ip="127.0.0.1";
        private const double WorldSize = 50000.0;

        public RadarSystem()
        {
            flyingEntitiesDetected = new List<FlyingEntity>();
             listener = new TcpListener(IPAddress.Parse(ip), port);
        }

        public void DetectFlyingEntity(FlyingEntity entity)
        {
            flyingEntitiesDetected.Add(entity);
        }

        public void RemoveFlyingEntity(FlyingEntity entity)
        {
            flyingEntitiesDetected.Remove(entity);
        }

        public void Simulation(double canvasW, double canvasH)
        {
            Random rand = new Random();
            bool isRunning = true;
            Stopwatch stopwatch = new Stopwatch();
            Stopwatch spawnTimer = new Stopwatch();
            Stopwatch updateTimer = new Stopwatch();
            stopwatch.Start();
            spawnTimer.Start();
            updateTimer.Start();
            double deltaTime = 0;
            double currentTime = 0;
            double lastUpdateTime = 0;
            var toRemove = new List<FlyingEntity>();



            while (isRunning)
            {
                currentTime = stopwatch.Elapsed.TotalSeconds;
                deltaTime = currentTime - lastUpdateTime;

                MoveAll(deltaTime);

                if (updateTimer.ElapsedMilliseconds >= 1000)
                {
                    GetImpactedEntities(toRemove);
                    RemoveImpacted(toRemove);
                    toRemove.Clear();

                    updateTimer.Restart();
                }

                if (spawnTimer.ElapsedMilliseconds >= 1000 && flyingEntitiesDetected.Count < 4)
                {
                    SpawnFlyingEntity(rand);
                    spawnTimer.Restart();
                }

                lastUpdateTime = currentTime;
            }
        }

        public void MoveAll(double dt)
        {
            foreach (var entity in flyingEntitiesDetected.ToList())
            {
                entity.UpdatePosition(dt);

                if (entity.HasImpacted())
                    RemoveFlyingEntity(entity);
            }
        }

        public void SpawnFlyingEntity(Random rand)
        {
            int type = rand.Next(0, 3);
            double angle = rand.NextDouble() * 2 * Math.PI;
            double radius = WorldSize / 2;

            double x = radius * Math.Cos(angle);
            double y = radius * Math.Sin(angle);

            if (type == 0)
            {
                double directionX = -x;
                double directionY = -y;
                double length = Math.Sqrt(directionX * directionX + directionY * directionY);
                if (length > 0)
                {
                    directionX /= length;
                    directionY /= length;
                }
                double speed = rand.Next(150, 350);
                double vx = speed * directionX;
                double vy = speed * directionY;
                DetectFlyingEntity(new BallisticMissile(
                    x, y,
                    "Ballistic" + flyingEntitiesDetected.Count,
                    vx,
                    vy,
                    rand.Next(100, 300),
                    rand.Next(100, 500),
                    rand.Next(0, 360),
                    ThreatLevel.threatLevel.Ballistic_Missile,
                    rand.Next(10, 30),
                    20000
                ));
            }
            else if (type == 1)
            {
                double directionX = -x;
                double directionY = -y;
                double length = Math.Sqrt(directionX * directionX + directionY * directionY);
                if (length > 0)
                {
                    directionX /= length;
                    directionY /= length;
                }
                double speed = rand.Next(350, 700);
                double vx = speed * directionX;
                double vy = speed * directionY;
                DetectFlyingEntity(new SupersonicMissile(
                    x, y,
                    "Supersonic" + flyingEntitiesDetected.Count,
                    vx,
                    vy,
                    rand.Next(100, 300),
                    rand.Next(100, 500),
                    rand.Next(0, 360),
                    ThreatLevel.threatLevel.Supersonic_Missile,
                    rand.Next(10, 30),
                    30000,
                    20000
                ));
            }
            else
            {
                
                double directionX = -x;
                double directionY = -y;
                double length = Math.Sqrt(directionX*directionX+directionY*directionY);
                if (length > 0)
                {
                    directionX /= length;
                    directionY /= length;
                }
                double speed = rand.Next(50, 150);
                double vx = speed * directionX;
                double vy = speed * directionY;

                DetectFlyingEntity(new Drone(
                    x, y,
                    "Drone" + flyingEntitiesDetected.Count,
                    vx,
                    vy,
                    rand.Next(5, 10),
                    10000,
                    10
                ));
            }
        }

        public List<FlyingEntity> GetFlyingEntities() => flyingEntitiesDetected;


        private void GetImpactedEntities(List<FlyingEntity> toRemove)
        {
            foreach (var entity in flyingEntitiesDetected)
            {
                if (entity.HasImpacted())
                    toRemove.Add(entity);
            }
        }

        private void RemoveImpacted(List<FlyingEntity> toRemove)
        {
            foreach (var entity in toRemove)
                RemoveFlyingEntity(entity);
        }

    }
}