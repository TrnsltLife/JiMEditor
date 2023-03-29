using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiME.Procedural.GenerationLogic
{
    static class GeneratorUtils
    {
        public static T GetRandomEnum<T>(Random random, params T[] except) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            return Enum.GetNames(typeof(T))
                .Select(e => (T)Enum.Parse(typeof(T), e))
                .Except(except)
                .GetRandomFromEnumerable(random);
        }

        public static T GetRandomFromEnumerable<T>(this IEnumerable<T> enumerable, Random random)
        {
            var index = random.Next(enumerable.Count());
            return enumerable.Skip(index).Take(1).Single();
        }

        public static IEnumerable<T> GetRandomFromEnumerable<T>(this IEnumerable<T> enumerable, Random random, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return GetRandomFromEnumerable(enumerable, random);
            }
        }
    }
}
