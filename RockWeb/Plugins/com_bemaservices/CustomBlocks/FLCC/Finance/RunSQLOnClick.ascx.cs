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
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Extension;
using Rock.Model;
using Rock.Web.UI;
using Rock.Data;
using Rock.Web;
using Rock.Model;
using System.IO;
using Rock.Attribute;
using Rock.Web.UI.Controls;
using System.Data;

namespace RockWeb.Plugins.com_bemadev.Finance
{
    [DisplayName( "Run SQL OnClick" )]
    [Category( "com_bemadev > Finance" )]
    [Description( "Specify SQL in Block Attribute. OnClicking button, it is run." )]
    [CodeEditorField( "Query", "The query to execute. Note that if you are providing SQL you can add items from the query string using Lava like {{ QueryParmName }}.", CodeEditorMode.Sql, CodeEditorTheme.Rock, 400, false, "" )]
    [IntegerField("Query Timeout", "Amount of time before query times out.",false, 180)]
    [TextField("Button Title", "Text you would like displayed on button (THIS MUST BE UNIQUE)", false, "Run Code")]
    [TextField("Success Text", "Text that is displayed when query is successful.", true, "Run Code button was clicked.")]
    [IntegerField("Times Executable in 1 Minute", "Amount of times button can be clicked in 1 minute.", true, 60)]
    public partial class RunSQLOnClick : RockBlock
    {
        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Setting button title
            var buttonTitle = GetAttributeValue( "ButtonTitle" );
            lbRunSQL.Text = "<i class='fa fa-share-square-o'></i> " + buttonTitle;

            /*
            var currentLog = GetAttributeValue( "LogAttribute" ).ToStringSafe();
            string script = string.Format( "$( '.statusUpdates' ).append(\"{0}\");", currentLog );
            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "Alert", script, true );
            */

            this.AddConfigurationUpdateTrigger( upnlRunSQL );
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
        }
        #endregion

        #region Method
        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private int GetData( out string errorMessage )
        {
            errorMessage = string.Empty;

            string query = GetAttributeValue( "Query" );
            if ( !string.IsNullOrWhiteSpace( query ) )
            {
                try
                {
                    var mergeFields = GetDynamicDataMergeFields();

                    foreach ( var pageParam in PageParameters() )
                    {
                        mergeFields.AddOrReplace( pageParam.Key, pageParam.Value );
                    }

                    query = query.ResolveMergeFields( mergeFields );

                    int timeout = GetAttributeValue( "QueryTimeout" ).AsInteger();

                    return DbService.ExecuteCommand(query, CommandType.Text, null, timeout );
                }
                catch ( System.Exception ex )
                {
					HttpContext context2 = HttpContext.Current;
					ExceptionLogService.LogException( ex, context2 );
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets the dynamic data merge fields.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetDynamicDataMergeFields()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            if ( CurrentPerson != null )
            {
                // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
                mergeFields.Add( "Person", CurrentPerson );
            }

            mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
            mergeFields.Add( "CurrentPage", this.PageCache );

            return mergeFields;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the Click event of the lbRunSQL control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRunSQL_Click( object sender, EventArgs e )
        {
            // Creating unique Id
            var uniqueName = GetAttributeValue( "ButtonTitle" ).Replace( " ", "_" );

            // Getting timeout time from DB
            if( !string.IsNullOrEmpty( GetUserPreference( "com.bemadev.RunSQLOnClick." + uniqueName ) ) )
            {
                DateTime timeoutTime = Convert.ToDateTime( GetUserPreference( "com.bemadev.RunSQLOnClick." + uniqueName ) );

                // Checking if require time has passed
                if ( DateTime.Now <= timeoutTime )
                {
                    var text = GetAttributeValue( "ButtonTitle" ) + " was just clicked. You must wait " + 60 / GetAttributeValue( "TimesExecutablein1Minute" ).AsInteger() + " seconds before clicking it again.";
                    var script = "alert('" + text + "');";
                    ScriptManager.RegisterStartupScript( Page, Page.GetType(), "Alert", script, true );
                    return;
                }
            }

            // Getting data
            string errorMessage = string.Empty;
            var rowCount = GetData( out errorMessage );

            // Checking if query errored
            if( !string.IsNullOrEmpty( errorMessage ) )
            {
                var script = "alert('An error occurred. Please check exception log');";
				ScriptManager.RegisterStartupScript( Page, Page.GetType(), "Alert", script, true );
				return;
            }

            // Getting Success message
            var successText = GetAttributeValue( "SuccessText" );
            var offset = 60 / GetAttributeValue( "TimesExecutablein1Minute" ).AsInteger();
            var dateTime = DateTime.Now.AddSeconds( offset ).ToString();

            // Setting timestamp
            SetUserPreference( "com.bemadev.RunSQLOnClick." + uniqueName, dateTime );

            var scriptMessage = "alert('" + successText + "');";
            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "Alert", scriptMessage, true );
            return;           
        }
        #endregion
    }
}
