namespace Light.DependencyInjection.Tests
{
    public class ClassWithoutDependencies
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
}
