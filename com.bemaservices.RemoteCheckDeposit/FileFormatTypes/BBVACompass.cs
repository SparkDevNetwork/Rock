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
    [Description( "Processes a batch export for BBVA Compass." )]
    [Export( typeof( FileFormatTypeComponent ) )]
    [ExportMetadata( "ComponentName", "BBVA Compass" )]
    [EncryptedTextField("Origin Routing Number", "Used on Type 10 Record 3 for Account Routing", true, key:"OriginRoutingNumber" )]
    [EncryptedTextField( "Destination Routing Number", "", true, key:"DestinationRoutingNumber" )]
    [CodeEditorField( "Deposit Slip Template", "The template for the deposit slip that will be generated. <span class='tip tip-lava'></span>", Rock.Web.UI.Controls.CodeEditorMode.Lava, defaultValue: @"Customer: {{ FileFormat | Attribute:'OriginName' }}
Account: {{ FileFormat | Attribute:'AccountNumber' }}
Amount: {{ Amount }}", order: 20 )]
    public class BBVACompass : X937DSTU
    {
        #region System Setting Keys

        /// <summary>
        /// The system setting for the next cash header identifier. These should never be
        /// repeated. Ever.
        /// </summary>
        protected const string SystemSettingNextCashHeaderId = "BBVACompass.NextCashHeaderId";

        /// <summary>
        /// The system setting that contains the last file modifier we used.
        /// </summary>
        protected const string SystemSettingLastFileModifier = "BBVACompass.LastFileModifier";

        /// <summary>
        /// The last item sequence number used for items.
        /// </summary>
        protected const string LastItemSequenceNumberKey = "BBVACompass.LastItemSequenceNumber";

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

            //BBVT Compass: change Destination Routing
            header.ImmediateOriginRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "OriginRoutingNumber" ) );
            header.ImmediateDestinationRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "DestinationRoutingNumber" ) );
            header.UserField = "CR61";
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

            // BBVTCOmpass Add C to Fed Work Type
            header.WorkType = "C";
            //Add Destination Routing Number
            header.DestinationRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "DestinationRoutingNumber" ) );

            return header;
        }

        //Type 90
        protected override CashLetterControl GetCashLetterControlRecord( ExportOptions options, List<Record> records )
        {
            var control = base.GetCashLetterControlRecord( options, records );

            control.ImageCount = control.ImageCount - ( records.Where( r => r.RecordType == 61 ).Count() * 2 ); //remove Deposit slip images from count

            return control;
        }

        protected override BundleHeader GetBundleHeader( ExportOptions options, int bundleIndex )
        {
            var header = base.GetBundleHeader( options, bundleIndex );

            header.DestinationRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "DestinationRoutingNumber" ) );
            header.ID = ( bundleIndex + 1 ).ToString( "0000000000" );
            header.CycleNumber = "01";
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
            var accountNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "AccountNumber" ) );
            var routingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "RoutingNumber" ) );

            var records = new List<Record>();

            var creditDetail = new CreditDetail
            {
                PayorRoutingNumber = "107005319",
                CreditAccountNumber = accountNumber + "/10",
                Amount = transactions.Sum( t => t.TotalAmount ),
                InstitutionItemSequenceNumber = GetNextItemSequenceNumber().ToString( "000000000000000" ),
                DocumentTypeIndicator = "G",
                DebitCreditIndicator = "2"
            };
            records.Add( creditDetail );

            for ( int i = 0; i < 2; i++ )
            {
                using ( var ms = GetDepositSlipImage(options, creditDetail, i == 0 ) )
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
                        CycleNumber = "01",
                        BundleBusinessDate = options.BusinessDateTime,
                        ClientInstitutionItemSequenceNumber = creditDetail.InstitutionItemSequenceNumber,
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

            //BBVACompass
            checkDetail.ElectronicReturnAcceptanceIndicator = "0";
            checkDetail.ArchiveTypeIndicator = "B";

            foreach ( var imageData in records.Where( r => r.RecordType == 52 ).Cast<dynamic>() )
            {
                imageData.ClientInstitutionItemSequenceNumber = sequenceNumber.ToString( "000000000000000" );
            }
            //BBVACompass 
            foreach ( CheckDetailAddendumA detailA in records.Where( r => r.RecordType == 26 ).Cast<dynamic>() )
            {
                detailA.TruncationIndicator = "Y";  //required for an original bofd
                detailA.BankOfFirstDepositItemSequenceNumber = sequenceNumber.ToString( "000000000000000" );
            }

            return records;
        }

        /// <summary>
        /// Gets the credit detail deposit record (type 61).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="transactions">The transactions associated with this deposit.</param>
        /// <param name="isFrontSide">True if the image to be retrieved is the front image.</param>
        /// <returns>A stream that contains the image data in TIFF 6.0 CCITT Group 4 format.</returns>
        protected virtual Stream GetDepositSlipImage( ExportOptions options, CreditDetail creditDetail, bool isFrontSide )
        {
            var bitmap = new System.Drawing.Bitmap( 1200, 550 );
            var g = System.Drawing.Graphics.FromImage( bitmap );

            var depositSlipTemplate = GetAttributeValue( options.FileFormat, "DepositSlipTemplate" );
            var mergeFields = new Dictionary<string, object>
            {
                { "FileFormat", options.FileFormat },
                { "Amount", creditDetail.Amount.ToString( "C" ) }
            };
            var depositSlipText = depositSlipTemplate.ResolveMergeFields( mergeFields );

            //
            // Ensure we are opague with white.
            //
            g.FillRectangle( System.Drawing.Brushes.White, new System.Drawing.Rectangle( 0, 0, 1200, 550 ) );

            if ( isFrontSide )
            {
                g.DrawString( depositSlipText,
                    new System.Drawing.Font( "Tahoma", 30 ),
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

        protected override BundleControl GetBundleControl( ExportOptions options, List<Record> records )
        {
            var control = base.GetBundleControl( options, records );

            //Remove type 61 from control totals
            control.ItemCount = records.Where( r => r.RecordType == 25 ).Count();
            control.ImageCount = control.ImageCount - ( records.Where( r => r.RecordType == 61 ).Count() * 2 ); //remove the two deposit slip images
            return control;

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
