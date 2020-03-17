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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;


namespace com.bemaservices.WorkflowExtensions.Workflow.Action
{
    [ActionCategory( "BEMA Services > Workflow Extensions" )]
    [Description( "Adds an item to a specified Attribute Matrix with the specified values." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Attribute Matrix Item Add" )]

    [WorkflowAttribute( "Matrix", "The Matrix to add the item to", true, "", "", 0, "TargetMatrix", new string[] { "Rock.Field.Types.MatrixFieldType" } )]
    [MatrixField( "BF2EE993-AC09-4EDA-95D8-7E3757CB8A46", "Attribute Item Values", "", false, "", 1, "ItemMatrix" )]
    public class AddItemToMatrix : ActionComponent
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
            var attributeMatrixItemService = new AttributeMatrixItemService( rockContext );
            var attributeMatrixService = new AttributeMatrixService( rockContext );
            AttributeMatrix targetMatrix = null;

            // Get Target Matrix
            var targetMatrixAttributeGuid = GetAttributeValue( action, "TargetMatrix" ).AsGuidOrNull();
            if ( targetMatrixAttributeGuid.HasValue )
            {
                var targetMatrixAttribute = AttributeCache.Get( targetMatrixAttributeGuid.Value );
                if ( targetMatrixAttribute != null )
                {
                    var targetMatrixGuid = action.GetWorklowAttributeValue( targetMatrixAttributeGuid.Value ).AsGuidOrNull();
                    if ( !targetMatrixGuid.HasValue )
                    {
                        var templateQualifier = targetMatrixAttribute.QualifierValues.Where( aq => aq.Key == "attributematrixtemplate" ).FirstOrDefault();
                        if ( targetMatrixAttribute.QualifierValues.ContainsKey( "attributematrixtemplate" )
                            && templateQualifier.Value != null
                            && templateQualifier.Value.Value != null
                            && templateQualifier.Value.Value.ToString().AsIntegerOrNull() != null )
                        {
                            // Create the AttributeMatrix now and save it even though they haven't hit save yet. We'll need the AttributeMatrix record to exist so that we can add AttributeMatrixItems to it
                            // If this ends up creating an orphan, we can clean up it up later
                            targetMatrix = new AttributeMatrix { Guid = Guid.NewGuid() };
                            targetMatrix.AttributeMatrixTemplateId = templateQualifier.Value.Value.ToString().AsIntegerOrNull().Value;
                            targetMatrix.AttributeMatrixItems = new List<AttributeMatrixItem>();
                            attributeMatrixService.Add( targetMatrix );
                            SetWorkflowAttributeValue( action, targetMatrixAttribute.Guid, targetMatrix.Guid.ToString() );
                            rockContext.SaveChanges();
                        }
                        else
                        {
                            errorMessages.Add( "Matrix specified in attribute does not exist, and no default settings exist to create one. Please select a default AttributeMatrixTemplate in the workflow settings." );
                            return false;
                        }
                    }
                    else
                    {
                        targetMatrix = attributeMatrixService.Get( targetMatrixGuid.Value );
                    }

                    if ( targetMatrix != null )
                    {
                        var newMatrixItem = new AttributeMatrixItem();
                        newMatrixItem.AttributeMatrix = targetMatrix;
                        newMatrixItem.AttributeMatrixId = targetMatrix.Id;
                        newMatrixItem.LoadAttributes();

                        // Get Matrix with new Matrix Item
                        var attributeMatrixGuid = GetAttributeValue( action, "ItemMatrix" ).AsGuid();
                        var attributeMatrix = attributeMatrixService.Get( attributeMatrixGuid );
                        if ( attributeMatrix != null )
                        {
                            foreach ( AttributeMatrixItem attributeMatrixItem in attributeMatrix.AttributeMatrixItems )
                            {
                                attributeMatrixItem.LoadAttributes();

                                string columnKey = attributeMatrixItem.GetMatrixAttributeValue( action, "ColumnKey", true ).ResolveMergeFields( GetMergeFields( action ) );
                                if ( newMatrixItem.Attributes.ContainsKey( columnKey ) )
                                {
                                    string columnValue = attributeMatrixItem.GetMatrixAttributeValue( action, "ColumnValue", true ).ResolveMergeFields( GetMergeFields( action ) );
                                    newMatrixItem.SetAttributeValue( columnKey, columnValue );
                                }
                            }
                        }

                        if ( newMatrixItem.AttributeValues.Where( av => av.Value.Value.IsNotNullOrWhiteSpace() ).Any() )
                        {
                            attributeMatrixItemService.Add( newMatrixItem );
                            rockContext.SaveChanges();
                            newMatrixItem.SaveAttributeValues();
                        }

                    }

                }
            }

            return true;
        }

    }
}