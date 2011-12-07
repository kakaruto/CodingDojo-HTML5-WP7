namespace Galcon.Server
{
    public class Planet
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Radius { get; set; }

        public int NumShips { get; set; }

        public Player Owner { get; set; }
    }
}