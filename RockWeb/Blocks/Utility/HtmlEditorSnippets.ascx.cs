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
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "HtmlEditor Snippets" )]
    [Category( "Utility" )]
    [Description( "Block to be used as part of the Snippets HtmlEditor Plugin" )]

    #region Block Attributes
    [LinkedPage(
        "Edit Page",
        Description = "The page for editing HTML snippets.",
        IsRequired = true,
        Key = AttributeKey.EditPage
        )]
    #endregion
    public partial class HtmlEditorSnippets : RockBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
            /// <summary>
            /// The edit page attribute key
            /// </summary>
            public const string EditPage = "EditPage";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        protected static class PageParameterKey
        {
            /// <summary>
            /// The modal mode page parameter key
            /// </summary>
            public const string ModalMode = "ModalMode";

            /// <summary>
            /// The title page parameter key
            /// </summary>
            public const string Title = "Title";
        }
        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gSnippets.Actions.ShowAdd = true;
            gSnippets.Actions.ShowExcelExport = false;
            gSnippets.Actions.ShowMergeTemplate = false;
            gSnippets.Actions.AddClick += Actions_AddClick;

            var securityField = gSnippets.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( HtmlContent ) ).Id;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !this.IsPostBack )
            {
                pnlModalHeader.Visible = PageParameter( PageParameterKey.ModalMode ).AsBoolean();

                if ( pnlModalHeader.Visible )
                {
                    //In modal mode the admin footer gets in the way.
                    System.Web.UI.HtmlControls.HtmlGenericControl hideFooterStyle;
                    hideFooterStyle = new System.Web.UI.HtmlControls.HtmlGenericControl();
                    hideFooterStyle.TagName = "style";
                    string css = "#cms-admin-footer { visibility: hidden; }";
                    hideFooterStyle.InnerText = css;
                    Page.Header.Controls.Add( hideFooterStyle );
                }

                pnlModalFooterActions.Visible = PageParameter( PageParameterKey.ModalMode ).AsBoolean();
                lTitle.Text = PageParameter( PageParameterKey.Title );
                BindGrid();
            }
        }

        /// <summary>Binds the grid.</summary>
        protected void BindGrid()
        {
            HtmlContentService htmlContentService = new HtmlContentService( new RockContext() );

            var query = htmlContentService.Queryable().Where( hc => hc.BlockId == null && hc.CreatedByPersonAliasId.HasValue
                        && hc.CreatedByPersonAlias.PersonId == CurrentPerson.Id );
            gSnippets.DataSource = query.ToList();
            gSnippets.DataBind();
        }

        #endregion


        /// <summary>Handles the Click event of the Delete control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void Delete_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                HtmlContentService htmlContentService = new HtmlContentService( rockContext );
                var htmlContent = htmlContentService.Get( e.RowKeyId );
                if ( htmlContent != null )
                {
                    htmlContentService.Delete( htmlContent );
                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        /// <summary>Handles the AddClick event of the Actions control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Actions_AddClick( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();

            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.ModalMode ) ) )
            {
                queryParams[PageParameterKey.ModalMode] = PageParameter( PageParameterKey.ModalMode );
            }

            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.Title ) ) )
            {
                queryParams[PageParameterKey.Title] = PageParameter( PageParameterKey.Title );
            }

            NavigateToLinkedPage( "EditPage", queryParams );
        }

        /// <summary>Handles the Click event of the efEdit control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void efEdit_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var queryParams = new Dictionary<string, string>
            {
                { "htmlcontentid", e.RowKeyId.ToString() }
            };

            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.ModalMode ) ) )
            {
                queryParams[PageParameterKey.ModalMode] = PageParameter( PageParameterKey.ModalMode );
            }

            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.Title ) ) )
            {
                queryParams[PageParameterKey.Title] = PageParameter( PageParameterKey.Title );
            }

            NavigateToLinkedPage( "EditPage", queryParams );
        }

        /// <summary>Handles the Click event of the lbfCopy control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void lbfCopy_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            RockContext rockContext = new RockContext();
            HtmlContentService htmlContentService = new HtmlContentService( rockContext );
            var htmlContent = htmlContentService.Get( e.RowKeyId );

            string newName = "Copy of " + htmlContent.Name;
            int copyNum = 1;
            bool alreadyExists = true;
            while ( alreadyExists )
            {
                alreadyExists = htmlContentService.Queryable().Any( hc => hc.Name == newName && hc.CreatedByPersonAliasId == CurrentPersonAliasId );
                if ( alreadyExists )
                {
                    copyNum++;
                    newName = "Copy " + copyNum + " of " + htmlContent.Name;
                }
            }

            HtmlContent newHtmlContent = new HtmlContent();
            newHtmlContent.Id = 0;
            newHtmlContent.Name = newName;
            newHtmlContent.Content = htmlContent.Content;
            newHtmlContent.CreatedByPersonAliasId = CurrentPersonAliasId;
            newHtmlContent.Version = 0;
            htmlContentService.Add( newHtmlContent );
            rockContext.SaveChanges();

            // Rebind the Grid control
            BindGrid();
        }
    }
}