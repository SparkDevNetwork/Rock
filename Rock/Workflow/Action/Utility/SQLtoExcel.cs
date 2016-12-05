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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.Data;
using Rock.Workflow;
using Rock.Web.UI.Controls;
using Rock;
using System.Linq;
using Rock.Utility;

namespace Rock.Workflow.Action
{
    [ActionCategory( "Utility" )]
    [Description( "Runs the specified SQL query to perform an action against the database." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "SQL to Excel" )]
    [CodeEditorField( "SQLQuery", "The SQL query to run. <span class='tip tip-lava'></span>", CodeEditorMode.Sql, CodeEditorTheme.Rock, 400, true, "", "", 0 )]
    [TextField( "Worksheet Name(s)", "Comma separated list of names to use on each tab", required: false, order: 1, key: "WorksheetNames" )]
    [WorkflowAttribute( "File Name Attribute", "The attribute that contains the filename to save to.", true, "", "", 2 )]
    [WorkflowAttribute( "Redirect Location Attribute", "The attribute that contains the URL where the binary file can be found.", false, "", "", 3 )]
    [BooleanField( "Continue On Error", "Should processing continue even if SQL Error occurs?", false, "", 4 )]
    public class SQLtoExcel : ActionComponent
    {
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

            try
            {
                var fileName = GetFileName( action, rockContext );
                var sqlResults = GetDatSet( action );
                var excel = ExcelHelper.CreateNewFile( sqlResults, fileName.Replace( ".xlsx", "" ) );
                var binaryFile = ExcelHelper.Save( excel, fileName, rockContext );
                SetRedirectLocationAttribute( action, binaryFile, rockContext );

                return true;
            }
            catch ( Exception ex )
            {
                action.AddLogEntry( ex.Message, true );

                if ( !GetAttributeValue( action, "ContinueOnError" ).AsBoolean() )
                {
                    errorMessages.Add( ex.Message );
                    return false;
                }

                return true;
            }
        }

        private string GetFileName( WorkflowAction action, RockContext rockContext )
        {
            var fileName = "File.xlsx";
            Guid? attributeGuid = GetAttributeValue( action, "FileNameAttribute" ).AsGuidOrNull();
            if ( attributeGuid.HasValue )
            {
                var attribute = AttributeCache.Read( attributeGuid.Value, rockContext );
                if ( attribute != null )
                {
                    if ( attribute.EntityTypeId == new Rock.Model.Workflow().TypeId )
                    {
                        fileName = action.Activity.Workflow.GetAttributeValue( attribute.Key );
                    }
                    else if ( attribute.EntityTypeId == new Rock.Model.WorkflowActivity().TypeId )
                    {
                        fileName = action.Activity.GetAttributeValue( attribute.Key );
                    }
                }
            }
            if ( !fileName.EndsWith( ".xlsx" ) )
            {
                fileName += ".xlsx";
            }
            fileName = fileName.MakeValidFileName();
            return fileName;
        }

        private DataSet GetDatSet( WorkflowAction action )
        {
            var query = GetAttributeValue( action, "SQLQuery" );
            var mergeFields = GetMergeFields( action );
            query = query.ResolveMergeFields( mergeFields );

            var sqlResults = DbService.GetDataSet( query, CommandType.Text, null );

            int worksheets = 0;
            var worksheetNames = GetAttributeValue( action, "WorksheetNames" ).Split( ',' ).Select( x => x.Trim() ).ToList();
            foreach ( DataTable data in sqlResults.Tables )
            {
                worksheets++;
                data.TableName = ( worksheets <= worksheetNames.Count && !string.IsNullOrEmpty( worksheetNames[worksheets - 1] ) ) ? worksheetNames[worksheets - 1] : $"Sheet {worksheets}";
            }

            action.AddLogEntry( "SQL query has been run" );
            return sqlResults;
        }

        private void SetRedirectLocationAttribute( WorkflowAction action, BinaryFile binaryFile, RockContext rockContext )
        {
            string redirectUrl = $"{binaryFile.Path.Replace( "~", "" )}&attachment=true";

            var attributeGuid = GetAttributeValue( action, "RedirectLocationAttribute" ).AsGuidOrNull();
            if ( attributeGuid.HasValue )
            {
                var attribute = AttributeCache.Read( attributeGuid.Value, rockContext );
                if ( attribute != null )
                {
                    if ( attribute.EntityTypeId == new Rock.Model.Workflow().TypeId )
                    {
                        action.Activity.Workflow.SetAttributeValue( attribute.Key, redirectUrl );
                        action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, redirectUrl ) );
                    }
                    else if ( attribute.EntityTypeId == new Rock.Model.WorkflowActivity().TypeId )
                    {
                        action.Activity.SetAttributeValue( attribute.Key, redirectUrl );
                        action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, redirectUrl ) );
                    }
                }
            }
        }
    }
}
