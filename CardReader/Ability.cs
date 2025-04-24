using SixLabors.ImageSharp;

namespace CardReader
{
    public class Ability
    {
        public string Name { get; }
        public Rectangle Location { get; }
        public string? Value { get; set; }

        public Ability(string name, Rectangle location)
        {
            Name = name;
            Location = location;
        }
    }
}
