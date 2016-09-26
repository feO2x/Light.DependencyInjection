using System.Threading;
using Microsoft.Concurrency.TestTools.UnitTesting;

namespace Light.DependencyInjection.ConcurrencyTests
{
    public class DiContainerTests
    {
        [DataRaceTestMethod]
        public void CreateChildContainerWhileAdd()
        {
            var testTarget = new DiContainer();

            var registerThread = new Thread(() =>
                                            {
                                                testTarget.RegisterTransient<A>();
                                                Thread.Sleep(10);
                                                testTarget.RegisterTransient<B>();
                                                Thread.Sleep(10);
                                                testTarget.RegisterTransient<C>();
                                                Thread.Sleep(10);
                                                testTarget.RegisterTransient<D>();
                                                Thread.Sleep(10);
                                                testTarget.RegisterTransient<E>();
                                            });

            var childContainerThread = new Thread(() =>
                                                  {
                                                      for (var i = 0; i < 4; i++)
                                                      {
                                                          testTarget.CreateChildContainer(createCopyOfMappings: true);
                                                          Thread.Sleep(10);
                                                      }
                                                  });

            registerThread.Start();
            childContainerThread.Start();

            registerThread.Join();
            childContainerThread.Join();
        }
    }

    public class A { }

    public class B { }

    public class C { }

    public class D { }

    public class E { }
}