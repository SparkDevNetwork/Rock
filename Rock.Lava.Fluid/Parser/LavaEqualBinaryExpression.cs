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
    /// * Modifies boolean comparisons to align with previous Lava implementations.
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

            /* 
               2024-04-19 - DJL

               # Lava (v1.17) and Shopify Liquid Differences.
               The comparison of equality for boolean values in Lava differs from the Shopify Liquid standard.
               Lava is more lenient when interpreting operands as either true or false, in an effort to better capture
               the intention of the code and preserve backward compatibility with previous Lava engine implementations.

               The Shopify standard implementation always returns false for comparisons between operands of different types.
               By contrast, Lava uses a "duck-typing" approach; if either operand is a boolean, an attempt is made to convert
               the other operand to a "true/false" value before any comparison is made. This leads to the differences
               summarized in the following table.

               +----------------+----------------+-------------+-------------------------------------------+
               | EXPRESSION     | SHOPIFY OUTPUT | LAVA OUTPUT | NOTES                                     |
               +----------------+----------------+-------------+-------------------------------------------+
               | true == 'true' | false          | true        | boolean and string type comparison fails. |
               | 'true' == true | false          | true        |                                           |
               | True == true   | false          | true        | Sentence case 'True' is undefined.        |
               +----------------+----------------+-------------+-------------------------------------------+

               # Lava (v1.17) and DotLiquid Differences.
               Lava supports the same boolean equality comparisons as the DotLiquid framework (v1.8), with some minor
               corrections for an inconsistency shown in the table below.

               +----------------+------------------+-------------+ -------------------------------------------------------+
               | EXPRESSION     | DOTLIQUID OUTPUT | LAVA OUTPUT | NOTES                                                  |
               +----------------+------------------+-------------+--------------------------------------------------------+
               | true == 'true' | true             | true        | RHS is converted to boolean and comparison succeeds.   |
               | 'true' == true | false            | true        | RHS is converted to 'True', and case comparison fails. |
               | 'true' == True | false            | true        |                                                        |
               +----------------+------------------+-------------+--------------------------------------------------------+
            */

            // If either value is a boolean and both values can be converted to a boolean, perform a boolean comparison.
            if ( leftValue is BooleanValue )
            {
                var rightBoolean = ConvertToBooleanValueOrNull( rightValue );
                if ( rightBoolean != null )
                {
                    if ( FailIfEqual )
                    {
                        return leftValue == rightBoolean ? BooleanValue.False : BooleanValue.True;
                    }
                    else
                    {
                        return leftValue == rightBoolean ? BooleanValue.True : BooleanValue.False;
                    }
                }
                else
                {
                    if ( rightValue.Type == FluidValues.String )
                    {
                        // When comparing a boolean to a non-truthy string, the result should be false.
                        // Fluid incorrectly converts any non-empty string to BooleanValue.True.
                        return FailIfEqual ? BooleanValue.True : BooleanValue.False;
                    }
                }
            }
            if ( rightValue is BooleanValue )
            {
                var leftBoolean = ConvertToBooleanValueOrNull( leftValue );
                if ( leftBoolean != null )
                {
                    if ( FailIfEqual )
                    {
                        return leftBoolean == rightValue ? BooleanValue.False : BooleanValue.True;
                    }
                    else
                    {
                        return leftBoolean == rightValue ? BooleanValue.True : BooleanValue.False;
                    }
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

            // If both values can be converted to a number, perform a numeric comparison.
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
        /// Converts a FluidValue object to a BooleanValue by applying Rock parsing rules for boolean values.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private BooleanValue ConvertToBooleanValueOrNull( FluidValue value )
        {
            if ( value.Type == FluidValues.Boolean )
            {
                return ( BooleanValue ) value;
            }

            // The Fluid ToBooleanValue() interprets any non-empty string as a true value.
            // However, we only want to interpret specific values as true.
            if ( value.Type == FluidValues.String )
            {
                var stringValue = value.ToStringValue()?.ToLower();
                if ( stringValue == "true" )
                {
                    return BooleanValue.True;
                }
                if ( stringValue == "false" )
                {
                    return BooleanValue.False;
                }
            }

            return null;
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
