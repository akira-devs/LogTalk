using LogTalk.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace LogTalk.UnitTests.Utils
{
    public class ArpabetTests
    {
        [Fact]
        void TestLoadKanaTable()
        {
            var solution = Regex.Replace(Environment.CurrentDirectory, @"\\LogTalk\.UnitTests\\.*$", "", RegexOptions.IgnoreCase);
            var path = Path.Combine(solution, @"LogTalk\arpabet-kana.csv");

            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                var table = Arpabet.LoadKanaTable(reader);
                Assert.NotEmpty(table);
            }
        }

        [Fact]
        void TestTryParse()
        {
            var solution = Regex.Replace(Environment.CurrentDirectory, @"\\LogTalk\.UnitTests\\.*$", "", RegexOptions.IgnoreCase);
            var path = Path.Combine(solution, @"LogTalk\arpabet-kana.csv");

            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                var table = Arpabet.LoadKanaTable(reader);
                var symbols = new string[] { "K", "AE1", "T" };

                var succeeded = Arpabet.TryParse(table, symbols, out var kana);
                Assert.True(succeeded);
                Assert.Equal("カト", kana);
            }
        }
    }
}
