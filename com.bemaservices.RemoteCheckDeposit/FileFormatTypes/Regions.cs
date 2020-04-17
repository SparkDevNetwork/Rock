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
    [Description( "Processes a batch export for Regions Bank." )]
    [Export( typeof( FileFormatTypeComponent ) )]
    [ExportMetadata( "ComponentName", "Regions" )]
    [CodeEditorField( "Deposit Slip Template", "The template for the deposit slip that will be generated. <span class='tip tip-lava'></span>", Rock.Web.UI.Controls.CodeEditorMode.Lava,
        defaultValue: @"Customer: {{ FileFormat | Attribute:'OriginName' }}
CICL-{{ FileFormat | Attribute:'AccountNumber' }}
Account: {{ FileFormat | Attribute:'AccountNumber' }}
Amount: {{ Amount }}
ItemCount: {{ ItemCount }}", order: 20 )]
    public class Regions : X937DSTU
    {
        #region System Setting Keys

        /// <summary>
        /// The system setting for the next cash header identifier. These should never be
        /// repeated. Ever.
        /// </summary>
        protected const string SystemSettingNextCashHeaderId = "Regions.NextCashHeaderId";

        /// <summary>
        /// The system setting that contains the last file modifier we used.
        /// </summary>
        protected const string SystemSettingLastFileModifier = "Regions.LastFileModifier";

        /// <summary>
        /// The last item sequence number used for items.
        /// </summary>
        protected const string LastItemSequenceNumberKey = "Regions.LastItemSequenceNumber";

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

            var originRoutingNumber = Rock.Security.Encryption.DecryptString(GetAttributeValue(options.FileFormat, "InstitutionRoutingNumber"));

            // Override account number with institution routing number for Field 5
            header.ImmediateOriginRoutingNumber = originRoutingNumber;

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
            var originRoutingNumber = Rock.Security.Encryption.DecryptString(GetAttributeValue(options.FileFormat, "InstitutionRoutingNumber"));

            var header = base.GetCashLetterHeaderRecord( options );
            header.ID = cashHeaderId.ToString( "D8" );
            SetSystemSetting( SystemSettingNextCashHeaderId, ( cashHeaderId + 1 ).ToString() );

            // Override routing number with institution routing number for Field 4
            header.ClientInstitutionRoutingNumber = originRoutingNumber;

            return header;
        }

        /// <summary>
        /// Gets the bundle header record (type 20).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="bundleIndex">Number of existing bundle records in the cash letter.</param>
        /// <returns>
        /// A BundleHeader record.
        /// </returns>
        protected override BundleHeader GetBundleHeader(ExportOptions options, int bundleIndex)
        {
            var header = base.GetBundleHeader(options, bundleIndex);
            var originRoutingNumber = Rock.Security.Encryption.DecryptString(GetAttributeValue(options.FileFormat, "InstitutionRoutingNumber"));

            // Override routing number with institution routing number for Field 4
            header.ClientInstitutionRoutingNumber = originRoutingNumber;

            // Override Return Location Routing Number
            header.ReturnLocationRoutingNumber = originRoutingNumber;

            // Set Bundle ID  (should be same as Bundle Sequence Number )
            header.ID = (bundleIndex + 1).ToString();

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

            CreditReconciliation creditReconciliation = new CreditReconciliation
            {
                RecordUsageIndicator = 5,
                AuxiliaryOnUs = string.Empty,
                ExternalProcessingCode = string.Empty,
                PostingAccountRoutingNumber = routingNumber.AsInteger(),
                PostingAccountBankOnUs = accountNumber + "/20",
                ItemAmount = transactions.Sum(p => p.TotalAmount),
                ECEInstitutionSequenceNumber = GetNextItemSequenceNumber().ToString("000000000000000"), // A number assigned by you that uniquely identifies the item in the cash letter
                DocumentationTypeIndicator = "G", // Field value must be "G" - Meaning there are 2 images present.
                //TypeOfAccountCode = string.Empty,
                //SourceOfWork = string.Empty, // Field value must be "2" space or "02" meaning internal - branch
            };
            records.Add( creditReconciliation );

            for ( int i = 0; i < 2; i++ )
            {
                using ( var ms = GetDepositSlipImage(options, creditReconciliation, i == 0, transactions ) )
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
                        DigitalSignatureIndicator = 0,
                        DataSize = (int)ms.Length
                    };

                    //
                    // Get the Image View Data record (type 52).
                    //
                    var data = new ImageViewData
                    {
                        InstitutionRoutingNumber = routingNumber,
                        BundleBusinessDate = options.BusinessDateTime,
                        ClientInstitutionItemSequenceNumber = creditReconciliation.ECEInstitutionSequenceNumber,
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
            var checkDetail = records.Where( r => r.RecordType == 25 ).Cast<CheckDetail>().FirstOrDefault();
            checkDetail.ClientInstitutionItemSequenceNumber = sequenceNumber.ToString( "000000000000000" );
            checkDetail.MICRValidIndicator = 1;
            checkDetail.CheckDetailRecordAddendumCount = 0; // removing item 26

            // Remove all check detail addendum a records (type 26)
            records = records.Where(r => r.RecordType != 26).ToList();

            foreach ( var imageData in records.Where( r => r.RecordType == 52 ).Cast<ImageViewData>() )
            {
                imageData.ClientInstitutionItemSequenceNumber = sequenceNumber.ToString( "000000000000000" );
            }

            return records;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="records"></param>
        /// <returns></returns>
        protected override CashLetterControl GetCashLetterControlRecord( ExportOptions options, List<Record> records )
        {
            var controlRecord = base.GetCashLetterControlRecord( options, records );
            //count deposit records as well as items
            controlRecord.ItemCount = records.Where( r => r.RecordType == 25 || r.RecordType == 61 ).Count();

            return controlRecord;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="records"></param>
        /// <returns></returns>
        protected override FileControl GetFileControlRecord( ExportOptions options, List<Record> records )
        {
            var controlRecord = base.GetFileControlRecord( options, records );
            //count deposit records as well as items
            controlRecord.TotalItemCount = records.Where( r => r.RecordType == 25 || r.RecordType == 61 ).Count();

            return controlRecord;
        }

        /// <summary>
        /// Gets the credit detail deposit record (type 61).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="transactions">The transactions associated with this deposit.</param>
        /// <param name="isFrontSide">True if the image to be retrieved is the front image.</param>
        /// <returns>A stream that contains the image data in TIFF 6.0 CCITT Group 4 format.</returns>
        protected virtual Stream GetDepositSlipImage( ExportOptions options, CreditReconciliation creditReconciliation, bool isFrontSide, List<FinancialTransaction> transactions )
        {
            var bitmap = new System.Drawing.Bitmap( 1200, 550 );
            var g = System.Drawing.Graphics.FromImage( bitmap );

            var depositSlipTemplate = GetAttributeValue( options.FileFormat, "DepositSlipTemplate" );
            var mergeFields = new Dictionary<string, object>
            {
                { "FileFormat", options.FileFormat },
                { "Amount", creditReconciliation.ItemAmount.ToString( "C" ) },
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
