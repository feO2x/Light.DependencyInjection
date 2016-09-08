using System.Threading;
using FluentAssertions;
using Light.DependencyInjection.Multithreading;
using Microsoft.Concurrency.TestTools.UnitTesting;

namespace Light.DependencyInjection.ConcurrencyTests
{
    public class ChessTest
    {
        [DataRaceTestMethod]
        public void GetOrAdd()
        {
            var testTarget = new SynchronizedDictionary<int, object>();
            var createObjectMock = new CreateObjectMock();

            ThreadStart getOrAdd = () => testTarget.GetOrAdd(42, createObjectMock.CreateObject);
            var thread1 = new Thread(getOrAdd);
            var thread2 = new Thread(getOrAdd);
            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            createObjectMock.CallCount.Should().Be(1);
        }

        public class CreateObjectMock
        {
            private int _callCount;

            public int CallCount => _callCount;

            public object CreateObject()
            {
                ++_callCount;
                return new object();
            }
        }
    }
}