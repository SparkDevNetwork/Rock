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
    /// Defines the basic functionality of any component that will be exporting using the X9.37
    /// DSTU standard.
    /// </summary>
    [Description( "Processes a batch export for BB&T (Branch Bank & Trust)." )]
    [Export( typeof( FileFormatTypeComponent ) )]
    [ExportMetadata( "ComponentName", "BB&T" )]
    [EncryptedTextField("Client ID (BB&T)", "Up to 8 digit Client ID provided by BB&T for Bundle Header (Record 20, Field 12) ", key:"ClientID")]
    [EncryptedTextField("Record 25 Routing Number", "Routing Number to Display on Deposit Ticket - Record 25, Field 4-5", key:"Record25Routing")]
    [CodeEditorField( "Deposit Slip Template", "The template for the deposit slip that will be generated. <span class='tip tip-lava'></span>", Rock.Web.UI.Controls.CodeEditorMode.Lava, defaultValue: @"Customer: {{ FileFormat | Attribute:'OriginName' }}
Account: {{ FileFormat | Attribute:'AccountNumber' }}
Amount: {{ Amount }}", order: 20 )]
    public class BBandT : X937DSTU
    {
        #region System Setting Keys

        /// <summary>
        /// The system setting for the next cash header identifier. These should never be
        /// repeated. Ever.
        /// </summary>
        protected const string SystemSettingNextCashHeaderId = "BBandT.NextCashHeaderId";

        /// <summary>
        /// The system setting that contains the last file modifier we used.
        /// </summary>
        protected const string SystemSettingLastFileModifier = "BBandT.LastFileModifier";

        /// <summary>
        /// The last item sequence number used for items.
        /// </summary>
        protected const string LastItemSequenceNumberKey = "BBandT.LastItemSequenceNumber";

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

            //BB&T: Modify record field 4 to Institution Routing Number, field 5 to origin routing number, and field 12 to blank spaces
            var institutionRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "InstitutionRoutingNumber" ) );
            var originRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "RoutingNumber" ) );
            header.ImmediateDestinationRoutingNumber = institutionRoutingNumber;
            header.ImmediateOriginRoutingNumber = originRoutingNumber;
            header.CountryCode = "  ";

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

            //BB&T change field 3 to institution routing number
            var institutionRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "InstitutionRoutingNumber" ) );
            header.DestinationRoutingNumber = institutionRoutingNumber;

            return header;
        }

        /// <summary>
        /// Gets the bundle header record (Type 20)
        /// </summary>
        /// <param name="options"></param>
        /// <param name="bundleIndex"></param>
        /// <returns></returns>
        protected override BundleHeader GetBundleHeader( ExportOptions options, int bundleIndex )
        {
            //BB&T change field 3 to institution routing number
            var institutionRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "InstitutionRoutingNumber" ) );

            var bundleHeader = base.GetBundleHeader( options, bundleIndex );

            bundleHeader.DestinationRoutingNumber = institutionRoutingNumber;

            //BB&T change field 7 to a Bundle ID
            bundleHeader.ID = ( bundleIndex + 1 ).ToString( "0000000000" );

            //BB&T change field 10 to blanks
            bundleHeader.ReturnLocationRoutingNumber = "         "; //9 spaces

            //BB&T change field 12 Reserved to Work Type ID & Cliend ID (see docs for details)
            var clientID = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "ClientID" ) ).AsInteger();
            bundleHeader.Reserved = "01" + clientID.ToString( "00000000" ) + "  ";

            return bundleHeader;
        }

        /// <summary>
        /// Gets the credit detail deposit record (Type 25).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="bundleIndex">Number of existing bundle records in the cash letter.</param>
        /// <param name="transactions">The transactions associated with this deposit.</param>
        /// <returns>
        /// A collection of records.
        /// </returns>
        protected override List<Record> GetCreditDetailRecords( ExportOptions options, int bundleIndex, List<FinancialTransaction> transactions )
        {
            var accountNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "AccountNumber" ) );
            var routingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "RoutingNumber" ) );
            var record25routingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "Record25Routing" ) );

            var records = new List<Record>();

            //DO NOT USE, because of record type 61 not used in BB&T
            //var creditDetail = new CreditDetail
            //{
            //    PayorRoutingNumber = "500100015",
            //    CreditAccountNumber = accountNumber + "/",
            //    Amount = transactions.Sum( t => t.TotalAmount ),
            //    InstitutionItemSequenceNumber = GetNextItemSequenceNumber().ToString( "000000000000000" ),
            //    DebitCreditIndicator = "2"
            //};
            //records.Add( creditDetail );

            var creditDetail = new CheckDetail //Type 25
            {
                PayorBankRoutingNumber = record25routingNumber.Substring(0,8),
                PayorBankRoutingNumberCheckDigit = record25routingNumber.Substring(8,1),
                OnUs = ( accountNumber + "/" + "13" ).PadLeft(18, '0').PadLeft(20, ' '),
                ItemAmount = transactions.Sum( t => t.TotalAmount ),
                ClientInstitutionItemSequenceNumber = GetNextItemSequenceNumber().ToString( "000000000000000" ),
                BankOfFirstDepositIndicator = "U",
                CheckDetailRecordAddendumCount = 00,
                DocumentationTypeIndicator = "G"
            };

            var detailA = new CheckDetailAddendumA
            {
                RecordNumber = 1,
                BankOfFirstDepositRoutingNumber = routingNumber,
                BankOfFirstDepositBusinessDate = options.BusinessDateTime,
                TruncationIndicator = "Y",
                BankOfFirstDepositConversionIndicator = "2",
                BankOfFirstDepositCorrectionIndicator = ""                
            };

            records.Add( creditDetail );
            records.Add( detailA );

            for ( int i = 0; i < 2; i++ )
            {
                using ( var ms = GetDepositSlipImage(options, creditDetail, i == 0, transactions ) )
                {
                    //
                    // Get the Image View Detail record (type 50).
                    //
                    var detail = new ImageViewDetail
                    {
                        ImageIndicator = 1,
                        ImageCreatorRoutingNumber = routingNumber,
                        ImageCreatorDate = options.ExportDateTime,
                        ImageViewFormatIndicator = 0,
                        CompressionAlgorithmIdentifier = 0,
                        SideIndicator = i,
                        ViewDescriptor = 0,
                        DigitalSignatureIndicator = 0
                    };

                    //
                    // Get the Image View Data record (type 52).
                    //
                    var data = new ImageViewData
                    {
                        InstitutionRoutingNumber = routingNumber,
                        BundleBusinessDate = options.BusinessDateTime,
                        ClientInstitutionItemSequenceNumber = creditDetail.ClientInstitutionItemSequenceNumber,
                        ClippingOrigin = 0,
                        ImageData = ms.ReadBytesToEnd()
                    };

                    records.Add( detail );
                    records.Add( data );
                }
            }

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
            var checkDetail = records.Where( r => r.RecordType == 25 ).Cast<dynamic>().FirstOrDefault();
            checkDetail.ClientInstitutionItemSequenceNumber = sequenceNumber.ToString( "000000000000000" );

            foreach ( var imageData in records.Where( r => r.RecordType == 52 ).Cast<dynamic>() )
            {
                imageData.ClientInstitutionItemSequenceNumber = sequenceNumber.ToString( "000000000000000" );
            }

            //BB&T for each record where 25
            foreach ( CheckDetail detail in records.Where( r => r.RecordType == 25 ).Cast<dynamic>() )
            {
                detail.DocumentationTypeIndicator = "G";
                detail.BankOfFirstDepositIndicator = "U";
                detail.CheckDetailRecordAddendumCount = 0;
            }

            //BB&T for each record where 26
            foreach ( CheckDetailAddendumA detailA in records.Where( r => r.RecordType == 26 ).Cast<dynamic>() )
            {
                detailA.TruncationIndicator = "Y";  //required for an original bofd
            }

            return records;
        }

        /// <summary>
        /// Get the cash letter control records (Type 90)
        /// </summary>
        /// <param name="options"></param>
        /// <param name="records"></param>
        /// <returns></returns>
        protected override CashLetterControl GetCashLetterControlRecord( ExportOptions options, List<Record> records )
        {
            var clControl = base.GetCashLetterControlRecord( options, records );

            //BB&T blanks for 6
            clControl.ECEInstitutionName = "".PadRight(18, ' ');

            return clControl;
        }

        protected override FileControl GetFileControlRecord( ExportOptions options, List<Record> records )
        {
            var fileControl = base.GetFileControlRecord( options, records );

            fileControl.ImmediateOriginContactName = "".PadRight( 14, ' ' );
            fileControl.ImmediateOriginContactPhoneNumber = "0".PadRight( 10, ' ' );

            return fileControl;
        }

        /// <summary>
        /// Gets the credit detail deposit record (type 61).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="transactions">The transactions associated with this deposit.</param>
        /// <param name="isFrontSide">True if the image to be retrieved is the front image.</param>
        /// <returns>A stream that contains the image data in TIFF 6.0 CCITT Group 4 format.</returns>
        protected virtual Stream GetDepositSlipImage( ExportOptions options, CheckDetail creditDetail, bool isFrontSide, List<FinancialTransaction> transactions )
        {
            var bitmap = new System.Drawing.Bitmap( 1200, 550 );
            var g = System.Drawing.Graphics.FromImage( bitmap );

            var depositSlipTemplate = GetAttributeValue( options.FileFormat, "DepositSlipTemplate" );
            var mergeFields = new Dictionary<string, object>
            {
                { "FileFormat", options.FileFormat },
                { "Amount", creditDetail.ItemAmount.ToString( "C" ) },
                { "ItemCount", transactions.Count() }
            };
            var depositSlipText = depositSlipTemplate.ResolveMergeFields( mergeFields, null );

            //
            // Ensure we are opague with white.
            //
            g.FillRectangle( System.Drawing.Brushes.White, new System.Drawing.Rectangle( 0, 0, 1200, 550 ) );

            if ( isFrontSide )
            {
                g.DrawString( depositSlipText,
                    new System.Drawing.Font( "Tahoma", 20 ),
                    System.Drawing.Brushes.Black,
                    new System.Drawing.PointF( 50, 50 ) );
            }

            g.Flush();

            //
            // Ensure the DPI is correct.
            //
            bitmap.SetResolution( 200, 200 );

            //
            // Compress using TIFF, CCITT Group 4 format.
            //
            var codecInfo = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                .Where( c => c.MimeType == "image/tiff" )
                .First();
            var parameters = new System.Drawing.Imaging.EncoderParameters( 1 );
            parameters.Param[0] = new System.Drawing.Imaging.EncoderParameter( System.Drawing.Imaging.Encoder.Compression, ( long ) System.Drawing.Imaging.EncoderValue.CompressionCCITT4 );

            var ms = new MemoryStream();
            bitmap.Save( ms, codecInfo, parameters );
            ms.Position = 0;

            return ms;
        }

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
