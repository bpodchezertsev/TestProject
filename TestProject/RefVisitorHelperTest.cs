using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject;

[TestClass]
public class RefVisitorHelperTest
{
    [TestMethod]
    public void MyTestMethod()
    {
    }

    public static void Main()
    {
        testWrite();
    }

    private static void testWrite()
    {
        RefHelperTest.TestClass2 b = new RefHelperTest.TestClass2();
        RefVisitorHelper.WriteStruct(Console.Out, b);
        Console.WriteLine();

        RefHelperTest.TestClass3 c = new RefHelperTest.TestClass3();
        c.A = c;
        c.B = c;
        RefVisitorHelper.WriteStruct(Console.Out, c);
        Console.WriteLine();
    }
}