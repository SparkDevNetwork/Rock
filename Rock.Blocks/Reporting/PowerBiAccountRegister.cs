using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Routing;

using OpenXmlPowerTools;

using Rock.Attribute;
using Rock.Blocks.Types.Mobile.Cms;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Reporting;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Reporting.PowerBiAccount;
using Rock.ViewModels.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Block to register a Power BI account for Rock to use.
    /// </summary>
    [DisplayName("Power Bi Account Register")]
    [Category("Reporting")]
    [Description("This block registers a Power BI account for Rock to use.")]
    [Rock.SystemGuid.BlockTypeGuid( "6373c4cc-65cc-41e9-9b52-d93d0c2542a6" )]
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
                var currentUrl = HttpContext.Current.Request.Url.AbsoluteUri;
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