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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.FollowingByEntityLava;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Takes an entity type and displays a person's following items for that entity using a Lava template.
    /// </summary>
    [DisplayName( "Following By Entity" )]
    [Category( "Core" )]
    [Description( "Takes an entity type and displays a person's following items for that entity using a Lava template." )]

    [EntityTypeField( "Entity Type",
        Description = "The type of entity to show following for.",
        Order = 0,
        Key = AttributeKey.EntityType )]

    [TextField( "Link URL",
        Description = "The address to use for the link. The text '[Id]' will be replaced with the Id of the entity '[Guid]' will be replaced with the Guid.",
        IsRequired = false,
        DefaultValue = "/samplepage/[Id]",
        Order = 1,
        Key = AttributeKey.LinkUrl )]

    [CodeEditorField( "Lava Template",
        Description = "Lava template to use to display content.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"<div class=""panel panel-block"">
<div class=""panel-heading"">
    <h4 class=""panel-title"">Followed {{ EntityType | Pluralize }}</h4>
</div>
<div class=""panel-body"">
    <ul>
    {% for item in FollowingItems %}
        {% if EntityType == 'Person' %}
            {% assign itemName = item.FullName %}
        {% else %}
            {% assign itemName = item.Name %}
        {% endif %}
        {% if LinkUrl != '' %}
            <li><a href=""{{ LinkUrl | Replace:'[Id]',item.Id }}"">{{ itemName }}</a>
            <a class=""pull-right"" href=""#"" data-action=""delete-following"" data-entity-id=""{{ item.Id }}"">
            <i class=""ti ti-x""></i>
            </a></li>
        {% else %}
            <li>{{ itemName }}
            <a class=""pull-right"" href=""#"" data-action=""delete-following"" data-entity-id=""{{ item.Id }}"">
            <i class=""ti ti-x""></i>
            </a></li>
        {% endif %}
    {% endfor %}

    {% if HasMore %}
        <li><i class='ti ti-fw'></i> <small>(showing top {{ Quantity }})</small></li>
    {% endif %}
    </ul>
</div>
</div>",
        Order = 2,
        Key = AttributeKey.LavaTemplate )]

    [IntegerField( "Max Results",
        Description = "The maximum number of results to display.",
        IsRequired = true,
        DefaultValue = "100",
        Order = 4,
        Key = AttributeKey.MaxResults )]

    [Rock.SystemGuid.EntityTypeGuid( "440A6471-9554-468E-8025-6F33ECBDCBDB" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "BD3AA05E-DC6D-49B0-8389-242F6773B046" )]
    [Rock.SystemGuid.BlockTypeGuid( "36B56055-7AA2-4169-82DD-CCFBD2C7B4CC" )]
    public class FollowingByEntityLava : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string EntityType = "EntityType";
            public const string LinkUrl = "LinkUrl";
            public const string MaxResults = "MaxResults";
            public const string LavaTemplate = "LavaTemplate";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<FollowingByEntityLavaBag, FollowingByEntityLavaOptionsBag>();

            box.Bag = new FollowingByEntityLavaBag
            {
                Content = RenderLavaContent()
            };

            return box;
        }

        /// <summary>
        /// Renders the Lava template with the current person's followed items.
        /// </summary>
        /// <returns>The rendered HTML content string.</returns>
        private string RenderLavaContent()
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return "<div class='alert alert-warning'>You must be logged in to view followed items.</div>";
            }

            var entityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull();

            if ( !entityTypeGuid.HasValue )
            {
                return "<div class='alert alert-warning'>Please configure an entity in the block settings.</div>";
            }

            var entityType = EntityTypeCache.Get( entityTypeGuid.Value );

            if ( entityType == null )
            {
                return "<div class='alert alert-warning'>Please configure an entity in the block settings.</div>";
            }

            var mergeFields = RequestContext.GetCommonMergeFields();

            var personId = RequestContext.CurrentPerson.Id;

            var followingService = new FollowingService( RockContext );
            var qryFollowedItems = followingService.GetFollowedItems( entityType.Id, personId );

            int quantity = GetAttributeValue( AttributeKey.MaxResults ).AsInteger();
            var items = qryFollowedItems.Take( quantity + 1 ).Distinct().ToList();

            bool hasMore = quantity < items.Count;

            mergeFields.Add( "FollowingItems", items.Take( quantity ) );
            mergeFields.Add( "HasMore", hasMore );
            mergeFields.Add( "EntityType", entityType.FriendlyName );
            mergeFields.Add( "LinkUrl", GetAttributeValue( AttributeKey.LinkUrl ) );
            mergeFields.Add( "Quantity", quantity );
            mergeFields.Add( "BlockId", BlockId );

            string template = GetAttributeValue( AttributeKey.LavaTemplate );
            return template.ResolveMergeFields( mergeFields );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes a following for the specified entity and returns the re-rendered content.
        /// </summary>
        /// <param name="entityId">The identifier of the entity to unfollow.</param>
        /// <returns>A bag containing the updated rendered HTML content.</returns>
        [BlockAction]
        public BlockActionResult DeleteFollowing( int entityId )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionBadRequest( "You must be logged in to unfollow items." );
            }

            var entityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull();

            if ( !entityTypeGuid.HasValue )
            {
                return ActionBadRequest( "Entity type is not configured." );
            }

            var entityType = EntityTypeCache.Get( entityTypeGuid.Value );

            if ( entityType == null )
            {
                return ActionBadRequest( "Entity type is not configured." );
            }

            var personId = RequestContext.CurrentPerson.Id;

            var entityIds = new List<int> { entityId };

            /*
                3/27/26 - MSE

                If the entity type is Person, we need to look up the PersonAlias records
                for the given PersonId because followings for Person entities are stored
                against the PersonAlias entity type, not directly against Person.

                Reason: Person followings use PersonAlias entity type internally.
            */
            if ( entityType.Guid == Rock.SystemGuid.EntityType.PERSON.AsGuid() )
            {
                entityIds = new PersonAliasService( RockContext ).Queryable()
                    .Where( pa => pa.PersonId == entityId )
                    .Select( pa => pa.Id )
                    .ToList();

                entityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.PERSON_ALIAS.AsGuid() );
            }

            var followingService = new FollowingService( RockContext );

            var followings = followingService.Queryable()
                .Where( a => a.EntityTypeId == entityType.Id )
                .Where( a => entityIds.Contains( a.EntityId ) )
                .Where( a => string.IsNullOrEmpty( a.PurposeKey ) )
                .Where( a => a.PersonAlias.PersonId == personId )
                .ToList();

            foreach ( var following in followings )
            {
                followingService.Delete( following );
            }

            RockContext.SaveChanges();

            var bag = new FollowingByEntityLavaBag
            {
                Content = RenderLavaContent()
            };

            return ActionOk( bag );
        }

        #endregion Block Actions
    }
}
