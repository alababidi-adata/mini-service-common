using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AutoFixture;
using Bogus;

namespace Common.Tests
{
    public static class FixtureExtension
    {
        public static IFixture RegisterFakers<TAssemblyMarker>(this IFixture fixture)
        {
            fixture.Inject(new Faker());

            var fakers = typeof(TAssemblyMarker).Assembly
                .GetExportedTypes()
                .Where(o => !o.IsAbstract && !o.IsInterface)
                .Where(o => o.IsAssignableTo(typeof(IFakerTInternal)))
                .ToList();

            var fakerList = string.Join(Environment.NewLine, fakers.Select(o => o.Name));
            Debug.WriteLine($"Found {fakers.Count} fakers:{Environment.NewLine}{fakerList}");

            var newMethod = typeof(FixtureExtension).GetMethod(
                                name: nameof(Register),
                                bindingAttr: BindingFlags.Public | BindingFlags.Static)
                            ?? throw new Exception($"Couldn't find consumer test harness method.");

            foreach (var faker in fakers)
            {
                var regMethod = newMethod.MakeGenericMethod(faker);
                regMethod.Invoke(null, new object?[] { fixture });
                Debug.WriteLine($"Registered {faker.FullName}");
            }

            return fixture;
        }

        public static void Register<T>(IFixture fixture) where T : new() => fixture.Register(New<T>);
        private static T New<T>() where T : new() => new();
    }
}
