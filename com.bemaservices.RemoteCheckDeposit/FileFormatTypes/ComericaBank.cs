using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Model;

using com.bemaservices.RemoteCheckDeposit.Model;
using com.bemaservices.RemoteCheckDeposit.Records;
using com.bemaservices.RemoteCheckDeposit;
using System.Text;

namespace com.bemaservices.RemoteCheckDeposit.FileFormatTypes
{
    /// <summary>
    /// Defines the x937 File export for Comerica Bank
    /// </summary>
    [Description( "Processes a batch export for Comerica Bank." )]
    [Export( typeof( FileFormatTypeComponent ) )]
    [ExportMetadata( "ComponentName", "Comerica Bank" )]
    [EncryptedTextField( "Origin Routing Number", "Used on Type 10 Record 3 for Account Routing", true, key: "OriginRoutingNumber" )]
    [EncryptedTextField( "Destination Routing Number", "", true, "072000096", key: "DestinationRoutingNumber" )]

    public class ComericaBank : X937DSTU
    {
        #region System Setting Keys

        /// <summary>
        /// The system setting for the next cash header identifier. These should never be
        /// repeated. Ever.
        /// </summary>
        protected const string SystemSettingNextCashHeaderId = "ComericaBank.NextCashHeaderId";

        /// <summary>
        /// The system setting that contains the last file modifier we used.
        /// </summary>
        protected const string SystemSettingLastFileModifier = "ComericaBank.LastFileModifier";

        /// <summary>
        /// The last item sequence number used for items.
        /// </summary>
        protected const string LastItemSequenceNumberKey = "ComericaBank.LastItemSequenceNumber";

        #endregion

        /// <summary>
        /// Gets the next item sequence number.
        /// </summary>
        /// <returns>An integer that identifies the unique item sequence number that can be used.</returns>
        protected int GetNextItemSequenceNumber()
        {
            int lastSequence = GetSystemSetting( LastItemSequenceNumberKey ).AsIntegerOrNull() ?? 0;
            int nextSequence = lastSequence + 1;

            SetSystemSetting( LastItemSequenceNumberKey, nextSequence.ToString() );

            return nextSequence;
        }

