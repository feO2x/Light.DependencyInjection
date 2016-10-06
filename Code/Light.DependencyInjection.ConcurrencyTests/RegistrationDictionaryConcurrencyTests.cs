﻿using System.Threading;
using Light.DependencyInjection.Lifetimes;
using Light.DependencyInjection.Multithreading;
using Light.DependencyInjection.Registrations;
using Light.DependencyInjection.Services;
using Microsoft.Concurrency.TestTools.UnitTesting;

namespace Light.DependencyInjection.ConcurrencyTests
{
    public class RegistrationDictionaryConcurrencyTests
    {
        [DataRaceTestMethod]
        public void GetOrAdd()
        {
            var testTarget = new RegistrationDictionary<Registration>();

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

        [DataRaceTestMethod]
        public void ReadWhileAdd()
        {
            var testTarget = new RegistrationDictionary<Registration>();
            var types = new[] { typeof(Foo), typeof(Bar), typeof(Baz) };
            var containerService = new ContainerServices();
            var writeThread = new Thread(() =>
                                         {
                                             foreach (var type in types)
                                             {
                                                 var typeKey = new TypeKey(type);
                                                 testTarget.GetOrAdd(typeKey, () => containerService.CreateDefaultRegistration(typeKey));
                                                 Thread.Sleep(0);
                                             }
                                         });
            var readThread = new Thread(() =>
                                        {
                                            foreach (var type in types)
                                            {
                                                Registration registration;
                                                testTarget.TryGetValue(new TypeKey(type), out registration);
                                                Thread.Sleep(0);
                                            }
                                        });
            readThread.Start();
            writeThread.Start();

            readThread.Join();
            writeThread.Join();
        }
    }

    public class Foo { }
    public class Bar { }
    public class Baz { }
}