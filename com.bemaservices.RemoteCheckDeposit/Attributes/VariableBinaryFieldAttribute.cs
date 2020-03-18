using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace com.bemaservices.RemoteCheckDeposit.Attributes
{
    /// <summary>
    /// Defines a variable length binary data field on a property.
    /// </summary>
    public class VariableBinaryFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Defines a new field attribute on a record.
        /// </summary>
        /// <param name="fieldNumber">The order of this field on the record.</param>
        /// <param name="size">The size of the length indicator for this field.</param>
        public VariableBinaryFieldAttribute( int fieldNumber, int size )
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

            property.SetValue( record, reader.ReadBytes( length ) );
        }

        /// <summary>
        /// Encode a field by writing the property value into the writer.
        /// </summary>
        /// <param name="writer">The writer that will contain the encoded data.</param>
        /// <param name="record">The object whose property will be encoded.</param>
        /// <param name="property">The information abou the property to be encoded.</param>
        public override void EncodeField( BinaryWriter writer, Record record, PropertyInfo property )
        {
            ICollection<byte> value = ( ICollection<byte> ) property.GetValue( record ) ?? new byte[0];
            string length = value.Count.ToString();

            if ( length.Length > Size )
            {
                throw new OverflowException( "Length of variable binary data too long." );
            }

            writer.WriteEbcdicString( length.PadLeft( Size, '0' ) );
            writer.Write( value.ToArray() );
        }

        /// <summary>
        /// Gets the total size of this field when it's encoded.
        /// </summary>
        /// <returns>An integer that represents the number of bytes that will be used once it is encoded.</returns>
        public override int DataSize( Record record, PropertyInfo property )
        {
            ICollection<byte> value = ( ICollection<byte> ) property.GetValue( record ) ?? new byte[0];

            return Size + value.Count;
        }
    }
}
