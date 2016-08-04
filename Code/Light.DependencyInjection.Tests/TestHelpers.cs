using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.Tests
{
    public class A { }

    public class B
    {
        public readonly A OtherObject;
        public readonly int Value;

        public B(A otherObject, int value)
        {
            otherObject.MustNotBeNull(nameof(otherObject));

            OtherObject = otherObject;
            Value = value;
        }
    }

    public interface IC { }

    public class C : IC
    {
        public readonly A ReferenceToA;

        public C(A referenceToA)
        {
            referenceToA.MustNotBeNull(nameof(referenceToA));

            ReferenceToA = referenceToA;
        }
    }

    public class D
    {
        public readonly IList<int> Collection;
        public readonly int SomeNumber = 42;

        public D() : this(new List<int>())
        {
            
        }

        public D(IList<int> collection)
        {
            collection.MustNotBeNull(nameof(collection));

            Collection = collection;
        }

        public D(IList<int> collection, int number) : this(collection)
        {
            SomeNumber = number;
        }
    }
}