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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Integration.TestData;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Tests.Shared.Constants;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Finance.CheckScanner
{
    /// <summary>
    /// Verifies that a member of the RSR - Finance Worker role has the entity
    /// permissions the Check Scanner app needs, and that a person in no finance
    /// role does not.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These tests check the entity security that the REST controllers and the
    /// file upload handler apply. They run against the shipped default security
    /// (migrations, hotfixes and sample data) rather than mocked Auth rules, so
    /// they catch a missing default rule. The REST endpoint security and the
    /// end to end scanning flow are covered by <see cref="CheckScannerRestTests"/>.
    /// </para>
    /// <para>
    /// Cindy Decker is the sample data member of RSR - Finance Worker. She is
    /// also in RSR - Staff Like Workers, which matches how most churches set up
    /// their finance staff. Bill Marble is in no security role.
    /// </para>
    /// </remarks>
    [TestClass]
    public class CheckScannerSecurityTests : DatabaseTestsBase
    {
        /*
            10/7/26 - CLAUDE

            In 10/2026 FinancialTransactionImage started enforcing EDIT security
            on insert, but it had no default EDIT rules. The Check Scanner then
            failed to link check images for finance workers. Each test below
            covers one permission the scanner depends on, so a change to default
            security that breaks the scanner fails here first.

            Reason: Catch Check Scanner security regressions before release.
        */

        #region Add New Records

        [TestMethod]
        public void AddFinancialBatch_FinanceWorker_IsAuthorized()
        {
            AssertOnlyFinanceWorkerIsAuthorized( person => IsAuthorizedToAdd<FinancialBatch>( batch =>
            {
                batch.Name = "Check Scanner Test Batch";
                batch.Status = BatchStatus.Pending;
                batch.BatchStartDateTime = RockDateTime.Now;
            }, person ) );
        }

        [TestMethod]
        public void AddFinancialPaymentDetail_FinanceWorker_IsAuthorized()
        {
            var currencyTypeCheckId = DefinedValueCache.GetId( SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid() );

            AssertOnlyFinanceWorkerIsAuthorized( person => IsAuthorizedToAdd<FinancialPaymentDetail>( paymentDetail =>
            {
                paymentDetail.CurrencyTypeValueId = currencyTypeCheckId;
            }, person ) );
        }

        [TestMethod]
        public void AddFinancialTransaction_FinanceWorker_IsAuthorized()
        {
            var existingTransaction = GetSampleTransaction();

            AssertOnlyFinanceWorkerIsAuthorized( person => IsAuthorizedToAdd<FinancialTransaction>( transaction =>
            {
                transaction.BatchId = existingTransaction.BatchId;
                transaction.TransactionDateTime = RockDateTime.Now;
                transaction.TransactionTypeValueId = existingTransaction.TransactionTypeValueId;
            }, person ) );
        }

        [TestMethod]
        public void AddFinancialTransactionDetail_FinanceWorker_IsAuthorized()
        {
            var existingTransaction = GetSampleTransaction();

            // A transaction detail uses its transaction as its parent authority,
            // so the transaction must be set for the check to be meaningful.
            AssertOnlyFinanceWorkerIsAuthorized( person => IsAuthorizedToAdd<FinancialTransactionDetail>( detail =>
            {
                detail.TransactionId = existingTransaction.Id;
                detail.AccountId = existingTransaction.TransactionDetails.First().AccountId;
                detail.Amount = 10.00M;
            }, person ) );
        }

        [TestMethod]
        public void AddFinancialTransactionImage_FinanceWorker_IsAuthorized()
        {
            var existingTransaction = GetSampleTransaction();

            AssertOnlyFinanceWorkerIsAuthorized( person => IsAuthorizedToAdd<FinancialTransactionImage>( image =>
            {
                image.TransactionId = existingTransaction.Id;
                image.Order = 0;
            }, person ) );
        }

        #endregion Add New Records

        #region Edit Existing Records

        [TestMethod]
        public void EditFinancialBatch_FinanceWorker_IsAuthorized()
        {
            var existingTransaction = GetSampleTransaction();

            AssertOnlyFinanceWorkerIsAuthorized( person => IsAuthorizedToEdit<FinancialBatch>( existingTransaction.BatchId.Value, person ) );
        }

        [TestMethod]
        public void EditFinancialTransaction_FinanceWorker_IsAuthorized()
        {
            // The scanner deletes transactions too, and the REST DELETE
            // endpoint checks EDIT on the existing record.
            var existingTransaction = GetSampleTransaction();

            AssertOnlyFinanceWorkerIsAuthorized( person => IsAuthorizedToEdit<FinancialTransaction>( existingTransaction.Id, person ) );
        }

        [TestMethod]
        public void EditFinancialTransactionDetail_FinanceWorker_IsAuthorized()
        {
            var existingTransaction = GetSampleTransaction();
            var existingDetailId = existingTransaction.TransactionDetails.First().Id;

            AssertOnlyFinanceWorkerIsAuthorized( person => IsAuthorizedToEdit<FinancialTransactionDetail>( existingDetailId, person ) );
        }

        #endregion Edit Existing Records

        #region Check Images

        [TestMethod]
        public void UploadContributionImage_FinanceWorker_IsAuthorized()
        {
            // FileUploader.ashx allows a binary file upload when the file type
            // allows anonymous uploads or the person can EDIT the file type.
            // The scanner sends no security grant token.
            var contributionImageType = GetContributionImageType();

            AssertOnlyFinanceWorkerIsAuthorized( person => contributionImageType.AllowAnonymous
                || contributionImageType.IsAuthorized( Authorization.EDIT, person ) );
        }

        [TestMethod]
        public void ViewContributionImage_FinanceWorker_IsAuthorized()
        {
            // GetImage.ashx only checks VIEW on an image when its file type
            // requires view security. Scanned images have no parent entity
            // and no rules of their own, so the file type decides.
            var contributionImageType = GetContributionImageType();

            if ( !contributionImageType.RequiresViewSecurity )
            {
                return;
            }

            AssertOnlyFinanceWorkerIsAuthorized( person => contributionImageType.IsAuthorized( Authorization.VIEW, person ) );
        }

        #endregion Check Images

        #region Support Methods

        /// <summary>
        /// Asserts that the finance worker is authorized and that a person
        /// in no finance role is not. Checking both directions makes sure a
        /// test cannot pass just because everyone is allowed.
        /// </summary>
        /// <param name="isAuthorized">Returns <c>true</c> if the person is authorized.</param>
        private static void AssertOnlyFinanceWorkerIsAuthorized( Func<Person, bool> isAuthorized )
        {
            var financeWorker = TestDataHelper.GetTestPerson( TestGuids.TestPeople.CindyDecker );
            var unauthorizedPerson = TestDataHelper.GetTestPerson( TestGuids.TestPeople.BillMarble );

            Assert.IsTrue( isAuthorized( financeWorker ), $"{financeWorker.FullName} (RSR - Finance Worker) should be authorized." );
            Assert.IsFalse( isAuthorized( unauthorizedPerson ), $"{unauthorizedPerson.FullName} (no finance role) should not be authorized." );
        }

        /// <summary>
        /// Determines whether the person can EDIT a new record, the same way
        /// the v1 REST POST endpoint does. The record is created as a proxy
        /// so parent authorities resolve from the foreign keys, and it is
        /// never saved.
        /// </summary>
        /// <typeparam name="T">The type of record to add.</typeparam>
        /// <param name="setValues">Sets the values the scanner would post.</param>
        /// <param name="person">The person to check.</param>
        /// <returns><c>true</c> if the person is authorized.</returns>
        private static bool IsAuthorizedToAdd<T>( Action<T> setValues, Person person )
            where T : Model<T>, new()
        {
            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                var proxyModel = rockContext.Set<T>().Create();

                rockContext.Set<T>().Add( proxyModel );
                setValues( proxyModel );

                return proxyModel.IsAuthorized( Authorization.EDIT, person );
            }
        }

        /// <summary>
        /// Determines whether the person can EDIT an existing record, the same
        /// way the v1 REST PUT and DELETE endpoints do.
        /// </summary>
        /// <typeparam name="T">The type of record.</typeparam>
        /// <param name="id">The identifier of the record.</param>
        /// <param name="person">The person to check.</param>
        /// <returns><c>true</c> if the person is authorized.</returns>
        private static bool IsAuthorizedToEdit<T>( int id, Person person )
            where T : Model<T>, new()
        {
            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                var model = rockContext.Set<T>().Find( id );

                Assert.IsNotNull( model, $"{typeof( T ).Name} {id} was not found." );

                return model.IsAuthorized( Authorization.EDIT, person );
            }
        }

        /// <summary>
        /// Gets the Contribution Image binary file type from the database, the
        /// same way FileUploader.ashx loads it.
        /// </summary>
        /// <returns>The binary file type.</returns>
        private static BinaryFileType GetContributionImageType()
        {
            var rockContext = RockApp.Current.CreateRockContext();

            return new BinaryFileTypeService( rockContext ).Get( SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE.AsGuid() );
        }

        /// <summary>
        /// Gets a sample data transaction that is in a batch and has at least
        /// one transaction detail.
        /// </summary>
        /// <returns>The transaction.</returns>
        private static FinancialTransaction GetSampleTransaction()
        {
            var rockContext = RockApp.Current.CreateRockContext();

            var transaction = new FinancialTransactionService( rockContext ).Queryable( "TransactionDetails" )
                .Where( t => t.BatchId.HasValue && t.TransactionDetails.Any() )
                .OrderBy( t => t.Id )
                .FirstOrDefault();

            Assert.IsNotNull( transaction, "No sample data transaction was found." );

            return transaction;
        }

        #endregion Support Methods
    }
}
