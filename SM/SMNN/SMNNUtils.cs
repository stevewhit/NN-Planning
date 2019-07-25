using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SM.Data;

namespace SMNN
{
    public static class SMNNUtils
    {
        /// <summary>
        /// Performs the specified action on each of the lists.
        /// </summary>
        public static void ForEach<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second, Action<T1, T2> action)
        {
            using (var enum1 = first.GetEnumerator())
            using (var enum2 = second.GetEnumerator())
            {
                while (enum1.MoveNext() && enum2.MoveNext())
                {
                    action(enum1.Current, enum2.Current);
                }
            }
        }

        /// <summary>
        /// Generates a random number between (and including) min and max.
        /// </summary>
        public static int GenerateRandomNumber(int min, int max)
        {
            var rand = new RNGCryptoServiceProvider();
            var scale = uint.MaxValue;

            while (scale == uint.MaxValue)
            {
                var byteArr = new byte[4];
                rand.GetBytes(byteArr);

                scale = BitConverter.ToUInt32(byteArr, 0);
            }

            return (int)(min + ((max - min) * (scale / (double)uint.MaxValue)));
        }

        /// <summary>
        /// Randomizes the order of items in a list.
        /// </summary>
        public static void Shuffle<T>(this IList<T> listToShuffle)
        {
            var listSize = listToShuffle.Count;
            var numItemsLeftToShuffle = listSize;

            while (numItemsLeftToShuffle > 1)
            {
                numItemsLeftToShuffle--;
                var randomLocation = GenerateRandomNumber(0, listSize - 1);

                var listItemToMove = listToShuffle[numItemsLeftToShuffle];
                listToShuffle[numItemsLeftToShuffle] = listToShuffle[randomLocation];
                listToShuffle[randomLocation] = listItemToMove;
            }
        }
    }
}
