using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUnit.Test
{
    [TestClass]
    public class MockDataDrivenClass
    {
        [TestMethod]
        [DataMethod(typeof(MockDataDrivenClass), "PairsOfInteger")]
        public void DataDrivenTest(int a, int b, int expectedResult)
        {
            int actualResult = a + b;
            Assert.Expect(actualResult, expectedResult, nameof(actualResult));
        }

        public static IEnumerable<object[]> PairsOfInteger()
        {
            yield return new object[] { 1, 2, 3 };
            yield return new object[] { 2, 2, 4 };
            yield return new object[] { 3, 4, 7 };
        }
    }
}
