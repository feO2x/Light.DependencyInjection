using System;
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

    public class E
    {
        public readonly int Number1 = -42;
        public readonly uint Number2 = 42;
        public readonly string Text = "Foo";
        public readonly DateTime Date = new DateTime(2016, 8, 5);
        public readonly double Number3 = 42.778;

        public E() { }

        public E(int number1)
        {
            Number1 = number1;
        }

        public E(int number1, uint number2) : this (number1)
        {
            Number2 = number2;
        }

        public E(int number1, uint number2, string text) : this(number1, number2)
        {
            Text = text;
        }

        public E(int number1, uint number2, string text, DateTime date) : this(number1, number2, text)
        {
            Date = date;
        }

        public E(int number1, uint number2, string text, DateTime date, double number3) : this(number1, number2, text, date)
        {
            Number3 = number3;
        }
    }
}