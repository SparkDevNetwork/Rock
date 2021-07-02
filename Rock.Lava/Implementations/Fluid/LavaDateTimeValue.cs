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
    /// A derivation of the Fluid DateTimeValue that renders the value as a local datetime string in the general system format,
    /// as opposed to the UTC format used by Fluid.
    /// </summary>
    public class LavaDateTimeValue : FluidValue
    {
        private readonly DateTimeOffset _value;

        public LavaDateTimeValue( DateTimeOffset value )
        {
            _value = value;
        }

        public override FluidValues Type => FluidValues.DateTime;

        public override bool Equals( FluidValue other )
        {
            if ( other.IsNil() )
            {
                return false;
            }

            if ( other.Type != FluidValues.DateTime )
            {
                return false;
            }

            return _value.Equals( ( ( LavaDateTimeValue ) other )._value );
        }

        public override bool ToBooleanValue()
        {
            return true;
        }

        public override decimal ToNumberValue()
        {
            return _value.Ticks;
        }

        public override string ToStringValue()
        {
            return _value.ToString( "u", CultureInfo.InvariantCulture );
        }

        public override void WriteTo( TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo )
        {
            AssertWriteToParameters( writer, encoder, cultureInfo );

            // Output the value as a local datetime in the General Date/Time format.
            writer.Write( _value.LocalDateTime.ToString( "G", cultureInfo ) );
            //$"{ cultureInfo.DateTimeFormat.ShortDatePattern } { cultureInfo.DateTimeFormat.ShortTimePattern }" ) );
        }

        public override object ToObjectValue()
        {
            return _value;
        }

        public override bool Equals( object other )
        {
            // The is operator will return false if null
            if ( other is DateTimeOffset otherValue )
            {
                return _value.Equals( otherValue );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }
}
