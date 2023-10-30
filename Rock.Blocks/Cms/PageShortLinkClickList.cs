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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.PageShortLinkClickList;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Lists clicks for a particular short link.
    /// </summary>

    [DisplayName( "Short Link Click List" )]
    [Category( "CMS" )]
    [Description( "Lists clicks for a particular short link." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes(Model.SiteType.Web)]

    [Rock.SystemGuid.EntityTypeGuid( "aa860dc7-d590-4d0e-bbb3-16990f2cd680" )]
    [Rock.SystemGuid.BlockTypeGuid( "e44cac85-346f-41a4-884b-a6fb5fc64de1" )]
    [CustomizedGrid]
    public class PageShortLinkClick : RockListBlockType<PageShortLinkClickList>
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PageShortLinkClickListOptionsBag>();
            var builder = GetGridBuilder();

            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private PageShortLinkClickListOptionsBag GetBoxOptions()
        {
            var options = new PageShortLinkClickListOptionsBag();

            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<PageShortLinkClickList> GetListQueryable( RockContext rockContext )
        {
            int shortLinkId = RequestContext?.PageParameters?["ShortLinkId"]?.AsInteger() ?? 0;
            if ( shortLinkId == 0 )
            {
                return Enumerable.Empty<PageShortLinkClickList>().AsQueryable();
            }

            var interactions = new InteractionService( rockContext )
                .Queryable().AsNoTracking()
                .Include( i => i.PersonAlias )
                .Include( i => i.PersonAlias.Person )
                .Where( i => i.InteractionComponent.EntityId == shortLinkId && i.PersonAlias != null )
                .ToList();

            var result = interactions.Select( i => new PageShortLinkClickList
            {
                PageShortLinkId = shortLinkId,
                PersonAlias = i.PersonAlias,
                InteractionDateTime = i.InteractionDateTime,
                PersonId = i.PersonAlias.PersonId,
                Application = i.InteractionSession.DeviceType.Application,
                ClientType = i.InteractionSession.DeviceType.ClientType,
                OperatingSystem = i.InteractionSession.DeviceType.OperatingSystem,
                Source = i.Source
            } )
            .AsQueryable();

            return result;
        }

        /// <inheritdoc/>
        protected override GridBuilder<PageShortLinkClickList> GetGridBuilder()
        {
            return new GridBuilder<PageShortLinkClickList>()
                .WithBlock( this )
                .AddField( "id", a => a.PageShortLinkId )
                .AddField( "interactionDateTime", a => a.InteractionDateTime )
                .AddPersonField( "person", a => a.PersonAlias?.Person )
                .AddTextField( "application", a => a.Application )
                .AddTextField( "clientType", a => a.ClientType )
                .AddTextField( "operatingSystem", a => a.OperatingSystem )
                .AddTextField( "source", a => a.Source );
        }

        #endregion
    }
}

