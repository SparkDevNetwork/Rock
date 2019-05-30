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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using DotLiquid;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Lava.Shortcodes;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using System.Collections.Generic;
using System.Web;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Lava Shortcode Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a Lava Shortcode." )]
    public partial class LavaShortcodeDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "lavaShortcodeId" ).AsInteger() );
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var lavaShortcode = new LavaShortcodeService( new RockContext() ).Get( hfLavaShortcodeId.ValueAsInt() );
            ShowEditDetails( lavaShortcode );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            LavaShortcode lavaShortcode;
            var rockContext = new RockContext();
            var lavaShortCodeService = new LavaShortcodeService( rockContext );

            int lavaShortCode = hfLavaShortcodeId.ValueAsInt();

            if ( lavaShortCodeService.Queryable().Any( a => a.TagName == tbTagName.Text && a.Id != lavaShortCode ) )
            {
                Page.ModelState.AddModelError( "DuplicateTag", "Tag with the same name is already in use." );
                return;
            }
            
            if ( lavaShortCode == 0 )
            {
                lavaShortcode = new LavaShortcode();
                lavaShortCodeService.Add( lavaShortcode );
            }
            else
            {
                lavaShortcode = lavaShortCodeService.Get( lavaShortCode );
            }

            lavaShortcode.Name = tbLavaShortcodeName.Text;
            lavaShortcode.IsActive = cbIsActive.Checked;
            lavaShortcode.Description = tbDescription.Text;
            lavaShortcode.Documentation = htmlDocumentation.Text;
            lavaShortcode.TagType = rblTagType.SelectedValueAsEnum<TagType>();
            lavaShortcode.TagName = tbTagName.Text;
            lavaShortcode.Markup = ceMarkup.Text;
            lavaShortcode.Parameters = kvlParameters.Value;
            lavaShortcode.EnabledLavaCommands = String.Join(",", lcpLavaCommands.SelectedLavaCommands);

            rockContext.SaveChanges();

            // unregister shortcode
            if ( hfOriginalTagName.Value.IsNotNullOrWhiteSpace() )
            {
                Template.UnregisterShortcode( hfOriginalTagName.Value );
            }

            // register shortcode
            if (lavaShortcode.TagType == TagType.Block )
            {
                Template.RegisterShortcode<DynamicShortcodeBlock>( lavaShortcode.TagName );
            }
            else
            {
                Template.RegisterShortcode<DynamicShortcodeInline>( lavaShortcode.TagName );
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="lavaShortcodeId">The Lava Shortcode identifier.</param>
        public void ShowDetail( int lavaShortcodeId )
        {
            pnlDetails.Visible = true;

            // Load depending on Add(0) or Edit
            LavaShortcode lavaShortcode = null;

            if ( !lavaShortcodeId.Equals( 0 ) )
            {
                lavaShortcode = new LavaShortcodeService( new RockContext() ).Get( lavaShortcodeId );
                lActionTitle.Text = ActionTitle.Edit( LavaShortcode.FriendlyTypeName ).FormatAsHtmlTitle();
                pdAuditDetails.SetEntity( lavaShortcode, ResolveRockUrl( "~" ) );
            }

            if ( lavaShortcode == null )
            {
                lavaShortcode = new LavaShortcode { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( LavaShortcode.FriendlyTypeName ).FormatAsHtmlTitle();
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfLavaShortcodeId.Value = lavaShortcode.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( LavaShortcode.FriendlyTypeName );
            }

            if ( lavaShortcode.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = string.Format( "<strong>Note</strong> This is a system {0} and is not able to be edited.", LavaShortcode.FriendlyTypeName.ToLower() );
            }

            if ( readOnly )
            {
                ShowReadonlyDetails( lavaShortcode );
            }
            else
            {
                ShowEditDetails( lavaShortcode );
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="LavaShortcode">The lavaShortcode.</param>
        private void ShowReadonlyDetails( LavaShortcode lavaShortcode )
        {
            SetEditMode( false );

            lActionTitle.Text = lavaShortcode.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !lavaShortcode.IsActive;

            lLayoutDescription.Text = lavaShortcode.Description;
            var list = lavaShortcode.Markup.ToKeyValuePairList();

            DescriptionList headerMarkup = new DescriptionList();
            headerMarkup.Add( "System", lavaShortcode.IsSystem.ToYesNo() );
            headerMarkup.Add( "Tag Name", lavaShortcode.TagName );
            headerMarkup.Add( "Tag Type", lavaShortcode.TagType );

            lblHeaderFields.Text = headerMarkup.Html;

            ceView.Text = lavaShortcode.Markup;

            if ( !string.IsNullOrEmpty( lavaShortcode.Parameters ) )
            {
                var values = new List<string>();
                string[] nameValues = lavaShortcode.Parameters.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );

                foreach ( string nameValue in nameValues )
                {
                    string[] nameAndValue = nameValue.Split( new char[] { '^' } );

                    // url decode array items just in case they were UrlEncoded (in the KeyValueList controls)
                    nameAndValue = nameAndValue.Select( s => HttpUtility.UrlDecode( s ) ).ToArray();

                    if ( nameAndValue.Length == 2 )
                    {
                        values.Add( string.Format( "<strong>{0}:</strong> {1}", nameAndValue[0], nameAndValue[1] ) );
                    }
                    else
                    {
                        values.Add( nameValue );
                    }
                }

                lblParameters.Text = string.Join( "<br/>", values );
            }

            lblEnabledCommands.Text = lavaShortcode.EnabledLavaCommands;
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="LavaShortcode">The lavaShortcode.</param>
        private void ShowEditDetails( LavaShortcode lavaShortcode )
        {
            if ( lavaShortcode.Id.Equals( 0 ) )
            {
                lActionTitle.Text = ActionTitle.Add( LavaShortcode.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = ActionTitle.Edit( LavaShortcode.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            SetEditMode( true );

            hlInactive.Visible = !lavaShortcode.IsActive;
            tbLavaShortcodeName.Text = lavaShortcode.Name;
            cbIsActive.Checked = lavaShortcode.IsActive;
            tbDescription.Text = lavaShortcode.Description;
            htmlDocumentation.Text = lavaShortcode.Documentation;
            ceMarkup.Text = lavaShortcode.Markup;
            tbTagName.Text = lavaShortcode.TagName;
            kvlParameters.Value = lavaShortcode.Parameters;
            hfOriginalTagName.Value = lavaShortcode.TagName;

            if ( lavaShortcode.EnabledLavaCommands.IsNotNullOrWhiteSpace() )
            {
                lcpLavaCommands.SetValues( lavaShortcode.EnabledLavaCommands.Split( ',' ).ToList() );
            }

            rblTagType.BindToEnum<TagType>();
            rblTagType.SetValue( ( int ) lavaShortcode.TagType );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        #endregion
    }
}