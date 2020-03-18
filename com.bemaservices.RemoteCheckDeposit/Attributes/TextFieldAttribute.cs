using System.IO;
using System.Reflection;

namespace com.bemaservices.RemoteCheckDeposit.Attributes
{
    /// <summary>
    /// Defines a text string field on a property.
    /// </summary>
    public class TextFieldAttribute : FieldAttribute
    {
        protected FieldJustification Justification { get; set; }

        /// <summary>
        /// Defines a new field attribute on a record, text will be left justified.
        /// </summary>
        /// <param name="fieldNumber">The order of this field on the record.</param>
        /// <param name="size">The size of this field if known.</param>
        public TextFieldAttribute( int fieldNumber, int size )
            : base( fieldNumber, size )
        {
            Justification = FieldJustification.Left;
        }

        /// <summary>
        /// Defines a new field attribute on a record.
        /// </summary>
        /// <param name="fieldNumber">The order of this field on the record.</param>
        /// <param name="size">The size of this field if known.</param>
        /// <param name="justification">The justification of this field.</param>
        public TextFieldAttribute( int fieldNumber, int size, FieldJustification justification )
            : this( fieldNumber, size )
        {
            Justification = justification;
        }

        /// <summary>
        /// Decode a field from a reader into the specified property.
        /// </summary>
        /// <param name="reader">The reader we will be reading from.</param>
        /// <param name="record">The object whose property will be set.</param>
        /// <param name="property">The information about the property to be set.</param>
        public override void DecodeField( BinaryReader reader, Record record, PropertyInfo property )
        {
            property.SetValue( record, reader.ReadEbcdicString( Size ).Trim() );
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

            if ( Justification == FieldJustification.Left )
            {
                value = value.PadRight( Size, ' ' ).Left( Size );
            }
            else
            {
                value = value.Left( Size ).PadLeft( Size, ' ' );
            }

            writer.WriteEbcdicString( value );
        }
    }
}
