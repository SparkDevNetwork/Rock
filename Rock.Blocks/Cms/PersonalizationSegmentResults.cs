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
using Rock.ViewModels.Blocks.Cms.PersonalizationSegmentResults;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of people.
    /// </summary>

    [DisplayName( "Personalization Segment Results" )]
    [Category( "CMS" )]
    [Description( "Block that lists Known Individuals for a given Personalization Segment." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "3d9b1436-3022-4875-9346-020317aee3b5" )]
    //WAS [Rock.SystemGuid.BlockTypeGuid( "b286e17f-1faa-48c4-8a0b-035018207212" )]
    [Rock.SystemGuid.BlockTypeGuid( "438432E3-22A8-43D9-9F06-179C3B65D298" )]
    [CustomizedGrid]
    public class PersonalizationSegmentResults : RockEntityListBlockType<Person>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string PersonalizationSegmentGuid = "PersonalizationSegmentGuid";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersonalizationSegmentResultsOptionsBag>();
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
        private PersonalizationSegmentResultsOptionsBag GetBoxOptions()
        {
            var options = new PersonalizationSegmentResultsOptionsBag();
            var segment = PersonalizationSegmentCache.Get( PageParameter( PageParameterKey.PersonalizationSegmentGuid ).AsGuid() );
            options.SegmentName = segment?.Name;
            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<Person> GetListQueryable( RockContext rockContext )
        {
            var personalizationSegmentGuid = PageParameter( PageParameterKey.PersonalizationSegmentGuid ).AsGuid();
            var personalizationSegment = PersonalizationSegmentCache.Get( personalizationSegmentGuid );

            var personService = new PersonService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );
            var personalizationSegmentService = new PersonalizationSegmentService( rockContext );

            if ( personalizationSegment == null )
            {
                return Enumerable.Empty<Person>().AsQueryable();
            }

            // Person Aliases in segment
            var segmentAliasQry = personalizationSegmentService.GetPersonAliasPersonalizationSegmentQuery( personalizationSegment );

            // Select unique person Ids from the Aliases found in the segment
            var personIdsQry = segmentAliasQry.Select( pa => pa.PersonAlias.PersonId ).Distinct();

            // Get each person from the unique Ids
            var personQry = new PersonService( rockContext ).Queryable()
                .Where( p => personIdsQry.Contains( p.Id ) )
                .Include( p => p.ConnectionStatusValue )
                .Include( p => p.RecordStatusValue );

            return personQry;
        }

        /// <inheritdoc/>
        protected override GridBuilder<Person> GetGridBuilder()
        {
            return new GridBuilder<Person>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.FullName )
                .AddTextField( "connectionStatus", a => a.ConnectionStatusValue?.Value )
                .AddTextField( "gender", a => a.Gender.ToString() )
                .AddTextField( "ageClassification", a => a.AgeClassification.ToString() )
                .AddTextField( "recordStatus", a => a.RecordStatusValue?.Value )
                .AddAttributeFields( GetGridAttributes() );
        }

        #endregion

        #region Block Actions

        #endregion
    }
}

