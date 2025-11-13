using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class SystemUtilitySkill
    {
        #region Tool(s)
        [Description( "Sums a list of numbers." )]
        [AgentToolGuid( "0EED94C4-7546-4EE4-BCA8-4EA855E92736" )]
        public RockToolResult Sum( List<double> numbers )
        {
            if ( numbers == null || numbers.Count == 0 )
            {
                return RockToolResult.Error( "The list of numbers cannot be null or empty." );
            }

            return RockToolResult.Success( numbers.Sum() );
        }

        [Description( "Subtracts two numbers." )]
        [AgentToolGuid( "afe5f831-e4e0-495d-8590-723db1214535" )]
        public RockToolResult Subtract( double a, double b )
        {
            return RockToolResult.Success( a - b );
        }

        [Description( "Multiplies two numbers." )]
        [AgentToolGuid( "9179537e-9304-4a61-a875-c2d23d55fe7d" )]
        public RockToolResult Multiply( double a, double b )
        {
            return RockToolResult.Success( a * b );
        }

        [Description( "Divides two numbers." )]
        [AgentToolGuid( "6b8aac69-f30a-486d-b7fe-df754bf4fef1" )]
        public RockToolResult Divide( double a, double b )
        {
            if ( b == 0 )
            {
                return RockToolResult.Error( "Division by zero is not allowed." );
            }
            return RockToolResult.Success( a / b );
        }

        [Description( "Provides an average for a list of numbers." )]
        [AgentToolGuid( "8666c4da-e786-40af-ab67-8782e343fc07" )]
        public RockToolResult Average( List<double> numbers )
        {
            if ( numbers == null || numbers.Count == 0 )
            {
                return RockToolResult.Error( "The list of numbers cannot be null or empty." );
            }

            return RockToolResult.Success( numbers.Average() );
        }

        [Description( "Returns the largest number from a list of numbers." )]
        [AgentToolGuid( "658eea98-209b-41aa-adda-280fbe5b5bac" )]
        public RockToolResult Max( List<double> numbers )
        {
            if ( numbers == null || numbers.Count == 0 )
            {
                return RockToolResult.Error( "The list of numbers cannot be null or empty." );
            }
            return RockToolResult.Success( numbers.Max() );
        }

        [Description( "Provides the smallest number from a list of numbers." )]
        [AgentToolGuid( "585d7e4d-1127-4391-a7cf-e1fcbfc2798b" )]
        public RockToolResult Min( List<double> numbers )
        {
            if ( numbers == null || numbers.Count == 0 )
            {
                return RockToolResult.Error( "The list of numbers cannot be null or empty." );
            }
            return RockToolResult.Success( numbers.Min() );
        }

        [Description( "Provides the median value from a list of numbers." )]
        [AgentToolGuid( "5363a0bc-8868-4eca-b7a2-952d765d72ab" )]
        public RockToolResult Median( List<double> numbers )
        {
            if ( numbers == null || numbers.Count == 0 )
            {
                return RockToolResult.Error( "The list of numbers cannot be null or empty." );
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
            return RockToolResult.Success( median );
        }

        [Description( "Calculates the standard deviation from a list of numbers." )]
        [AgentToolGuid( "5d6f2666-c929-4a9a-bcc9-a673069801bc" )]
        public RockToolResult StandardDeviation( List<double> numbers )
        {
            if ( numbers == null || numbers.Count == 0 )
            {
                return RockToolResult.Error( "The list of numbers cannot be null or empty." );
            }

            double avg = numbers.Average();
            double sumOfSquares = numbers.Select( n => ( n - avg ) * ( n - avg ) ).Sum();
            double stdDev = Math.Sqrt( sumOfSquares / numbers.Count );

            return RockToolResult.Success( stdDev );
        }

        #endregion
    }
}
