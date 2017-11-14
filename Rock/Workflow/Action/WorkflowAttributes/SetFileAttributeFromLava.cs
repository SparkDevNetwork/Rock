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
using System.IO;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action.WorkflowAttributes
{
    /// <summary>
    /// Sets an attribute's value to the selected value
    /// </summary>
    [ActionCategory( "Workflow Attributes" )]
    [Description( "Takes a Lava template and renders it as a text file. The resulting output is placed into a provided workflow attribute of type file." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Attribute Set from Lava" )]

    [WorkflowAttribute( "Result Attribute", "The attribute to put the resulting file into.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.FileFieldType" } )]
    [TextField( "File Name", "The file name to use for the file.", true, "file.txt" )]
    [TextField( "Mimetype", "The mimetype of the file", true, "text/plain" )]
    [CodeEditorField( "Lava Template", "The Lava template to use to create the text file. <span class='tip tip-lava'></span>", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 500, true, order: 2, defaultValue: @"" )]
    public class SetFileAttributeFromLava : ActionComponent
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
            string filename = GetAttributeValue( action, "FileName" );
            filename = filename.ResolveMergeFields( GetMergeFields( action ) );

            if ( string.IsNullOrWhiteSpace( filename ) )
            {
                filename = "file.txt";
            }

            string mimeType = GetAttributeValue( action, "MimeType" );
            mimeType = mimeType.ResolveMergeFields( GetMergeFields( action ) );

            if ( string.IsNullOrWhiteSpace( mimeType ) )
            {
                mimeType = "text/plain";
            }

            Guid guid = GetAttributeValue( action, "ResultAttribute" ).AsGuid();
            if ( !guid.IsEmpty() )
            {
                var destinationAttribute = AttributeCache.Read( guid, rockContext );
                if ( destinationAttribute != null )
                {
                    string lavaTemplate = GetAttributeValue( action, "LavaTemplate" );

                    // determine binary file type to use for storing the file
                    Guid binaryFileTypeGuid = Guid.Empty;
                    var binaryFileTypeQualifier = destinationAttribute.QualifierValues["binaryFileType"];
                    if ( binaryFileTypeQualifier != null )
                    {
                        if ( binaryFileTypeQualifier.Value != null )
                        {
                            binaryFileTypeGuid = binaryFileTypeQualifier.Value.AsGuid();
                        }
                    }

                    if ( binaryFileTypeGuid == Guid.Empty )
                    {
                        binaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid();
                    }

                    int binaryFileTypeId = new BinaryFileTypeService( rockContext ).Get( binaryFileTypeGuid ).Id;

                    // merge lava to text content
                    var lavaOutput = lavaTemplate.ResolveMergeFields( GetMergeFields( action ) );

                    // convert lava to text
                    using ( Stream fileOutput = new MemoryStream( System.Text.Encoding.UTF8.GetBytes( lavaOutput ) ) )
                    {
                        // save output to binary file
                        var binaryFileService = new BinaryFileService( rockContext );
                        var binaryFile = new BinaryFile();
                        binaryFileService.Add( binaryFile );

                        binaryFile.BinaryFileTypeId = binaryFileTypeId;
                        binaryFile.MimeType = mimeType;
                        binaryFile.FileName = filename;
                        binaryFile.ContentStream = fileOutput;
                        rockContext.SaveChanges();

                        if ( destinationAttribute.EntityTypeId == new Rock.Model.Workflow().TypeId )
                        {
                            action.Activity.Workflow.SetAttributeValue( destinationAttribute.Key, binaryFile.Guid.ToString() );
                            action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", destinationAttribute.Name, lavaOutput ) );
                        }
                        else if ( destinationAttribute.EntityTypeId == new Rock.Model.WorkflowActivity().TypeId )
                        {
                            action.Activity.SetAttributeValue( destinationAttribute.Key, binaryFile.Guid.ToString() );
                            action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", destinationAttribute.Name, lavaOutput ) );
                        }
                    }

                }
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }
    }
}
