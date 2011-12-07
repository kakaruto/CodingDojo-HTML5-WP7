using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;

namespace Galcon.SlXnaApp.Models
{
    public class Planet
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Radius { get; set; }

        public int NumShips { get; set; }

        public bool IsSelected { get; set; }

        public string Owner { get; set; }

        public static Planet Load(JToken jToken)
        {
            return new Planet
                       {
                           Id = jToken.Value<int>("Id"),
                           X = jToken.Value<int>("X"),
                           Y = jToken.Value<int>("Y"),
                           Radius = jToken.Value<int>("Radius"),
                           NumShips = jToken.Value<int>("NumShips"),
                           Owner = jToken.Value<JToken>("Owner").Value<string>("ClientId")
                       };
        }

        private Texture2D _planetTexture;
        private Texture2D _selectedTexture;
        private Vector2 _position;
        private Rectangle _rectanglePosition;        
        private SpriteFont _font;


        public void Initialize(Texture2D planetTexture,Texture2D selectedTexture, SpriteFont font)
        {
            _planetTexture = planetTexture;
            _selectedTexture = selectedTexture;

            _position = new Vector2(X - Radius/2, Y - Radius/2);
            _rectanglePosition = new Rectangle(X - Radius, Y - Radius, Radius * 2, Radius * 2);            
            _font = font;
        }

        public void Update()
        {
        }

        public void Draw(SpriteBatch spriteBatch, string cliendId)
        {
            Color color = Color.White;
            if (Owner == cliendId)
            {
                color = Color.Green;
            }
            else if (Owner != "")
            {
                color = Color.Red;
            }

            spriteBatch.Draw(_planetTexture, _rectanglePosition, color);


            spriteBatch.DrawString(_font, NumShips.ToString(), _position, Color.White);
            if (IsSelected)
            {
                spriteBatch.Draw(_selectedTexture, _rectanglePosition, Color.White);
            }
        }

    }
}