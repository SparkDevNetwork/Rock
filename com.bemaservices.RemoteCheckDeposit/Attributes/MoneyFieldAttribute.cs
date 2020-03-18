using System;
using System.IO;
using System.Reflection;

namespace com.bemaservices.RemoteCheckDeposit.Attributes
{
    /// <summary>
    /// Defines a monetary field on a property. Money is stored as an Ebcdic string
    /// with no decimal characters. Example, $12.34 is stored as "00001234".
    /// </summary>
    public class MoneyFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Defines a new field attribute on a record.
        /// </summary>
        /// <param name="fieldNumber">The order of this field on the record.</param>
        /// <param name="size">The size of this field if known.</param>
        public MoneyFieldAttribute( int fieldNumber, int size )
            : base( fieldNumber, size )
        {
        }

        /// <summary>
        /// Decode a field from a reader into the specified property.
        /// </summary>
        /// <param name="reader">The reader we will be reading from.</param>
        /// <param name="record">The object whose property will be set.</param>
        /// <param name="property">The information about the property to be set.</param>
        public override void DecodeField( BinaryReader reader, Record record, PropertyInfo property )
        {
            string valueStr = reader.ReadEbcdicString( Size );
            decimal value = 0;

            if ( !decimal.TryParse( valueStr, out value ) )
            {
                if ( property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof( Nullable<> ) )
                {
                    property.SetValue( record, null );
                }
                else
                {
                    throw new Exception( "Null money found in data but property not nullable." );
                }
            }
            else
            {
                property.SetValue( record, value / 100 );
            }
        }

        /// <summary>
        /// Encode a field by writing the property value into the writer.
        /// </summary>
        /// <param name="writer">The writer that will contain the encoded data.</param>
        /// <param name="record">The object whose property will be encoded.</param>
        /// <param name="property">The information abou the property to be encoded.</param>
        public override void EncodeField( BinaryWriter writer, Record record, PropertyInfo property )
        {
            decimal? value = ( decimal? ) property.GetValue( record );

            if ( value.HasValue )
            {
                value *= 100;
                if ( value != Math.Floor( value.Value ) )
                {
                    throw new OverflowException( "Money value has more than 2 decimal places." );
                }
            }

            string valueStr = value.HasValue ? value.Value.ToString( "#" ) : new string( ' ', Size );

            writer.WriteEbcdicString( valueStr.PadLeft( Size, '0' ).Right( Size ) );
        }
    }
}
