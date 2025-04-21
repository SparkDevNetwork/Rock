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
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using Fluid.Values;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// A Fluid Value that represents a .NET Enumeration.
    /// This replaces the default Fluid behavior of interpreting an enumeration as an integer value,
    /// by allowing the value to be interpreted as either a string or an integer according to context.
    /// </summary>
    public class LavaEnumValue : FluidValue
    {
        private readonly Enum _enum;

        public LavaEnumValue( Enum value )
        {
            _enum = value;
        }

        public override FluidValues Type
        {
            get
            {
                // Represents multiple types, so return an Object.
                return FluidValues.Object;
            }
        }

        public override bool Equals( FluidValue other )
        {
            if ( other.IsNil() )
            {
                return false;
            }
            if ( other.Type == FluidValues.String )
            {
                return other.ToStringValue().Equals( Enum.GetName( _enum.GetType(), _enum ), StringComparison.OrdinalIgnoreCase );
            }
            if ( other.Type == FluidValues.Number )
            {
                return other.ToNumberValue().ToIntSafe().Equals( Convert.ToInt32( _enum ) );
            }
            if ( other is LavaEnumValue ev )
            {
                return ev.ToNumberValue() == ToNumberValue();
            }
            return false;
        }

        public override bool ToBooleanValue()
        {
            return true;
        }

        public override decimal ToNumberValue()
        {
            return Convert.ToDecimal( _enum );
        }

        public override string ToStringValue()
        {
            return Enum.GetName( _enum.GetType(), _enum );
        }

        public override void WriteTo( TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo )
        {
            AssertWriteToParameters( writer, encoder, cultureInfo );

            writer.Write( _enum.ToString() );
        }

        public override object ToObjectValue()
        {
            return _enum;
        }

        public override bool Equals( object other )
        {
            // The is operator will return false if null
            if ( other is int otherInt )
            {
                return otherInt.Equals( Convert.ToInt32( _enum ) );
            }
            else if ( other is string otherString )
            {
                return otherString.Equals( _enum.ToString(), StringComparison.OrdinalIgnoreCase );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _enum.GetHashCode();
        }
    }
}
