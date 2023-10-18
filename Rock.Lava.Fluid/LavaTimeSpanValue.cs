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
    /// A FluidValue that renders a TimeSpan value using the general system format,
    /// as opposed to the UTC format that is the Fluid default.
    /// </summary>
    public class LavaTimeSpanValue : FluidValue
    {
        private readonly TimeSpan _value;

        public LavaTimeSpanValue( TimeSpan value )
        {
            _value = value;
        }

        public override FluidValues Type
        {
            get
            {
                return FluidValues.Object;
            }
        }

        public override bool Equals( FluidValue other )
        {
            if ( other.IsNil() )
            {
                return false;
            }

            if ( other.Type != FluidValues.Object )
            {
                return false;
            }

            return _value.Equals( ( ( LavaTimeSpanValue ) other )._value );
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
            return _value.ToString( "c", CultureInfo.InvariantCulture );
        }

        public override void WriteTo( TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo )
        {
            AssertWriteToParameters( writer, encoder, cultureInfo );

            // Output the value in the Constant/Invariant Time format (d.hh:mm:ss.fffffff).
            var outputDate = _value.ToString( "c", cultureInfo );

            writer.Write( outputDate );
        }

        public override object ToObjectValue()
        {
            return _value;
        }

        public override bool Equals( object other )
        {
            if ( other is TimeSpan otherValue )
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
