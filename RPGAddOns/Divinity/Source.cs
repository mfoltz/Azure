namespace RPGAddOns.Divinity
{
    public class DivineData
    {
        public int Divinity { get; set; }
        public List<int> Buffs { get; set; } = new List<int>();

        public DivineData(int divinity, List<int> buffs)
        {
            Divinity = divinity;
            Buffs = buffs;
        }
    }

    internal class Source
    {
        public static void AscendPlayer()
        {
        }
    }
}