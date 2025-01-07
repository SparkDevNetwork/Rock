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
using Rock.Lava;
using DotLiquid;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Lava Shortcode Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a Lava Shortcode." )]
    [Rock.SystemGuid.BlockTypeGuid( "092BFC5F-A291-4472-B737-0C69EA33D08A" )]
    public partial class LavaShortcodeDetail : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            var lavaShortcodeId = PageParameter( "LavaShortcodeId" ).AsInteger();

            if ( !Page.IsPostBack )
            {
                ShowDetail( lavaShortcodeId );
            }

            base.OnLoad( e );
        }

        #endregion

        #region Edit Events

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
            var categoryService = new CategoryService( rockContext );

            int lavaShortCodeId = hfLavaShortcodeId.ValueAsInt();

            if ( lavaShortCodeService.Queryable().Any( a => a.TagName == tbTagName.Text && a.Id != lavaShortCodeId ) )
            {
                Page.ModelState.AddModelError( "DuplicateTag", "Tag with the same name is already in use." );
                return;
            }

            if ( lavaShortCodeId == 0 )
            {
                lavaShortcode = new LavaShortcode();
                lavaShortCodeService.Add( lavaShortcode );
            }
            else
            {
                lavaShortcode = lavaShortCodeService.Get( lavaShortCodeId );
            }

            var oldTagName = hfOriginalTagName.Value;

            if ( !lavaShortcode.IsSystem )
            {
                lavaShortcode.Name = tbLavaShortcodeName.Text;
                lavaShortcode.IsActive = cbIsActive.Checked;
                lavaShortcode.Description = tbDescription.Text;
                lavaShortcode.Documentation = htmlDocumentation.Text;
                lavaShortcode.TagType = rblTagType.SelectedValueAsEnum<TagType>();
                lavaShortcode.TagName = tbTagName.Text.Trim();
                lavaShortcode.Markup = ceMarkup.Text;
                lavaShortcode.Parameters = kvlParameters.Value;
                lavaShortcode.EnabledLavaCommands = String.Join( ",", lcpLavaCommands.SelectedLavaCommands );
            }

            lavaShortcode.Categories.Clear();
            var categoryPicker = lavaShortcode.IsSystem ? cpViewCategories : cpShortCodeCat;
            foreach ( var categoryId in categoryPicker.SelectedValuesAsInt() )
            {
                lavaShortcode.Categories.Add( categoryService.Get( categoryId ) );
            }

            // Since changes to Categories isn't tracked by ChangeTracker, set the ModifiedDateTime just in case Categories changed
            lavaShortcode.ModifiedDateTime = RockDateTime.Now;
                        
            rockContext.SaveChanges();

            if ( LavaService.RockLiquidIsEnabled )
            {
                // unregister shortcode
                if ( oldTagName.IsNotNullOrWhiteSpace() )
                {
                    Template.UnregisterShortcode( oldTagName );
                }

                // Register the new shortcode definition. Note that RockLiquid shortcode tags are case-sensitive.
                if ( lavaShortcode.TagType == TagType.Block )
                {
                    Template.RegisterShortcode<Rock.Lava.Shortcodes.DynamicShortcodeBlock>( lavaShortcode.TagName );
                }
                else
                {
                    Template.RegisterShortcode<Rock.Lava.Shortcodes.DynamicShortcodeInline>( lavaShortcode.TagName );
                }

                // (bug fix) Now we have to clear the entire LavaTemplateCache because it's possible that some other
                // usage of this shortcode is cached with a key we can't predict.
#pragma warning disable CS0618 // Type or member is obsolete
                // This obsolete code can be deleted when support for the DotLiquid Lava implementation is removed.
                LavaTemplateCache.Clear();
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if ( oldTagName.IsNotNullOrWhiteSpace() )
            {
                LavaService.DeregisterShortcode( oldTagName );
            }

            // Register the new shortcode definition.
            LavaService.RegisterShortcode( lavaShortcode.TagName, ( shortcodeName ) => WebsiteLavaShortcodeProvider.GetShortcodeDefinition( shortcodeName ) );

            LavaService.ClearTemplateCache();

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
            var isUserAuthorized = IsUserAuthorized( Authorization.EDIT );
            if ( !isUserAuthorized )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( LavaShortcode.FriendlyTypeName );
            }

            if ( lavaShortcode.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = string.Format( "<strong>Note</strong> This is a system {0} so editing is limited.", LavaShortcode.FriendlyTypeName.ToLower() );
            }

            if ( readOnly )
            {
                var allowlimitedEditing = lavaShortcode.IsSystem && isUserAuthorized;
                ShowReadonlyDetails( lavaShortcode, allowlimitedEditing );
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
        /// <param name="allowlimitedEditing">if set to <c>true</c> [allow limited editing].</param>
        private void ShowReadonlyDetails( LavaShortcode lavaShortcode, bool allowlimitedEditing )
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
            cpViewCategories.Visible = allowlimitedEditing;
            divActions.Visible = allowlimitedEditing;
            if ( allowlimitedEditing )
            {
                LoadCategories( cpViewCategories, lavaShortcode.Id );
            }

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

            divActions.Visible = true;
            hlInactive.Visible = !lavaShortcode.IsActive;
            tbLavaShortcodeName.Text = lavaShortcode.Name;
            cbIsActive.Checked = lavaShortcode.IsActive;
            tbDescription.Text = lavaShortcode.Description;
            htmlDocumentation.Text = lavaShortcode.Documentation;
            ceMarkup.Text = lavaShortcode.Markup;
            tbTagName.Text = lavaShortcode.TagName;
            kvlParameters.Value = lavaShortcode.Parameters;
            hfOriginalTagName.Value = lavaShortcode.TagName;
            LoadCategories( cpShortCodeCat, lavaShortcode.Id );

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

        /// <summary>
        /// Loads the categories.
        /// </summary>
        private void LoadCategories( CategoryPicker categoryPicker, int lavaShortcodeId )
        {
            var rockContext = new RockContext();
            var lavaShortcodeService = new LavaShortcodeService( rockContext );
                        
            if ( lavaShortcodeId > 0 )
            {
                var categories = lavaShortcodeService.Queryable().SingleOrDefault( v => v.Id == lavaShortcodeId )?.Categories;
                if ( categories?.Count > 0 )
                {
                    categoryPicker.SetValues( categories.Select(v=>v.Id) );
                }
            }
        }

        #endregion
    }
}