namespace CardReader
{
    public class CardAbility
    {
        public required string Alias { get; set; }
        public required double PosX1 { get; set; }
        public required double PosX2 { get; set; }
        public required double PosY1 { get; set; }
        public required double PosY2 { get; set; }
        public bool IgnoreWhitespace { get; set; }
        public bool IgnoreSpecialCharacters { get; set; }
        public Dictionary<string, string[]> Conditions { get; set; }

        public CardAbility()
        {
            Conditions = new Dictionary<string, string[]>();
        }
    }
}
