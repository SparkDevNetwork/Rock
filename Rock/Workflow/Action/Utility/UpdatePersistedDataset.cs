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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Updates persisted datasets.
    /// </summary>
    [ActionCategory( "Utility" )]
    [Description( "Updates the provided persisted dataset." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Persisted Dataset Update" )]

    [TextField( "Persisted Dataset Access Key",
        Description = "The access key to the persisted dataset that should be updated <span class='tip tip-lava'></span>",
        Key = AttributeKey.PersistedDataAccessKey,
        IsRequired = true,
        Order = 0 )]

    [BooleanField( "Delay Processing Until Complete",
        Key = AttributeKey.DelayProcessingUntilComplete,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Description = "When checked the action will wait until the persisted dataset is done being updated. Otherwise it will trigger the update to begin then continue before it's complete",
        Order = 1 )]

    [Rock.SystemGuid.EntityTypeGuid( "E827D5B7-8BC7-422B-8C35-19E774ACD4FA" )]
    public class UpdatePersistedDataset : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string PersistedDataAccessKey = "PersistedDataAccessKey";
            public const string DelayProcessingUntilComplete = "DelayProcessingUntilComplete";
        }

        #endregion

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var service = new PersistedDatasetService( rockContext );

            // Get the merge fields
            var mergeFields = GetMergeFields( action );

            var dataSetKey = GetAttributeValue( action, AttributeKey.PersistedDataAccessKey, true ).ResolveMergeFields( mergeFields );

            var dataset = service.Queryable().FirstOrDefault(d => d.AccessKey == dataSetKey );

            if ( dataset == null )
            {
                var errorMessage = $"Unable to find PersistedDataset with key {dataSetKey}";
                action.AddLogEntry( errorMessage );
                errorMessages.Add( errorMessage );
                return false;
            }

            var delayProcessingUntilComplete = GetAttributeValue( action, AttributeKey.DelayProcessingUntilComplete, true ).AsBoolean();

            if ( delayProcessingUntilComplete )
            {
                try
                {
                    dataset.UpdateResultData();
                    rockContext.SaveChanges();
                    action.AddLogEntry( $"Updated {dataset.Name}" );
                }
                catch ( System.Exception ex )
                {
                    action.AddLogEntry( ex.Message, true );
                    errorMessages.Add( ex.Message );
                    return false;
                }
            }
            else
            {
                var message = new Rock.Tasks.UpdatePersistedDataset.Message()
                {
                    AccessKey = dataSetKey
                };
                message.Send();
                action.AddLogEntry( $"Updating {dataset.Name}." );
            }

            return true;
        }
    }
}
