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
    /// A derivation of the Fluid EqualBinaryExpression that implements the comparison operators "==" and "!=".
    /// This implementation modifies the default Fluid behavior as follows:
    /// * Adds implicit conversion of non-numeric operands where the other operand is a number.
    /// * Adds date comparisons.
    /// * Adds the default comparison for Types that implement IComparable.
    /// </summary>
    public class LavaEqualBinaryExpression : Expression
    {
        public LavaEqualBinaryExpression( Expression left, Expression right, bool failIfEqual )
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
            // If the values are of the same type, use the default comparison.
            if ( leftValue.Type == rightValue.Type )
            {
                if ( FailIfEqual )
                {
                    return leftValue.Equals( rightValue ) ? BooleanValue.False : BooleanValue.True;
                }
                else
                {
                    return leftValue.Equals( rightValue ) ? BooleanValue.True : BooleanValue.False;
                }
            }

            // If either value is boolean, perform a boolean comparison.
            if ( leftValue is BooleanValue || rightValue is BooleanValue )
            {
                // If comparing with an empty string, any value will satisfy the predicate.
                if ( leftValue.Type == FluidValues.String && leftValue.ToStringValue() == string.Empty )
                {
                    return FailIfEqual ? BooleanValue.True : BooleanValue.False;
                }
                else if ( rightValue.Type == FluidValues.String && rightValue.ToStringValue() == string.Empty )
                {

                    return FailIfEqual ? BooleanValue.True : BooleanValue.False;
                }

                var leftBoolean = leftValue.ToBooleanValue();
                var rightBoolean = rightValue.ToBooleanValue();

                if ( FailIfEqual )
                {
                    return leftBoolean == rightBoolean ? BooleanValue.False : BooleanValue.True;
                }
                else
                {
                    return leftBoolean == rightBoolean ? BooleanValue.True : BooleanValue.False;
                }
            }

            // If either value is an Enum, perform an Enum comparison.
            if ( leftValue is LavaEnumValue lv )
            {
                return EnumValueIsEqualToOtherTypeValue( lv, rightValue );
            }
            if ( rightValue is LavaEnumValue rv )
            {
                return EnumValueIsEqualToOtherTypeValue( rv, leftValue );
            }

            // If either value is numeric, and both values can be converted to a number, perform a numeric comparison.
            if ( leftValue is NumberValue || rightValue is NumberValue )
            {
                decimal leftDecimal;
                decimal rightDecimal;

                if ( decimal.TryParse( leftValue.ToStringValue(), out leftDecimal )
                    && decimal.TryParse( rightValue.ToStringValue(), out rightDecimal ) )
                {
                    if ( FailIfEqual )
                    {
                        return leftDecimal == rightDecimal ? BooleanValue.False : BooleanValue.True;
                    }
                    else
                    {
                        return leftDecimal == rightDecimal ? BooleanValue.True : BooleanValue.False;
                    }
                }
            }

            // If either value is a Date, perform a Date comparison.
            if ( leftValue.Type == FluidValues.DateTime || rightValue.Type == FluidValues.DateTime )
            {
                var leftDateTime = leftValue.AsDateTimeOrDefault();
                var rightDateTime = rightValue.AsDateTimeOrDefault();

                if ( FailIfEqual )
                {
                    return leftDateTime == rightDateTime ? BooleanValue.False : BooleanValue.True;
                }
                else
                {
                    return leftDateTime == rightDateTime ? BooleanValue.True : BooleanValue.False;
                }
            }

            // Use the default comparison.
            if ( FailIfEqual )
            {
                return leftValue.Equals( rightValue ) ? BooleanValue.False : BooleanValue.True;
            }
            else
            {
                return leftValue.Equals( rightValue ) ? BooleanValue.True : BooleanValue.False;
            }
        }

        /// <summary>
        /// Compares an Enum value with another value for equality.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private BooleanValue EnumValueIsEqualToOtherTypeValue( LavaEnumValue enumValue, FluidValue otherValue )
        {
            if ( otherValue.Type == FluidValues.Number )
            {
                // If the other value is a number, perform a numeric comparison.
                var leftValueInt = enumValue.ToNumberValue();
                int rightValueInt = 0;

                var rightValueNumber = otherValue.ToNumberValue();
                if ( rightValueNumber % 1 == 0 )
                {
                    rightValueInt = ( int ) rightValueNumber;
                }

                if ( FailIfEqual )
                {
                    return leftValueInt == rightValueInt ? BooleanValue.False : BooleanValue.True;
                }
                else
                {
                    return leftValueInt == rightValueInt ? BooleanValue.True : BooleanValue.False;
                }
            }
            else
            {
                // For all other value types, perform a string comparison.
                var leftValueString = enumValue.ToStringValue();
                var rightValueString = otherValue.ToStringValue();

                if ( FailIfEqual )
                {
                    return leftValueString == rightValueString ? BooleanValue.False : BooleanValue.True;
                }
                else
                {
                    return leftValueString == rightValueString ? BooleanValue.True : BooleanValue.False;
                }
            }
        }
    }
}
