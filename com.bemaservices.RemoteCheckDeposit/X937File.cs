using System;
using System.Collections.Generic;
using System.IO;

namespace com.bemaservices.RemoteCheckDeposit
{
    /// <summary>
    /// Defines the basic structure of an X937 file as stored in memory. 
    /// NOTE: This class is not currently used and may be removed.
    /// </summary>
    public class X937File
    {
        #region Properties

        /// <summary>
        /// The list of records that have been added to this file.
        /// </summary>
        public List<Record> Records { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new, empty X937 file.
        /// </summary>
        public X937File()
        {
            Records = new List<Record>();
        }

        /// <summary>
        /// Create a new X937 file by decoding data in the Stream.
        /// </summary>
        /// <param name="dataStream">The Stream that contains the previously encoded X937 data.</param>
        public X937File(Stream dataStream)
            : this()
        {
            using (var reader = new BinaryReader(dataStream))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    Records.Add(Record.DecodeRecord(reader));
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Encode the file by writing the contents into the Stream.
        /// </summary>
        /// <param name="stream">The stream to write the encoded data into.</param>
        public void Encode(Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var record in Records)
                {
                    record.Encode(writer);
                }
            }
        }

        #endregion
    }
}
