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
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Html Snippet Editor" )]
    [Category( "Utility" )]
    [Description( "Block to edit HTML Snippets" )]
    public partial class HtmlSnippetEditor : RockBlock
    {
        #region Keys
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

                lTitle.Text = PageParameter( PageParameterKey.Title );

                var id = PageParameter( "htmlcontentid" ).AsInteger();
                var rockContext = new RockContext();
                HtmlContentService htmlContentService = new HtmlContentService( rockContext );
                var htmlContent = htmlContentService.Get( id );
                if ( htmlContent != null )
                {
                    if ( ( htmlContent.CreatedByPersonAlias != null && htmlContent.CreatedByPersonAlias.PersonId == CurrentPersonId )
                        || htmlContent.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                    {
                        tbName.Text = htmlContent.Name;
                        heSnippet.Text = htmlContent.Content;
                        rockContext.SaveChanges();
                    }
                    else
                    {
                        upSnippets.Visible = false;
                        nbMessage.Text = "You are not authorized to edit this snippet.";
                        nbMessage.Visible = true;
                    }
                }
            }
        }

        /// <summary>Handles the Click event of the btnCancel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
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

            NavigateToParentPage( queryParams );
        }

        /// <summary>Handles the Click event of the btnSave control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var id = PageParameter( "htmlcontentid" ).AsInteger();
            var rockContext = new RockContext();
            HtmlContentService htmlContentService = new HtmlContentService( rockContext );
            var htmlContent = htmlContentService.Get( id );
            if ( htmlContent == null )
            {
                htmlContent = new HtmlContent();
                htmlContentService.Add( htmlContent );
                htmlContent.CreatedByPersonAliasId = CurrentPersonAliasId;

            }
            else if ( !( ( htmlContent.CreatedByPersonAlias != null && htmlContent.CreatedByPersonAlias.PersonId == CurrentPersonId )
              || htmlContent.IsAuthorized( Authorization.EDIT, CurrentPerson ) ) )
            {
                upSnippets.Visible = false;
                nbMessage.Text = "You are not authorized to edit this snippet.";
                nbMessage.Visible = true;
                return;
            }
            htmlContent.Name = tbName.Text;
            htmlContent.Content = heSnippet.Text;

            rockContext.SaveChanges();
            var queryParams = new Dictionary<string, string>();

            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.ModalMode ) ) )
            {
                queryParams[PageParameterKey.ModalMode] = PageParameter( PageParameterKey.ModalMode );
            }

            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.Title ) ) )
            {
                queryParams[PageParameterKey.Title] = PageParameter( PageParameterKey.Title );
            }
            NavigateToParentPage( queryParams );
        }
    }
}