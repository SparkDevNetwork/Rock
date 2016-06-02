// <copyright>
// Copyright by Central Christian Church
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

using LumenWorks.Framework.IO.Csv;

using System.IO;
using Rock.Workflow;
namespace com.centralaz.DpsMatch.Workflow.Action
{
    /// <summary>
    /// Imports offenders from a CSV file into our custom _com_centralaz_DpsMatch_Offender table.  Uses the custom stored procedure called _com_centralaz_spDpsMatch_Offender.
    /// </summary>
    [ActionCategory( "com_centralaz: Dept Public Safety" )]
    [Description( "Imports offenders from a CSV file into our custom _com_centralaz_DpsMatch_Offender table.  Uses the custom stored procedure called _com_centralaz_spDpsMatch_Offender." )]
    [Export( typeof( Rock.Workflow.ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Import Offenders CSV" )]
    public class ImportOffenders : Rock.Workflow.ActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            //Get the excel file
            Guid dpsFileGuid = action.Activity.Workflow.GetAttributeValue( "DPSFile" ).AsGuid();
            BinaryFile binaryFile = new BinaryFileService( new RockContext() ).Get( dpsFileGuid );

            //For each row in excel file
            Dictionary<string, object> parameters;
            using ( CsvReader csvReader = new CsvReader( new StreamReader( binaryFile.ContentStream ), true ) )
            {
                while ( csvReader.ReadNextRecord() )
                {
                    try
                    {
                        if ( !string.IsNullOrWhiteSpace( csvReader[csvReader.GetFieldIndex( "Last_Name" )] ) )
                        {
                            //Build new SO Object
                            parameters = new Dictionary<string, object>();
                            parameters.Add( "LastName", csvReader[csvReader.GetFieldIndex( "Last_Name" )] );
                            parameters.Add( "FirstName", csvReader[csvReader.GetFieldIndex( "First_Name" )] );
                            if ( !string.IsNullOrWhiteSpace( csvReader[csvReader.GetFieldIndex( "MI" )] ) )
                            {
                                String middleName = csvReader[csvReader.GetFieldIndex( "MI" )];
                                char middleInitial = middleName.ElementAt( 0 );
                                parameters.Add( "MiddleInitial", middleInitial );
                            }
                            else
                            {
                                parameters.Add( "MiddleInitial", DBNull.Value );
                            }
                            parameters.Add( "Age", csvReader[csvReader.GetFieldIndex( "Age" )].AsInteger() );
                            parameters.Add( "Height", csvReader[csvReader.GetFieldIndex( "HT" )].AsInteger() );
                            parameters.Add( "Weight", csvReader[csvReader.GetFieldIndex( "WT" )].AsInteger() );
                            parameters.Add( "Race", csvReader[csvReader.GetFieldIndex( "Race" )] );
                            parameters.Add( "Sex", csvReader[csvReader.GetFieldIndex( "Sex" )] );
                            parameters.Add( "Hair", csvReader[csvReader.GetFieldIndex( "Hair" )] );
                            parameters.Add( "Eyes", csvReader[csvReader.GetFieldIndex( "Eyes" )] );

                            parameters.Add( "ResidentialAddress", csvReader[csvReader.GetFieldIndex( "Res_Add" )] );
                            parameters.Add( "ResidentialCity", csvReader[csvReader.GetFieldIndex( "Res_City" )] );
                            parameters.Add( "ResidentialState", csvReader[csvReader.GetFieldIndex( "Res_State" )] );
                            parameters.Add( "ResidentialZip", csvReader[csvReader.GetFieldIndex( "Res_Zip" )].AsInteger() );

                            if ( csvReader.GetFieldIndex( "Verification Date" ) != -1 && !string.IsNullOrWhiteSpace( csvReader[csvReader.GetFieldIndex( "Verification Date" )] ) && csvReader[csvReader.GetFieldIndex( "Verification Date" )].AsDateTime().HasValue )
                            {
                                parameters.Add( "VerificationDate", csvReader[csvReader.GetFieldIndex( "Verification Date" )].AsDateTime().Value );
                            }
                            else
                            {
                                parameters.Add( "VerificationDate", DBNull.Value );
                            }
                            parameters.Add( "Offense", csvReader[csvReader.GetFieldIndex( "Offense" )] );
                            parameters.Add( "OffenseLevel", csvReader[csvReader.GetFieldIndex( "Level" )].AsInteger() );
                            parameters.Add( "Absconder", csvReader[csvReader.GetFieldIndex( "Absconder" )].AsBoolean() );
                            parameters.Add( "ConvictingJurisdiction", csvReader[csvReader.GetFieldIndex( "Conviction_State" )] );
                            if ( csvReader.GetFieldIndex( "Verification Date" ) != -1 && !string.IsNullOrWhiteSpace( csvReader[csvReader.GetFieldIndex( "Unverified" )] ) )
                            {
                                parameters.Add( "Unverified", csvReader[csvReader.GetFieldIndex( "Unverified" )].AsBoolean() );
                            }
                            else
                            {
                                parameters.Add( "Unverified", DBNull.Value );
                            }
                            parameters.Add( "KeyString", String.Format( "{0}{1}{2}{3}{4}{5}{6}", parameters["LastName"], parameters["FirstName"], parameters["Race"], parameters["Sex"], parameters["Hair"], parameters["Eyes"], parameters["ResidentialZip"] ) );

                            DbService.ExecuteCommand( "_com_centralaz_spDpsMatch_Offender", System.Data.CommandType.StoredProcedure, parameters );
                        }
                    }
                    catch ( Exception e )
                    {
                        ExceptionLogService.LogException( e, null );
                    }
                }
            }
            return true;
        }
    }
}
