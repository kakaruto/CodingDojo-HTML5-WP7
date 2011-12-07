using System.Collections.Generic;

namespace Galcon.Server
{
    public class Space
    {
        public Space()
        {
            Spectators = new List<Player>();
            Players = new List<Player>();
            IsStarted = false;
            Planets = new List<Planet>();
            Ships = new List<Ship>();

            InitPlanets();

        }

        public IList<Player> Players { get; set; }
        public IList<Player> Spectators { get; set; }

        public bool IsStarted { get; set; }

        public IList<Planet> Planets { get; set; }
        public IList<Ship> Ships { get; set; }

        public Player NoPlayer = new Player { ClientId = "", Name = "" };

        public void InitPlanets()
        {
            Planets.Add(new Planet { Id = 1, NumShips = 50, Owner = NoPlayer, Radius = 40, X = 50, Y = 60 });
            Planets.Add(new Planet { Id = 2, NumShips = 50, Owner = NoPlayer, Radius = 40, X = 270, Y = 420 });
            Planets.Add(new Planet { Id = 3, NumShips = 45, Owner = NoPlayer, Radius = 35, X = 160, Y = 240 });
            Planets.Add(new Planet { Id = 4, NumShips = 20, Owner = NoPlayer, Radius = 25, X = 250, Y = 100 });
            Planets.Add(new Planet { Id = 5, NumShips = 20, Owner = NoPlayer, Radius = 25, X = 70, Y = 380 });

            Planets.Add(new Planet { Id = 6, NumShips = 15, Owner = NoPlayer, Radius = 20, X = 270, Y = 30 });
            Planets.Add(new Planet { Id = 7, NumShips = 15, Owner = NoPlayer, Radius = 20, X = 140, Y = 125 });
            Planets.Add(new Planet { Id = 8, NumShips = 15, Owner = NoPlayer, Radius = 20, X = 45, Y = 170 });
            Planets.Add(new Planet { Id = 9, NumShips = 15, Owner = NoPlayer, Radius = 20, X = 35, Y = 275 });


            Planets.Add(new Planet { Id = 10, NumShips = 15, Owner = NoPlayer, Radius = 20, X = 50, Y = 450 });
            Planets.Add(new Planet { Id = 11, NumShips = 15, Owner = NoPlayer, Radius = 20, X = 180, Y = 355 });
            Planets.Add(new Planet { Id = 12, NumShips = 15, Owner = NoPlayer, Radius = 20, X = 275, Y = 310 });
            Planets.Add(new Planet { Id = 13, NumShips = 15, Owner = NoPlayer, Radius = 20, X = 285, Y = 205 });
        }


        public void UpdateShips()
        {

            for (int i = Ships.Count; i > 0; i--)
            {
                Ships[i - 1].Update();

                if (Ships[i - 1].IsFinished())
                {
                    if (Ships[i - 1].TargetPlanet.Owner.ClientId == Ships[i - 1].SourcePlanet.Owner.ClientId)
                    {
                        Ships[i - 1].TargetPlanet.NumShips += 5;
                    }
                    else
                    {
                        Ships[i - 1].TargetPlanet.NumShips -= 5;
                        if (Ships[i - 1].TargetPlanet.NumShips <= 0)
                        {
                            Ships[i - 1].TargetPlanet.Owner = Ships[i - 1].SourcePlanet.Owner;
                        }
                    }

                    Ships.Remove(Ships[i - 1]);
                }
            }

            //foreach (Ship ship in Ships)
            //{
            //    if (ship.IsFinished())
            //    {
            //        if (ship.TargetPlanet.Owner.ClientId == ship.SourcePlanet.Owner.ClientId)
            //        {
            //            ship.TargetPlanet.NumShips++;
            //        }
            //        else
            //        {
            //            ship.TargetPlanet.NumShips--;
            //            if (ship.TargetPlanet.NumShips <= 0)
            //            {
            //                ship.TargetPlanet.Owner = ship.SourcePlanet.Owner;
            //            }
            //        }
            //        Ships.Remove(ship);
            //    }
            //    else
            //    {
            //        ship.Update();
            //    }
            //}
        }
    }
}