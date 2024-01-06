namespace RPGAddOns.Divinity
{
    public class DivineData
    {
        public int Divinity { get; set; }

        public DivineData(int divinity, List<int> buffs)
        {
            Divinity = divinity;
        }
    }

    internal class Source
    {
        public static void AscendPlayer()
        {
        }
    }
}