using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogTalk.Services;
using Xunit;

namespace LogTalk.UnitTests.Services
{
    public class KatakotoServiceTests
    {
        IKatakotoService service = new KatakotoService();

        [Fact]
        void TestTranslateGreen() => Assert.Equal("グリーン", service.Translate("Green"));

        [Fact]
        void TestTranslateStar() => Assert.Equal("スター", service.Translate("Star"));
    }
}
