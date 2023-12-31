namespace RPGAddOns
{
    public class RankData
    {
        public int Level { get; set; }
        public int Points { get; set; }
        public List<string> Buffs { get; set; } = new List<string>();

        public RankData(int level, int points, List<string> buffs)
        {
            Level = level;
            Points = points;
            Buffs = buffs;
        }
    }

    internal class PvERankSystem
    {
        // WIP
    }
}