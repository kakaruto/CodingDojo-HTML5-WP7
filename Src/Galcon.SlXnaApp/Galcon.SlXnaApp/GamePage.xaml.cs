using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Galcon.SlXnaApp.Models;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json.Linq;
using SignalR.Client.Hubs;

namespace Galcon.SlXnaApp
{
    public partial class GamePage : PhoneApplicationPage
    {
        public readonly List<GestureSample> Gestures = new List<GestureSample>();
        private readonly ContentManager _contentManager;
        private readonly GameTimer _timer;

        public TouchCollection TouchState;
        private HubConnection _connection;
        private SpriteFont _font;
        private IHubProxy _myHub;
        private Texture2D _planetSelectionTexture;
        private Texture2D _planetTexture;
        private Texture2D _shipTexture;
        private SpriteBatch _spriteBatch;
        private Texture2D _backgroundTexture;

        public GamePage()
        {
            InitializeComponent();

            // Get the content manager from the application
            _contentManager = (Application.Current as App).Content;

            // Create a timer for this page
            _timer = new GameTimer {UpdateInterval = TimeSpan.FromTicks(333333)};
            _timer.Update += OnUpdate;
            _timer.Draw += OnDraw;

            TouchPanel.EnabledGestures = GestureType.Tap;

            Ships = new List<Ship>();
        }

        private IList<Planet> Planets { get; set; }
        private List<Ship> Ships { get; set; }

        private string _clientId;

        private void InitConnectionToServer()
        {
            _connection = new HubConnection("http://localhost:8717/");
            _myHub = _connection.CreateProxy("Galcon.Server.GalconHub");

            _myHub.On<string>("initPlanets", InitPlanets);
            _myHub.On<string>("refreshPlanets", RefreshPlanets);
            _myHub.On<string>("addShips", AddShips);

            _connection.Start().ContinueWith(task =>
                                                 {
                                                     if ( task.IsFaulted )
                                                     {
                                                         Debug.WriteLine("Failed to start: {0}", task.Exception.GetBaseException());
                                                     }
                                                     else
                                                     {
                                                         Debug.WriteLine("Success! Connected with client id {0}", _connection.ClientId);
                                                         _clientId = _connection.ClientId;
                                                         // Do more stuff here
                                                         _myHub.Invoke("Join", "WP7");
                                                     }
                                                 });
        }

        private void AddShips(string data)
        {
            JObject jObject = JObject.Parse(data);
            Ship ship = Ship.Load(jObject);
            ship.TargetPlanet = Planets.Where(p => p.Id == jObject["TargetPlanet"].Value<int>("Id")).Single();
            
            ship.Initialize(_shipTexture);
            Ships.Add(ship);
        }


        private void RefreshPlanets(string data)
        {
            Debug.WriteLine("RefreshPlanets" + data);
            JArray jArray = JArray.Parse(data);

            foreach (JToken jToken in jArray)
            {
                int planetId = jToken.Value<int>("Id");
                Planet planet = Planets.Where(p => p.Id == planetId).Single();
                planet.NumShips = jToken.Value<int>("NumShips");
                string cliendId = jToken.Value<JToken>("Owner").Value<string>("ClientId");
                if (planet.Owner!= cliendId && planet.IsSelected)
                {
                    planet.IsSelected = false;
                }

                planet.Owner = cliendId;
            }
        }

        private void InitPlanets(string data)
        {
            Debug.WriteLine("InitPlanets" + data);

            JArray jArray = JArray.Parse(data);

            Planets = new List<Planet>();
            foreach (JToken jToken in jArray)
            {
                Planets.Add(Planet.Load(jToken));
            }

            foreach (Planet planet in Planets)
            {
                planet.Initialize(_planetTexture, _planetSelectionTexture, _font);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the sharing mode of the graphics device to turn on XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice);

            _planetTexture = _contentManager.Load<Texture2D>("planet");
            _shipTexture = _contentManager.Load<Texture2D>("ship");
            _planetSelectionTexture = _contentManager.Load<Texture2D>("select");
            _font = _contentManager.Load<SpriteFont>("gameFont");
            _backgroundTexture = _contentManager.Load<Texture2D>("background05");

            Planets = new List<Planet>();

            InitConnectionToServer();

            // Start the timer
            _timer.Start();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Stop the timer
            _timer.Stop();

            _connection.Stop();

            // Set the sharing mode of the graphics device to turn off XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);

            base.OnNavigatedFrom(e);
        }

        private bool IsCollidingWithPlanet(float x, float y, Planet planet)
        {
            return Math.Pow((x - planet.X), 2) + Math.Pow((y - planet.Y), 2) < Math.Pow(planet.Radius, 2);
        }

        /// <summary>
        ///   Allows the page to run logic such as updating the world,
        ///   checking for collisions, gathering input, and playing audio.
        /// </summary>
        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            TouchState = TouchPanel.GetState();

            Gestures.Clear();
            while (TouchPanel.IsGestureAvailable)
            {
                Gestures.Add(TouchPanel.ReadGesture());
            }

            Vector2 lastTouchPosition = new Vector2();
            if ( TouchState.Count > 0 )
            {
                foreach (TouchLocation touch in TouchState)
                {
                    if (touch.State == TouchLocationState.Pressed) lastTouchPosition = touch.Position;
                }
            }

            foreach (Planet planet in Planets)
            {
                if (IsCollidingWithPlanet(lastTouchPosition.X, lastTouchPosition.Y, planet))
                {
                    if (planet.Owner == _clientId)
                    {
                        planet.IsSelected = !planet.IsSelected;
                        
                    }
                    else
                    {
                        foreach (Planet selectedPlanet in Planets)
                        {
                            if (selectedPlanet.IsSelected)
                            {
                                SendAttack(selectedPlanet, planet);
                            }
                        }
                    }
                }
            }
            //foreach (Ship ship in Ships)
            //{
            //    ship.Update();
               
            //}

            for (int i = Ships.Count; i > 0; i--)
            {
                Ships[i -1].Update();
                if(Ships[i -1].IsFinished())
                {
                    Ships.Remove(Ships[i -1]);
                }
            }
            
        }

        private void SendAttack(Planet selectedPlanet, Planet planet)
        {
            _myHub.Invoke("launchAttack", selectedPlanet.Id, planet.Id);
        }

        /// <summary>
        ///   Allows the page to draw itself.
        /// </summary>
        private void OnDraw(object sender, GameTimerEventArgs e)
        {
            SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Start drawing
            _spriteBatch.Begin();

            _spriteBatch.Draw(_backgroundTexture, Vector2.Zero, Color.White);

            // Draw the Planets

            foreach (Planet planet in Planets)
            {
                planet.Draw(_spriteBatch, _clientId);
            }

            foreach (Ship ship in Ships)
            {
                ship.Draw(_spriteBatch);
            }

            // Stop drawing
            _spriteBatch.End();
        }
    }
}