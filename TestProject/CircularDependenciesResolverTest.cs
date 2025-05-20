using System.Runtime.CompilerServices;

namespace TestProject;

public class CircularDependenciesResolverTest
{
    class Test1
    {
        private readonly Test2 _t2;
        private readonly Test3 _t3;

        public Test1(Action<Test1> register, LazyValue<Test2> test2, LazyValue<Test3> test3)
        {
            register(this);
            _t2 = test2.Get();
            _t3 = test3.Get();
        }

        public override string ToString() =>
            $"{GetType().FullName}[{RuntimeHelpers.GetHashCode(this)}]: {nameof(_t2)}: {RuntimeHelpers.GetHashCode(_t2)}, {nameof(_t3)}: {RuntimeHelpers.GetHashCode(_t3)}";
    }

    class Test2
    {
        private readonly Test1 _t1;
        private readonly Test3 _t3;

        public Test2(Action<Test2> register, LazyValue<Test1> test1, LazyValue<Test3> test3)
        {
            register(this);
            _t1 = test1.Get();
            _t3 = test3.Get();
        }

        public override string ToString() =>
            $"{GetType().FullName}[{RuntimeHelpers.GetHashCode(this)}]: {nameof(_t1)}: {RuntimeHelpers.GetHashCode(_t1)}, {nameof(_t3)}: {RuntimeHelpers.GetHashCode(_t3)}";
    }

    class Test3
    {
        private readonly Test1 _t1;
        private readonly Test2 _t2;

        public Test3(Action<Test3> register, LazyValue<Test1> test1, LazyValue<Test2> test2)
        {
            register(this);
            _t1 = test1.Get();
            _t2 = test2.Get();
        }

        public override string ToString() =>
            $"{GetType().FullName}[{RuntimeHelpers.GetHashCode(this)}]: {nameof(_t1)}: {RuntimeHelpers.GetHashCode(_t1)}, {nameof(_t2)}: {RuntimeHelpers.GetHashCode(_t2)}";
    }

    class References
    {
        private readonly LazyValue<Test1> _value1;
        private readonly LazyValue<Test2> _value2;
        private readonly LazyValue<Test3> _value3;

        public References()
        {
            _value1 = new LazyValue<Test1>(register => new Test1(register, Value2, Value3));
            _value2 = new LazyValue<Test2>(register => new Test2(register, Value1, Value3));
            _value3 = new LazyValue<Test3>(register => new Test3(register, Value1, Value2));
        }

        // ReSharper disable once ConvertToAutoProperty
        public LazyValue<Test1> Value1 => _value1;

        // ReSharper disable once ConvertToAutoProperty
        public LazyValue<Test2> Value2 => _value2;

        // ReSharper disable once ConvertToAutoProperty
        public LazyValue<Test3> Value3 => _value3;
    }

    public static void Main()
    {
        References references = new References { };
        Test2 test2 = references.Value2.Get();
        RefVisitorHelper.WriteStruct(Console.Out, test2);
        Console.WriteLine();
        Console.WriteLine(references.Value1.Get());
        Console.WriteLine(references.Value2.Get());
        Console.WriteLine(references.Value3.Get());
    }

}