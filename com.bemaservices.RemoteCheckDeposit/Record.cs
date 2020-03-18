using System;
using System.IO;
using System.Linq;
using System.Reflection;

using com.bemaservices.RemoteCheckDeposit.Attributes;

namespace com.bemaservices.RemoteCheckDeposit
{
    /// <summary>
    /// Provides a basic definition of an X937 record.
    /// </summary>
    public abstract class Record
    {
        #region Properties

        /// <summary>
        /// The type of record this is.
        /// </summary>
        [IntegerField( 1, 2 )]
        public int RecordType { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new data record.
        /// </summary>
        /// <param name="recordType">The type of record that is being initialized.</param>
        protected Record( int recordType )
        {
            RecordType = recordType;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Decodes a record from the binary data reader. The reader should be positioned
        /// at the RecordType field, not at the Record Length Indicator.
        /// </summary>
        /// <param name="reader">The reader containing the data to be decoded.</param>
        /// <returns>A new Record object that has been initialized from the reader.</returns>
        public static Record DecodeRecord( BinaryReader reader )
        {
            int recordType = 0;
            Record record = null;
            long startPosition = reader.BaseStream.Position;

            //
            // Determine the type of record to be decoded.
            //
            try
            {
                reader.ReadBytes( 4 ); // Skip record length indicator
                recordType = Convert.ToInt32( reader.ReadEbcdicString( 2 ) );
            }
            catch
            {
                throw new ArgumentOutOfRangeException( "reader", "Unable to determine record type." );
            }

            //
            // Initialize the blank record based on the type.
            //
            switch ( recordType )
            {
                case 1:
                    record = new Records.FileHeader();
                    break;

                case 10:
                    record = new Records.CashLetterHeader();
                    break;

                case 20:
                    record = new Records.BundleHeader();
                    break;

                case 25:
                    record = new Records.CheckDetail();
                    break;

                case 26:
                    record = new Records.CheckDetailAddendumA();
                    break;

                case 50:
                    record = new Records.ImageViewDetail();
                    break;

                case 52:
                    record = new Records.ImageViewData();
                    break;

                case 70:
                    record = new Records.BundleControl();
                    break;

                case 90:
                    record = new Records.CashLetterControl();
                    break;

                case 99:
                    record = new Records.FileControl();
                    break;

                default:
                    throw new Exception( string.Format( "Unknown record type '{0}' found.", recordType ) );
            }

            //
            // Instruct the record to decode itself from the reader.
            //
            reader.BaseStream.Position = startPosition;
            record.Decode( reader );

            return record;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Decode and load a record into memory by using the data in the reader.
        /// </summary>
        /// <param name="reader">The BinaryReader that contains the data to be decoded.</param>
        public virtual void Decode( BinaryReader reader )
        {
            int recordLength;

            //
            // Determine the length of the record as specified by the Record Length Indicator.
            //
            try
            {
                recordLength = System.Net.IPAddress.NetworkToHostOrder( BitConverter.ToInt32( reader.ReadBytes( 4 ), 0 ) );
            }
            catch ( Exception e )
            {
                throw new InvalidDataException( "Invalid record length.", e );
            }

            long startPosition = reader.BaseStream.Position;

            //
            // Get all properties that have a FieldAttribute defined on them and then order
            // by the FieldNumber.
            //
            var fields = GetType().GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
                .Where( p => Attribute.IsDefined( p, typeof( FieldAttribute ) ) )
                .Select( p => new
                {
                    Property = p,
                    Attribute = p.GetCustomAttribute<FieldAttribute>()
                } )
                .OrderBy( f => f.Attribute.FieldNumber )
                .ToList();

            //
            // Step through each field and decode it into the record.
            //
            foreach ( var field in fields )
            {
                field.Attribute.DecodeField( reader, this, field.Property );
            }

            //
            // Verify that the RLI matches how many bytes we actually decoded.
            //
            if ( recordLength != ( reader.BaseStream.Position - startPosition ) )
            {
                throw new InvalidDataException( string.Format( "Record Length Indicator {0} != {1} actual bytes decoded.", recordLength, reader.BaseStream.Position - startPosition ) );
            }
        }

        /// <summary>
        /// Encode the record into the BinaryWriter. This will include the Record Length Indicator.
        /// </summary>
        /// <param name="writer">The writer that will contain the data.</param>
        public virtual void Encode( BinaryWriter writer )
        {
            //
            // Get all properties that have a FieldAttribute defined on them and then order
            // by the FieldNumber.
            //
            var fields = GetType().GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
                .Where( p => Attribute.IsDefined( p, typeof( FieldAttribute ) ) )
                .Select( p => new
                {
                    Property = p,
                    Attribute = p.GetCustomAttribute<FieldAttribute>()
                } )
                .OrderBy( f => f.Attribute.FieldNumber )
                .ToList();

            //
            // Write the Record Length Indicator.
            //
            int recordSize = fields.Sum( f => f.Attribute.DataSize( this, f.Property ) );
            writer.Write( BitConverter.GetBytes( System.Net.IPAddress.HostToNetworkOrder( recordSize ) ) );

            //
            // Step through each field and encode it into the data writer.
            //
            foreach ( var field in fields )
            {
                field.Attribute.EncodeField( writer, this, field.Property );
            }
        }

        #endregion
    }
}
