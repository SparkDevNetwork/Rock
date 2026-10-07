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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.Web.Http;
using System.Web.Http.Controllers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Model;
using Rock.Rest;
using Rock.Rest.Controllers;
using Rock.Rest.Filters;
using Rock.Tests.Integration.TestData;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Tests.Shared.Constants;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Finance.CheckScanner
{
    /// <summary>
    /// Verifies that a member of the RSR - Finance Worker role can use the
    /// REST endpoints the Check Scanner app calls, and can scan a check from
    /// start to finish.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The controllers are called in-process. Each call first runs the
    /// <see cref="SecuredAttribute"/> filter with the user's login, the same as
    /// the Web API pipeline, and then calls the action with the person the
    /// filter resolved. Calling an action directly would otherwise skip the
    /// endpoint security check.
    /// </para>
    /// <para>
    /// The calls follow ScanningPageUtility.UploadScannedItem in the Check
    /// Scanner app. Image uploads go through FileUploader.ashx, which cannot be
    /// called from tests, so the flow creates those files directly. Permission
    /// to upload them is covered by <see cref="CheckScannerSecurityTests"/>.
    /// </para>
    /// </remarks>
    [TestClass]
    public class CheckScannerRestTests : DatabaseTestsBase
    {
        /// <summary>
        /// The user name of the sample data member of RSR - Finance Worker.
        /// </summary>
        private const string FinanceWorkerUserName = "cdecker";

        /// <summary>
        /// The user name of a sample data person in no security role.
        /// </summary>
        private const string UnauthorizedUserName = "bmarble";

        #region Endpoint Security

        /// <summary>
        /// The endpoints the Check Scanner app writes to, as the controller
        /// type, action name and HTTP method.
        /// </summary>
        private static readonly List<(Type ControllerType, string ActionName, HttpMethod Method)> WriteEndpoints = new List<(Type, string, HttpMethod)>
        {
            ( typeof( FinancialBatchesController ), nameof( FinancialBatchesController.Post ), HttpMethod.Post ),
            ( typeof( FinancialBatchesController ), nameof( FinancialBatchesController.Put ), HttpMethod.Put ),
            ( typeof( FinancialBatchesController ), nameof( FinancialBatchesController.Delete ), HttpMethod.Delete ),
            ( typeof( FinancialPaymentDetailsController ), nameof( FinancialPaymentDetailsController.Post ), HttpMethod.Post ),
            ( typeof( FinancialTransactionsController ), nameof( FinancialTransactionsController.PostScanned ), HttpMethod.Post ),
            ( typeof( FinancialTransactionsController ), nameof( FinancialTransactionsController.Post ), HttpMethod.Post ),
            ( typeof( FinancialTransactionsController ), nameof( FinancialTransactionsController.Delete ), HttpMethod.Delete ),
            ( typeof( FinancialTransactionImagesController ), nameof( FinancialTransactionImagesController.Post ), HttpMethod.Post ),
            ( typeof( FinancialTransactionDetailsController ), nameof( FinancialTransactionDetailsController.Post ), HttpMethod.Post ),
            ( typeof( FinancialTransactionDetailsController ), nameof( FinancialTransactionDetailsController.Put ), HttpMethod.Put )
        };

        /// <summary>
        /// The endpoints the Check Scanner app reads from, as the controller
        /// type and action name. All of them are GET requests.
        /// </summary>
        private static readonly List<(Type ControllerType, string ActionName)> ReadEndpoints = new List<(Type, string)>
        {
            ( typeof( PeopleController ), nameof( PeopleController.GetByUserName ) ),
            ( typeof( PeopleController ), nameof( PeopleController.GetByPersonAliasId ) ),
            ( typeof( CampusesController ), nameof( CampusesController.Get ) ),
            ( typeof( DefinedTypesController ), nameof( DefinedTypesController.Get ) ),
            ( typeof( DefinedValuesController ), nameof( DefinedValuesController.Get ) ),
            ( typeof( FinancialAccountsController ), nameof( FinancialAccountsController.Get ) ),
            ( typeof( FinancialBatchesController ), nameof( FinancialBatchesController.Get ) ),
            ( typeof( FinancialBatchesController ), nameof( FinancialBatchesController.GetControlTotals ) ),
            ( typeof( FinancialTransactionsController ), nameof( FinancialTransactionsController.Get ) ),
            ( typeof( FinancialTransactionImagesController ), nameof( FinancialTransactionImagesController.Get ) ),
            ( typeof( FinancialTransactionDetailsController ), nameof( FinancialTransactionDetailsController.Get ) ),
            ( typeof( FinancialPaymentDetailsController ), nameof( FinancialPaymentDetailsController.Get ) ),
            ( typeof( BinaryFileTypesController ), nameof( BinaryFileTypesController.Get ) ),
            ( typeof( BinaryFilesController ), nameof( BinaryFilesController.Get ) ),
            ( typeof( AttributesController ), nameof( AttributesController.Get ) )
        };

        [TestMethod]
        public void WriteEndpoints_FinanceWorker_IsAuthorized()
        {
            var failures = WriteEndpoints
                .Where( e => GetSecuredStatusCode( e.ControllerType, e.ActionName, e.Method, FinanceWorkerUserName ) != HttpStatusCode.OK )
                .Select( e => $"{e.Method} {e.ControllerType.Name}.{e.ActionName}" )
                .ToList();

            Assert.IsEmpty( failures, $"RSR - Finance Worker was denied: {failures.AsDelimited( ", " )}" );
        }

        [TestMethod]
        public void WriteEndpoints_PersonWithNoFinanceRole_IsNotAuthorized()
        {
            var failures = WriteEndpoints
                .Where( e => GetSecuredStatusCode( e.ControllerType, e.ActionName, e.Method, UnauthorizedUserName ) != HttpStatusCode.Unauthorized )
                .Select( e => $"{e.Method} {e.ControllerType.Name}.{e.ActionName}" )
                .ToList();

            Assert.IsEmpty( failures, $"A person with no finance role was allowed: {failures.AsDelimited( ", " )}" );
        }

        [TestMethod]
        public void ReadEndpoints_FinanceWorker_IsAuthorized()
        {
            var failures = ReadEndpoints
                .Where( e => GetSecuredStatusCode( e.ControllerType, e.ActionName, HttpMethod.Get, FinanceWorkerUserName ) != HttpStatusCode.OK )
                .Select( e => $"GET {e.ControllerType.Name}.{e.ActionName}" )
                .ToList();

            Assert.IsEmpty( failures, $"RSR - Finance Worker was denied: {failures.AsDelimited( ", " )}" );
        }

        #endregion Endpoint Security

        #region Scanning Flow

        [TestMethod]
        public void ScanCheck_FinanceWorker_SavesTransactionWithLinkedImages()
        {
            var financeWorker = TestDataHelper.GetTestPerson( TestGuids.TestPeople.CindyDecker );
            var generalFund = FinancialAccountCache.Get( SystemGuid.FinancialAccount.GENERAL_FUND.AsGuid() );
            var batchStartDateTime = RockDateTime.Today;
            var micrData = $"d123456789d 0123456789c {Guid.NewGuid():N}";

            // Create the batch from the batch page.
            var batch = new FinancialBatch
            {
                Guid = Guid.NewGuid(),
                Name = "Check Scanner Test Batch",
                Status = BatchStatus.Pending,
                BatchStartDateTime = batchStartDateTime,
                ControlAmount = 25.00M,
                CreatedByPersonAliasId = financeWorker.PrimaryAliasId
            };

            var batchId = PostAsFinanceWorker<FinancialBatchesController>( nameof( FinancialBatchesController.Post ), controller => controller.Post( batch ) );

            // Upload the front and back images. The app does this through
            // FileUploader.ashx, so create the files the way it does.
            var frontImageId = AddUploadedCheckImage( "image1_test.png", financeWorker );
            var backImageId = AddUploadedCheckImage( "image2_test.png", financeWorker );

            // Create the payment detail.
            var paymentDetail = new FinancialPaymentDetail
            {
                Guid = Guid.NewGuid(),
                CurrencyTypeValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid() )
            };

            var paymentDetailId = PostAsFinanceWorker<FinancialPaymentDetailsController>( nameof( FinancialPaymentDetailsController.Post ), controller => controller.Post( paymentDetail ) );

            // Create the transaction from the scanned check.
            var scannedCheck = new FinancialTransactionScannedCheck
            {
                FinancialTransaction = new FinancialTransaction
                {
                    Guid = Guid.NewGuid(),
                    BatchId = batchId,
                    TransactionCode = "1001",
                    Summary = string.Empty,
                    TransactionDateTime = batchStartDateTime,
                    FinancialPaymentDetailId = paymentDetailId,
                    SourceTypeValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_ONSITE_COLLECTION.AsGuid() ),
                    TransactionTypeValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Value,
                    MICRStatus = MICRStatus.Success,
                    TransactionDetails = new List<FinancialTransactionDetail>
                    {
                        new FinancialTransactionDetail
                        {
                            Guid = Guid.NewGuid(),
                            AccountId = generalFund.Id,
                            Amount = 25.00M
                        }
                    }
                },
                ScannedCheckMicrData = micrData,
                ScannedCheckMicrParts = micrData
            };

            var transactionId = PostAsFinanceWorker<FinancialTransactionsController>( nameof( FinancialTransactionsController.PostScanned ), controller => controller.PostScanned( scannedCheck ) );

            // Link the front and back images to the transaction.
            PostAsFinanceWorker<FinancialTransactionImagesController>( nameof( FinancialTransactionImagesController.Post ), controller => controller.Post( new FinancialTransactionImage { Guid = Guid.NewGuid(), BinaryFileId = frontImageId, TransactionId = transactionId, Order = 0 } ) );

            PostAsFinanceWorker<FinancialTransactionImagesController>( nameof( FinancialTransactionImagesController.Post ), controller => controller.Post( new FinancialTransactionImage { Guid = Guid.NewGuid(), BinaryFileId = backImageId, TransactionId = transactionId, Order = 1 } ) );

            // Verify what was saved.
            var rockContext = RockApp.Current.CreateRockContext();
            var transaction = new FinancialTransactionService( rockContext ).Get( transactionId );

            Assert.IsNotNull( transaction );
            Assert.AreEqual( batchId, transaction.BatchId );
            Assert.AreEqual( paymentDetailId, transaction.FinancialPaymentDetailId );
            Assert.IsNotNull( transaction.CheckMicrHash, "The MICR hash was not set." );
            Assert.AreEqual( 25.00M, transaction.TotalAmount );

            var images = transaction.Images.OrderBy( i => i.Order ).ToList();

            Assert.HasCount( 2, images );
            Assert.AreEqual( frontImageId, images[0].BinaryFileId );
            Assert.AreEqual( backImageId, images[1].BinaryFileId );
            Assert.IsTrue( images.All( i => !i.BinaryFile.IsTemporary ), "A linked check image is still marked temporary." );

            // The app checks for duplicates before uploading the next check.
            Assert.IsTrue( new FinancialTransactionsController().AlreadyScanned( micrData ), "The scanned check was not found as a duplicate." );
        }

        #endregion Scanning Flow

        #region Support Methods

        /// <summary>
        /// Runs the <see cref="SecuredAttribute"/> filter for an action as the
        /// specified user and returns the resulting status code.
        /// </summary>
        /// <param name="controllerType">The type of controller.</param>
        /// <param name="actionName">The name of the action method.</param>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="userName">The user name making the request.</param>
        /// <returns><see cref="HttpStatusCode.OK"/> if the request is allowed.</returns>
        private static HttpStatusCode GetSecuredStatusCode( Type controllerType, string actionName, HttpMethod method, string userName )
        {
            var controller = ( ApiController ) Activator.CreateInstance( controllerType );

            return RunSecuredFilter( controller, actionName, method, userName );
        }

        /// <summary>
        /// Creates a controller for a request made by the specified user, runs
        /// the <see cref="SecuredAttribute"/> filter for the action and fails
        /// the test if the request is not allowed.
        /// </summary>
        /// <typeparam name="TController">The type of controller.</typeparam>
        /// <param name="actionName">The name of the action method that will be called.</param>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="userName">The user name making the request.</param>
        /// <returns>The controller, ready to call the action.</returns>
        private static TController CreateSecuredController<TController>( string actionName, HttpMethod method, string userName )
            where TController : ApiController, new()
        {
            var controller = new TController();
            var statusCode = RunSecuredFilter( controller, actionName, method, userName );

            Assert.AreEqual( HttpStatusCode.OK, statusCode, $"{method} {typeof( TController ).Name}.{actionName} was not allowed for {userName}." );

            return controller;
        }

        /// <summary>
        /// Sets up the controller with a request made by the specified user and
        /// runs the <see cref="SecuredAttribute"/> filter for the action. When
        /// the request is allowed, the filter stores the person on the request
        /// so the action runs as that person.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="actionName">The name of the action method.</param>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="userName">The user name making the request.</param>
        /// <returns>The status code set by the filter.</returns>
        private static HttpStatusCode RunSecuredFilter( ApiController controller, string actionName, HttpMethod method, string userName )
        {
            var controllerType = controller.GetType();
            var principal = new GenericPrincipal( new GenericIdentity( userName ), null );
            var request = new HttpRequestMessage( method, "http://localhost/" );
            var requestContext = new HttpRequestContext { Principal = principal };

            request.SetUserPrincipal( principal );

            controller.Request = request;
            controller.RequestContext = requestContext;
            controller.Configuration = new HttpConfiguration();

            // The base Get action is overloaded, so pick the one that takes no
            // parameters. Every other action the scanner uses has one overload.
            var methodInfo = actionName == nameof( ApiController<Campus>.Get )
                ? controllerType.GetMethod( actionName, Type.EmptyTypes )
                : controllerType.GetMethod( actionName );

            Assert.IsNotNull( methodInfo, $"{controllerType.Name}.{actionName} was not found." );

            var controllerDescriptor = new HttpControllerDescriptor( controller.Configuration, controllerType.Name.Replace( "Controller", string.Empty ), controllerType );
            var actionContext = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext( requestContext, request, controllerDescriptor, controller ),
                ActionDescriptor = new ReflectedHttpActionDescriptor( controllerDescriptor, methodInfo ),
                Response = new HttpResponseMessage( HttpStatusCode.OK )
            };

            new SecuredAttribute().OnActionExecuting( actionContext );

            return actionContext.Response.StatusCode;
        }

        /// <summary>
        /// Calls a POST action as the finance worker and returns the identifier
        /// of the new record, failing the test if the request is denied or the
        /// record is not created.
        /// </summary>
        /// <typeparam name="TController">The type of controller.</typeparam>
        /// <param name="actionName">The name of the action method being called.</param>
        /// <param name="post">Calls the action on the controller.</param>
        /// <returns>The identifier of the new record.</returns>
        private static int PostAsFinanceWorker<TController>( string actionName, Func<TController, HttpResponseMessage> post )
            where TController : ApiController, new()
        {
            var controller = CreateSecuredController<TController>( actionName, HttpMethod.Post, FinanceWorkerUserName );
            var endpointName = $"POST {typeof( TController ).Name}.{actionName}";
            HttpResponseMessage response;

            try
            {
                response = post( controller );
            }
            catch ( HttpResponseException ex )
            {
                // The entity security check throws instead of returning a response.
                Assert.Fail( $"{endpointName} returned {ex.Response.StatusCode} for {FinanceWorkerUserName}." );
                throw;
            }

            var content = response.Content?.ReadAsStringAsync().Result;

            Assert.AreEqual( HttpStatusCode.Created, response.StatusCode, $"{endpointName} did not create the record. {content}" );
            Assert.IsTrue( response.TryGetContentValue<int>( out var id ), $"{endpointName} did not return the new identifier." );

            return id;
        }

        /// <summary>
        /// Adds a check image the way FileUploader.ashx saves a file uploaded
        /// by the Check Scanner app.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="uploadedByPerson">The person uploading the file.</param>
        /// <returns>The identifier of the new file.</returns>
        private static int AddUploadedCheckImage( string fileName, Person uploadedByPerson )
        {
            var rockContext = RockApp.Current.CreateRockContext();
            var binaryFileService = new BinaryFileService( rockContext );
            var contributionImageType = new BinaryFileTypeService( rockContext ).Get( SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE.AsGuid() );
            var content = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

            var binaryFile = new BinaryFile
            {
                IsTemporary = false,
                BinaryFileTypeId = contributionImageType.Id,
                MimeType = "image/png",
                FileName = fileName,
                FileSize = content.Length,
                CreatedByPersonAliasId = uploadedByPerson.PrimaryAliasId,
                ContentStream = new MemoryStream( content )
            };

            binaryFileService.Add( binaryFile );
            rockContext.SaveChanges();

            return binaryFile.Id;
        }

        #endregion Support Methods
    }
}
