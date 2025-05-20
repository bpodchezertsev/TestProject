using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject
{
    [TestClass]
    public class RefHelperTest
    {

        public class TestClass1
        {
            private int _a;
        }

        public class TestClass2 : TestClass1
        {
            private double _b;
        }

        public class TestClass3 : TestClass1
        {
            public TestClass1? A;
            public TestClass1? B;
        }
        
        [TestMethod]
        public void MyTestMethod()
        {
        }

        public static void Main()
        {
            testInheritance();

            testFields();
        }

        private static void testFields()
        {
            string s = "";
            RefHelper.FieldsList(s.GetType()).Select(t => t.ToString()).ForEach(Console.WriteLine);
            Console.WriteLine();
            
            TestClass1 a = new TestClass1();
            RefHelper.FieldsList(a.GetType()).Select(t => t.ToString()).ForEach(Console.WriteLine);
            Console.WriteLine();
            
            TestClass2 b = new TestClass2();
            RefHelper.FieldsList(b.GetType()).Select(t => t.ToString()).ForEach(Console.WriteLine);
            Console.WriteLine();
        }

        private static void testInheritance()
        {
            string s = "";
            RefHelper.InheritanceArray(s.GetType()).Select(t => t.FullName).ForEach(Console.WriteLine);
            Console.WriteLine();
            RefHelper.InheritanceArray(typeof(String)).Select(t => t.FullName).ForEach(Console.WriteLine);
            Console.WriteLine();
            RefHelper.InheritanceArray(typeof(Object)).Select(t => t.FullName).ForEach(Console.WriteLine);
            Console.WriteLine();
            RefHelper.InheritanceArray(typeof(int)).Select(t => t.FullName).ForEach(Console.WriteLine);
            Console.WriteLine();
            RefHelper.InheritanceArray(123.GetType()).Select(t => t.FullName).ForEach(Console.WriteLine);
            Console.WriteLine();
            // Console.WriteLine(123 is Object);
            object o = 123;
            // Console.WriteLine(o is Object);
            RefHelper.InheritanceArray(o.GetType()).Select(t => t.FullName).ForEach(Console.WriteLine);
            Console.WriteLine();
        }
    }
}