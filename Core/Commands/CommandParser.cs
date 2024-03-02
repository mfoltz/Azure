namespace VPlus.Core.Commands
{
    public class CommandParser
    {
        private static readonly List<string> CaseSensitiveCommands = new List<string>()
        {
          "chungusbuff",
          "chungusunbuff"
        };

        public static (string command, string[] parameters) Parse(string input)
        {
            string[] source = input.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (source.Length == 0)
                return (null, null);
            string lower = source[0].TrimStart('.').ToLower();
            bool flag = false;
            foreach (string sensitiveCommand in CaseSensitiveCommands)
            {
                if (lower == sensitiveCommand)
                    flag = true;
            }
            string[] strArray = flag ? source.Skip(1).ToArray() : source.Skip(1).Select(p => p.ToLower()).ToArray();
            return (lower, strArray);
        }

        public enum BloodType
        {
            mutant = -2017994753,
            warrior = -1094467405,
            frailed = -899826404,
            scholar = -586506765,
            worker = -540707191,
            creature = -77658840,
            brute = 581377887,
            rogue = 793735874,
        }
    }
}
