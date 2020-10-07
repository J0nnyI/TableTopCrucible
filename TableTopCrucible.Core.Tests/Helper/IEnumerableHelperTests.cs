using Microsoft.VisualStudio.TestTools.UnitTesting;
using TableTopCrucible.Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using static TableTopCrucible.Core.Helper.IEnumerableHelper;
using System.Security.Cryptography.X509Certificates;
using System.Linq;

namespace TableTopCrucible.Core.Helper.Tests
{
    struct TestCLass
    {
        public TestCLass(int key, string value)
        {
            Key = key;
            Value = value;
        }

        public int Key { get; }
        public string Value { get; }
    }
    [TestClass()]
    public class IEnumerableHelperTests
    {
        [TestMethod()]
        public void WhereInTest()
        {
            var values = new TestCLass[]
            {
                new TestCLass(1,"val 1_1"),
                new TestCLass(1,"val 1_2"),
                new TestCLass(2,"val 2_1"),
                new TestCLass(2,"val 2_2"),
                new TestCLass(3,"val 3"),
                new TestCLass(4,"val 4"),
            };
            var res_2_3 = values.WhereIn(new int[] { 2, 3, 5 }, tc => tc.Key);
            Assert.IsTrue(res_2_3.Count() == 3, "count: " + res_2_3.Count().ToString() + Environment.NewLine + string.Join(Environment.NewLine, res_2_3.Select(x => $"{x.Key} | {x.Value}")));
            Assert.IsTrue(res_2_3.Contains(values[2]), "[2]");
            Assert.IsTrue(res_2_3.Contains(values[3]), "[3]");
            Assert.IsTrue(res_2_3.Contains(values[4]), "[4]");
        }
    }
}