// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System.Collections.Generic;
using System;

namespace Rock.AI.Utilities
{
    /// <summary>
    /// A set of helpful utilities for AI work.
    /// </summary>
    public static class AIHelpers
    {

        /// <summary>
        /// Calculates the cosine similarity between two sets of numerical data.
        /// </summary>
        /// <param name="setLeft"></param>
        /// <param name="setRight"></param>
        /// <returns></returns>
        static double GetCosineSimilarity( List<double> setLeft, List<double> setRight )
        {
            // https://stackoverflow.com/questions/7560760/cosine-similarity-code-non-term-vectors

            var itemsToCompare = 0;
            itemsToCompare = ( ( setRight.Count < setLeft.Count ) ? setRight.Count : setLeft.Count );

            var sumOfProductsOfSets = 0.0d;
            var totalSquaresOfLeft = 0.0d;
            var totalSquaresOfRight = 0.0d;
            for ( var n = 0; n < itemsToCompare; n++ )
            {
                sumOfProductsOfSets += setLeft[n] * setRight[n];
                totalSquaresOfLeft += Math.Pow( setLeft[n], 2 );
                totalSquaresOfRight += Math.Pow( setRight[n], 2 );
            }

            return sumOfProductsOfSets / ( Math.Sqrt( totalSquaresOfLeft ) * Math.Sqrt( totalSquaresOfRight ) );
        }

    }
}
