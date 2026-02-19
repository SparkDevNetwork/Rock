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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.PersonViewedSummary;
using Rock.Web.Cache;

using static Rock.Blocks.Engagement.StepTypeList;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Displays a list of person vieweds.
    /// </summary>

    [DisplayName( "Person Viewed Summary" )]
    [Category( "Security" )]
    [Description( "Displays a list of person vieweds." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the person viewed details.",
        Key = AttributeKey.DetailPage )]

    [BooleanField
        ( "See Profiles Viewed",
          Key = AttributeKey.SeeProfilesViewed,
          Description = "Allows selecting to show who has viewed person, or who person has viewed.",
          DefaultValue = "false"
          )]

    [Rock.SystemGuid.EntityTypeGuid( "cfbf8295-a8a4-47d8-800b-6d27f8456168" )]
    // Was [Rock.SystemGuid.BlockTypeGuid( "928af090-fe49-4ac4-9032-8057d1cf3e69" )]
    [Rock.SystemGuid.BlockTypeGuid( "1DAF15F9-E237-4B2B-8309-F335456F8FE4" )]
    [CustomizedGrid]
    public class PersonViewedSummary : RockListBlockType<PersonViewedSummary.ViewerSummary>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string SeeProfilesViewed = "SeeProfilesViewed";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersonViewedSummaryOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private PersonViewedSummaryOptionsBag GetBoxOptions()
        {
            var options = new PersonViewedSummaryOptionsBag()
            {
                SeeProfilesViewed = GetAttributeValue( AttributeKey.SeeProfilesViewed ).AsBoolean()
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var seeProfilesViewed = GetAttributeValue( AttributeKey.SeeProfilesViewed );
            var queryParams = new Dictionary<string, string>
            {
                ["ViewedBy"] = seeProfilesViewed,
                ["ViewerId"] = bool.Parse(seeProfilesViewed) ? PageParameter( "PersonId" ) : "((Key))",
                ["TargetId"] = bool.Parse( seeProfilesViewed ) ? "((Key))": PageParameter( "PersonId" ) 
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<ViewerSummary> GetListQueryable( RockContext rockContext )
        {
            var showProfilesViewed = GetAttributeValue( AttributeKey.SeeProfilesViewed ).AsBoolean();

            var personKey = PageParameter( "PersonId" );
            var person = new PersonService( rockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
            if( person == null )
            {
                return new List<ViewerSummary>().AsQueryable();
            }
            var personViewedService = new PersonViewedService( rockContext );

            var viewerDetailsQueryable = personViewedService.Queryable();

            var viewDetails = showProfilesViewed
                ? viewerDetailsQueryable
                    .Where( p => p.ViewerPersonAlias != null && p.ViewerPersonAlias.PersonId == person.Id )
                    .GroupBy( p => p.TargetPersonAlias.PersonId )
                : viewerDetailsQueryable
                    .Where( p => p.TargetPersonAlias != null && p.TargetPersonAlias.PersonId == person.Id )
                    .GroupBy( p => p.ViewerPersonAlias.PersonId );

            var viewDetailSummary = viewDetails.Select( g => new
            {
                Id = g.Key,
                FirstViewedDate = g.Min( x => x.ViewDateTime ),
                LastViewedDate = g.Max( x => x.ViewDateTime ),
                ViewedCount = g.Count()
            } );

            var personQry = new PersonService( rockContext ).Queryable();

            var viewerPersonDetails = viewDetailSummary
                .Join(
                    personQry,
                    v => v.Id,
                    p => p.Id,
                    ( v, p ) => new ViewerSummary
                    {
                        Id = p.Id,
                        Name = p.NickName + " " + p.LastName,
                        BirthDate = p.BirthDate,
                        Gender = p.Gender.ToString(),
                        FirstViewedDate = v.FirstViewedDate,
                        LastViewedDate = v.LastViewedDate,
                        ViewedCount = v.ViewedCount
                    }
                );

            return viewerPersonDetails;
        }

        /// <inheritdoc/>
        protected override GridBuilder<ViewerSummary> GetGridBuilder()
        {
            return new GridBuilder<ViewerSummary>()
                .WithBlock( this )
                .AddTextField( "idKey", v => v.Id.AsIdKey() )
                .AddTextField( "name", v => v.Name )
                .AddField( "age", v => Person.GetAge( v.BirthDate, null ) ?? 0 )
                .AddTextField( "gender", v => v.Gender )
                .AddDateTimeField( "firstViewedDate", v => v.FirstViewedDate )
                .AddDateTimeField( "lastViewedDate", v => v.LastViewedDate )
                .AddField( "viewedCount", v => v.ViewedCount );
        }

        #endregion

        #region Block Actions

        #endregion

        #region Helper Classes
        public class ViewerSummary
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime? BirthDate { get; set; }
            public string Gender { get; set; }
            public DateTime? FirstViewedDate { get; set; }
            public DateTime? LastViewedDate { get; set; }
            public int ViewedCount { get; set; }
        }

        #endregion
    }
}