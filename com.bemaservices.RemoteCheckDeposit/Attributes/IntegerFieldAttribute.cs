using System;
using System.IO;
using System.Reflection;

namespace com.bemaservices.RemoteCheckDeposit.Attributes
{
    /// <summary>
    /// Defines an integer field on a property. Integers are stored as Ebcdic strings
    /// that are decoded into integer values.
    /// </summary>
    public class IntegerFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Defines a new integer field attribute on a record.
        /// </summary>
        /// <param name="fieldNumber">The order of this field on the record.</param>
        /// <param name="size">The size of this field if known.</param>
        public IntegerFieldAttribute( int fieldNumber, int size )
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
            int value;

            if ( !Int32.TryParse( valueStr, out value ) )
            {
                if ( property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof( Nullable<> ) )
                {
                    property.SetValue( record, null );
                }
                else
                {
                    throw new Exception( "Null integer found in data but property not nullable." );
                }
            }
            else
            {
                property.SetValue( record, value );
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
            int? value = ( int? ) property.GetValue( record );

            string valueStr = value.HasValue ? value.ToString() : new string( ' ', Size );

            writer.WriteEbcdicString( valueStr.PadLeft( Size, '0' ).Right( Size ) );
        }
    }
}
