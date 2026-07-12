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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.WorkFlow.WorkflowLaunch;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.WorkFlow
{
    /// <summary>
    /// Previews the members of an entity set and launches a workflow for each one.
    /// </summary>
    [DisplayName( "Workflow Launch" )]
    [Category( "Workflow" )]
    [Description( "Block that enables previewing an entity set and then launching a workflow for each item within the set." )]

    #region Block Attributes

    [WorkflowTypeField(
        "Workflow Types",
        Key = AttributeKey.WorkflowTypes,
        Description = "Only the selected workflow types will be shown. If left blank, any workflow type can be launched.",
        AllowMultiple = true,
        IsRequired = false,
        Order = 1 )]

    [BooleanField(
        "Allow Multiple Workflow Launches",
        Key = AttributeKey.AllowMultipleWorkflowLaunches,
        Description = "If set to yes, allows launching multiple different types of workflows. After one is launched, the block will allow the individual to select another type to be launched. This will only show if more than one type is configured.",
        DefaultBooleanValue = AttributeDefault.AllowMultipleWorkflowLaunches,
        Order = 2 )]

    [TextField(
        "Panel Title",
        Key = AttributeKey.PanelTitle,
        Description = "The title to display in the block panel.",
        DefaultValue = AttributeDefault.PanelTitle,
        Order = 3 )]

    [TextField(
        "Panel Title Icon CSS Class",
        Key = AttributeKey.PanelIcon,
        Description = "The icon to use before the panel title.",
        DefaultValue = AttributeDefault.PanelIcon,
        Order = 4 )]

    [IntegerField(
        "Default Number of Items to Show",
        Key = AttributeKey.DefaultNumberOfItemsToShow,
        Description = "The number of entities to list on screen before summarizing ('...and xx more').",
        DefaultIntegerValue = AttributeDefault.DefaultNumberOfItemsToShow,
        Order = 5 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "CD1808C6-2F42-4A32-8114-CB2377BEA0C7" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "44E662E6-F9E9-49CA-91BA-57009B08BA3C" )]
    [Rock.SystemGuid.BlockTypeGuid( "D7C15C1B-7487-42C3-A485-AD154F46558A" )]
    public class WorkflowLaunch : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string WorkflowTypes = "WorkflowTypes";
            public const string AllowMultipleWorkflowLaunches = "AllowMultipleWorkflowLaunches";
            public const string PanelTitle = "PanelTitle";
            public const string PanelIcon = "PanelIcon";
            public const string DefaultNumberOfItemsToShow = "DefaultNumberOfItemsToShow";
        }

        private static class AttributeDefault
        {
            public const bool AllowMultipleWorkflowLaunches = true;
            public const string PanelTitle = "Workflow Launch";
            public const string PanelIcon = "ti ti-settings";
            public const int DefaultNumberOfItemsToShow = 50;
        }

        private static class PageParameterKey
        {
            public const string EntitySetId = "EntitySetId";
            public const string WorkflowTypeId = "WorkflowTypeId";
            public const string BypassConfirm = "BypassConfirm";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<WorkflowLaunchBag, WorkflowLaunchOptionsBag>();

            box.Options = GetBoxOptions();

            var entitySetId = PageParameter( PageParameterKey.EntitySetId ).AsInteger();

            if ( entitySetId == 0 )
            {
                box.Bag = new WorkflowLaunchBag { ErrorMessage = "An entity set id is required" };
                return box;
            }

            var query = GetEntitySetQuery( entitySetId, out var entityTypeCache );

            if ( query == null || entityTypeCache == null )
            {
                box.Bag = new WorkflowLaunchBag { ErrorMessage = "A valid entity set is required" };
                return box;
            }

            var limit = GetAttributeValue( AttributeKey.DefaultNumberOfItemsToShow ).AsIntegerOrNull()
                ?? AttributeDefault.DefaultNumberOfItemsToShow;

            box.Bag = new WorkflowLaunchBag
            {
                EntityTypeName = entityTypeCache.FriendlyName.Pluralize(),
                TotalItemCount = query.Count(),
                Items = BuildItems( query.Take( limit ), entityTypeCache )
            };

            PopulateWorkflowTypeSelection( box );

            // A bypass-confirm request launches immediately and renders in the launched state.
            if ( PageParameter( PageParameterKey.BypassConfirm ).AsBoolean()
                && PageParameter( PageParameterKey.WorkflowTypeId ).IsNotNullOrWhiteSpace() )
            {
                if ( TryLaunch( null, out var successMessage, out _ ) )
                {
                    box.Bag.HasLaunched = true;
                    box.Bag.SuccessMessage = successMessage;
                }
            }

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the block.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private WorkflowLaunchOptionsBag GetBoxOptions()
        {
            var panelTitle = GetAttributeValue( AttributeKey.PanelTitle );
            var panelIcon = GetAttributeValue( AttributeKey.PanelIcon );

            var options = new WorkflowLaunchOptionsBag
            {
                PanelTitle = panelTitle.IsNullOrWhiteSpace() ? AttributeDefault.PanelTitle : panelTitle,
                PanelIconCssClass = panelIcon.IsNullOrWhiteSpace() ? AttributeDefault.PanelIcon : panelIcon,
                AllowMultipleWorkflowLaunches = GetAttributeValue( AttributeKey.AllowMultipleWorkflowLaunches ).AsBooleanOrNull()
                    ?? AttributeDefault.AllowMultipleWorkflowLaunches
            };

            return options;
        }

        /// <summary>
        /// Populates the workflow type selection on the box: a locked type name when the
        /// selection is fixed, or a list of options when the individual chooses from two or
        /// more configured types. Neither is set when any workflow type may be launched.
        /// </summary>
        /// <param name="box">The block initialization box.</param>
        private void PopulateWorkflowTypeSelection( CustomBlockBox<WorkflowLaunchBag, WorkflowLaunchOptionsBag> box )
        {
            // A workflow type page parameter overrides the configuration when it can be viewed.
            var workflowTypeFromParameter = GetWorkflowTypeFromParameter();

            if ( workflowTypeFromParameter != null && workflowTypeFromParameter.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                box.Bag.LockedWorkflowTypeName = workflowTypeFromParameter.Name;
                return;
            }

            var configuredWorkflowTypes = GetConfiguredWorkflowTypes();

            if ( configuredWorkflowTypes.Count == 1 )
            {
                box.Bag.LockedWorkflowTypeName = configuredWorkflowTypes[0].Name;
            }
            else if ( configuredWorkflowTypes.Count >= 2 )
            {
                box.Options.WorkflowTypeOptions = configuredWorkflowTypes
                    .Select( workflowType => new ListItemBag { Value = workflowType.Guid.ToString(), Text = workflowType.Name } )
                    .ToList();
            }
        }

        /// <summary>
        /// Builds the preview items for the given entity query, rendering each entity in the
        /// most human-friendly way available for its type.
        /// </summary>
        /// <param name="query">The entity query to materialize.</param>
        /// <param name="entityTypeCache">The entity type of the entity set.</param>
        /// <returns>A list of preview items.</returns>
        private List<WorkflowLaunchItemBag> BuildItems( IQueryable<IEntity> query, EntityTypeCache entityTypeCache )
        {
            var entityType = entityTypeCache.GetEntityType();
            var hasName = entityType.GetProperty( "Name" ) != null;
            var hasTitle = entityType.GetProperty( "Title" ) != null;

            /*
                The rendering branch is chosen before iterating so the type checks run once
                rather than per entity. This block can display thousands of entities.
            */

            // Person and Group both have a useful ToString implementation.
            if ( entityTypeCache.Id == EntityTypeCache.Get<Person>().Id || entityTypeCache.Id == EntityTypeCache.Get<Model.Group>().Id )
            {
                return query.ToList()
                    .Select( entity => new WorkflowLaunchItemBag { Text = entity.ToString() } )
                    .ToList();
            }

            // Group members show both the person and the group.
            if ( entityTypeCache.Id == EntityTypeCache.Get<GroupMember>().Id )
            {
                return query.Include( "Person" ).Include( "Group" ).ToList()
                    .Select( entity => new WorkflowLaunchItemBag
                    {
                        Text = ( ( GroupMember ) entity ).Person.ToStringSafe(),
                        SubText = ( ( GroupMember ) entity ).Group.ToStringSafe()
                    } )
                    .ToList();
            }

            // Connection requests show both the person and the opportunity.
            if ( entityTypeCache.Id == EntityTypeCache.Get<ConnectionRequest>().Id )
            {
                return query.Include( "PersonAlias.Person" ).Include( "ConnectionOpportunity" ).ToList()
                    .Select( entity => new WorkflowLaunchItemBag
                    {
                        Text = ( ( ConnectionRequest ) entity ).PersonAlias?.Person.ToStringSafe(),
                        SubText = ( ( ConnectionRequest ) entity ).ConnectionOpportunity.ToStringSafe()
                    } )
                    .ToList();
            }

            // Prefer a Name or Title property, falling back to the entity type and id.
            if ( hasName || hasTitle )
            {
                return query.ToList()
                    .Select( entity => new WorkflowLaunchItemBag
                    {
                        Text = ( ( hasName ? entity.GetPropertyValue( "Name" ) : null )
                            ?? ( hasTitle ? entity.GetPropertyValue( "Title" ) : null )
                            ?? $"{entityTypeCache.FriendlyName} Id: {entity.Id}" ).ToStringSafe()
                    } )
                    .ToList();
            }

            // A mapped Person navigation property shows the person with the entity id beneath.
            var personProperty = entityType.GetProperty( "Person" );

            if ( personProperty != null && personProperty.GetCustomAttribute( typeof( NotMappedAttribute ) ) == null )
            {
                return query.Include( "Person" ).ToList()
                    .Select( entity => new WorkflowLaunchItemBag
                    {
                        Text = entity.GetPropertyValue( "Person" ).ToStringSafe(),
                        SubText = $"{entityTypeCache.FriendlyName} Id: {entity.Id}"
                    } )
                    .ToList();
            }

            // A mapped PersonAlias navigation property shows the person with the entity id beneath.
            var personAliasProperty = entityType.GetProperty( "PersonAlias" );

            if ( personAliasProperty != null && personAliasProperty.GetCustomAttribute( typeof( NotMappedAttribute ) ) == null )
            {
                return query.Include( "PersonAlias.Person" ).ToList()
                    .Select( entity => new WorkflowLaunchItemBag
                    {
                        Text = ( ( PersonAlias ) entity.GetPropertyValue( "PersonAlias" ) )?.Person.ToStringSafe(),
                        SubText = $"{entityTypeCache.FriendlyName} Id: {entity.Id}"
                    } )
                    .ToList();
            }

            // Nothing better is available, so use the entity type name and id.
            return query.ToList()
                .Select( entity => new WorkflowLaunchItemBag { Text = $"{entityTypeCache.FriendlyName} Id: {entity.Id}" } )
                .ToList();
        }

        /// <summary>
        /// Launches a workflow for each item in the entity set when a workflow type can be resolved.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type selected by the individual, if any.</param>
        /// <param name="successMessage">The success message to display when the launch succeeds.</param>
        /// <param name="errorMessage">The error message to display when the launch fails.</param>
        /// <returns><c>true</c> when the workflows were launched; otherwise <c>false</c>.</returns>
        private bool TryLaunch( string workflowTypeGuid, out string successMessage, out string errorMessage )
        {
            successMessage = null;
            errorMessage = null;

            var workflowType = ResolveWorkflowType( workflowTypeGuid );

            if ( workflowType == null )
            {
                errorMessage = "Please select a workflow type.";
                return false;
            }

            var entitySetId = PageParameter( PageParameterKey.EntitySetId ).AsInteger();

            // Every page parameter is carried forward as a workflow attribute value.
            var workflowAttributeValues = RequestContext.GetPageParameters()
                .ToDictionary( parameter => parameter.Key, parameter => parameter.Value );

            new EntitySetService( RockContext )
                .LaunchWorkflows( entitySetId, workflowType.Id, RequestContext.CurrentPerson?.PrimaryAliasId, workflowAttributeValues );

            var entityTypeCache = GetEntityTypeCache( entitySetId );
            var entityLabel = entityTypeCache?.FriendlyName.Pluralize() ?? "entities";
            successMessage = $"A new {workflowType.Name} workflow is being launched for each of the {entityLabel} above.";
            return true;
        }

        /// <summary>
        /// Resolves the workflow type to launch using, in order of precedence, the workflow type
        /// page parameter, a single configured type, then the individual's selection.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type selected by the individual, if any.</param>
        /// <returns>The resolved workflow type, or <c>null</c> when none can be determined.</returns>
        private WorkflowTypeCache ResolveWorkflowType( string workflowTypeGuid )
        {
            // A workflow type page parameter overrides everything else, so when one is present its
            // resolved value is returned even when null (an unresolved value fails the launch).
            if ( PageParameter( PageParameterKey.WorkflowTypeId ).IsNotNullOrWhiteSpace() )
            {
                return GetWorkflowTypeFromParameter();
            }

            var configuredWorkflowTypes = GetConfiguredWorkflowTypes();

            if ( configuredWorkflowTypes.Count == 1 )
            {
                return configuredWorkflowTypes[0];
            }

            var selectedGuid = workflowTypeGuid.AsGuidOrNull();

            if ( !selectedGuid.HasValue )
            {
                return null;
            }

            // When the block restricts the available workflow types, only a configured type may be launched.
            if ( configuredWorkflowTypes.Count >= 2
                && !configuredWorkflowTypes.Any( workflowType => workflowType.Guid == selectedGuid.Value ) )
            {
                return null;
            }

            return WorkflowTypeCache.Get( selectedGuid.Value );
        }

        /// <summary>
        /// Gets the workflow types configured in the block settings.
        /// </summary>
        /// <returns>The configured workflow types.</returns>
        private List<WorkflowTypeCache> GetConfiguredWorkflowTypes()
        {
            return GetAttributeValues( AttributeKey.WorkflowTypes )
                .Select( value => value.AsGuidOrNull() )
                .Where( guid => guid.HasValue )
                .Select( guid => WorkflowTypeCache.Get( guid.Value ) )
                .Where( workflowType => workflowType != null )
                .ToList();
        }

        /// <summary>
        /// Gets the workflow type identified by the page parameter, or <c>null</c> when the
        /// parameter is absent or does not resolve.
        /// </summary>
        /// <returns>The workflow type from the page parameter, or <c>null</c>.</returns>
        private WorkflowTypeCache GetWorkflowTypeFromParameter()
        {
            return WorkflowTypeCache.Get( PageParameter( PageParameterKey.WorkflowTypeId ), !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets the entity query for the entity set along with its entity type.
        /// </summary>
        /// <param name="entitySetId">The entity set identifier.</param>
        /// <param name="entityTypeCache">The resolved entity type, or <c>null</c> when the set is invalid.</param>
        /// <returns>The entity query, or <c>null</c> when the set is missing or has no entity type.</returns>
        private IQueryable<IEntity> GetEntitySetQuery( int entitySetId, out EntityTypeCache entityTypeCache )
        {
            entityTypeCache = GetEntityTypeCache( entitySetId );

            if ( entityTypeCache == null )
            {
                return null;
            }

            return new EntitySetService( RockContext ).GetEntityQuery( entitySetId )?.AsNoTracking();
        }

        /// <summary>
        /// Gets the entity type of the entity set.
        /// </summary>
        /// <param name="entitySetId">The entity set identifier.</param>
        /// <returns>The entity type, or <c>null</c> when the set is missing or has no entity type.</returns>
        private EntityTypeCache GetEntityTypeCache( int entitySetId )
        {
            var entityTypeId = new EntitySetService( RockContext ).Get( entitySetId )?.EntityTypeId;
            return entityTypeId.HasValue ? EntityTypeCache.Get( entityTypeId.Value ) : null;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Launches a workflow for each item in the entity set.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type selected by the individual, if any.</param>
        /// <returns>The success message, or a bad request when no workflow type could be resolved.</returns>
        [BlockAction]
        public BlockActionResult LaunchWorkflows( string workflowTypeGuid )
        {
            if ( TryLaunch( workflowTypeGuid, out var successMessage, out var errorMessage ) )
            {
                return ActionOk( successMessage );
            }

            return ActionBadRequest( errorMessage );
        }

        /// <summary>
        /// Gets every preview item in the entity set, without the default display limit.
        /// </summary>
        /// <returns>The full list of preview items.</returns>
        [BlockAction]
        public BlockActionResult GetAllItems()
        {
            var entitySetId = PageParameter( PageParameterKey.EntitySetId ).AsInteger();
            var query = GetEntitySetQuery( entitySetId, out var entityTypeCache );

            if ( query == null || entityTypeCache == null )
            {
                return ActionBadRequest( "A valid entity set is required" );
            }

            return ActionOk( BuildItems( query, entityTypeCache ) );
        }

        #endregion Block Actions
    }
}
