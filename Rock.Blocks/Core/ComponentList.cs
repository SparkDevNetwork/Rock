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
using System.Linq;
using System.Reflection;

using Rock.Attribute;
using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.ComponentList;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of MEF plugin components and allows editing their attributes.
    /// </summary>

    [DisplayName( "Component List" )]
    [Category( "Core" )]
    [Description( "Block to administrate MEF plugins." )]
    [IconCssClass( "ti ti-settings" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [TextField( "Component Container",
        Description = "The Rock Extension Managed Component Container to manage. For example: 'Rock.Search.SearchContainer, Rock'",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.ComponentContainer )]

    [BooleanField( "Support Ordering",
        Description = "Should user be allowed to re-order list of components?",
        DefaultValue = "true",
        Order = 2,
        Key = AttributeKey.SupportOrdering )]

    [BooleanField( "Support Security",
        Description = "Should the user be allowed to configure security for the components?",
        DefaultValue = "true",
        Order = 3,
        Key = AttributeKey.SupportSecurity )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "2C8F24A1-E651-4AFF-907B-BE3A15F80E72" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "6B0E81E7-E26E-4601-A1A3-B350CA0B35AB" )]
    [Rock.SystemGuid.BlockTypeGuid( "21F5F466-59BC-40B2-8D73-7314D936C3CB" )]
    public class ComponentList : RockListBlockType<ComponentListRowBag>
    {
        #region Keys

        /// <summary>
        /// Block attribute keys.
        /// </summary>
        private static class AttributeKey
        {
            public const string ComponentContainer = "ComponentContainer";
            public const string SupportOrdering = "SupportOrdering";
            public const string SupportSecurity = "SupportSecurity";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ComponentListOptionsBag>();
            var builder = GetGridBuilder();

            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options bag.</returns>
        private ComponentListOptionsBag GetBoxOptions()
        {
            var isAuthorizedToConfigure = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );

            return new ComponentListOptionsBag
            {
                IsSupportOrdering = GetAttributeValue( AttributeKey.SupportOrdering ).AsBoolean( true ) && isAuthorizedToConfigure,
                IsSupportSecurity = GetAttributeValue( AttributeKey.SupportSecurity ).AsBoolean( true ) && isAuthorizedToConfigure
            };
        }

        /// <summary>
        /// Resolves the MEF container from the ComponentContainer block attribute.
        /// </summary>
        /// <returns>The container instance, or <c>null</c> if it could not be resolved.</returns>
        private Rock.Extension.IContainer GetContainer()
        {
            var containerTypeName = GetAttributeValue( AttributeKey.ComponentContainer );
            if ( containerTypeName.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var containerType = Type.GetType( containerTypeName );
            if ( containerType == null )
            {
                return null;
            }

            var instanceProperty = containerType.GetProperty( "Instance" );
            if ( instanceProperty == null )
            {
                return null;
            }

            return instanceProperty.GetValue( null, null ) as Rock.Extension.IContainer;
        }

        /// <summary>
        /// Gets the components from the container, ensuring attribute definitions
        /// are up to date. The list is ordered by the "Order" attribute when
        /// ordering is supported, or by name otherwise.
        /// </summary>
        /// <param name="container">The MEF container.</param>
        /// <param name="rockContext">The Rock context.</param>
        /// <returns>The ordered list of component descriptions.</returns>
        private List<ComponentDescription> GetComponentDescriptions( Rock.Extension.IContainer container, RockContext rockContext )
        {
            var supportOrdering = GetAttributeValue( AttributeKey.SupportOrdering ).AsBoolean( true );

            // When ordering is not supported, sort by name up-front.
            var entries = supportOrdering
                ? container.Dictionary.ToList()
                : container.Dictionary.OrderBy( c => c.Value.Key ).ToList();

            var descriptions = new List<ComponentDescription>();

            foreach ( var entry in entries )
            {
                var type = entry.Value.Value.GetType();

                // Ensure any newly added attributes are registered.
                if ( Rock.Attribute.Helper.UpdateAttributes( type, EntityTypeCache.GetId( type.FullName ), string.Empty, string.Empty, rockContext ) )
                {
                    entry.Value.Value.LoadAttributes( rockContext );
                }

                descriptions.Add( new ComponentDescription( entry.Key, entry.Value ) );
            }

            // Apply a deterministic sort by Order then Name to break ties regardless
            // of MEF discovery order, and to incorporate any Order values that
            // LoadAttributes may have updated during the loop above.
            if ( supportOrdering )
            {
                descriptions = descriptions.OrderBy( d => d.Order ).ThenBy( d => d.Name ).ToList();
            }

            return descriptions;
        }

        /// <inheritdoc/>
        protected override IQueryable<ComponentListRowBag> GetListQueryable( RockContext rockContext )
        {
            var container = GetContainer();
            if ( container == null )
            {
                return Enumerable.Empty<ComponentListRowBag>().AsQueryable();
            }

            var componentDescriptions = GetComponentDescriptions( container, rockContext );

            var rows = componentDescriptions.Select( d =>
            {
                var entityType = EntityTypeCache.Get( d.Type );
                return new ComponentListRowBag
                {
                    Guid = entityType?.Guid ?? Guid.Empty,
                    Name = d.Name,
                    Description = d.Description,
                    IsActive = d.IsActive,
                    EntityTypeId = entityType?.Id ?? 0
                };
            } );

            return rows.AsQueryable();
        }

        /// <inheritdoc/>
        protected override GridBuilder<ComponentListRowBag> GetGridBuilder()
        {
            return new GridBuilder<ComponentListRowBag>()
                .WithBlock( this )
                .AddTextField( "guid", a => a.Guid.ToString() )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "description", a => a.Description )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "entityTypeId", a => a.EntityTypeId );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the component's public attributes for editing in the modal.
        /// </summary>
        /// <param name="key">The EntityType GUID of the component.</param>
        /// <returns>A <see cref="ComponentListEditBag"/> with attribute data.</returns>
        [BlockAction]
        public BlockActionResult GetComponentAttributes( string key )
        {
            if ( !Guid.TryParse( key, out var entityTypeGuid ) || entityTypeGuid == Guid.Empty )
            {
                return ActionBadRequest( "Invalid component key." );
            }

            var container = GetContainer();
            if ( container == null )
            {
                return ActionNotFound( "Component not found." );
            }

            var entry = container.Dictionary
                .Select( c => new
                {
                    Name = c.Value.Key,
                    Component = c.Value.Value
                } )
                .FirstOrDefault( c => EntityTypeCache.Get( c.Component.GetType() )?.Guid == entityTypeGuid );

            if ( entry == null )
            {
                return ActionNotFound( "Component not found." );
            }

            entry.Component.LoadAttributes( RockContext );

            // Exclude the "Order" attribute from the edit UI.
            bool ExcludeOrderAttribute( Web.Cache.AttributeCache attr ) => attr.Key != "Order";

            var currentPerson = RequestContext.CurrentPerson;

            var editBag = new ComponentListEditBag
            {
                Name = entry.Name,
                /*
                    3/11/2026 - MSE

                    Attribute-level security is not enforced here. Access to this action relies on
                    page-level security. Enforcing attribute-level security would exclude users who
                    lack explicit permissions on individual attributes, preventing them from seeing
                    and editing certain fields.

                    Reason: Avoid hiding editable fields from users who lack attribute-level permissions.
                */
                Attributes = entry.Component.GetPublicAttributesForEdit( currentPerson, enforceSecurity: false, attributeFilter: ExcludeOrderAttribute ),
                AttributeValues = entry.Component.GetPublicAttributeValuesForEdit( currentPerson, enforceSecurity: false, attributeFilter: ExcludeOrderAttribute ),
                IsSmtpTransport = entry.Component is Rock.Communication.Transport.SMTP
            };

            return ActionOk( editBag );
        }

        /// <summary>
        /// Saves the edited attribute values for a component.
        /// </summary>
        /// <param name="key">The EntityType GUID of the component.</param>
        /// <param name="attributeValues">The updated attribute values.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult SaveComponentAttributes( string key, Dictionary<string, string> attributeValues )
        {
            if ( !Guid.TryParse( key, out var entityTypeGuid ) || entityTypeGuid == Guid.Empty )
            {
                return ActionBadRequest( "Invalid component key." );
            }

            var container = GetContainer();
            if ( container == null )
            {
                return ActionNotFound( "Component not found." );
            }

            var component = container.Dictionary.Values
                .Select( kvp => kvp.Value )
                .FirstOrDefault( c => EntityTypeCache.Get( c.GetType() )?.Guid == entityTypeGuid );

            if ( component == null )
            {
                return ActionNotFound( "Component not found." );
            }

            component.LoadAttributes( RockContext );

            // Apply the public attribute values from the client.
            component.SetPublicAttributeValues( attributeValues, RequestContext.CurrentPerson, enforceSecurity: false );

            // Validate attribute values before saving.
            if ( !component.ValidateAttributeValues( out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            component.SaveAttributeValues();

            return ActionOk();
        }

        /// <summary>
        /// Reorders the components in the container. The component identified
        /// by <paramref name="key"/> is moved to be immediately before the
        /// component identified by <paramref name="beforeKey"/>. If
        /// <paramref name="beforeKey"/> is null the item is moved to the end.
        /// </summary>
        /// <param name="key">The EntityType GUID of the component to move.</param>
        /// <param name="beforeKey">The EntityType GUID of the component to insert before, or <c>null</c> to move to the end.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            var container = GetContainer();
            if ( container == null )
            {
                return ActionBadRequest( "Could not resolve component container." );
            }

            var itemEntityTypeGuid = key.AsGuidOrNull();
            if ( !itemEntityTypeGuid.HasValue )
            {
                return ActionBadRequest( "Invalid component key." );
            }

            // Sort by Order then Name — the same criteria used when displaying the grid —
            // so that positional indices here exactly match what the user saw.
            var components = container.Dictionary
                .OrderBy( c => c.Value.Value.Order )
                .ThenBy( c => c.Value.Key )
                .ToList();
            var movedIndex = components.FindIndex( c => EntityTypeCache.Get( c.Value.Value.GetType() )?.Guid == itemEntityTypeGuid.Value );
            if ( movedIndex < 0 )
            {
                return ActionBadRequest( "Component not found in container." );
            }

            var movedItem = components[movedIndex];
            components.RemoveAt( movedIndex );

            if ( beforeKey.IsNotNullOrWhiteSpace() )
            {
                var beforeEntityTypeGuid = beforeKey.AsGuidOrNull();
                var beforeIndex = beforeEntityTypeGuid.HasValue
                    ? components.FindIndex( c => EntityTypeCache.Get( c.Value.Value.GetType() )?.Guid == beforeEntityTypeGuid.Value )
                    : -1;

                if ( beforeIndex < 0 )
                {
                    return ActionBadRequest( "Before component not found in container." );
                }

                components.Insert( beforeIndex, movedItem );
            }
            else
            {
                components.Add( movedItem );
            }

            // Persist the new order by updating each component's "Order" attribute value.
            var order = 0;
            foreach ( var item in components )
            {
                var component = item.Value.Value;
                if ( component.Attributes != null && component.Attributes.ContainsKey( "Order" ) )
                {
                    Rock.Attribute.Helper.SaveAttributeValue( component, component.Attributes["Order"], order.ToString(), RockContext );
                }

                order++;
            }

            container.Refresh();

            return ActionOk();
        }

        #endregion Block Actions
    }
}
