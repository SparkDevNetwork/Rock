using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    [Ignore("These tests require additional data. Rows are expected in the IdentityVerificationCode table.")]
    public class IdentityVerificationTests
    {

        [TestMethod]
        public void CreateIdentityVerificationRecordReturnsValue()
        {
            var referenceNumber = "TEST-REFERENCE-NUMBER";
            var ipAddress = "TEST-IP-ADDRESS";
            dataToCleanUp.Add( new string[] { referenceNumber, ipAddress } );

            using ( var rockContext = new RockContext() )
            {
                var service = new IdentityVerificationService( rockContext );
                var verificationRecord = service.CreateIdentityVerificationRecord( ipAddress, 10, referenceNumber );

                Assert.That.IsNotNull( verificationRecord );
            }
        }

        [TestMethod]
        public void CreateIdentityVerificationRecordShouldThrowExceptionIfOverIpLimit()
        {
            var ipLimit = 10;
            var referenceNumber = "TEST-REFERENCE-NUMBER";
            var ipAddress = "TEST-IP-ADDRESS";

            CreateIdentityVerificationRecords( ipLimit, referenceNumber, ipAddress );

            using ( var rockContext = new RockContext() )
            {
                var service = new IdentityVerificationService( rockContext );
                Assert.That.ThrowsException<IdentityVerificationIpLimitReachedException>(
                    () => service.CreateIdentityVerificationRecord( ipAddress, ipLimit, referenceNumber )
                );
            }
        }

        [TestMethod]
        public void CreateIdentityVerificationRecordShouldNotThrowExceptionIfAtIpLimit()
        {
            var ipLimit = 9;
            var referenceNumber = "TEST-REFERENCE-NUMBER";
            var ipAddress = "TEST-IP-ADDRESS";

            CreateIdentityVerificationRecords( ipLimit - 1, referenceNumber, ipAddress );

            using ( var rockContext = new RockContext() )
            {
                var service = new IdentityVerificationService( rockContext );
                var verificationRecord = service.CreateIdentityVerificationRecord( ipAddress, ipLimit, referenceNumber );

                Assert.That.IsNotNull( verificationRecord );
            }
        }

        [TestMethod]
        public void CreateIdentityVerificationRecordShouldNotThrowExceptionWithUnrelatedIps()
        {
            var ipLimit = 9;
            var referenceNumber = "TEST-REFERENCE-NUMBER";
            var ipAddress = "TEST-IP-ADDRESS";

            CreateIdentityVerificationRecords( ipLimit - 1, referenceNumber, ipAddress );
            CreateIdentityVerificationRecords( 20, referenceNumber, $"{ipAddress}-2" );
            CreateIdentityVerificationRecords( 20, $"{referenceNumber}-3", $"{ipAddress}-3" );
            CreateIdentityVerificationRecords( 20, $"{referenceNumber}-4", $"{ipAddress}-4" );

            using ( var rockContext = new RockContext() )
            {
                var service = new IdentityVerificationService( rockContext );
                var verificationRecord = service.CreateIdentityVerificationRecord( ipAddress, ipLimit, referenceNumber );

                Assert.That.IsNotNull( verificationRecord );
            }
        }

        [TestMethod]
        public void CreateIdentityVerificationRecordShouldNotThrowExceptionWithRequestFromYesterday()
        {
            var ipLimit = 9;
            var referenceNumber = "TEST-REFERENCE-NUMBER";
            var ipAddress = "TEST-IP-ADDRESS";

            CreateIdentityVerificationRecords( ipLimit - 1, referenceNumber, ipAddress );
            CreateIdentityVerificationRecords( ipLimit, referenceNumber, ipAddress, RockDateTime.Today.AddSeconds( -10 ) );
            CreateIdentityVerificationRecords( 20, referenceNumber, $"{ipAddress}-2" );
            CreateIdentityVerificationRecords( 20, $"{referenceNumber}-3", $"{ipAddress}-3" );
            CreateIdentityVerificationRecords( 20, $"{referenceNumber}-4", $"{ipAddress}-4" );

            using ( var rockContext = new RockContext() )
            {
                var service = new IdentityVerificationService( rockContext );
                var verificationRecord = service.CreateIdentityVerificationRecord( ipAddress, ipLimit, referenceNumber );

                Assert.That.IsNotNull( verificationRecord );
            }
        }

        [TestMethod]
        public void VerifyIdentityVerificationCodeShouldValidateCorrectly()
        {
            var referenceNumber = "TEST-REFERENCE-NUMBER";
            var ipAddress = "TEST-IP-ADDRESS";
            var validationAttempts = 10;
            dataToCleanUp.Add( new string[] { referenceNumber, ipAddress } );

            using ( var rockContext = new RockContext() )
            {
                var service = new IdentityVerificationService( rockContext );
                var verificationRecord = service.CreateIdentityVerificationRecord( ipAddress, 10, referenceNumber );

                Assert.That.IsNotNull( verificationRecord );

                Assert.That.IsTrue( service.VerifyIdentityVerificationCode( verificationRecord.Id, 1, verificationRecord.IdentityVerificationCode.Code, validationAttempts ) );
            }
        }

        [TestMethod]
        public void VerifyIdentityVerificationCodeShouldNotValidateIfWrongIdentityVerificationId()
        {
            var referenceNumber = "TEST-REFERENCE-NUMBER";
            var ipAddress = "TEST-IP-ADDRESS";
            var validationAttempts = 10;

            dataToCleanUp.Add( new string[] { referenceNumber, ipAddress } );

            using ( var rockContext = new RockContext() )
            {
                var service = new IdentityVerificationService( rockContext );
                var verificationRecord1 = service.CreateIdentityVerificationRecord( ipAddress, 10, referenceNumber );
                var verificationRecord2 = service.CreateIdentityVerificationRecord( ipAddress, 10, referenceNumber );

                Assert.That.IsNotNull( verificationRecord1 );
                Assert.That.IsNotNull( verificationRecord2 );

                Assert.That.IsFalse( service.VerifyIdentityVerificationCode( verificationRecord1.Id, 1, verificationRecord2.IdentityVerificationCode.Code, validationAttempts ) );
            }
        }

        [TestMethod]
        public void VerifyIdentityVerificationCodeShouldNotValidateIfWrongVerificationCode()
        {
            var referenceNumber = "TEST-REFERENCE-NUMBER";
            var ipAddress = "TEST-IP-ADDRESS";
            var validationAttempts = 10;

            dataToCleanUp.Add( new string[] { referenceNumber, ipAddress } );

            using ( var rockContext = new RockContext() )
            {
                var service = new IdentityVerificationService( rockContext );
                var verificationRecord = service.CreateIdentityVerificationRecord( ipAddress, 10, referenceNumber );

                Assert.That.IsNotNull( verificationRecord );

                Assert.That.IsFalse( service.VerifyIdentityVerificationCode( verificationRecord.Id, 1, "120003", validationAttempts ) );
            }
        }

        [TestMethod]
        public void VerifyIdentityVerificationCodeShouldNotValidateIfTimeLimitExpired()
        {
            var referenceNumber = "TEST-REFERENCE-NUMBER";
            var ipAddress = "TEST-IP-ADDRESS";
            var validationAttempts = 10;

            dataToCleanUp.Add( new string[] { referenceNumber, ipAddress } );

            using ( var rockContext = new RockContext() )
            {
                var service = new IdentityVerificationService( rockContext );
                var verificationRecord = service.CreateIdentityVerificationRecord( ipAddress, 10, referenceNumber );

                Assert.That.IsNotNull( verificationRecord );

                verificationRecord.IssueDateTime = verificationRecord.IssueDateTime.AddSeconds( -61 );
                rockContext.SaveChanges();

                Assert.That.IsFalse( service.VerifyIdentityVerificationCode( verificationRecord.Id, 1, verificationRecord.IdentityVerificationCode.Code, validationAttempts ) );
            }
        }

        [TestMethod]
        public void VerifyIdentityVerificationCodeShouldNotValidateIfTooManyAttempts()
        {
            var referenceNumber = "TEST-REFERENCE-NUMBER";
            var ipAddress = "TEST-IP-ADDRESS";
            var validationAttempts = 10;

            dataToCleanUp.Add( new string[] { referenceNumber, ipAddress } );

            using ( var rockContext = new RockContext() )
            {
                var service = new IdentityVerificationService( rockContext );
                var verificationRecord = service.CreateIdentityVerificationRecord( ipAddress, 10, referenceNumber );

                Assert.That.IsNotNull( verificationRecord );

                verificationRecord.FailedMatchAttemptCount = 10;
                rockContext.SaveChanges();

                Assert.That.IsFalse( service.VerifyIdentityVerificationCode( verificationRecord.Id, 1, verificationRecord.IdentityVerificationCode.Code, validationAttempts ) );
            }
        }

        [TestMethod]
        public void VerifyIdentityVerificationCodeShouldCorrectlyIncrementFailedAttempts()
        {
            var referenceNumber = "TEST-REFERENCE-NUMBER";
            var ipAddress = "TEST-IP-ADDRESS";
            var validationAttempts = 10;

            dataToCleanUp.Add( new string[] { referenceNumber, ipAddress } );

            using ( var rockContext = new RockContext() )
            {
                var service = new IdentityVerificationService( rockContext );
                var verificationRecord = service.CreateIdentityVerificationRecord( ipAddress, 10, referenceNumber );

                Assert.That.IsNotNull( verificationRecord );

                service.VerifyIdentityVerificationCode( verificationRecord.Id, 1, "Fail1", validationAttempts );
                service.VerifyIdentityVerificationCode( verificationRecord.Id, 1, "Fail2", validationAttempts );
                service.VerifyIdentityVerificationCode( verificationRecord.Id, 1, "Fail3", validationAttempts );
                service.VerifyIdentityVerificationCode( verificationRecord.Id, 1, "Fail4", validationAttempts );
                service.VerifyIdentityVerificationCode( verificationRecord.Id, 1, "Fail5", validationAttempts );

                var actualVerificationRecord = service.Get( verificationRecord.Id );

                Assert.That.AreEqual( 5, actualVerificationRecord.FailedMatchAttemptCount );
            }
        }

        private List<string[]> dataToCleanUp = new List<string[]>();

        [TestCleanup]
        public void TestCleanUp()
        {
            for ( var i = 0; i < dataToCleanUp.Count; i++ )
            {
                DeleteIdentityVerificationRecords( dataToCleanUp[i][0], dataToCleanUp[i][1] );
            }
        }

        private void CreateIdentityVerificationRecords( int ipLimit, string referenceNumber, string ipAddress, DateTime? issueDateTime = null )
        {
            if ( issueDateTime == null )
            {
                issueDateTime = RockDateTime.Now;
            }

            dataToCleanUp.Add( new string[] { referenceNumber, ipAddress } );

            using ( var rockContext = new RockContext() )
            {
                for ( var i = 0; i < ipLimit; i++ )
                {
                    rockContext.Database.ExecuteSqlCommand( $@"
                        INSERT INTO IdentityVerification (ReferenceNumber, IssueDateTime, RequestIpAddress, IdentityVerificationCodeId, CreatedDateTime, ModifiedDateTime, [Guid])
                        VALUES ('{referenceNumber}','{issueDateTime:s}', '{ipAddress}', 863120, GETDATE(), GETDATE(), NEWID())" );
                }
            }
        }

        private void DeleteIdentityVerificationRecords( string referenceNumber, string ipAddress )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( $@"
                    DELETE IdentityVerification WHERE ReferenceNumber = '{referenceNumber}' AND RequestIpAddress = '{ipAddress}'
                " );
            }
        }
    }
}
