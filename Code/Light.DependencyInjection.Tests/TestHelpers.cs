namespace Light.DependencyInjection.Tests
{
    public class A { }

    public class B
    {
        public readonly A OtherObject;
        public readonly int Value;

        public B(A otherObject, int value)
        {
            OtherObject = otherObject;
            Value = value;
        }
    }

    public class C
    {
        public readonly A ReferenceToA;

        public C(A referenceToA)
        {
            ReferenceToA = referenceToA;
        }
    }
}