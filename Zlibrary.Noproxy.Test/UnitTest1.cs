using System.Diagnostics;
using Xunit.Abstractions;
using Zlibrary.Noproxy;

namespace Zlibrary.Noproxy.Test
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;

        public UnitTest1(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public void Test1()
        {
            //var html = Tool.Test().Result;
            //_output.WriteLine(html);
            var result = Tool.DownloadBook("958697", "Downloads").Result;
        }
    }
}