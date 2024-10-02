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

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "Page to use for editing next-gen themes.",
        Order = 0 )]

    [LinkedPage(
        "Theme Styler Page",
        Key = AttributeKey.ThemeStylerPage,
        Description = "Page to use for the theme styler page.",
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "FD99E0AA-E1CB-4049-A6F6-9C5F2A34F694" )]
    public partial class ThemeList : Rock.Web.UI.RockBlock, ICustomGridColumns
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ThemeStylerPage = "ThemeStylerPage";
        }

        #endregion Attribute Keys

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
                    e.Row.Cells[4].Controls[0].Visible = false;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                using ( var rockContext = new RockContext() )
                {
                    var themeService = new ThemeService( rockContext );

                    if ( themeService.UpdateThemes() )
                    {
                        rockContext.SaveChanges();
                    }
                }

                BindGrid();
            }

            base.OnLoad( e );
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
                using ( var rockContext = new RockContext() )
                {
                    if ( new ThemeService( rockContext ).UpdateThemes() )
                    {
                        rockContext.SaveChanges();
                    }
                }

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
        /// Handles the GridRebind event of the gList control.
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

            using ( var rockContext = new RockContext() )
            {
                var themeService = new ThemeService( rockContext );
                var themesToDelete = themeService.Queryable()
                    .Where( t => t.Name == e.RowKeyValue.ToString() )
                    .ToList();

                themeService.DeleteRange( themesToDelete );

                rockContext.SaveChanges();
            }

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
            var themes = GetThemes();

            var sortProperty = gThemes.SortProperty;

            if ( sortProperty != null )
            {
                themes = themes.AsQueryable().Sort( sortProperty ).ToList();
            }

            if ( themes.Any( f => f.Name == "RockOriginal" ) )
            {
                DeleteRockOriginalTheme();
            }

            gThemes.DataSource = themes.Where( f => f.Name != "RockOriginal" ).ToList();
            gThemes.DataBind();
        }

        private List<ThemePoco> GetThemes()
        {
            using ( var rockContext = new RockContext() )
            {
                var legacyThemeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.THEME_PURPOSE_WEBSITE_LEGACY.AsGuid(), rockContext ).Id;

                return new ThemeService( rockContext )
                    .Queryable()
                    .ToList()
                    .DistinctBy( t => t.Name )
                    .Select( t => new ThemePoco
                    {
                        IdKey = t.IdKey,
                        Name = t.Name,
                        Purpose = DefinedValueCache.Get( t.PurposeValueId ?? 0, rockContext )?.Value ?? string.Empty,
                        PurposeValueId = t.PurposeValueId,
                        IsActive = t.IsActive,
                        IsSystem = t.IsSystem,
                        AllowsCompile = t.PurposeValueId == legacyThemeValueId,
                    } )
                    .OrderBy( t => t.Name )
                    .ToList();
            }
        }

        private void DeleteRockOriginalTheme()
        {
            var themeDirectory = Path.Combine( HttpRuntime.AppDomainAppPath, "Themes", "RockOriginal" );

            if ( Directory.Exists( themeDirectory ) )
            {
                Directory.Delete( themeDirectory, true );
            }
        }

        #endregion

        protected void gThemes_RowSelected( object sender, RowEventArgs e )
        {
            var theme = GetThemes().Where( t => t.Name == e.RowKeyValue.ToString() ).FirstOrDefault();
            var legacyThemeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.THEME_PURPOSE_WEBSITE_LEGACY.AsGuid() ).Id;

            if ( theme != null && theme.PurposeValueId != legacyThemeValueId )
            {
                var qryParams = new Dictionary<string, string>
                {
                    { "ThemeId", theme.IdKey }
                };

                NavigateToLinkedPage( AttributeKey.DetailPage, qryParams );
            }
            else
            {
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                qryParams.Add( "EditTheme", e.RowKeyValue.ToString() );
                NavigateToLinkedPage( AttributeKey.ThemeStylerPage, qryParams );
            }
        }

        private class ThemePoco
        {
            public string IdKey { get; set; }

            public string Name { get; set; }

            public string Purpose { get; set; }

            public int? PurposeValueId { get; set; }

            public bool IsSystem { get; set; }

            public bool IsActive { get; set; }

            public bool AllowsCompile { get; set; }
        }
    }
}
