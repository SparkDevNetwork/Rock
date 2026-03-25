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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class SystemUtilitySkill
    {
        #region Tool(s)

        [Description( "Sums a list of numbers." )]
        [AgentToolGuid( "0EED94C4-7546-4EE4-BCA8-4EA855E92736" )]
        public IAgentToolResult Sum( List<double> numbers )
        {
            if ( numbers == null || numbers.Count == 0 )
            {
                return Error( "The list of numbers cannot be null or empty." );
            }

            return Success( numbers.Sum() );
        }

        [Description( "Subtracts two numbers." )]
        [AgentToolGuid( "afe5f831-e4e0-495d-8590-723db1214535" )]
        public IAgentToolResult Subtract( double a, double b )
        {
            return Success( a - b );
        }

        [Description( "Multiplies two numbers." )]
        [AgentToolGuid( "9179537e-9304-4a61-a875-c2d23d55fe7d" )]
        public IAgentToolResult Multiply( double a, double b )
        {
            return Success( a * b );
        }

        [Description( "Divides two numbers." )]
        [AgentToolGuid( "6b8aac69-f30a-486d-b7fe-df754bf4fef1" )]
        public IAgentToolResult Divide( double a, double b )
        {
            if ( b == 0 )
            {
                return Error( "Division by zero is not allowed." );
            }
            return Success( a / b );
        }

        [Description( "Provides an average for a list of numbers." )]
        [AgentToolGuid( "8666c4da-e786-40af-ab67-8782e343fc07" )]
        public IAgentToolResult Average( List<double> numbers )
        {
            if ( numbers == null || numbers.Count == 0 )
            {
                return Error( "The list of numbers cannot be null or empty." );
            }

            return Success( numbers.Average() );
        }

        [Description( "Returns the largest number from a list of numbers." )]
        [AgentToolGuid( "658eea98-209b-41aa-adda-280fbe5b5bac" )]
        public IAgentToolResult Max( List<double> numbers )
        {
            if ( numbers == null || numbers.Count == 0 )
            {
                return Error( "The list of numbers cannot be null or empty." );
            }
            return Success( numbers.Max() );
        }

        [Description( "Provides the smallest number from a list of numbers." )]
        [AgentToolGuid( "585d7e4d-1127-4391-a7cf-e1fcbfc2798b" )]
        public IAgentToolResult Min( List<double> numbers )
        {
            if ( numbers == null || numbers.Count == 0 )
            {
                return Error( "The list of numbers cannot be null or empty." );
            }
            return Success( numbers.Min() );
        }

        [Description( "Provides the median value from a list of numbers." )]
        [AgentToolGuid( "5363a0bc-8868-4eca-b7a2-952d765d72ab" )]
        public IAgentToolResult Median( List<double> numbers )
        {
            if ( numbers == null || numbers.Count == 0 )
            {
                return Error( "The list of numbers cannot be null or empty." );
            }
            var sortedNumbers = numbers.OrderBy( n => n ).ToList();
            int count = sortedNumbers.Count;
            double median;
            if ( count % 2 == 0 )
            {
                // Even count, average the two middle numbers
                median = ( sortedNumbers[count / 2 - 1] + sortedNumbers[count / 2] ) / 2;
            }
            else
            {
                // Odd count, take the middle number
                median = sortedNumbers[count / 2];
            }
            return Success( median );
        }

        [Description( "Calculates the standard deviation from a list of numbers." )]
        [AgentToolGuid( "5d6f2666-c929-4a9a-bcc9-a673069801bc" )]
        public IAgentToolResult StandardDeviation( List<double> numbers )
        {
            if ( numbers == null || numbers.Count == 0 )
            {
                return Error( "The list of numbers cannot be null or empty." );
            }

            double avg = numbers.Average();
            double sumOfSquares = numbers.Select( n => ( n - avg ) * ( n - avg ) ).Sum();
            double stdDev = Math.Sqrt( sumOfSquares / numbers.Count );

            return Success( stdDev );
        }

        #endregion
    }
}
