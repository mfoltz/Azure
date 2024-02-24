using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.Core
{
    public class CommandParser
    {
        private static List<string> CaseSensitiveCommands = new List<string>()
        {
          "chungusbuff",
          "chungusunbuff"
        };

        public static (string command, string[] parameters) Parse(string input)
        {
            string[] source = input.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (source.Length == 0)
                return ((string)null, (string[])null);
            string lower = source[0].TrimStart('.').ToLower();
            bool flag = false;
            foreach (string sensitiveCommand in CommandParser.CaseSensitiveCommands)
            {
                if (lower == sensitiveCommand)
                    flag = true;
            }
            string[] strArray = flag ? ((IEnumerable<string>)source).Skip<string>(1).ToArray<string>() : ((IEnumerable<string>)source).Skip<string>(1).Select<string, string>((Func<string, string>)(p => p.ToLower())).ToArray<string>();
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
