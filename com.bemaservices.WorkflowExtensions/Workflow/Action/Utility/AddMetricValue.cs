// <copyright>
// Copyright by BEMA Information Technologies
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
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.bemaservices.WorkflowExtensions.Workflow.Action
{
    /// <summary>
    /// Sets an entity property.
    /// </summary>
    [ActionCategory( "BEMA Services > Workflow Extensions" )]
    [Description( "Adds a metric value." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Metric Value Add" )]

    [WorkflowTextOrAttribute( "Metric Id or Guid", "Metric Attribute", "The id or guid of the Metric. <span class='tip tip-lava'></span>", true, "", "", 1, "MetricIdGuid" )]
    [EnumField( "Metric Value Type", "", typeof( MetricValueType ), true, "Measure", "", 2 )]
    [WorkflowTextOrAttribute( "Metric Value Date", "MetricValueDate Attribute", "The datetime of the Metric Value. <span class='tip tip-lava'></span>", true, "", "", 3, "MetricValueDate" )]
    [WorkflowTextOrAttribute( "YValue", "YValue Attribute", "The YValue of the Metric Value. <span class='tip tip-lava'></span>", true, "", "", 3, "YValue" )]
    [WorkflowTextOrAttribute( "Note", "Note Attribute", "The Note of the Metric Value. <span class='tip tip-lava'></span>", true, "", "", 3, "Note" )]

    [MatrixField( "b8e01e4f-1faa-4dbd-be23-664b88dc9d63", "Set Metric Value Partitions", "", false, "", 4, "Matrix" )]
    [CustomDropdownListField( "Empty Value Handling", "How to handle empty property values.", "IGNORE^Ignore empty values,EMPTY^Set to empty,NULL^Set to NULL", true, "", "", 5, "EmptyValueHandling" )]
    public class AddMetricValue : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var mergeFields = GetMergeFields( action );
            RockContext _rockContext = new RockContext();
            string metricIdGuidString = GetAttributeValue( action, "MetricIdGuid", true ).ResolveMergeFields( mergeFields ).Trim();
            DateTime? metricValueDate = GetAttributeValue( action, "MetricValueDate", true ).ResolveMergeFields( mergeFields ).Trim().AsDateTime();
            var metricValueType = GetAttributeValue( action, "MetricValueType", true ).Trim().ConvertToEnum<MetricValueType>();
            decimal? yValue = GetAttributeValue( action, "YValue", true ).ResolveMergeFields( mergeFields ).Trim().AsDecimalOrNull();
            string note = GetAttributeValue( action, "Note", true ).ResolveMergeFields( mergeFields ).Trim();


            // Get the entity
            MetricService metricService = new MetricService( _rockContext );
            MetricValueService metricValueService = new MetricValueService( _rockContext );
            EntityTypeService entityTypeService = new EntityTypeService( _rockContext );
            Metric metric = null;
            var metricGuid = metricIdGuidString.AsGuidOrNull();
            if ( metricGuid.HasValue )
            {
                metric = metricService.Get( metricGuid.Value );
            }
            else
            {
                var metricId = metricIdGuidString.AsIntegerOrNull();
                if ( metricId.HasValue )
                {
                    metric = metricService.Get( metricId.Value );
                }
            }

            if ( metric == null )
            {
                errorMessages.Add( string.Format( "Metric could not be found for selected value ('{0}')!", metricIdGuidString ) );
                return false;
            }

            if ( metricValueDate == null )
            {
                errorMessages.Add( string.Format( "MetricValueDate is in an invalid format ('{0}')!", GetAttributeValue( action, "MetricValueDate", true ).ResolveMergeFields( mergeFields ).Trim() ) );
                return false;
            }

            if ( metricValueType == null )
            {
                errorMessages.Add( string.Format( "Metric Value Type could not be found for selected value ('{0}')!", GetAttributeValue( action, "MetricValueType", true ).Trim() ) );
                return false;
            }

            if ( yValue == null )
            {
                errorMessages.Add( string.Format( "YValue is in an invalid format ('{0}')!", GetAttributeValue( action, "YValue", true ).ResolveMergeFields( mergeFields ).Trim() ) );
                return false;
            }

            var metricValue = new MetricValue();
            metricValue.MetricValueType = metricValueType;
            metricValue.MetricId = metric.Id;
            metricValue.MetricValueDateTime = metricValueDate.Value;
            metricValue.YValue = yValue;
            metricValue.Note = note;
            metricValueService.Add( metricValue );

            string emptyValueHandling = GetAttributeValue( action, "EmptyValueHandling" );
            var attributeMatrixGuid = GetAttributeValue( action, "Matrix" ).AsGuid();
            var attributeMatrix = new AttributeMatrixService( rockContext ).Get( attributeMatrixGuid );
            if ( attributeMatrix != null )
            {
                foreach ( AttributeMatrixItem attributeMatrixItem in attributeMatrix.AttributeMatrixItems )
                {
                    attributeMatrixItem.LoadAttributes();

                    string partitionLabel = attributeMatrixItem.GetMatrixAttributeValue( action, "Label", true ).ResolveMergeFields( mergeFields );
                    string entityIdGuidString = attributeMatrixItem.GetMatrixAttributeValue( action, "Value", true ).ResolveMergeFields( mergeFields ).Trim();

                    if ( emptyValueHandling == "IGNORE" && String.IsNullOrWhiteSpace( entityIdGuidString ) )
                    {
                        action.AddLogEntry( "Skipping empty value." );
                        return true;
                    }

                    var partition = metric.MetricPartitions.Where( p => p.Label == partitionLabel ).FirstOrDefault();

                    if ( partition == null )
                    {
                        errorMessages.Add( string.Format( "Partition does not exist ('{0}')!", partitionLabel ) );
                        return false;
                    }

                    if ( partition.EntityTypeId == null )
                    {
                        errorMessages.Add( "Partition does not have an entity type!" );
                        return false;
                    }

                    IEntity entityObject = null;
                    var entityGuid = entityIdGuidString.AsGuidOrNull();
                    if ( entityGuid.HasValue )
                    {
                        entityObject = entityTypeService.GetEntity( partition.EntityTypeId.Value, entityGuid.Value );
                    }
                    else
                    {
                        var entityId = entityIdGuidString.AsIntegerOrNull();
                        if ( entityId.HasValue )
                        {
                            entityObject = entityTypeService.GetEntity( partition.EntityTypeId.Value, entityId.Value );
                        }
                    }

                    if ( entityObject == null && partition.EntityType.Guid == Rock.SystemGuid.EntityType.PERSON.AsGuid() && entityGuid.HasValue )
                    {
                        var personAlias = new PersonAliasService( _rockContext ).Get( entityGuid.Value );
                        if ( personAlias != null )
                        {
                            entityObject = personAlias.Person;
                        }
                    }

                    if ( entityObject == null )
                    {
                        errorMessages.Add( string.Format( "Entity could not be found for selected value ('{0}')!", entityIdGuidString ) );
                        return false;
                    }

                    var valuePartition = new MetricValuePartition();
                    valuePartition.MetricPartitionId = partition.Id;
                    valuePartition.EntityId = entityObject.Id;
                    metricValue.MetricValuePartitions.Add( valuePartition );

                }
            }

            try
            {
                _rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                errorMessages.Add( string.Format( "Could not save metric value! {0}", ex.Message ) );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Converts a string to the specified type of object.
        /// </summary>
        /// <param name="theObject">The string to convert.</param>
        /// <param name="objectType">The type of object desired.</param>
        /// <param name="tryToNull">If empty strings should return as null.</param>
        /// <returns></returns>
        private static object ConvertObject( string theObject, Type objectType, RockContext rockContext, bool tryToNull = true )
        {
            if ( objectType.IsEnum )
            {
                return string.IsNullOrWhiteSpace( theObject ) ? null : Enum.Parse( objectType, theObject, true );
            }

            var theObjectGuid = theObject.AsGuidOrNull();
            if ( objectType.IsClass == true && theObjectGuid.HasValue )
            {
                // Get the service type corresponding to the object type
                Type serviceType = typeof( Rock.Data.Service<> );
                Type[] modelType = { objectType };
                Type service = serviceType.MakeGenericType( modelType );

                // Create new service of the above type
                var serviceInstance = Activator.CreateInstance( service, new object[] { rockContext } );

                // Find the get method for the service
                var getMethod = service.GetMethod( "Get", new Type[] { typeof( Guid ) } );

                // Call the get method to get the object
                object entity = getMethod.Invoke( serviceInstance, new object[] { theObjectGuid.Value } ) as object;

                // Return object
                return entity;
            }

            Type underType = Nullable.GetUnderlyingType( objectType );
            if ( underType == null ) // not nullable
            {
                return Convert.ChangeType( theObject, objectType );
            }

            if ( tryToNull && string.IsNullOrWhiteSpace( theObject ) )
            {
                return null;
            }
            return Convert.ChangeType( theObject, underType );
        }

    }
}
