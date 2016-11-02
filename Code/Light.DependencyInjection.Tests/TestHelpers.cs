using System;
using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;

namespace Light.DependencyInjection.Tests
{
    public class A { }

    public class B
    {
        public readonly A ReferenceToA;
        public readonly int Value;

        public B(A referenceToA, int value)
        {
            referenceToA.MustNotBeNull(nameof(referenceToA));

            ReferenceToA = referenceToA;
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

        public D() : this(new List<int>()) { }

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

    public class E : A, IE, IF, IG
    {
        public readonly DateTime Date = new DateTime(2016, 8, 5);
        public readonly int Number1 = -42;
        public readonly uint Number2 = 42;
        public readonly double Number3 = 42.778;
        public readonly string Text = "Foo";

        public E() { }

        public E(int number1)
        {
            Number1 = number1;
        }

        public E(int number1, uint number2) : this(number1)
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

    public interface IE { }

    public interface IF { }

    public interface IG { }

    public class F : IF
    {
        public readonly int Number;
        public readonly string Text;

        private F(string text, int number)
        {
            text.MustNotBeNullOrWhiteSpace();
            number.MustNotBeLessThan(0);

            Text = text;
            Number = number;
        }

        public static F Create(string text, int number)
        {
            text = string.IsNullOrWhiteSpace(text) ? "Foo" : text;
            number = number < 0 ? 42 : number;

            return new F(text, number);
        }
    }

    public class G : IG
    {
        private A _referenceToA;

        public A ReferenceToA
        {
            get { return _referenceToA; }
            set
            {
                value.MustNotBeNull(nameof(value));
                _referenceToA = value;
            }
        }
    }

    public class SubG : G { }

    public class H
    {
        public static A StaticInstance;
        public bool BooleanValue;
    }

    public class I
    {
        public I(string text)
        {
            Text = text;
        }

        public string Text { get; }

        public static int SomeStaticNumber { get; set; }
    }

    public class J
    {
        public G ReferenceToG;
    }

    public class SubJ : J { }

    public class K
    {
        public readonly string First;
        public readonly string Second;

        public K(string first, string second)
        {
            First = first;
            Second = second;
        }
    }

    public class GenericType<T>
    {
        public readonly T Value;

        public GenericType(T value)
        {
            Value = value;
        }

        public GenericType(IEnumerable<T> values)
        {
            Value = values.First();
        }
    }

    public class ServiceLocatorClient
    {
        public readonly DependencyInjectionContainer Container;

        public ServiceLocatorClient(DependencyInjectionContainer container)
        {
            Container = container;
        }
    }

    public class L
    {
        public readonly A ReferenceToA;
        public readonly B ReferenceToB;

        public L(A referenceToA, B referenceToB)
        {
            ReferenceToA = referenceToA;
            ReferenceToB = referenceToB;
        }
    }

    public class M
    {
        public readonly IC First;
        public readonly IC Second;

        public M(IC first, IC second)
        {
            First = first;
            Second = second;
        }
    }
}