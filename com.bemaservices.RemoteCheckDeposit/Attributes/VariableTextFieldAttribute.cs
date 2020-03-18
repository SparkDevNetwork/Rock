using System;
using System.IO;
using System.Reflection;

namespace com.bemaservices.RemoteCheckDeposit.Attributes
{
    /// <summary>
    /// Defines a variable length text field on a property.
    /// </summary>
    public class VariableTextFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Defines a new field attribute on a record.
        /// </summary>
        /// <param name="fieldNumber">The order of this field on the record.</param>
        /// <param name="size">The size of the length indicator field.</param>
        public VariableTextFieldAttribute( int fieldNumber, int size )
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
            int length = Convert.ToInt32( reader.ReadEbcdicString( Size ) );
            string value = null;

            if ( length > 0 )
            {
                value = reader.ReadEbcdicString( length );
            }

            property.SetValue( record, value );
        }

        /// <summary>
        /// Encode a field by writing the property value into the writer.
        /// </summary>
        /// <param name="writer">The writer that will contain the encoded data.</param>
        /// <param name="record">The object whose property will be encoded.</param>
        /// <param name="property">The information abou the property to be encoded.</param>
        public override void EncodeField( BinaryWriter writer, Record record, PropertyInfo property )
        {
            string value = ( string ) property.GetValue( record ) ?? string.Empty;
            string length = value.Length.ToString();

            if ( length.Length > Size )
            {
                throw new OverflowException( "Length of variable string too long." );
            }

            writer.WriteEbcdicString( length.PadLeft( Size, '0' ) );
            writer.WriteEbcdicString( value );
        }

        /// <summary>
        /// Gets the total size of this field when it's encoded.
        /// </summary>
        /// <returns>An integer that represents the number of bytes that will be used once it is encoded.</returns>
        public override int DataSize( Record record, PropertyInfo property )
        {
            string value = ( string ) property.GetValue( record ) ?? string.Empty;

            return Size + value.Length;
        }
    }
}
