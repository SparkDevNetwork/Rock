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
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Reporting
{
    /// <summary>
    /// Service class for handling Power BI account operations.
    /// </summary>
    public class PowerBiAccountService
    {
        /// <summary>
        /// Creates a new Power BI account.
        /// </summary>
        /// <param name="accountName">The account name.</param>
        /// <param name="accountDescription">The account description.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="redirectUrl">The redirect URL.</param>
        /// <param name="currentUrl">The current URL.</param>
        public static void CreateAccount(string accountName, string accountDescription, string clientId, string clientSecret, string redirectUrl, string currentUrl)
        {
            var rockContext = new RockContext();
            var definedTypeService = new DefinedTypeService(rockContext);
            var definedValueService = new DefinedValueService(rockContext);

            var powerBiAccountType = definedTypeService.Get(Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS.AsGuid());

            if (powerBiAccountType == null)
            {
                throw new Exception("Power BI Account type not found.");
            }

            var definedValue = new DefinedValue
            {
                DefinedTypeId = powerBiAccountType.Id,
                Value = accountName,
                Description = accountDescription,
                IsSystem = false
            };

            definedValue.LoadAttributes();
            definedValue.SetAttributeValue("ClientId", clientId);
            definedValue.SetAttributeValue("ClientSecret", Encryption.EncryptString(clientSecret));
            definedValue.SetAttributeValue("RedirectUrl", redirectUrl);

            definedValueService.Add(definedValue);
            rockContext.SaveChanges();

            definedValue.SaveAttributeValues(rockContext);

            // Redirect to Power BI for authentication
            var authUrl = $"https://login.microsoftonline.com/common/oauth2/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUrl}&state={definedValue.Id}";
            System.Web.HttpContext.Current.Response.Redirect(authUrl);
        }
    }
} 