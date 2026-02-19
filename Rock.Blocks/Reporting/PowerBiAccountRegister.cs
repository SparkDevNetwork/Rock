using System;
using System.ComponentModel;
using System.Web;

using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Blocks.Reporting.PowerBiAccount;
using Rock.Web.Cache;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Block to register a Power BI account for Rock to use.
    /// </summary>
    [DisplayName("Power Bi Account Register")]
    [Category("Reporting")]
    [Description("This block registers a Power BI account for Rock to use.")]
    [Rock.SystemGuid.BlockTypeGuid( "6373c4cc-65cc-41e9-9b52-d93d0c2542a6" )]
    [Rock.SystemGuid.EntityTypeGuid( "B96E7E86-64E5-4A37-9035-C62908A14E71")]
    public class PowerBiAccountRegister : RockBlockType
    {
        #region Block Actions

        /// <summary>
        /// Gets the initialization data for the block.
        /// </summary>
        /// <returns>The initialization data.</returns>
        public override object GetObsidianBlockInitialization()
        {
            var box = new PowerBiAccountRegisterBox();
            var globalAttributes = GlobalAttributesCache.Get();
            var externalUrl = globalAttributes.GetValue("InternalApplicationRoot");

            if (!externalUrl.EndsWith(@"/"))
            {
                externalUrl += @"/";
            }

            var redirectUrl = externalUrl + "Webhooks/PowerBiAuth.ashx";

            box.Options = new PowerBiAccountRegisterOptionsBag
            {
                RedirectUrl = redirectUrl,
                HomepageUrl = externalUrl
            };

            return box;
        }

        /// <summary>
        /// Registers a new Power BI account.
        /// </summary>
        /// <param name="accountName">The account name.</param>
        /// <param name="accountDescription">The account description.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="redirectUrl">The redirect URL.</param>
        /// <returns>The result of the registration attempt.</returns>
        [BlockAction]
        public BlockActionResult RegisterAccount(string accountName, string accountDescription, string clientId, string clientSecret, string redirectUrl)
        {
            if (string.IsNullOrWhiteSpace(accountName))
            {
                return ActionBadRequest("Account name is required.");
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                return ActionBadRequest("Client ID is required.");
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                return ActionBadRequest("Client secret is required.");
            }

            if (string.IsNullOrWhiteSpace(redirectUrl))
            {
                return ActionBadRequest("Redirect URL is required.");
            }

            try
            {
#if REVIEW_WEBFORMS
                var currentUrl = HttpContext.Current.Request.Url.AbsoluteUri;
#else
                var currentUrl = RequestContext.RequestUri.AbsoluteUri;
#endif
                PowerBiAccountService.CreateAccount(accountName, accountDescription, clientId, clientSecret, redirectUrl, currentUrl);
                return ActionOk();
            }
            catch (Exception ex)
            {
                ExceptionLogService.LogException(ex);
                return ActionBadRequest(ex.Message);
            }
        }

        #endregion
    }
} 