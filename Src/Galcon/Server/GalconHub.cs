using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using Newtonsoft.Json;
using SignalR.Hubs;

namespace Galcon.Server
{
    public class GalconHub : Hub, IDisconnect
    {
        private static readonly IList<Player> Players = new List<Player>();
        private static Space CurrentSpace = new Space();


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan SweepIntervalUpdate = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan SweepIntervalGameLoop = TimeSpan.FromMilliseconds(1000/60);

        private static Timer _timer = new Timer(_ => Refresh(), null, SweepIntervalUpdate, SweepIntervalUpdate);
        //private static Timer _timerGameLoop = new Timer(_ => GameLoop(), null, SweepIntervalGameLoop, SweepIntervalGameLoop);

        public static void Refresh()
        {
            if (!CurrentSpace.IsStarted) return;

            CurrentSpace.UpdateShips();

            foreach (Planet planet in CurrentSpace.Planets)
            {
                planet.NumShips++;
            }

            dynamic clients = GetClients<GalconHub>();

            var list = CurrentSpace.Planets.Select(p => new {p.Id, p.NumShips, p.Owner}).ToList();
            string json = JsonConvert.SerializeObject(list);
            clients.refreshPlanets(json);
        }

        public static void GameLoop()
        {
           
        }

        public GalconHub()
        {
            Logger.Info("New instance of GalconHub");
        }

        public void ResetGame()
        {
            CurrentSpace = new Space();
            //CurrentSpace.IsStarted = true;
            //foreach (Player player in Players)
            //{
            //    AffectPlayerToSpace(player);
            //}
            //string planetsJson = JsonConvert.SerializeObject(CurrentSpace.Planets);
            Clients.reJoin();
        }

        #region IDisconnect Members

        public void Disconnect()
        {
            Disconnect(Context.ClientId);
        }

        #endregion

        public void LaunchAttack(int sourcePlanetId, int targetPlanetId)
        {
            Logger.Info(string.Format("LaunchAttack {0} to {1}", sourcePlanetId, targetPlanetId));

            // legit ?
            string clientId = Context.ClientId;

            Planet sourcePlanet = CurrentSpace.Planets.Where(p => p.Id == sourcePlanetId).Single();
            Planet targetPlanet = CurrentSpace.Planets.Where(p => p.Id == targetPlanetId).Single();

            if (sourcePlanet.Owner.ClientId == clientId)
            {
                try
                {
                    // seems ok, attack !!!
                    Ship ship = new Ship(sourcePlanet, targetPlanet);
                    CurrentSpace.Ships.Add(new Ship(sourcePlanet, targetPlanet));

                    Logger.Info("Ship Launched");                    

                    string shipJson = JsonConvert.SerializeObject(ship);
                    Clients.addShips(shipJson);
                }
                catch (Exception ex)
                {
                   
                }
            }
            else
            {
                Logger.Warn("Tricheur : " + clientId);
                return;
            }
        }

        private void AffectPlayerToSpace(Player player)
        {
            bool spectactor = true;

            foreach (Planet planet in CurrentSpace.Planets)
            {
                if (planet.Owner == null || planet.Owner.ClientId == "")
                {
                    planet.Owner = player;
                    spectactor = false;
                    CurrentSpace.Players.Add(player);
                    break;
                }
            }

            if (spectactor)
            {
                CurrentSpace.Spectators.Add(player);
            }

            if (CurrentSpace.Players.Count >= 2 && CurrentSpace.IsStarted == false)
            {
                //CurrentSpace.InitPlanets();
                CurrentSpace.IsStarted = true;

                Logger.Info("Game Started");
                string planetsJson = JsonConvert.SerializeObject(CurrentSpace.Planets);
                Clients.initPlanets(planetsJson);
            }
        }

        public void Join(string playerName)
        {
            Logger.Info("Player join " + playerName);
            Player player = new Player {ClientId = Context.ClientId, Name = playerName};

            Space mySpace = CurrentSpace;
            bool alreadyStarted = mySpace.IsStarted;

            Players.Add(player);
            AffectPlayerToSpace(player);


            if (alreadyStarted)
            {
                string planetsJson = JsonConvert.SerializeObject(mySpace.Planets);
                Caller.initPlanets(planetsJson);
            }
        }

        public void Leave()
        {
            Disconnect(Context.ClientId);
        }

        public void Send(string message)
        {
            Clients.addMessage(message);
        }

        private void Disconnect(string clientId)
        {
            Logger.Info("Disconnected " + clientId);
            Player player = Players.FirstOrDefault(u => u.ClientId == clientId);

            if (player == null)
            {
                return;
            }

            Players.Remove(player);
        }
    }
}