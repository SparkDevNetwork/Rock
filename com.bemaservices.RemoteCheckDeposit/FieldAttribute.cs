using System;
using System.IO;
using System.Reflection;

namespace com.bemaservices.RemoteCheckDeposit
{
    /// <summary>
    /// Defines the basic information about X937 fields.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property, Inherited = false, AllowMultiple = false )]
    public abstract class FieldAttribute : System.Attribute
    {
        /// <summary>
        /// The field number, used to sort fields so they get encoded and decoded in the correct order.
        /// </summary>
        public int FieldNumber { get; private set; }

        /// <summary>
        /// Specifies the length of the field when it is a known length field.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Defines a new field attribute on a record.
        /// </summary>
        /// <param name="fieldNumber">The order of this field on the record.</param>
        /// <param name="size">The size of this field if known.</param>
        protected FieldAttribute( int fieldNumber, int size )
        {
            FieldNumber = fieldNumber;
            Size = size;
        }

        /// <summary>
        /// Decode a field from a reader into the specified property.
        /// </summary>
        /// <param name="reader">The reader we will be reading from.</param>
        /// <param name="record">The object whose property will be set.</param>
        /// <param name="property">The information about the property to be set.</param>
        public abstract void DecodeField( BinaryReader reader, Record record, PropertyInfo property );

        /// <summary>
        /// Encode a field by writing the property value into the writer.
        /// </summary>
        /// <param name="writer">The writer that will contain the encoded data.</param>
        /// <param name="record">The object whose property will be encoded.</param>
        /// <param name="property">The information abou the property to be encoded.</param>
        public abstract void EncodeField( BinaryWriter writer, Record record, PropertyInfo property );

        /// <summary>
        /// Gets the total size of this field when it's encoded.
        /// </summary>
        /// <returns>An integer that represents the number of bytes that will be used once it is encoded.</returns>
        public virtual int DataSize( Record record, PropertyInfo property )
        {
            return Size;
        }
    }

    /// <summary>
    /// The justification of a field in the EBCDIC encoded stream.
    /// </summary>
    public enum FieldJustification
    {
        /// <summary>
        /// Left justify the field, padding will be performed on the right side.
        /// </summary>
        Left = 0,

        /// <summary>
        /// Right justify the field, padding will be performed on the left side.
        /// </summary>
        Right = 1
    }
}
