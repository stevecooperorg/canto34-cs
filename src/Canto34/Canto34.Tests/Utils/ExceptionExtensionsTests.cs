using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Canto34.Utils;

namespace Canto34.Tests.Utils
{
    [TestClass]
    public class ExceptionExtensionsTests
    {
        [TestMethod]
        public void ShouldThrowExceptionProperly()
        {
            try
            {
                throw new Exception("Foo");
            }
            catch (Exception ex)
            {
                ex.ImpersonateErrorInExternalFile(@"c:\foo\bar.txt", 99);

                var stackTrace = ex.StackTrace;

                var hasFileReference = stackTrace.EndsWith(@"in c:\foo\bar.txt:line 99");
                Assert.IsTrue(hasFileReference);
            }
        }
    }
}
