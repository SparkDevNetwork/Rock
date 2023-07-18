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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Example
{
    /// <summary>
    /// Demonstrates the various parts of the Obsidian List blocks.
    /// </summary>

    [DisplayName( "Obsidian Gallery List" )]
    [Category( "Example" )]
    [Description( "Demonstrates the various parts of the Obsidian List blocks." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "4315fd92-f9f1-4038-abdc-a2d661b9dedf" )]
    [Rock.SystemGuid.BlockTypeGuid( "121eec5e-f8aa-4cd8-a61d-9c99c269280e" )]
    [CustomizedGrid]
    public class ObsidianGalleryList : RockEntityListBlockType<Person>
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var builder = GetGridBuilder();

            return new ListBlockBox<Dictionary<string, object>>
            {
                IsAddEnabled = true,
                IsDeleteEnabled = true,
                GridDefinition = builder.BuildDefinition()
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Person> GetListQueryable( RockContext rockContext )
        {
            return base.GetListQueryable( rockContext )
                .Include( p => p.PrimaryCampus )
                .Take( RequestContext.GetPageParameter( "count" ).AsIntegerOrNull() ?? 1_000 );
        }

        /// <summary>
        /// Gets the grid builder that will be used to construct the definition
        /// and the final row data.
        /// </summary>
        /// <returns>A <see cref="GridBuilder{T}"/> instance that will handle building the grid data.</returns>
        protected override GridBuilder<Person> GetGridBuilder()
        {
            return new GridBuilder<Person>()
                .WithBlock( this )
                .AddField( "guid", p => p.Guid.ToString() )
                .AddTextField( "nickName", p => p.NickName )
                .AddTextField( "lastName", p => p.LastName )
                .AddTextField( "photoUrl", p => p.PhotoUrl )
                .AddTextField( "email", p => p.Email )
                .AddField( "isEmailActive", p => p.IsEmailActive )
                .AddDateTimeField( "birthDate", p => p.BirthDate )
                .AddField( "campus", p => p.PrimaryCampus?.Name )
                .AddField( "connectionStatus", p => GetConnectionStatus( p ) )
                .AddField( "age", p => p.Age )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <summary>
        /// Gets the connection status as a <see cref="ListItemBag"/>. This
        /// is a special format used by the Label column to handle the color.
        /// </summary>
        /// <param name="person">The person whose connection status we want.</param>
        /// <returns>A <see cref="ListItemBag"/> that represents the connection status.</returns>
        private ListItemBag GetConnectionStatus( Person person )
        {
            if ( !person.ConnectionStatusValueId.HasValue )
            {
                return null;
            }

            var connectionStatus = DefinedValueCache.Get( person.ConnectionStatusValueId.Value );

            if ( connectionStatus == null )
            {
                return null;
            }

            var color = connectionStatus.GetAttributeValue( "Color" );

            return new ListItemBag
            {
                Value = color.IsNullOrWhiteSpace() ? "#c3c3c3" : color,
                Text = connectionStatus.Value
            };
        }

        #endregion
    }
}
