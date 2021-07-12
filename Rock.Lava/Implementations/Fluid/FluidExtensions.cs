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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Fluid;
using Fluid.Ast;
using Fluid.Values;

namespace Rock.Lava.Fluid
{
    public static class FluidExtensions
    {
        /// <summary>
        /// Convert a string to a FluidValue object.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static FluidValue ToFluidValue( this string input, TemplateOptions options )
        {
            return FluidValue.Create( input, options );
        }

        /// <summary>
        /// Get a filter argument from the arguments collection, or throw an exception if the argument does not exist.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="ordinalPosition"></param>
        /// <param name="name">An optional name for the parameter that describes the expected value.</param>
        /// <returns></returns>
        public static FluidValue GetArgumentOrThrow( this FilterArguments arguments, int ordinalPosition, string name = null )
        {
            // Check for named argument first.
            if ( !string.IsNullOrWhiteSpace( name )
                 && arguments.HasNamed( name ) )
            {
                return arguments[name];
            }

            // Check for argument in specified ordinal position.
            var value = arguments.At( ordinalPosition - 1 );

            if ( !value.IsNil() )
            {
                return value;
            }

            throw new ArgumentException( $"Argument \"[{ordinalPosition:0;0;?}]{ name }\" not defined." );
        }

        /// <summary>
        /// Get a filter argument from the arguments collection, or return a default value if the argument does not exist.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="ordinalPosition"></param>
        /// <param name="name">An optional name for the parameter that describes the expected value.</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static FluidValue GetArgumentOrDefault( this FilterArguments arguments, int ordinalPosition, string name, string defaultValue, TemplateOptions options )
        {
            // Check for named argument first.
            if ( !string.IsNullOrWhiteSpace( name )
                 && arguments.HasNamed( name ) )
            {
                return arguments[name] as StringValue;
            }

            // Check for argument in specified ordinal position.
            var value = arguments.At( ordinalPosition - 1 );

            if ( !value.IsNil() )
            {
                return FluidValue.Create( value, options );
            }

            // Return default value
            return FluidValue.Create( defaultValue, options );
        }

        /// <summary>
        /// Convert a filter argument to an integer value, or return a default value if the argument cannot be converted.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="ordinalPosition"></param>
        /// <param name="name">An optional name for the parameter that describes the expected value.</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int AsIntegerOrDefault( this FluidValue value, int defaultValue = 0 )
        {
            if ( value.Type == FluidValues.Number )
            {
                return (int)value.ToNumberValue();
            }
            else
            {
                return value.ToStringValue().AsIntegerOrNull() ?? defaultValue;
            }
        }

        /// <summary>
        /// Convert a filter argument to a boolean value, or return a default value if the argument cannot be converted.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="ordinalPosition"></param>
        /// <param name="name">An optional name for the parameter that describes the expected value.</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool AsBooleanOrDefault( this FluidValue value, bool defaultValue = false )
        {
            if ( value.Type == FluidValues.String )
            {
                // To provide a consistent interpretation of True values, we use the Rock conversion function here in place of Fluid.
                return value.ToStringValue().AsBoolean( defaultValue );
            }

            return value.ToBooleanValue();
        }

        /// <summary>
        /// Convert a filter argument to a datetimeoffset value, or return a default value if the argument cannot be converted.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static DateTimeOffset? AsDateTimeOrDefault( this FluidValue value, DateTime? defaultValue = null )
        {
            if ( value.Type == FluidValues.DateTime )
            {
                var offset = (DateTimeOffset)value.ToObjectValue();

                return offset;
            }

            var rawValue = value.ToObjectValue();

            if ( rawValue is DateTimeOffset isDto )
            {
                return isDto;
            }

            if ( rawValue is DateTime isDt )
            {
                return new DateTimeOffset( isDt );
            }

            DateTimeOffset dto;

            var success = DateTimeOffset.TryParse( value.ToStringValue(), out dto );

            if ( success )
            {
                return dto;
            }

            return null;
        }

        /// <summary>
        /// Gets real object value. The standard .ToObjectValue() method may
        /// not return an actual object for certain value types, this one unwraps those
        /// returned values and returns the raw value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The raw object from the FluidValue.</returns>
        /// <remarks>https://github.com/sebastienros/fluid/issues/158</remarks>
        public static object ToRealObjectValue( this FluidValue value )
        {
            if ( value is DictionaryValue )
            {
                var dictionary = value.ToObjectValue();

                var fieldInfo = dictionary.GetType().GetField( "_dictionary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );

                return fieldInfo.GetValue( dictionary );
            }
            else if ( value is ArrayValue )
            {
                var fieldInfo = value.GetType().GetField( "_value", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );

                var values = (IEnumerable<FluidValue>)fieldInfo.GetValue( value );

                return values.Select( a => a.ToRealObjectValue() ).ToList();
            }
            else if ( value is NumberValue nv )
            {
                // Fluid stores all numeric values as decimal. If the value has no decimal places, return an integer instead.
                var d = nv.ToNumberValue();

                int placeCount = BitConverter.GetBytes( decimal.GetBits( d )[3] )[2];

                if ( placeCount == 0 )
                {
                    return (int)d;
                }

                return d;
            }
            else
            {
                return value.ToObjectValue();
            }
        }
        /// <summary>
        /// Add the specified filter to a filter collection, giving the filter the same name as the implementing method.
        /// </summary>
        /// <param name="filterCollection"></param>
        /// <param name="filterDelegate"></param>
        public static void AddFilter( this FilterCollection filterCollection, FilterDelegate filterDelegate )
        {
            filterCollection.AddFilter( filterDelegate.Method.Name, filterDelegate );
        }

        /// <summary>
        /// Registers the Fluid filters from the given type.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="sourceType">Type that contains the static methods to register.</param>
        public static void RegisterFiltersFromType( this FilterCollection collection, Type sourceType )
        {
            var methods = sourceType.GetMethods( BindingFlags.Public | BindingFlags.Static )
                .Where( a =>
                {
                    var p = a.GetParameters();

                    return p.Length == 3
                        && p[0].ParameterType == typeof( FluidValue )
                        && p[1].ParameterType == typeof( FilterArguments )
                        && p[2].ParameterType == typeof( TemplateContext );
                } )
                .ToList();

            foreach ( var m in methods )
            {
                collection.AddFilter( m.Name, (FilterDelegate)m.CreateDelegate( typeof( FilterDelegate ) ) );
            }
        }

        /// <summary>
        /// Convert a set of Fluid Filter arguments to a named dictionary of .NET-typed values.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>

        public static IDictionary<string, object> ConvertFluidFilterArguments( this FilterArgument[] arguments, TemplateContext context )
        {
            var parameters = new Dictionary<string, object>();

            foreach ( var arg in arguments )
            {
                var value = arg.Expression.EvaluateAsync( context ).Result;

                parameters.Add( arg.Name ?? string.Empty, value.ToRealObjectValue() );
            }

            return parameters;
        }
    }
}
