using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;

using com.bemaservices.RemoteCheckDeposit.Model;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace com.bemaservices.RemoteCheckDeposit.FileFormatTypes
{
    /// <summary>
    /// Defines the basic functionality of any component that will be exporting using the X9.37
    /// DSTU standard.
    /// </summary>
    [EncryptedTextField( "Routing Number", "Your bank account routing number.", false, "", order: 0 )]
    [EncryptedTextField( "Account Number", "Your bank account number.", false, "", order: 1 )]
    [EncryptedTextField( "Institution Routing Number", "This is defined by your bank, it is usually either the bank routing number or a customer number.", false, "", order: 2 )]
    [TextField( "Destination Name", "The name of the bank the deposit will be made to.", false, "", order: 3 )]
    [TextField( "Origin Name", "The name of the church.", false, "", order: 4 )]
    [TextField( "Contact Name", "The name of the person the bank will contact if there are issues.", false, "", order: 5 )]
    [TextField( "Contact Phone", "The phone number the bank will call if there are issues.", false, "", order: 6 )]
    [BooleanField( "Test Mode", "If true then the generated files will be marked as test-mode.", true, order: 7 )]
    [DefinedValueField( "1D1304DE-E83A-44AF-B11D-0C66DD600B81","Currency Types To Export", "Select which check types are valid to send in batches to export (Defaults to Checks)", allowMultiple: true, required: true, defaultValue: "8B086A19-405A-451F-8D44-174E92D6B402", order: 8, key: "CurrencyTypes" )]
    [BooleanField( "Enable Digital Endorsement", "Prints text on the back of the check digitally, as an endoresment of the check", defaultValue: false, order: 9)]
    [CodeEditorField( "Check Endorsement Template", "The template for the back of check endorsement. <span class='tip tip-lava'></span>", Rock.Web.UI.Controls.CodeEditorMode.Lava, defaultValue: @"{{ FileFormat | Attribute:'OriginName' }}
Account: {{ FileFormat | Attribute:'AccountNumber' }}
Date: {{ BusinessDate | Date:'M/d/yyyy' }}", order: 10, required: false )]
    public abstract class X937DSTU : FileFormatTypeComponent
    {
        /// <summary>
        /// Gets the maximum items per bundle. Most banks limit the number of checks that
        /// can exist in each bundle. This specifies what that maximum is.
        /// </summary>
        public virtual int MaxItemsPerBundle
        {
            get
            {
                return 200;
            }
        }

        /// <summary>
        /// Exports a collection of batches to a binary file that can be downloaded by the user
        /// and sent to their financial institution. The returned BinaryFile should not have been
        /// saved to the database yet.
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="errorMessages">On return will contain a list of error messages if not empty.</param>
        /// <returns>
        /// A <see cref="Stream" /> of data that should be downloaded to the user in a file.
        /// </returns>
        public override Stream ExportBatches( ExportOptions options, out List<string> errorMessages )
        {
            var records = new List<Record>();

            errorMessages = new List<string>();

            //
            // Get all the transactions that will be exported from these batches.
            //
            var transactions = options.Batches.SelectMany( b => b.Transactions )
                .OrderBy( t => t.ProcessedDateTime )
                .ThenBy( t => t.Id )
                .ToList();

            //
            // Perform error checking to ensure that all the transactions in these batches
            // are of the proper currency type.
            //
            List<Guid> currencyGuids = GetAttributeValue( options.FileFormat, "CurrencyTypes" ).SplitDelimitedValues().AsGuidList();
            if (!currencyGuids.Any())
            {
                //Add the default check option if nothing is selected
                currencyGuids.Add( Guid.Parse( "8B086A19-405A-451F-8D44-174E92D6B402" ) );
            }
            List<int> currencyIds = new List<int>();
            foreach ( Guid guid in currencyGuids )
            {
                currencyIds.Add( Rock.Web.Cache.DefinedValueCache.Get( guid ).Id );
            }
            
            if ( transactions.Any( t => !currencyIds.Contains( t.FinancialPaymentDetail.CurrencyTypeValueId ?? -1) ) )
            {
                errorMessages.Add( "One or more transactions is not of a selected Check type." );
                throw new Exception( "One or more transactions is not of a selected Check type." );
            }

            //
            // Generate all the X9.37 records for this set of transactions.
            //
            records.Add( GetFileHeaderRecord( options) );
            records.Add( GetCashLetterHeaderRecord( options) );
            records.AddRange( GetBundleRecords( options, transactions ) );
            records.Add( GetCashLetterControlRecord( options, records ) );
            records.Add( GetFileControlRecord( options, records ) );

            //
            // Encode all the records into a memory stream so that it can be saved to a file
            // by the caller.
            //
            var stream = new MemoryStream();
            using ( var writer = new BinaryWriter( stream, System.Text.Encoding.UTF8, true ) )
            {
                foreach ( var record in records )
                {
                    record.Encode( writer );
                }
            }

            stream.Position = 0;

            return stream;
        }

        #region File Records

        /// <summary>
        /// Gets the file header record (type 01).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <returns>A FileHeader record.</returns>
        protected virtual Records.FileHeader GetFileHeaderRecord( ExportOptions options )
        {
            var destinationRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "RoutingNumber" ) );
            var originRoutingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "AccountNumber" ) );

            var header = new Records.FileHeader
            {
                StandardLevel = 3,
                FileTypeIndicator = GetAttributeValue( options.FileFormat, "TestMode" ).AsBoolean( true ) ? "T" : "P",
                ImmediateDestinationRoutingNumber = destinationRoutingNumber,
                ImmediateOriginRoutingNumber = originRoutingNumber,
                FileCreationDateTime = options.ExportDateTime,
                ResendIndicator = "N",
                ImmediateDestinationName = GetAttributeValue( options.FileFormat, "DestinationName" ),
                ImmediateOriginName = GetAttributeValue( options.FileFormat, "OriginName" ),
                FileIdModifier = "1", // TODO: Need some way to track this and reset each day.
                CountryCode = "US", /* Should be safe, X9.37 is only used in the US as far as I know. */
                UserField = string.Empty
            };

            return header;
        }

        /// <summary>
        /// Gets the file control record (type 99).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="records">The existing records in the file.</param>
        /// <returns>A FileControl record.</returns>
        protected virtual Records.FileControl GetFileControlRecord( ExportOptions options, List<Record> records )
        {
            var cashHeaderRecords = records.Where( r => r.RecordType == 10 );
            var detailRecords = records.Where( r => r.RecordType == 25 ).Cast<dynamic>();
            var itemRecords = records.Where( r => r.RecordType == 25 ); // ONLY COUNT CHECKS

            var control = new Records.FileControl
            {
                CashLetterCount = cashHeaderRecords.Count(),
                TotalRecordCount = records.Count + 1, /* Plus one to include self */
                TotalItemCount = itemRecords.Count(),
                TotalAmount = detailRecords.Sum(c => (decimal)c.ItemAmount),
                ImmediateOriginContactName = GetAttributeValue(options.FileFormat, "ContactName"),
                ImmediateOriginContactPhoneNumber = GetAttributeValue(options.FileFormat, "ContactPhone").Replace(" ", string.Empty)
            };

            return control;
        }

        #endregion

        #region Cash Letter Records

        /// <summary>
        /// Gets the cash letter header record (type 10).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <returns>A CashLetterHeader record.</returns>
        /// 
        protected virtual Records.CashLetterHeader GetCashLetterHeaderRecord( ExportOptions options )
        {
            var routingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "RoutingNumber" ) );
            var contactName = GetAttributeValue( options.FileFormat, "ContactName" );
            var contactPhone = GetAttributeValue( options.FileFormat, "ContactPhone" );

            var header = new Records.CashLetterHeader
            {
                CollectionTypeIndicator = 1,
                DestinationRoutingNumber = routingNumber,
                ClientInstitutionRoutingNumber = routingNumber,
                BusinessDate = options.BusinessDateTime,
                CreationDateTime = options.ExportDateTime,
                RecordTypeIndicator = "I",
                DocumentationTypeIndicator = "G",
                OriginatorContactName = contactName,
                OriginatorContactPhoneNumber = contactPhone
            };

            return header;
        }
        

        /// <summary>
        /// Gets the cash letter control record (type 90).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="records">Existing records in the cash letter.</param>
        /// <returns>A CashLetterControl record.</returns>
        protected virtual Records.CashLetterControl GetCashLetterControlRecord( ExportOptions options, List<Record> records )
        {
            var bundleHeaderRecords = records.Where( r => r.RecordType == 20 );
            var checkDetailRecords = records.Where( r => r.RecordType == 25 ).Cast<dynamic>();
            var itemRecords = records.Where( r => r.RecordType == 25 ); // Only count checks!
            var imageDetailRecords = records.Where( r => r.RecordType == 52 );
            var organizationName = GetAttributeValue( options.FileFormat, "OriginName" );

            var control = new Records.CashLetterControl
            {
                BundleCount = bundleHeaderRecords.Count(),
                ItemCount = itemRecords.Count(),
                TotalAmount = checkDetailRecords.Sum( c => ( decimal ) c.ItemAmount ),
                ImageCount = imageDetailRecords.Count(),
                ECEInstitutionName = organizationName
            };

            return control;
        }

        #endregion

        #region Bundle Records

        /// <summary>
        /// Gets all the bundle records in required for the transactions specified.
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="transactions">The transactions to be exported.</param>
        /// <returns>A collection of records that identify all the exported transactions.</returns>
        protected virtual List<Record> GetBundleRecords( ExportOptions options, List<FinancialTransaction> transactions )
        {
            var records = new List<Record>();

            for ( int bundleIndex = 0; ( bundleIndex * MaxItemsPerBundle ) < transactions.Count(); bundleIndex++ )
            {
                var bundleRecords = new List<Record>();
                var bundleTransactions = transactions.Skip( bundleIndex * MaxItemsPerBundle )
                    .Take( MaxItemsPerBundle )
                    .ToList();

                //
                // Add the bundle header for this set of transactions.
                //
                bundleRecords.Add( GetBundleHeader( options, bundleIndex ) );

                //
                // Allow subclasses to provide credit detail records (type 61) if they want.
                //
                bundleRecords.AddRange( GetCreditDetailRecords( options, bundleIndex, bundleTransactions ) );

                //
                // Add records for each transaction in the bundle.
                //
                foreach ( var transaction in bundleTransactions )
                {
                    try
                    {
                        bundleRecords.AddRange( GetItemRecords( options, transaction ) );
                    }
                    catch ( Exception ex )
                    {
                        //Could be bad MICR or Amount; erase so we can re-run.
                        using ( Rock.Data.RockContext rockContext = new Rock.Data.RockContext() )
                        {
                            var badTransation = new FinancialTransactionService( rockContext ).Get( transaction.Id );
                            badTransation.MICRStatus = null;
                            badTransation.CheckMicrEncrypted = string.Empty;
                            badTransation.CheckMicrHash = string.Empty;
                            badTransation.CheckMicrParts = string.Empty;
                            rockContext.SaveChanges();
                        }
                        throw new Exception( string.Format( "Error processing transaction {0}. ({1})", transaction.Id, ex.Message ), ex );
                    }
                }

                //
                // Add the bundle control record.
                //
                bundleRecords.Add( GetBundleControl( options, bundleRecords ) );

                records.AddRange( bundleRecords );
            }

            return records;
        }

        /// <summary>
        /// Gets the bundle header record (type 20).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="bundleIndex">Number of existing bundle records in the cash letter.</param>
        /// <returns>A BundleHeader record.</returns>
        protected virtual Records.BundleHeader GetBundleHeader( ExportOptions options, int bundleIndex )
        {
            var routingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "RoutingNumber" ) );

            var header = new Records.BundleHeader
            {
                CollectionTypeIndicator = 1,
                DestinationRoutingNumber = routingNumber,
                ClientInstitutionRoutingNumber = routingNumber,
                BusinessDate = options.BusinessDateTime,
                CreationDate = options.ExportDateTime,
                ID = string.Empty,
                SequenceNumber = ( bundleIndex + 1 ).ToString(),
                CycleNumber = string.Empty,
                ReturnLocationRoutingNumber = routingNumber
            };

            return header;
        }

        /// <summary>
        /// Gets the credit detail deposit record (type 61).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="bundleIndex">Number of existing bundle records in the cash letter.</param>
        /// <param name="transactions">The transactions associated with this deposit.</param>
        /// <returns>A collection of records.</returns>
        protected virtual List<Record> GetCreditDetailRecords( ExportOptions options, int bundleIndex, List<FinancialTransaction> transactions )
        {
            return new List<Record>();
        }

        /// <summary>
        /// Gets the bundle control record (type 70).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="records">The existing records in the bundle.</param>
        /// <returns>A BundleControl record.</returns>
        protected virtual Records.BundleControl GetBundleControl( ExportOptions options, List<Record> records )
        {
            var itemRecords = records.Where( r => r.RecordType == 25 || r.RecordType == 61 );
            var checkDetailRecords = records.Where( r => r.RecordType == 25 ).Cast<dynamic>();
            var imageDetailRecords = records.Where( r => r.RecordType == 52 );

            var control = new Records.BundleControl
            {
                ItemCount = itemRecords.Count(),
                TotalAmount = checkDetailRecords.Sum( r => ( decimal ) r.ItemAmount ),
                MICRValidTotalAmount = checkDetailRecords.Sum( r => ( decimal ) r.ItemAmount ),
                ImageCount = imageDetailRecords.Count()
            };

            return control;
        }

        #endregion

        #region Item Records

        /// <summary>
        /// Gets the records that identify a single check being deposited.
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="transaction">The transaction to be deposited.</param>
        /// <returns>A collection of records.</returns>
        protected virtual List<Record> GetItemRecords( ExportOptions options, FinancialTransaction transaction )
        {
            var records = new List<Record>();

            records.AddRange( GetItemDetailRecords( options, transaction ) );

            try
            {
                records.AddRange( GetImageRecords( options, transaction, transaction.Images.Take( 1 ).First(), true ) );
                records.AddRange( GetImageRecords( options, transaction, transaction.Images.Skip( 1 ).Take( 1 ).First(), false ) );
            }
            catch ( Exception ex )
            {
                throw new ArgumentException( string.Format( "Transaction Does Not Contain Two Valid Image Records (Front and Back Check Scans)", transaction.Id ), ex );
            }

            return records;
        }

        /// <summary>
        /// Gets the item detail records (type 25, 26, etc.)
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="transaction">The transaction being deposited.</param>
        /// <returns>A collection of records.</returns>
        protected virtual List<Record> GetItemDetailRecords( ExportOptions options, FinancialTransaction transaction )
        {
            var accountNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "AccountNumber" ) );
            var routingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "RoutingNumber" ) );

            //
            // Parse the MICR data from the transaction.
            //
            var micr = GetMicrInstance( transaction.CheckMicrEncrypted );

            var transactionRoutingNumber = micr.GetRoutingNumber();

            //
            // On-Us Calculation (check account numbers can be too big)
            //
            string onUs = string.Format( "{0}/{1}", micr.GetAccountNumber().TrimStart('0'), micr.GetCheckNumber().TrimStart('0') );
            if ( onUs.Length > 20 ) //too big
            {
                int checkNumberLength = onUs.Length - micr.GetAccountNumber().Length - 1;  //number of characters left for the check number
                if ( checkNumberLength < 3 )
                {
                    checkNumberLength = 3; //Minimum length of 3
                }
                onUs = string.Format( "{0}/{1}", micr.GetAccountNumber().TrimStart('0'), micr.GetCheckNumber().TrimStart('0').Right( checkNumberLength ) );
            }

            //
            // Get the Check Detail record (type 25).
            //
            var detail = new Records.CheckDetail
            {
                PayorBankRoutingNumber = transactionRoutingNumber.Substring( 0, 8 ),
                PayorBankRoutingNumberCheckDigit = transactionRoutingNumber.Substring( 8, 1 ),
                OnUs = onUs.Right( 20 ), //get just right side of field as the front of an account number may be chopped off
                ExternalProcessingCode = micr.GetExternalProcessingCode(),
                AuxiliaryOnUs = micr.GetAuxOnUs(),
                ItemAmount = transaction.TotalAmount,
                ClientInstitutionItemSequenceNumber = accountNumber,
                DocumentationTypeIndicator = "G",
                BankOfFirstDepositIndicator = "Y",
                CheckDetailRecordAddendumCount = 1
            };

            //
            // Get the Addendum A record (type 26).
            //
            var detailA = new Records.CheckDetailAddendumA
            {
                RecordNumber = 1,
                BankOfFirstDepositRoutingNumber = routingNumber,
                BankOfFirstDepositBusinessDate = options.BusinessDateTime,
                TruncationIndicator = "N",
                BankOfFirstDepositConversionIndicator = "2",
                BankOfFirstDepositCorrectionIndicator = "0"
            };

            return new List<Record> { detail, detailA };
        }

        /// <summary>
        /// Gets the image record for a specific transaction image (type 50 and 52).
        /// </summary>
        /// <param name="options">Export options to be used by the component.</param>
        /// <param name="transaction">The transaction being deposited.</param>
        /// <param name="image">The check image scanned by the scanning application.</param>
        /// <param name="isFront">if set to <c>true</c> [is front].</param>
        /// <returns>A collection of records.</returns>
        protected virtual List<Record> GetImageRecords( ExportOptions options, FinancialTransaction transaction, FinancialTransactionImage image, bool isFront )
        {
            var routingNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "RoutingNumber" ) );
            var accountNumber = Rock.Security.Encryption.DecryptString( GetAttributeValue( options.FileFormat, "AccountNumber" ) );
            var checkEndorsement = GetAttributeValue( options.FileFormat, "CheckEndorsementTemplate" );
            var enableEndorsement = GetAttributeValue( options.FileFormat, "EnableDigitalEndorsement" ).AsBoolean();


            //
            // If endorsement, add to back of image
            //
            Stream imageData = image.BinaryFile.ContentStream;

            if ( !isFront && enableEndorsement && checkEndorsement.IsNotNullOrWhiteSpace() ) //if image is the back of the check, add endorsement
            {
                //add endorsement for back image
                var bitmap = new Bitmap( image.BinaryFile.ContentStream );
                bitmap.SetResolution( 200, 200 );
                var newBitmap = new Bitmap( bitmap.Width, bitmap.Height );
                newBitmap.SetResolution( 200, 200 );
                var g = System.Drawing.Graphics.FromImage( newBitmap );
                g.DrawImage( bitmap, 0, 0 ); //draw image onto blank bitmap
                
                var mergeFields = new Dictionary<string, object>
                {
                    { "FileFormat", options.FileFormat },
                    { "Amount", transaction.TotalAmount.ToString( "C" ) },
                    { "BusinessDate", options.BusinessDateTime }
                };
                var checkEndorsementText = checkEndorsement.ResolveMergeFields( mergeFields, null );
                
                g.DrawString( checkEndorsementText,
                    new System.Drawing.Font( "Tahoma", 10 ),
                    System.Drawing.Brushes.Black,
                    new System.Drawing.PointF( ( bitmap.Width / 2 ) - 40, ( bitmap.Height / 2 ) - 40 ) );

                g.Flush();

                //
                // Ensure the DPI is correct.
                //
                
                imageData = new System.IO.MemoryStream();
                newBitmap.Save( imageData, System.Drawing.Imaging.ImageFormat.Tiff );//maybe this works...

            }


            //
            // Get the Image View Detail record (type 50).
            //
            var detail = new Records.ImageViewDetail
            {
                ImageIndicator = 1,
                ImageCreatorRoutingNumber = routingNumber,
                ImageCreatorDate = image.CreatedDateTime ?? options.ExportDateTime,
                ImageViewFormatIndicator = 0,
                CompressionAlgorithmIdentifier = 0,
                SideIndicator = isFront ? 0 : 1,
                ViewDescriptor = 0,
                DigitalSignatureIndicator = 0,
                DataSize = (int)imageData.Length
            };

            //
            // Get the Image View Data record (type 52).
            //
            var data = new Records.ImageViewData
            {
                InstitutionRoutingNumber = routingNumber,
                BundleBusinessDate = options.BusinessDateTime,
                ClientInstitutionItemSequenceNumber = accountNumber,
                ClippingOrigin = 0,
                ImageData = ConvertImageToTiffG4( imageData ).ReadBytesToEnd()
            };

            return new List<Record> { detail, data };
        }
        

        #endregion

        #region Protected Methods

        /// <summary>
        /// Converts the image to tiff g4 specifications.
        /// </summary>
        /// <param name="imageStream">The image stream.</param>
        /// <returns></returns>
        protected Stream ConvertImageToTiffG4( Stream imageStream )
        {
            var bitmap = new Bitmap( imageStream );

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
        /// Gets a MICR object instance from the encrypted MICR data.
        /// </summary>
        /// <param name="encryptedMicrContent">Content of the encrypted MICR.</param>
        /// <returns>A <see cref="Micr"/> instance that can be used to get decrypted MICR data.</returns>
        /// <exception cref="ArgumentException">MICR data is empty.</exception>
        protected Micr GetMicrInstance( string encryptedMicrContent )
        {
            string decryptedMicrContent = Rock.Security.Encryption.DecryptString( encryptedMicrContent );

            if ( decryptedMicrContent == null )
            {
                throw new ArgumentException( "MICR data is empty." );
            }

            var micr = new Micr( decryptedMicrContent );

            return micr;
        }

        #endregion
    }
}
