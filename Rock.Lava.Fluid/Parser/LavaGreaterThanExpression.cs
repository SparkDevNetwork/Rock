// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// UnGreater required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Values;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// A derivation of the Fluid GreaterThanExpression that implements the comparsion operators ">" and ">=".
    /// This implementation modifies the default Fluid behavior as follows:
    /// * Adds implicit conversion of non-numeric operands where the other operand is a number.
    /// * Adds date comparisons.
    /// * Adds the default comparison for Types that implement IComparable.
    /// </summary>
    public class LavaGreaterThanExpression : Expression
    {
        public LavaGreaterThanExpression( Expression left, Expression right, bool failIfEqual )
        {
            Left = left;
            Right = right;
            FailIfEqual = failIfEqual;
        }

        /// <summary>
        /// The left operand in the comparison.
        /// </summary>
        public Expression Left { get; }

        /// <summary>
        /// The right operand in the comparison.
        /// </summary>
        public Expression Right { get; }

        /// <summary>
        /// Indicates if equal values will cause the comparison to fail.
        /// </summary>
        public bool FailIfEqual { get; }

        /// <summary>
        /// Evaluates two operands and tries to avoid state machines.
        /// </summary>
        public override ValueTask<FluidValue> EvaluateAsync( TemplateContext context )
        {
            var leftTask = Left.EvaluateAsync( context );
            var rightTask = Right.EvaluateAsync( context );

            if ( leftTask.IsCompletedSuccessfully && rightTask.IsCompletedSuccessfully )
            {
                return Evaluate( leftTask.Result, rightTask.Result );
            }

            return Awaited( leftTask, rightTask );
        }

        [MethodImpl( MethodImplOptions.NoInlining )]
        private async ValueTask<FluidValue> Awaited( ValueTask<FluidValue> leftTask, ValueTask<FluidValue> rightTask )
        {
            var leftValue = await leftTask;
            var rightValue = await rightTask;

            return Evaluate( leftValue, rightValue );
        }

        private FluidValue Evaluate( FluidValue leftValue, FluidValue rightValue )
        {
            // If either value is null, the comparison should evaluate to false.
            // This aligns with the behavior of Shopify Liquid and the historical DotLiquid implementation of Lava,
            // but differs from the standard Fluid implementation (v2.3.1)
            if ( leftValue.IsNil() || rightValue.IsNil() )
            {
                return BooleanValue.False;
            }

            // If either value is numeric, perform a numeric comparison.
            if ( leftValue is NumberValue || rightValue is NumberValue )
            {
                var leftDecimal = leftValue.ToNumberValue();
                var rightDecimal = rightValue.ToNumberValue();

                if ( FailIfEqual )
                {
                    return leftDecimal > rightDecimal
                        ? BooleanValue.True
                        : BooleanValue.False;
                }

                return leftDecimal >= rightDecimal
                    ? BooleanValue.True
                    : BooleanValue.False;
            }

            // If either value is a Date, perform a Date comparison.
            if ( leftValue.Type == FluidValues.DateTime || rightValue.Type == FluidValues.DateTime )
            {
                var leftDateTime = leftValue.AsDateTimeOrDefault();
                var rightDateTime = rightValue.AsDateTimeOrDefault();

                if ( FailIfEqual )
                {

                    return leftDateTime > rightDateTime
                        ? BooleanValue.True
                        : BooleanValue.False;
                }

                return leftDateTime >= rightDateTime
                    ? BooleanValue.True
                    : BooleanValue.False;
            }

            // If either value supports IComparable, attempt to use a built-in comparison.
            var leftObject = leftValue.ToRealObjectValue();
            var rightObject = rightValue.ToRealObjectValue();

            if ( leftObject is IComparable leftComparable )
            {
                if ( FailIfEqual )
                {
                    return leftComparable.CompareTo( rightObject ) > 0 ? BooleanValue.True : BooleanValue.False;
                }
                else
                {
                    return leftComparable.CompareTo( rightObject ) >= 0 ? BooleanValue.True : BooleanValue.False;
                }
            }

            if ( rightObject is IComparable rightComparable )
            {
                if ( FailIfEqual )
                {
                    return rightComparable.CompareTo( leftObject ) > 0 ? BooleanValue.True : BooleanValue.False;
                }
                else
                {
                    return rightComparable.CompareTo( leftObject ) >= 0 ? BooleanValue.True : BooleanValue.False;
                }
            }

            // The values cannot be compared in a meaningful way.
            return NilValue.Instance;
        }
    }
}