        /// <summary>
        /// Gets the file header record (type 01).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <returns>
        /// A FileHeader record.
        /// </returns>
        protected override FileHeader GetFileHeaderRecord( ExportOptions options )
        {
            var header = base.GetFileHeaderRecord( options );

            header.ImmediateOriginRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "OriginRoutingNumber" ) );
            header.ImmediateDestinationRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "DestinationRoutingNumber" ) );

            //
            // The combination of the following fields must be unique:
            // DestinationRoutingNumber + OriginatingRoutingNumber + CreationDateTime + FileIdModifier
            //
            // If the last file we sent has the same routing numbers and creation date time then
            // increment the file id modifier.
            //
            var fileIdModifier = "A";
            var hashText = header.ImmediateDestinationRoutingNumber + header.ImmediateOriginRoutingNumber + header.FileCreationDateTime.ToString( "yyyyMMdd" );
            var hash = HashString( hashText );

            //
            // find the last modifier, if there was one.
            //
            var lastModifier = GetSystemSetting( SystemSettingLastFileModifier );
            if ( !string.IsNullOrWhiteSpace( lastModifier ) )
            {
                var components = lastModifier.Split( '|' );

                if ( components.Length == 2 )
                {
                    //
                    // If the modifier is for the same file, increment the file modifier.
                    //
                    if ( components[0] == hash )
                    {
                        fileIdModifier = ( ( char ) ( components[1][0] + 1 ) ).ToString();

                        //
                        // If we have done more than 26 files today, assume we are testing and start back at 'A'.
                        //
                        if ( fileIdModifier[0] > 'Z' )
                        {
                            fileIdModifier = "A";
                        }
                    }
                }
            }

            header.FileIdModifier = fileIdModifier;
            SetSystemSetting( SystemSettingLastFileModifier, string.Join( "|", hash, fileIdModifier ) );

            return header;
        }

        /// <summary>
        /// Gets the cash letter header record (type 10).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <returns>
        /// A CashLetterHeader record.
        /// </returns>
        protected override CashLetterHeader GetCashLetterHeaderRecord( ExportOptions options )
        {
            int cashHeaderId = GetSystemSetting( SystemSettingNextCashHeaderId ).AsIntegerOrNull() ?? 0;

            var header = base.GetCashLetterHeaderRecord( options );
            header.ID = cashHeaderId.ToString( "D8" );
            SetSystemSetting( SystemSettingNextCashHeaderId, ( cashHeaderId + 1 ).ToString() );

            //Change ECE Institution Routing Number to one given in setup
            header.ClientInstitutionRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "InstitutionRoutingNumber" ) );

            //Change Work Type to C
            header.WorkType = "C";

            header.DestinationRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "DestinationRoutingNumber" ) );

            return header;
        }

        /// <summary>
        /// Gets the bundle header record (type 20).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="bundleIndex">Number of existing bundle records in the cash letter.</param>
        /// <returns>A BundleHeader record.</returns>
        protected override Records.BundleHeader GetBundleHeader( ExportOptions options, int bundleIndex )
        {
            var header = base.GetBundleHeader( options, bundleIndex );

            //Change Institution Routing
            header.ClientInstitutionRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "InstitutionRoutingNumber" ) );

            //Change Cycle to 01
            header.ID = ( bundleIndex + 1 ).ToString( "0000000000" );
            header.CycleNumber = "01";

            header.DestinationRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "DestinationRoutingNumber" ) );

            return header;
        }

        /// <summary>
        /// Gets the credit detail deposit record (type 61).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="bundleIndex">Number of existing bundle records in the cash letter.</param>
        /// <param name="transactions">The transactions associated with this deposit.</param>
        /// <returns>
        /// A collection of records.
        /// </returns>
        protected override List<Record> GetCreditDetailRecords( ExportOptions options, int bundleIndex, List<FinancialTransaction> transactions )
        {
            //No Type 61 Record Needed
            var records = new List<Record>();
            
            return records;
        }

        /// <summary>
        /// Gets the records that identify a single check being deposited.
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="transaction">The transaction to be deposited.</param>
        /// <returns>
        /// A collection of records.
        /// </returns>
        protected override List<Record> GetItemRecords( ExportOptions options, FinancialTransaction transaction )
        {
            var records = base.GetItemRecords( options, transaction );
            var sequenceNumber = GetNextItemSequenceNumber();

            //
            // Modify the Check Detail Record and Check Image Data records to have
            // a unique item sequence number.
            //
            var checkDetail = records.Where( r => r.RecordType == 25 ).Cast<Records.CheckDetail>().FirstOrDefault();
            checkDetail.ClientInstitutionItemSequenceNumber = sequenceNumber.ToString( "000000000000000" );
            checkDetail.ElectronicReturnAcceptanceIndicator = "0";
            checkDetail.MICRValidIndicator = 1; 

            //Modify Check Detail Adden A
            var checkDetailA = records.Where( r => r.RecordType == 26 ).Cast<Records.CheckDetailAddendumA>().FirstOrDefault();
            checkDetailA.TruncationIndicator = "Y";
            checkDetailA.BankOfFirstDepositAccountNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "AccountNumber" ) );

            foreach ( var imageData in records.Where( r => r.RecordType == 52 ).Cast<dynamic>() )
            {
                imageData.ClientInstitutionItemSequenceNumber = sequenceNumber.ToString( "000000000000000" );
            }

            return records;
        }

        ///// <summary>
        ///// Gets the credit detail deposit record (type 61).
        ///// </summary>
        ///// <param name="options">Export options to be used by the component.</param>
        ///// <param name="transactions">The transactions associated with this deposit.</param>
        ///// <param name="isFrontSide">True if the image to be retrieved is the front image.</param>
        ///// <returns>A stream that contains the image data in TIFF 6.0 CCITT Group 4 format.</returns>
        //protected virtual Stream GetDepositSlipImage( ExportOptions options, CreditDetail creditDetail, bool isFrontSide )
        //{
        //    var bitmap = new System.Drawing.Bitmap( 1200, 550 );
        //    var g = System.Drawing.Graphics.FromImage( bitmap );

        //    var depositSlipTemplate = GetAttributeValue( options.FileFormat, "DepositSlipTemplate" );
        //    var mergeFields = new Dictionary<string, object>
        //    {
        //        { "FileFormat", options.FileFormat },
        //        { "Amount", creditDetail.Amount.ToString( "C" ) }
        //    };
        //    var depositSlipText = depositSlipTemplate.ResolveMergeFields( mergeFields, null );

        //    //
        //    // Ensure we are opague with white.
        //    //
        //    g.FillRectangle( System.Drawing.Brushes.White, new System.Drawing.Rectangle( 0, 0, 1200, 550 ) );

        //    if ( isFrontSide )
        //    {
        //        g.DrawString( depositSlipText,
        //            new System.Drawing.Font( "Tahoma", 30 ),
        //            System.Drawing.Brushes.Black,
        //            new System.Drawing.PointF( 50, 50 ) );
        //    }

        //    g.Flush();

        //    //
        //    // Ensure the DPI is correct.
        //    //
        //    bitmap.SetResolution( 200, 200 );

        //    //
        //    // Compress using TIFF, CCITT Group 4 format.
        //    //
        //    var codecInfo = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
        //        .Where( c => c.MimeType == "image/tiff" )
        //        .First();
        //    var parameters = new System.Drawing.Imaging.EncoderParameters( 1 );
        //    parameters.Param[0] = new System.Drawing.Imaging.EncoderParameter( System.Drawing.Imaging.Encoder.Compression, ( long ) System.Drawing.Imaging.EncoderValue.CompressionCCITT4 );

        //    var ms = new MemoryStream();
        //    bitmap.Save( ms, codecInfo, parameters );
        //    ms.Position = 0;

        //    return ms;
        //}

        /// <summary>
        /// Hashes the string with SHA256.
        /// </summary>
        /// <param name="contents">The contents to be hashed.</param>
        /// <returns>A hex representation of the hash.</returns>
        protected string HashString( string contents )
        {
            byte[] byteContents = Encoding.Unicode.GetBytes( contents );

            var hash = new System.Security.Cryptography.SHA256CryptoServiceProvider().ComputeHash( byteContents );

            return string.Join( "", hash.Select( b => b.ToString( "x2" ) ).ToArray() );
        }
    }
}
