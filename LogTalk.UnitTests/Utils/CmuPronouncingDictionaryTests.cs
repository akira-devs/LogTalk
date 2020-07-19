using LogTalk.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace LogTalk.UnitTests.Utils
{
    public class CmuPronouncingDictionaryTests
    {
        [Fact]
        void TestLoad()
        {
            var solution = Regex.Replace(Environment.CurrentDirectory, @"\\LogTalk\.UnitTests\\.*$", "", RegexOptions.IgnoreCase);

            IReadOnlyList<(Arpabet.Symbol[], string?)>? arpabet = null;
            var path = Path.Combine(solution, @"LogTalk\arpabet-kana.csv");
            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                arpabet = Arpabet.LoadKanaTable(reader);
            }

            path = Path.Combine(solution, @"LogTalk\cmudict.dict");
            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                var dict = CmuPronouncingDictionary.Load(reader, arpabet);
                Assert.NotEmpty(dict);

                var pronunciation = dict["green"];
                Assert.NotEmpty(pronunciation);
            }
        }
    }
}
