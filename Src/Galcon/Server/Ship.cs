using System;

namespace Galcon.Server
{
    public class Ship
    {
        public Planet SourcePlanet { get; set; }
        public Planet TargetPlanet { get; set; }

        public Ship(Planet sourcePlanet, Planet targetPlanet)
        {
            SourcePlanet = sourcePlanet;
            TargetPlanet = targetPlanet;
            Random random = new Random();

            X = sourcePlanet.X + (Math.Ceiling(random.Next(1, 10000) * 0.0001 * (sourcePlanet.Radius * 1.5) - sourcePlanet.Radius));
            Y = sourcePlanet.Y + (Math.Ceiling(random.Next(1, 10000) * 0.0001 * (sourcePlanet.Radius * 1.5) - sourcePlanet.Radius));
            //Dx = (targetPlanet.X + (Math.Ceiling((double)((sourcePlanet.Radius) - sourcePlanet.Radius))) - X) / 60;
            //Dy = (targetPlanet.Y + (Math.Ceiling((double)((sourcePlanet.Radius) - sourcePlanet.Radius))) - Y) / 60;

            Dx = (targetPlanet.X + (Math.Ceiling((double)((sourcePlanet.Radius) - sourcePlanet.Radius))) - X) / 60;
            Dy = (targetPlanet.Y + (Math.Ceiling((double)((sourcePlanet.Radius) - sourcePlanet.Radius))) - Y) / 60;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Dx { get; set; }
        public double Dy { get; set; }


        public void Update()
        {
            // Append only 2 seconds
            for (int i = 0; i < 60; i++)
            {
                X = X + Dx;
                Y = Y + Dy;
            }
        }

        public bool CirclesColliding(int x1, int y1, int radius1, int x2, int y2, int radius2)
        {
            //compare the distance to combined radii
            int dx = x2 - x1;
            int dy = y2 - y1;
            int radii = radius1 + radius2;
            return (dx * dx) + (dy * dy) < radii * radii;
        }

        public bool IsCollidingWithPlanet(double x, double y, Planet planet)
        {
            return Math.Pow((x - planet.X), 2) + Math.Pow((y - planet.Y), 2) < Math.Pow(planet.Radius, 2);
        }

        public bool IsFinished()
        {
            return IsCollidingWithPlanet(X, Y, TargetPlanet);
        }
    }
}