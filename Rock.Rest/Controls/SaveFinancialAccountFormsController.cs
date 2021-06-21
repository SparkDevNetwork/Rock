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
using System.Web.Http;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controls
{
    /// <summary>
    /// Save Financial Account Form
    /// </summary>
    public class SaveFinancialAccountFormsController : ControlsControllerBase
    {
        /// <summary>
        /// Saves the financial account.
        /// </summary>
        /// <param name="gatewayGuid">The gateway unique identifier.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        [Authenticate]
        [HttpPost]
        [System.Web.Http.Route( "api/v2/Controls/SaveFinancialAccountForms/{gatewayGuid}" )]
        public SaveFinancialAccountFormResult SaveFinancialAccount( Guid gatewayGuid, [FromBody] SaveFinancialAccountFormArgs args )
        {
            // Validate the args
            if ( args?.TransactionCode.IsNullOrWhiteSpace() != false )
            {
                return new SaveFinancialAccountFormResult
                {
                    Title = "Sorry",
                    Detail = "The account information cannot be saved as there's not a valid transaction code to reference",
                    IsSuccess = false
                };
            }

            if ( args.SavedAccountName.IsNullOrWhiteSpace() )
            {
                return new SaveFinancialAccountFormResult
                {
                    Title = "Missing Account Name",
                    Detail = "Please enter a name to use for this account",
                    IsSuccess = false
                };
            }

            var currentPerson = GetPerson();
            var isAnonymous = currentPerson == null;

            using ( var rockContext = new RockContext() )
            {
                if ( isAnonymous )
                {
                    if ( args.Username.IsNullOrWhiteSpace() || args.Password.IsNullOrWhiteSpace() )
                    {
                        return new SaveFinancialAccountFormResult
                        {
                            Title = "Missing Information",
                            Detail = "A username and password are required when saving an account",
                            IsSuccess = false
                        };
                    }

                    var userLoginService = new UserLoginService( rockContext );

                    if ( userLoginService.GetByUserName( args.Username ) != null )
                    {
                        return new SaveFinancialAccountFormResult
                        {
                            Title = "Invalid Username",
                            Detail = "The selected Username is already being used. Please select a different Username",
                            IsSuccess = false
                        };
                    }

                    if ( !UserLoginService.IsPasswordValid( args.Password ) )
                    {
                        return new SaveFinancialAccountFormResult
                        {
                            Title = "Invalid Password",
                            Detail = UserLoginService.FriendlyPasswordRules(),
                            IsSuccess = false
                        };
                    }
                }

                // Load the gateway from the database
                var financialGatewayService = new FinancialGatewayService( rockContext );
                var financialGateway = financialGatewayService.Get( gatewayGuid );
                var gateway = financialGateway?.GetGatewayComponent();

                if ( gateway is null )
                {
                    return new SaveFinancialAccountFormResult
                    {
                        Title = "Invalid Gateway",
                        Detail = "Sorry, the financial gateway information is not valid.",
                        IsSuccess = false
                    };
                }

                // Load the transaction from the database
                var financialTransactionService = new FinancialTransactionService( rockContext );
                var transaction = financialTransactionService.GetByTransactionCode( financialGateway.Id, args.TransactionCode );
                var transactionPersonAlias = transaction?.AuthorizedPersonAlias;
                var transactionPerson = transactionPersonAlias?.Person;
                var paymentDetail = transaction?.FinancialPaymentDetail;

                if ( transactionPerson is null || paymentDetail is null )
                {
                    return new SaveFinancialAccountFormResult
                    {
                        Title = "Invalid Transaction",
                        Detail = "Sorry, the account information cannot be saved as there's not a valid transaction to reference",
                        IsSuccess = false
                    };
                }

                // Create the login if needed
                if ( isAnonymous )
                {
                    var user = UserLoginService.Create(
                        rockContext,
                        transactionPerson,
                        AuthenticationServiceType.Internal,
                        EntityTypeCache.Get( SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                        args.Username,
                        args.Password,
                        false );

                    var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );
                    // TODO mergeFields.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );
                    mergeFields.Add( "Person", transactionPerson );
                    mergeFields.Add( "User", user );

                    var emailMessage = new RockEmailMessage( SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT.AsGuid() );
                    emailMessage.AddRecipient( new RockEmailMessageRecipient( transactionPerson, mergeFields ) );
                    // TODO emailMessage.AppRoot = ResolveRockUrl( "~/" );
                    // TODO emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                    emailMessage.CreateCommunicationRecord = false;
                    emailMessage.Send();
                }

                var savedAccount = new FinancialPersonSavedAccount
                {
                    PersonAliasId = transactionPersonAlias.Id,
                    ReferenceNumber = args.TransactionCode,
                    GatewayPersonIdentifier = args.GatewayPersonIdentifier,
                    Name = args.SavedAccountName,
                    TransactionCode = args.TransactionCode,
                    FinancialGatewayId = financialGateway.Id,
                    FinancialPaymentDetail = new FinancialPaymentDetail
                    {
                        AccountNumberMasked = paymentDetail.AccountNumberMasked,
                        CurrencyTypeValueId = paymentDetail.CurrencyTypeValueId,
                        CreditCardTypeValueId = paymentDetail.CreditCardTypeValueId,
                        NameOnCard = paymentDetail.NameOnCard,
                        ExpirationMonth = paymentDetail.ExpirationMonth,
                        ExpirationYear = paymentDetail.ExpirationYear,
                        BillingLocationId = paymentDetail.BillingLocationId
                    }
                };

                var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( rockContext );
                financialPersonSavedAccountService.Add( savedAccount );
                rockContext.SaveChanges();

                return new SaveFinancialAccountFormResult
                {
                    Title = "Success",
                    Detail = "The account has been saved for future use",
                    IsSuccess = true
                };
            }
        }

        /// <summary>
        /// Save Financial Account Form Args
        /// </summary>
        public sealed class SaveFinancialAccountFormArgs
        {
            /// <summary>
            /// Gets or sets the username.
            /// </summary>
            /// <value>
            /// The username.
            /// </value>
            public string Username { get; set; }

            /// <summary>
            /// Gets or sets the password.
            /// </summary>
            /// <value>
            /// The password.
            /// </value>
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets the name of the saved account.
            /// </summary>
            /// <value>
            /// The name of the saved account.
            /// </value>
            public string SavedAccountName { get; set; }

            /// <summary>
            /// Gets or sets the transaction code.
            /// </summary>
            /// <value>
            /// The transaction code.
            /// </value>
            public string TransactionCode { get; set; }

            /// <summary>
            /// Gets or sets the gateway token.
            /// </summary>
            /// <value>
            /// The gateway token.
            /// </value>
            public string GatewayPersonIdentifier { get; set; }
        }

        /// <summary>
        /// Save Financial Account Form Result
        /// </summary>
        public sealed class SaveFinancialAccountFormResult
        {
            /// <summary>
            /// Gets or sets the title.
            /// </summary>
            /// <value>
            /// The title.
            /// </value>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the detail.
            /// </summary>
            /// <value>
            /// The detail.
            /// </value>
            public string Detail { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is success.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is success; otherwise, <c>false</c>.
            /// </value>
            public bool IsSuccess { get; set; }
        }
    }
}
