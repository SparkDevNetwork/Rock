// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Parse Zebra Label
    /// </summary>
    [Description( "Parses an uploaded Zebra Label for any available merge codes" )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Parse Zebra Label")]
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

                StringBuilder sb = new StringBuilder();

                var contentString = binaryFile.ContentsToString();
                foreach ( Match match in Regex.Matches( 
                    contentString,
                    @"(?<=\^FD)[^\^FS]*(?=\^FS)" ) )
                {
                    sb.AppendFormat( "{0}^|", match.Value );
                }

                binaryFile.LoadAttributes();

                var attributeValue = new AttributeValue();
                attributeValue.Value = sb.ToString();

                binaryFile.AttributeValues["MergeCodes"] = attributeValue;
                binaryFile.SaveAttributeValues( rockContext );
            }
            
            return true;
        }
    }
}