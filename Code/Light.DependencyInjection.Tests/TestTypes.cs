namespace Light.DependencyInjection.Tests
{
    public class ClassWithoutDependencies : IAbstractionA
    {
        
    }

    public class ClassWithDependency
    {
        public readonly ClassWithoutDependencies A;

        public ClassWithDependency(ClassWithoutDependencies a)
        {
            A = a;
        }
    }

    public class ClassWithTwoDependencies
    {
        public readonly ClassWithoutDependencies A;
        public readonly ClassWithDependency B;

        public ClassWithTwoDependencies(ClassWithoutDependencies a, ClassWithDependency b)
        {
            A = a;
            B = b;
        }
    }

    public interface IAbstractionA { }
}
