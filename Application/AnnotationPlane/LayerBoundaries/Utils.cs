using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSampleAnnotation.AnnotationPlane.LayerBoundaries
{
    public class Utils
    {
        /// <summary>
        /// Fills in Number array of each boundary with respect to the annotation direction
        /// </summary>
        /// <param name="boundaries">Including outer boundaries of the first and last layer</param>
        /// <returns></returns>
        public static void RecalcBoundaryNumbers(LayerBoundary[] boundaries, AnnotationDirection direction)
        {
            if (boundaries.Length == 0)
                return;

            //asserting conditions
            int maxRank = boundaries.Select(b => b.Rank).Max();
            if (boundaries[0].Rank != maxRank)
                new ArgumentException("the first boundary must be outer boundaries having the max rank");
            if (boundaries[boundaries.Length - 1].Rank != maxRank)
                new ArgumentException("the last boundary must be outer boundaries having the max rank");

            int N = boundaries.Length;

            switch (direction)
            {
                case AnnotationDirection.UpToBottom:
                    int[] recentNumbers = Enumerable.Repeat(0, maxRank + 1).ToArray();
                    recentNumbers[maxRank] = 0; //as the firest boundary increments the the highest rank number                    

                    for (int i = 0; i < N; i++)
                    {
                        LayerBoundary lb = boundaries[i];
                        int maxIdxToAccount = lb.Rank;
                        int[] boundNumbers = new int[maxRank + 1];
                        //updating recent numbers
                        recentNumbers[maxIdxToAccount]++; //highest rank number increases
                        for (int j = 0; j < maxIdxToAccount; j++)
                            recentNumbers[j] = 1; //lower numbers reset ot 1

                        //now copying the part of the recentNumbers to the lb.Number
                        lb.Numbers = recentNumbers.Take(maxIdxToAccount + 1).ToArray();
                    }
                    break;
                default:
                    throw new NotSupportedException();
                    break;
            }

            return;
        }
    }
}
