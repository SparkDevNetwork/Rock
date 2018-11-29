// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;

using Quartz;

using Rock;
using Rock.Data;
using Rock.Model;
using Attribute = Rock.Model.Attribute;

using org.newpointe.ReEncrypt.Utility;

namespace org.newpointe.ReEncrypt.Jobs
{
    /// <summary>
    /// This job is used to re-encrypt all encrypted database values to use a new key. Once all the values have been 
    /// updated, this job will delete itself.
    /// </summary>
    /// <remarks>
    /// 
    /// This will re-encrypt the following fields:
    ///  - Attribute.DefaultValue
    ///  - AttributeValue.Value
    ///  - FinancialPaymentDetail.ExpirationMonthEncrypted
    ///  - FinancialPaymentDetail.ExpirationYearEncrypted
    ///  - FinancialPaymentDetail.NameOnCardEncrypted
    ///  - FinancialTransaction.CheckMicrEncrypted
    ///  - FinancialTransaction.CheckMicrParts
    ///
    /// There are some items are ephemeral or stored outside of the application and cannot be re-encrypted:
    ///  - Page/Site Context Cookies
    ///  - Person Tokens
    ///  - Account Confirmation Codes
    /// All current instances of these items will stop working (but new ones generated after the procedure will work).
    ///  
    /// </remarks>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    public class MigrateEncryptedValues : IJob
    {

        static string[] _EncryptedAttributeFieldTypes = new string[] {
            "Rock.Field.Types.EncryptedTextFieldType",
            "Rock.Field.Types.SSNFieldType"
        };

        void IJob.Execute( IJobExecutionContext context )
        {
            // Get the current password
            string currentEncryptionPassword = RockCrypto.GetConfiguredEncryptionPassword();
            byte[] currentEncryptionKey = RockCrypto.GetDerivedKey( currentEncryptionPassword );

            // Generate a new password
            string newEncryptionPassword = RockCrypto.GenerateRandomBase64String( 128 );
            byte[] newEncryptionKey = RockCrypto.GetDerivedKey( newEncryptionPassword );

            context.UpdateLastStatusMessage( "Migrating Attribute DefaultValues" );

            // Re-encrypt all Attribute DefaultValues

            using (RockContext rockContext = new RockContext())
            {
                // Make a new Attribute service
                AttributeService attributeService = new AttributeService( rockContext );

                // Get all attributes that have an encrypted DefaultValue
                // Normally we just use the field type to define what's encrypted or not, but System
                // Settings don't have a field type so we have to do a bit of extra searching.
                IEnumerable<Attribute> attributes = attributeService.Queryable().Where( a =>
                     _EncryptedAttributeFieldTypes.Contains( a.FieldType.Class )
                     || a.DefaultValue.StartsWith( "EAAAA" )
                );

                // Go through each attribute and try re-encrypting it.
                foreach (Attribute attribute in attributes)
                {
                    if (!string.IsNullOrEmpty( attribute.DefaultValue ))
                    {
                        string decryptedValue = TryDecrypt( attribute.DefaultValue, currentEncryptionKey );
                        if (!string.IsNullOrEmpty( decryptedValue ))
                        {
                            attribute.DefaultValue = RockCrypto.EncryptString( decryptedValue, newEncryptionKey );
                        }
                    }
                }

                // Save the attributes
                rockContext.SaveChanges( true );

            }

            context.UpdateLastStatusMessage( "Migrating AttributeValue Values" );

            // Re-encrypt all AttributeValue Values
            using (RockContext rockContext = new RockContext())
            {

                // Make a new Attribute Value service
                AttributeValueService attributeValueService = new AttributeValueService( rockContext );

                // Get all attribute valuess that have an encrypted DefaultValue
                // Normally we just use the field type to define what's encrypted or not, but System
                // Settings don't have a field type so we have to do a bit of extra searching.
                IEnumerable<AttributeValue> attributeValues = attributeValueService.Queryable()
                    .Where( a => a.Value != null && a.Value != "" )
                    .Where( a => _EncryptedAttributeFieldTypes.Contains( a.Attribute.FieldType.Class ) || a.Value.StartsWith( "EAAAA" ) );

                // Track numbers since this will take a while
                int totalCount = attributeValues.Count();
                int currentCount = 1;

                // Go through each attribute value and try re-encrypting it.
                foreach (AttributeValue attributeValue in attributeValues)
                {

                    if (!string.IsNullOrEmpty( attributeValue.Value ))
                    {
                        string decryptedValue = TryDecrypt( attributeValue.Value, currentEncryptionKey );
                        if (!string.IsNullOrEmpty( decryptedValue ))
                        {
                            attributeValue.Value = RockCrypto.EncryptString( decryptedValue, newEncryptionKey );
                        }
                    }

                    // Update the status and save every 250 attribute values
                    currentCount++;
                    if (currentCount % 250 == 0)
                    {
                        context.UpdateLastStatusMessage( $@"Migrating AttributeValue {currentCount} of {totalCount}" );
                    }
                }

                context.UpdateLastStatusMessage( $@"Saving AttributeValues" );

                // Save the attribute values
                rockContext.SaveChanges( true );

            }

            context.UpdateLastStatusMessage( "Migrating encrypted FinancialPaymentDetail fields" );

            // Re-encrypt all encrypted FinancialPaymentDetail fields
            using (RockContext rockContext = new RockContext())
            {

                // Make a new Financial Payment Detail service
                FinancialPaymentDetailService financialPaymentDetailService = new FinancialPaymentDetailService( rockContext );

                IEnumerable<FinancialPaymentDetail> financialPaymentDetails = financialPaymentDetailService.Queryable().Where( fpd =>
                    (fpd.ExpirationMonthEncrypted != null && fpd.ExpirationMonthEncrypted != "") ||
                    (fpd.ExpirationYearEncrypted != null && fpd.ExpirationYearEncrypted != "") ||
                    (fpd.NameOnCardEncrypted != null && fpd.NameOnCardEncrypted != "")
                );

                // Track numbers since this will take a while
                int totalCount = financialPaymentDetails.Count();
                int currentCount = 1;

                // Go through each financial payment detail and try re-encrypting it.
                foreach (FinancialPaymentDetail financialPaymentDetail in financialPaymentDetails)
                {
                    // Check Expiration Month
                    if (!string.IsNullOrEmpty( financialPaymentDetail.ExpirationMonthEncrypted ))
                    {
                        string decryptedValue = TryDecrypt( financialPaymentDetail.ExpirationMonthEncrypted, currentEncryptionKey );
                        if (!string.IsNullOrEmpty( decryptedValue ))
                        {
                            financialPaymentDetail.ExpirationMonthEncrypted = RockCrypto.EncryptString( decryptedValue, newEncryptionKey );
                        }
                    }

                    // Check Expiration Year
                    if (!string.IsNullOrEmpty( financialPaymentDetail.ExpirationYearEncrypted ))
                    {
                        string decryptedValue = TryDecrypt( financialPaymentDetail.ExpirationYearEncrypted, currentEncryptionKey );
                        if (!string.IsNullOrEmpty( decryptedValue ))
                        {
                            financialPaymentDetail.ExpirationYearEncrypted = RockCrypto.EncryptString( decryptedValue, newEncryptionKey );
                        }
                    }

                    // Check Name On Card
                    if (!string.IsNullOrEmpty( financialPaymentDetail.NameOnCardEncrypted ))
                    {
                        string decryptedValue = TryDecrypt( financialPaymentDetail.NameOnCardEncrypted, currentEncryptionKey );
                        if (!string.IsNullOrEmpty( decryptedValue ))
                        {
                            financialPaymentDetail.NameOnCardEncrypted = RockCrypto.EncryptString( decryptedValue, newEncryptionKey );
                        }
                    }

                    // Update the status and save every 250 financial payment detail
                    currentCount++;
                    if (currentCount % 250 == 0)
                    {
                        context.UpdateLastStatusMessage( $@"Migrating FinancialPaymentDetail {currentCount} of {totalCount}" );
                    }
                }

                context.UpdateLastStatusMessage( $@"Saving FinancialPaymentDetails" );

                // Save the financial payment details
                rockContext.SaveChanges( true );

            }

            context.UpdateLastStatusMessage( "Migrating encrypted FinancialTransaction fields" );

            // Re-encrypt all encrypted FinancialTransaction fields
            using (RockContext rockContext = new RockContext())
            {

                // Make a new Financial Transaction service
                FinancialTransactionService financialTransactionService = new FinancialTransactionService( rockContext );

                IEnumerable<FinancialTransaction> financialTransactions = financialTransactionService.Queryable().Where( ft =>
                    (ft.CheckMicrEncrypted != null && ft.CheckMicrEncrypted != "") ||
                    (ft.CheckMicrParts != null && ft.CheckMicrParts != "")
                );

                // Track numbers since this will take a while
                int totalCount = financialTransactions.Count();
                int currentCount = 1;

                // Go through each financial payment detail and try re-encrypting it.
                foreach (FinancialTransaction financialTransaction in financialTransactions)
                {
                    // Check the Check Micr
                    if (!string.IsNullOrEmpty( financialTransaction.CheckMicrEncrypted ))
                    {
                        string decryptedValue = TryDecrypt( financialTransaction.CheckMicrEncrypted, currentEncryptionKey );
                        if (!string.IsNullOrEmpty( decryptedValue ))
                        {
                            financialTransaction.CheckMicrEncrypted = RockCrypto.EncryptString( decryptedValue, newEncryptionKey );
                        }
                    }

                    // Check the Check Micr Parts
                    if (!string.IsNullOrEmpty( financialTransaction.CheckMicrParts ))
                    {
                        string decryptedValue = TryDecrypt( financialTransaction.CheckMicrParts, currentEncryptionKey );
                        if (!string.IsNullOrEmpty( decryptedValue ))
                        {
                            financialTransaction.CheckMicrParts = RockCrypto.EncryptString( decryptedValue, newEncryptionKey );
                        }
                    }

                    // Update the status and save every 250 financial payment detail
                    currentCount++;
                    if (currentCount % 250 == 0)
                    {
                        context.UpdateLastStatusMessage( $@"Migrating FinancialTransaction {currentCount} of {totalCount}" );
                    }
                }

                context.UpdateLastStatusMessage( $@"Saving FinancialTransactions" );

                // Save the financial transactions
                rockContext.SaveChanges( true );

            }

            context.UpdateLastStatusMessage( "Done" );

            // Delete the job so it doesn't run again
            using (RockContext rockContext = new RockContext())
            {
                // Make a new Job service
                ServiceJobService jobService = new ServiceJobService( rockContext );

                // Get the current job
                var job = jobService.Get( context.GetJobId() );

                // Delete it
                if (job != null)
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges( true );
                }
            }



            // Save the new password
            // This will cause Rock to restart, so it has to be last
            RockCrypto.SetConfiguredEncryptionPassword( newEncryptionPassword );
        }

        public string TryDecrypt( string ciphertext, byte[] key )
        {
            try
            {
                return RockCrypto.DecryptString( ciphertext, key );
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
