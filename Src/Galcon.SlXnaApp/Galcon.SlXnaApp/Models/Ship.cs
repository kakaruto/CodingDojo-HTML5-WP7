using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Galcon.SlXnaApp.Models
{
    public class Ship
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Dx { get; set; }
        public double Dy { get; set; }

        public Planet TargetPlanet { get; set; }

        public void Update()
        {
            X = X + Dx;
            Y = Y + Dy;

            _position.X = (float)(_position.X + Dx);
            _position.Y = (float)(_position.Y + Dy);


        }

        public bool IsFinished()
        {
            return Math.Pow((X - TargetPlanet.X), 2) + Math.Pow((Y - TargetPlanet.Y), 2) < Math.Pow(TargetPlanet.Radius * 1.2, 2);
        }

        private Texture2D _shipTexture;
        private Vector2 _position;

        public void Initialize(Texture2D shipTexture)
        {
            _shipTexture = shipTexture;

            _position = new Vector2((float)X, (float)Y);
        }

        public static Ship Load(JToken jToken)
        {
            return new Ship
            {
                X = jToken.Value<int>("X"),
                Y = jToken.Value<int>("Y"),
                Dx = jToken.Value<int>("Dx"),
                Dy = jToken.Value<int>("Dy"),
            };
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_shipTexture, _position, Color.White);
        }
    }
}
