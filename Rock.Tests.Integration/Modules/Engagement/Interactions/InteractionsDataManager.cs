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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Data.Interactions
{
    /// <summary>
    /// Manages data for the Interactions feature of Rock.
    /// </summary>
    public class InteractionsDataManager
    {
        #region Static methods

        private static Lazy<InteractionsDataManager> _interactionsDataManager = new Lazy<InteractionsDataManager>();

        /// <summary>
        /// The default instance of this component.
        /// </summary>
        public static InteractionsDataManager Instance => _interactionsDataManager.Value;

        #endregion

        /// <summary>
        /// Remove the specified interaction.
        /// </summary>
        /// <param name="interactionIdentifier"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool DeleteInteraction( string interactionIdentifier )
        {
            var rockContext = new RockContext();
            var service = new InteractionService( rockContext );
            var interaction = service.Get( interactionIdentifier );

            if ( interaction == null )
            {
                return false;
            }

            return service.Delete( interaction );
        }

        /// <summary>
        /// Create an interaction for a Page View.
        /// </summary>
        /// <param name="actionInfo"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        public Interaction CreatePageViewInteraction( CreatePageViewInteractionActionArgs actionInfo )
        {
            var interactions = CreatePageViewInteractionInternal( new List<CreatePageViewInteractionActionArgs> { actionInfo } );
            return interactions.First();
        }

        /// <summary>
        /// Create an interaction for a Page View.
        /// </summary>
        /// <param name="actionInfo"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private List<Interaction> CreatePageViewInteractionInternal( List<CreatePageViewInteractionActionArgs> actionInfoList, RockContext rockContext = null )
        {
            string deviceApplication;
            string deviceOs;
            string deviceClientType;


            rockContext = rockContext ?? new RockContext();
            var interactionService = new InteractionService( rockContext );

            var interactions = new List<Interaction>();

            foreach ( var actionInfo in actionInfoList )
            {
                TestDataHelper.Web.ParseUserAgentString( actionInfo.UserAgentString,
                    out deviceOs,
                    out deviceApplication,
                    out deviceClientType );

                // Get the Page.
                var page = PageCache.Get( actionInfo.PageIdentifier, allowIntegerIdentifier: true );
                Assert.IsNotNull( page, "Invalid page." );

                // Get the Site.
                if ( string.IsNullOrWhiteSpace( actionInfo.SiteIdentifier ) )
                {
                    actionInfo.SiteIdentifier = page.SiteId.ToString();
                }
                var site = SiteCache.Get( actionInfo.SiteIdentifier, allowIntegerIdentifier: true );
                Assert.IsNotNull( site, "Invalid site." );

                // Get the Interaction Channel.
                var dvWebsiteChannelType = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE );
                var interactionChannelId = InteractionChannelCache.GetChannelIdByTypeIdAndEntityId( dvWebsiteChannelType.Id,
                    site.Id,
                    channelName: null,
                    componentEntityTypeId: null,
                    interactionEntityTypeId: null );

                // Get the Interaction Component.
                var interactionComponentId = InteractionComponentCache.GetComponentIdByChannelIdAndEntityId( interactionChannelId,
                    page.Id,
                    componentName: null );

                var interaction = interactionService.AddInteraction( interactionComponentId,
                    page.Id,
                    operation: "View",
                    $"Browser Session {actionInfo.BrowserSessionGuid}",
                    actionInfo.RequestUrl,
                    actionInfo.UserPersonAliasId,
                    actionInfo.ViewDateTime,
                    deviceApplication,
                    deviceOs,
                    deviceClientType,
                    deviceTypeData: "",
                    actionInfo.BrowserIpAddress,
                    actionInfo.BrowserSessionGuid );

                interaction.ForeignKey = actionInfo.ForeignKey;

                interactions.Add( interaction );
            }

            rockContext.SaveChanges();

            return interactions;
        }
    }

    #region Support Classes

    /// <summary>
    /// Arguments for the CreatePageViewInteraction action.
    /// </summary>
    public class CreatePageViewInteractionActionArgs
    {
        public Guid? Guid;
        public string ForeignKey;
        public DateTime ViewDateTime;
        public string SiteIdentifier;
        public string PageIdentifier;
        public string UserAgentString;
        public string BrowserIpAddress;
        public Guid? BrowserSessionGuid;
        public string RequestUrl;
        public int? UserPersonAliasId;
    }

    #endregion
}
