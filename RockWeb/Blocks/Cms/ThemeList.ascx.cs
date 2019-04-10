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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Web;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Block that lists themes in the theme folder
    /// </summary>
    [DisplayName( "Theme List" )]
    [Category( "CMS" )]
    [Description( "Lists themes in the Theme folder." )]

    [LinkedPage("Theme Styler Page", "Page to use for the theme styler page.")]
    public partial class ThemeList : Rock.Web.UI.RockBlock, ICustomGridColumns
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gThemes.GridRebind += gList_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gThemes.RowDataBound += gThemes_RowDataBound;
        }

        private void gThemes_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var dataItem = e.Row.DataItem;
            if ( dataItem != null )
            {
                var allowsCompile = dataItem.GetPropertyValue( "AllowsCompile" ).ToString().AsBoolean();

                if ( !allowsCompile )
                {
                    var cell = e.Row.Cells[3].Controls[0].Visible = false;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the SaveClick event of the mdThemeClone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdThemeClone_SaveClick( object sender, EventArgs e )
        {
            mdThemeClone.Hide();

            string resultMessages = string.Empty;


            var cloneWasSuccessful = RockTheme.CloneTheme( hfClonedThemeName.Value, tbNewThemeName.Text, out resultMessages );

            if ( cloneWasSuccessful )
            {
                nbMessages.NotificationBoxType = NotificationBoxType.Success;
                nbMessages.Text = string.Format( "The {0} theme has been successfully cloned to {1}.", hfClonedThemeName.Value, tbNewThemeName.Text );
                BindGrid();
            }
            else
            {
                nbMessages.NotificationBoxType = NotificationBoxType.Danger;
                nbMessages.Text = resultMessages;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the gCloneTheme control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCloneTheme_Click( object sender, RowEventArgs e )
        {
            tbNewThemeName.Text = string.Empty;
            hfClonedThemeName.Value = e.RowKeyValue.ToString();
            mdThemeClone.Show();
        }

        /// <summary>
        /// Handles the Click event of the gCompileTheme control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCompileTheme_Click( object sender, RowEventArgs e )
        {
            var theme = new RockTheme( e.RowKeyValue.ToString() );
            string messages = string.Empty;

            bool compileSuccess = theme.Compile( out messages );

            if ( compileSuccess )
            {
                mdThemeCompile.Show( "Theme was successfully compiled.", ModalAlertType.Information );
                nbMessages.Text = string.Empty;
            }
            else
            {
                nbMessages.NotificationBoxType = NotificationBoxType.Danger;
                nbMessages.Text = string.Format( "An error occurred while compiling the {0} theme.\nMessage: <pre>{1}</pre>", theme.Name, messages );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gThemes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gThemes_Delete( object sender, RowEventArgs e )
        {
            string messages = string.Empty;

            bool deleteSuccess = RockTheme.DeleteTheme( e.RowKeyValue.ToString(), out messages );

            if ( deleteSuccess )
            {
                nbMessages.NotificationBoxType = NotificationBoxType.Success;
                nbMessages.Text = string.Format( "The {0} theme has been successfully deleted.", e.RowKeyValue );
                BindGrid();
            }
            else
            {
                nbMessages.NotificationBoxType = NotificationBoxType.Danger;
                nbMessages.Text = "Could not delete theme. Message: " + messages;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            DirectoryInfo themeDirectory = new DirectoryInfo( HttpRuntime.AppDomainAppPath + "Themes" );

            var themes = RockTheme.GetThemes();

            var sortProperty = gThemes.SortProperty;

            if ( sortProperty != null )
            {
                switch ( sortProperty.Property )
                {
                    case "Name":
                        {
                            if ( sortProperty.Direction == SortDirection.Ascending )
                            {
                                themes = themes.OrderBy( t => t.Name ).ToList();
                            }
                            else
                            {
                                themes = themes.OrderByDescending( t => t.Name ).ToList();
                            }
                            break;
                        }
                }
            }

            gThemes.DataSource = themes.ToList();
            gThemes.DataBind();
        }

        #endregion

        protected void gThemes_RowSelected( object sender, RowEventArgs e )
        {
            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "EditTheme", e.RowKeyValue.ToString() );
            NavigateToLinkedPage( "ThemeStylerPage", qryParams );
        }
    }
}