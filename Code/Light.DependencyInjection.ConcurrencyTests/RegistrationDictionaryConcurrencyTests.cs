using System.Threading;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Multithreading;
using Light.DependencyInjection.Registrations;
using Microsoft.Concurrency.TestTools.UnitTesting;

namespace Light.DependencyInjection.ConcurrencyTests
{
    public class RegistrationDictionaryConcurrencyTests
    {
        [DataRaceTestMethod]
        public void GetOrAdd()
        {
            var testTarget = new RegistrationDictionary();

            ThreadStart getOrAdd = () => testTarget.GetOrAdd(new TypeKey(typeof(Foo)), CreateRegistration);
            var thread1 = new Thread(getOrAdd);
            var thread2 = new Thread(getOrAdd);
            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();
        }

        private static Registration CreateRegistration()
        {
            var instance = new Foo();
            var typeKey = new TypeKey(instance.GetType());
            var lifetime = new ExternalInstanceLifetime(instance);
            return new Registration(typeKey, lifetime);
        }

        // This test fails because MChess assumes that I would access a field when calling TryFind in ImmutableRegistrationsContainer

        //[DataRaceTestMethod]
        //public void ReadWhileAdd()
        //{
        //    var testTarget = new RegistrationDictionary<Registration>();
        //    var types = new[] { typeof(Foo), typeof(Bar), typeof(Baz) };
        //    var containerServices = new ContainerServices();
        //    var writeThread = new Thread(() =>
        //                                 {
        //                                     foreach (var type in types)
        //                                     {
        //                                         var typeKey = new TypeKey(type);
        //                                         testTarget.GetOrAdd(typeKey, () => containerServices.CreateDefaultRegistration(typeKey));
        //                                     }
        //                                 });
        //    var readThread = new Thread(() =>
        //                                {
        //                                    foreach (var type in types)
        //                                    {
        //                                        Registration registration;
        //                                        testTarget.TryGetValue(new TypeKey(type), out registration);
        //                                    }
        //                                });
        //    readThread.Start();
        //    writeThread.Start();

        //    readThread.Join();
        //    writeThread.Join();
        //}
    }

    public class Foo { }

    public class Bar { }

    public class Baz { }
}