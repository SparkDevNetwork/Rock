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
using System.Data.Entity.Core;
using System.Linq;
using System.Reflection;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Reporting.MetricValueDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Displays the details of a particular metric value.
    /// </summary>

    [DisplayName( "Metric Value Detail" )]
    [Category( "Reporting" )]
    [Description( "Displays the details of a particular metric value." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "af69aa1a-3eee-4f25-8014-1a02ba82ac32" )]
    [Rock.SystemGuid.BlockTypeGuid( "b52e7cae-c5cc-41cb-a5ec-1cf027074a2c" )]
    public class MetricValueDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string MetricId = "MetricId";
            public const string MetricValueId = "MetricValueId";
            public const string MetricCategoryId = "MetricCategoryId";
            public const string ExpandedIds = "ExpandedIds";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<MetricValueBag, MetricValueDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<MetricValue>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private MetricValueDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new MetricValueDetailOptionsBag();
            options.MetricValueTypes = typeof( MetricValueType ).ToEnumListItemBag();
            return options;
        }

        /// <summary>
        /// Validates the MetricValue for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="metricValue">The MetricValue to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the MetricValue is valid, <c>false</c> otherwise.</returns>
        private bool ValidateMetricValue( MetricValue metricValue, out string errorMessage )
        {
            errorMessage = null;

            foreach ( var metricPartition in metricValue.Metric.MetricPartitions )
            {
                var metricPartitionEntityType = EntityTypeCache.Get( metricPartition.EntityTypeId ?? 0 );
                var metricValuePartition = metricValue.MetricValuePartitions.FirstOrDefault( a => a.MetricPartitionId == metricPartition.Id );

                if ( metricPartition.IsRequired && metricPartitionEntityType != null && metricValuePartition?.EntityId.HasValue != true )
                {
                    errorMessage = $"A value for {metricPartition.Label ?? metricPartitionEntityType.FriendlyName} is required";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<MetricValueBag, MetricValueDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {MetricValue.FriendlyTypeName} was not found.";
                return;
            }

            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            // New entity is being created, prepare for edit mode by default.
            if ( box.IsEditable )
            {
                box.Entity = GetEntityBag( entity, rockContext );
                box.SecurityGrantToken = GetSecurityGrantToken( entity );
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( MetricValue.FriendlyTypeName );
            }
        }

        private List<MetricValuePartitionBag> ToPublicAttributeBag( MetricValue metricValue, RockContext rockContext )
        {
            List<MetricValuePartitionBag> attributeBags = new List<MetricValuePartitionBag>();

            if ( metricValue?.Metric != null )
            {
                foreach ( var metricPartition in metricValue.Metric.MetricPartitions )
                {
                    if ( metricPartition.EntityTypeId.HasValue )
                    {
                        var entityTypeCache = EntityTypeCache.Get( metricPartition.EntityTypeId.Value );
                        if ( entityTypeCache?.SingleValueFieldType != null )
                        {
                            var fieldType = entityTypeCache.SingleValueFieldType;
                            var metricValuePartition = metricValue.MetricValuePartitions.FirstOrDefault( a => a.MetricPartitionId == metricPartition.Id );

                            Dictionary<string, Rock.Field.ConfigurationValue> configurationValues;
                            if ( fieldType.Field is IEntityQualifierFieldType entityQualifierFieldType )
                            {
                                configurationValues = entityQualifierFieldType.GetConfigurationValuesFromEntityQualifier( metricPartition.EntityTypeQualifierColumn, metricPartition.EntityTypeQualifierValue );
                            }
                            else
                            {
                                configurationValues = new Dictionary<string, ConfigurationValue>();
                            }

                            var privateConfigurationValues = configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value );
                            var publicConfigurationValues = fieldType.Field.GetPublicConfigurationValues( privateConfigurationValues, ConfigurationValueUsage.Edit, metricValuePartition?.EntityId?.ToString() );
                            var entity = GetEntity( metricValuePartition?.EntityId, entityTypeCache.GetEntityType(), rockContext );

                            var attributeBag = new MetricValuePartitionBag
                            {
                                Attribute = new PublicAttributeBag()
                                {
                                    IsRequired = metricPartition.IsRequired,
                                    Name = metricPartition.Label,
                                    FieldTypeGuid = fieldType.Guid,
                                    ConfigurationValues = publicConfigurationValues,
                                },
                                Value = entity == null ? "" : fieldType.Field.GetPublicEditValue( entity.Guid.ToStringSafe(), privateConfigurationValues ),
                                MetricPartitionGuid = metricPartition.Guid,
                            };

                            attributeBags.Add( attributeBag );
                        }
                    }
                }
            }

            return attributeBags;
        }

        /// <summary>
        /// Gets the metric partition entity.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private IEntity GetEntity( int? entityId, Type entityType, RockContext rockContext )
        {
            if ( entityId.HasValue )
            {
                var methodParamTypes = new Type[] { typeof( int ) };
                var parameters = new object[] { entityId };
                MethodInfo getMethod;
                IService entityService;

                if ( entityType == typeof( Person ) )
                {
                    entityService = new PersonAliasService( rockContext );
                    getMethod = entityService.GetType().GetMethod( "GetByAliasId", methodParamTypes );
                }
                else
                {
                    entityService = Reflection.GetServiceForEntityType( entityType, rockContext );
                    getMethod = entityService.GetType().GetMethod( "Get", methodParamTypes );
                }

                return ( IEntity ) getMethod.Invoke( entityService, parameters );
            }

            return null;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="MetricValueBag"/> that represents the entity.</returns>
        private MetricValueBag GetEntityBag( MetricValue entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new MetricValueBag
            {
                IdKey = entity.IdKey,
                MetricValuePartitions = ToPublicAttributeBag( entity, rockContext ),
                MetricValueType = ( int ) entity.MetricValueType,
                Note = entity.Note,
                XValue = entity.XValue,
                YValue = entity.YValue,
                MetricValueDateTime = entity.MetricValueDateTime
            };

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( MetricValue entity, DetailBlockBox<MetricValueBag, MetricValueDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.MetricValueType ),
                () => entity.MetricValueType = ( MetricValueType ) box.Entity.MetricValueType );

            box.IfValidProperty( nameof( box.Entity.Note ),
                () => entity.Note = box.Entity.Note );

            box.IfValidProperty( nameof( box.Entity.XValue ),
                () => entity.XValue = box.Entity.XValue );

            box.IfValidProperty( nameof( box.Entity.YValue ),
                () => entity.YValue = box.Entity.YValue );

            box.IfValidProperty( nameof( box.Entity.MetricValuePartitions ),
                () => SaveMetricPartitionValues( entity, box.Entity, rockContext ) );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Saves the metric partition values.
        /// </summary>
        /// <param name="metricValue">The metric value.</param>
        /// <param name="bag">The metric value details from the client.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void SaveMetricPartitionValues( MetricValue metricValue, MetricValueBag bag, RockContext rockContext )
        {
            foreach ( var metricPartition in metricValue.Metric.MetricPartitions )
            {
                var entityTypeCache = EntityTypeCache.Get( metricPartition.EntityTypeId ?? 0 );
                var metricValuePartitionBag = bag.MetricValuePartitions.Find( a => a.MetricPartitionGuid == metricPartition.Guid );
                var metricValuePartition = metricValue.MetricValuePartitions.FirstOrDefault( a => a.MetricPartitionId == metricPartition.Id );

                if ( metricValuePartition == null )
                {
                    metricValuePartition = new MetricValuePartition();
                    metricValuePartition.MetricPartitionId = metricPartition.Id;
                    metricValue.MetricValuePartitions.Add( metricValuePartition );
                }

                if ( entityTypeCache?.SingleValueFieldType != null && entityTypeCache.SingleValueFieldType.Field is IEntityFieldType entityFieldType )
                {
                    var fieldType = entityTypeCache.SingleValueFieldType;
                    Dictionary<string, Rock.Field.ConfigurationValue> configurationValues;
                    if ( fieldType.Field is IEntityQualifierFieldType )
                    {
                        configurationValues = ( fieldType.Field as IEntityQualifierFieldType ).GetConfigurationValuesFromEntityQualifier( metricPartition.EntityTypeQualifierColumn, metricPartition.EntityTypeQualifierValue );
                    }
                    else
                    {
                        configurationValues = new Dictionary<string, ConfigurationValue>();
                    }

                    var privateConfigurationValues = configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value );
                    var editValue = fieldType.Field.GetPrivateEditValue( metricValuePartitionBag?.Value, privateConfigurationValues );
                    var entity = entityFieldType.GetEntity( editValue, rockContext );
                    metricValuePartition.EntityId = entity?.Id;
                }
                else
                {
                    metricValuePartition.EntityId = null;
                }
            }
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="MetricValue"/> to be viewed or edited on the page.</returns>
        private MetricValue GetInitialEntity( RockContext rockContext )
        {
            var entity = GetInitialEntity<MetricValue, MetricValueService>( rockContext, PageParameterKey.MetricValueId );

            if ( entity.Id == 0 )
            {
                int? metricId = PageParameter( PageParameterKey.MetricId ).AsIntegerOrNull();
                int? metricCategoryId = PageParameter( PageParameterKey.MetricCategoryId ).AsIntegerOrNull();

                if ( metricCategoryId > 0 )
                {
                    // editing a metric, but get the metricId from the metricCategory
                    var metricCategory = new MetricCategoryService( rockContext ).Get( metricCategoryId.Value );
                    if ( metricCategory != null )
                    {
                        entity.MetricId = metricCategory.MetricId;
                        entity.Metric = metricCategory.Metric;
                    }
                }
                else if ( metricId > 0 )
                {
                    var metric = new MetricService( rockContext ).Get( metricId.Value );
                    if ( metric != null )
                    {
                        entity.MetricId = metric.Id;
                        entity.Metric = metric;
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var qryParams = GetQueryParams();

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( qryParams )
            };
        }

        private Dictionary<string, string> GetQueryParams()
        {
            var qryParams = new Dictionary<string, string>();
            var metricId = RequestContext.GetPageParameter( PageParameterKey.MetricValueId );
            var metricCategoryId = RequestContext.GetPageParameter( PageParameterKey.MetricCategoryId );
            var expandedIds = RequestContext.GetPageParameter( PageParameterKey.ExpandedIds );

            if ( !string.IsNullOrWhiteSpace( metricId ) )
            {
                qryParams.Add( PageParameterKey.MetricId, metricId );
            }

            if ( !string.IsNullOrWhiteSpace( metricCategoryId ) )
            {
                qryParams.Add( PageParameterKey.MetricCategoryId, metricCategoryId );
            }

            if ( !string.IsNullOrWhiteSpace( expandedIds ) )
            {
                qryParams.Add( PageParameterKey.ExpandedIds, expandedIds );
            }

            return qryParams;
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( MetricValue entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out MetricValue entity, out BlockActionResult error )
        {
            var entityService = new MetricValueService( rockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new MetricValue();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{MetricValue.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${MetricValue.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<MetricValueBag, MetricValueDetailOptionsBag>
                {
                    Entity = GetEntityBag( entity, rockContext )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<MetricValueBag, MetricValueDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateMetricValue( entity, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                return ActionOk( this.GetParentPageUrl( GetQueryParams() ) );
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new MetricValueService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<MetricValueBag, MetricValueDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<MetricValueBag, MetricValueDetailOptionsBag>
                {
                    Entity = GetEntityBag( entity, rockContext )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        #endregion
    }
}
