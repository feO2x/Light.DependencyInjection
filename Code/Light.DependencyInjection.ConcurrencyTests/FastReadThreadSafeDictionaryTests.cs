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
            var testTarget = new FastReadThreadSafeDictionary<int, object>();
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

        [DataRaceTestMethod]
        public void ReadWhileAdd()
        {
            var testTarget = new FastReadThreadSafeDictionary<int, object>();
            var writeThread = new Thread(() =>
                                         {
                                             for (var i = 5; i < 10; i++)
                                             {
                                                 testTarget.GetOrAdd(i, () => new object());
                                                 Thread.Sleep(0);
                                             }
                                         });
            var readThread = new Thread(() =>
                                        {
                                            object value;
                                            testTarget.TryGetValue(5, out value);
                                            Thread.Sleep(0);
                                            testTarget.TryGetValue(7, out value);
                                            Thread.Sleep(10);
                                            testTarget.TryGetValue(9, out value);
                                        });
            readThread.Start();
            writeThread.Start();

            readThread.Join();
            writeThread.Join();
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