using IronDomeRader.Flying_Entity;
using IronDomeRader.Flying_Entity.AirCraft;
using IronDomeRader.Flying_Entity.Missle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Net.Sockets;
using System.Net;
using System.Windows;

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
            int type = rand.Next(0, 4);

            // -------------------------------------------------
            // Spawn point
            // -------------------------------------------------

            double angle =
                rand.NextDouble() * 2 * Math.PI;

            double radius =
                WorldSize / 2;

            double x =
                radius * Math.Cos(angle);

            double y =
                radius * Math.Sin(angle);


            // -------------------------------------------------
            // EVERY ENTITY GETS AN IMPACT LOCATION
            // -------------------------------------------------

            ImpactLocation.ImpactLocations location =
                GenerateImpactLocation(rand);

            Point impact =
                GetImpactPoint(location);


            // -------------------------------------------------
            // Direction toward impact point
            // -------------------------------------------------

            double directionX =
                impact.X - x;

            double directionY =
                impact.Y - y;


            double length =
                Math.Sqrt(
                    directionX * directionX +
                    directionY * directionY
                );


            if (length > 0)
            {
                directionX /= length;
                directionY /= length;
            }


            // =================================================
            // BALLISTIC MISSILE
            // =================================================

            if (type == 0)
            {
                double speed =
                    rand.Next(150, 350);

                double vx =
                    speed * directionX;

                double vy =
                    speed * directionY;


                BallisticMissile missile =
                    new BallisticMissile(
                        x,
                        y,
                        "Ballistic" + flyingEntitiesDetected.Count,
                        vx,
                        vy,
                        rand.Next(100, 300),
                        rand.Next(100, 500),
                        rand.Next(0, 360),
                        ThreatLevel.threatLevel.Ballistic_Missile,
                        rand.Next(10, 30),
                        20000
                    );


                missile.SetImpactPoint(
                    impact.X,
                    impact.Y
                );

                missile.SetImpactLocation(
                    location
                );


                DetectFlyingEntity(
                    missile
                );
            }


            // =================================================
            // SUPERSONIC MISSILE
            // =================================================

            else if (type == 1)
            {
                double speed =
                    rand.Next(350, 700);

                double vx =
                    speed * directionX;

                double vy =
                    speed * directionY;


                SupersonicMissile missile =
                    new SupersonicMissile(
                        x,
                        y,
                        "Supersonic" + flyingEntitiesDetected.Count,
                        vx,
                        vy,
                        rand.Next(100, 300),
                        rand.Next(100, 500),
                        rand.Next(0, 360),
                        ThreatLevel.threatLevel.Supersonic_Missile,
                        rand.Next(10, 30),
                        30000,
                        20000,
                        50000
                    );


                missile.SetImpactPoint(
                    impact.X,
                    impact.Y
                );

                missile.SetImpactLocation(
                    location
                );


                DetectFlyingEntity(
                    missile
                );
            }


            // =================================================
            // DRONE
            // =================================================

            else if (type == 2)
            {
                double speed =
                    rand.Next(50, 150);

                double vx =
                    speed * directionX;

                double vy =
                    speed * directionY;


                Drone drone =
                    new Drone(
                        x,
                        y,
                        "Drone" + flyingEntitiesDetected.Count,
                        vx,
                        vy,
                        rand.Next(5, 10),
                        10000,
                        10
                    );


                drone.SetImpactPoint(
                    impact.X,
                    impact.Y
                );

                drone.SetImpactLocation(
                    location
                );


                DetectFlyingEntity(
                    drone
                );
            }


            // =================================================
            // AIRCRAFT
            // =================================================

            else
            {
                double speed =
                    rand.Next(50, 150);

                double vx =
                    speed * directionX;

                double vy =
                    speed * directionY;


                double fuel =
                    rand.Next(1000, 2000);


                bool isFriendly =
                    rand.Next(0, 2) == 1;


                AirCraft aircraft =
                    new AirCraft(
                        x,
                        y,
                        "Aircraft" + (flyingEntitiesDetected.Count + 1),
                        vx,
                        vy,
                        1,
                        fuel,
                        rand.Next(1000, 3000),
                        isFriendly,
                        EntityType.Entitytype.aircraft
                    );


                aircraft.SetImpactPoint(
                    impact.X,
                    impact.Y
                );

                aircraft.SetImpactLocation(
                    location
                );


                DetectFlyingEntity(
                    aircraft
                );
            }
        }

        public List<FlyingEntity> GetFlyingEntities() => flyingEntitiesDetected;

        private ImpactLocation.ImpactLocations GenerateImpactLocation(Random rand)
        {
            ImpactLocation.ImpactLocations[] locations =
                Enum.GetValues<ImpactLocation.ImpactLocations>()
                    .Where(x => x != ImpactLocation.ImpactLocations.Unknown)
                    .ToArray();

            return locations[rand.Next(locations.Length)];
        }
        private Point GetImpactPoint(ImpactLocation.ImpactLocations location)
        {
            switch (location)
            {
                case ImpactLocation.ImpactLocations.Haifa:
                    return new Point(-1500, 7000);

                case ImpactLocation.ImpactLocations.TelAviv:
                    return new Point(-1200, 2500);

                case ImpactLocation.ImpactLocations.Jerusalem:
                    return new Point(1200, 1500);

                case ImpactLocation.ImpactLocations.Ashdod:
                    return new Point(-1000, 500);

                case ImpactLocation.ImpactLocations.Ashkelon:
                    return new Point(-900, -1000);

                case ImpactLocation.ImpactLocations.BeerSheva:
                    return new Point(0, -4000);

                case ImpactLocation.ImpactLocations.Eilat:
                    return new Point(500, -8500);

                default:
                    return new Point(0, 0);
            }
        }
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