using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogTalk.Utils;
using Xunit;

namespace LogTalk.UnitTests.Utils
{
    public class KatakotoTranslatorTests
    {
        ITranslator translator = new KatakotoTranslator();

        [Fact]
        void TestTranslateGreen() => Assert.Equal("グリーン", translator.Translate("Green"));

        [Fact]
        void TestTranslateStar() => Assert.Equal("スター", translator.Translate("Star"));
    }
}
