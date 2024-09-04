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

    [Rock.SystemGuid.EntityTypeGuid( "af69aa1a-3eee-4f25-8014-1a02ba82ac32" )]
    [Rock.SystemGuid.BlockTypeGuid( "b52e7cae-c5cc-41cb-a5ec-1cf027074a2c" )]
    public class MetricValueDetail : RockEntityDetailBlockType<MetricValue, MetricValueBag>
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
            var box = new DetailBlockBox<MetricValueBag, MetricValueDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private MetricValueDetailOptionsBag GetBoxOptions( bool isEditable )
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
        private void SetBoxInitialEntityState( DetailBlockBox<MetricValueBag, MetricValueDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {MetricValue.FriendlyTypeName} was not found.";
                return;
            }

            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );

            // New entity is being created, prepare for edit mode by default.
            if ( box.IsEditable )
            {
                box.Entity = GetEntityBagForEdit( entity );
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( MetricValue.FriendlyTypeName );
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Retrieves the metric value partitions as a <see cref="MetricValuePartitionBag"/>
        /// </summary>
        /// <param name="metricValue">The metric value.</param>
        /// <returns></returns>
        private List<MetricValuePartitionBag> ToMetricValuePartitionBag( MetricValue metricValue )
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
                            var entity = GetEntity( metricValuePartition?.EntityId, entityTypeCache.GetEntityType() );

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
        /// <returns></returns>
        private IEntity GetEntity( int? entityId, Type entityType )
        {
            if ( entityId.HasValue )
            {
                var methodParamTypes = new Type[] { typeof( int ) };
                var parameters = new object[] { entityId };
                MethodInfo getMethod;
                IService entityService;

                if ( entityType == typeof( Person ) )
                {
                    entityService = new PersonAliasService( RockContext );
                    getMethod = entityService.GetType().GetMethod( "GetByAliasId", methodParamTypes );
                }
                else
                {
                    entityService = Reflection.GetServiceForEntityType( entityType, RockContext );
                    getMethod = entityService.GetType().GetMethod( "Get", methodParamTypes );
                }

                return ( IEntity ) getMethod.Invoke( entityService, parameters );
            }

            return null;
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="MetricValue"/> that represents the entity.</returns>
        private MetricValueBag GetCommonEntityBag( MetricValue entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new MetricValueBag
            {
                IdKey = entity.IdKey,
                MetricValuePartitions = ToMetricValuePartitionBag( entity ),
                MetricValueType = ( int ) entity.MetricValueType,
                Note = entity.Note,
                XValue = entity.XValue,
                YValue = entity.YValue,
                MetricValueDateTime = entity.MetricValueDateTime
            };

            return bag;
        }

        /// <inheritdoc/>
        protected override MetricValueBag GetEntityBagForView( MetricValue entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override MetricValueBag GetEntityBagForEdit( MetricValue entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( MetricValue entity, ValidPropertiesBox<MetricValueBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.MetricValueType ),
                () => entity.MetricValueType = ( MetricValueType ) box.Bag.MetricValueType );

            box.IfValidProperty( nameof( box.Bag.Note ),
                () => entity.Note = box.Bag.Note );

            box.IfValidProperty( nameof( box.Bag.XValue ),
                () => entity.XValue = box.Bag.XValue );

            box.IfValidProperty( nameof( box.Bag.YValue ),
                () => entity.YValue = box.Bag.YValue );

            box.IfValidProperty( nameof( box.Bag.MetricValueDateTime ),
                () => entity.MetricValueDateTime = box.Bag.MetricValueDateTime );

            box.IfValidProperty( nameof( box.Bag.MetricValuePartitions ),
                () => SaveMetricPartitionValues( entity, box.Bag ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Saves the metric partition values.
        /// </summary>
        /// <param name="metricValue">The metric value.</param>
        /// <param name="bag">The metric value details from the client.</param>
        private void SaveMetricPartitionValues( MetricValue metricValue, MetricValueBag bag )
        {
            if ( metricValue.Metric?.MetricPartitions != null )
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
                        var entity = entityFieldType.GetEntity( editValue, RockContext );
                        metricValuePartition.EntityId = entity?.Id;
                    }
                    else
                    {
                        metricValuePartition.EntityId = null;
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override MetricValue GetInitialEntity()
        {
            var entity = GetInitialEntity<MetricValue, MetricValueService>( RockContext, PageParameterKey.MetricValueId );

            if ( entity.Id == 0 )
            {
                int? metricId = PageParameter( PageParameterKey.MetricId ).AsIntegerOrNull();
                int? metricCategoryId = PageParameter( PageParameterKey.MetricCategoryId ).AsIntegerOrNull();

                if ( metricCategoryId > 0 )
                {
                    // editing a metric, but get the metricId from the metricCategory
                    var metricCategory = new MetricCategoryService( RockContext ).Get( metricCategoryId.Value );
                    if ( metricCategory != null )
                    {
                        entity.MetricId = metricCategory.MetricId;
                        entity.Metric = metricCategory.Metric;
                    }
                }
                else if ( metricId > 0 )
                {
                    var metric = new MetricService( RockContext ).Get( metricId.Value );
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

        /// <summary>
        /// Gets the query parameters to be used for URL generations.
        /// </summary>
        /// <returns></returns>
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
        protected override bool TryGetEntityForEditAction( string idKey, out MetricValue entity, out BlockActionResult error )
        {
            var entityService = new MetricValueService( RockContext );
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
                entity = GetInitialEntity();
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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            var box = new ValidPropertiesBox<MetricValueBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            };

            return ActionOk( box );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<MetricValueBag> box )
        {
            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateMetricValue( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            return ActionOk( this.GetParentPageUrl( GetQueryParams() ) );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new MetricValueService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion
    }
}
