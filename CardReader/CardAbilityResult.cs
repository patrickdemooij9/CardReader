

using SixLabors.ImageSharp;

namespace CardReader
{
    public class CardAbilityResult
    {
        public Rectangle Location { get; set; }
        public required string Alias { get; set; }
        public required string Value { get; set; }
    }
}
