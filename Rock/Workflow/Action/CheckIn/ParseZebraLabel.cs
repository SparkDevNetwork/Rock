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
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Parse Zebra Label
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Parses an uploaded Zebra Label for any available merge codes" )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Zebra Label Parse" )]
    public class ParseZebraLabel : ActionComponent
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

            if ( entity is Model.BinaryFile )
            {
                var binaryFile = (Model.BinaryFile) entity;
                if ( binaryFile.BinaryFileType.Guid != new Guid( SystemGuid.BinaryFiletype.CHECKIN_LABEL ) )
                {
                    errorMessages.Add( "Binary file is not a check-in label" );
                    action.AddLogEntry( "Binary file is not a check-in label", true );
                    return false;
                }

                if ( binaryFile.Attributes == null )
                {
                    binaryFile.LoadAttributes();
                }

                // Get the existing merge fields
                var existingMergeFields = new Dictionary<string, string>();
                foreach ( var keyAndVal in binaryFile.GetAttributeValue( "MergeCodes" ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    var keyVal = keyAndVal.Split( new char[] { '^' } );
                    if ( keyVal.Length == 2 )
                    {
                        existingMergeFields.AddOrIgnore( keyVal[0], keyVal[1] );
                    }
                }

                // Build new merge fields
                var newMergeFields = new List<string>();
                foreach ( Match match in Regex.Matches( binaryFile.ContentsToString(), @"(?<=\^FD)((?!\^FS).)*(?=\^FS)" ) )
                {
                    string value = existingMergeFields.ContainsKey( match.Value ) ? existingMergeFields[match.Value] : "";
                    newMergeFields.Add( string.Format( "{0}^{1}", match.Value, value ) );
                }

                // Save attribute value
                var attributeValue = new AttributeValueCache();
                attributeValue.Value = newMergeFields.AsDelimited( "|" );

                binaryFile.AttributeValues["MergeCodes"] = attributeValue;
                binaryFile.SaveAttributeValues( rockContext );
            }
            
            return true;
        }
    }
}