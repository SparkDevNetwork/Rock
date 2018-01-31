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
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block for administrating a tag
    /// </summary>
    [DisplayName( "Tag Detail" )]
    [Category( "Core" )]
    [Description( "Block for administrating a tag." )]
    public partial class TagDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Fields

        private bool _canConfigure = false;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _canConfigure = IsUserAuthorized( Authorization.EDIT );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Group.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Tag ) ).Id;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbEditError.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "TagId" ).AsInteger(), PageParameter( "EntityTypeId" ).AsIntegerOrNull() );
            }
        }

        public override List<Rock.Web.UI.BreadCrumb> GetBreadCrumbs( Rock.Web.PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();
            
            string pageTitle = "New Tag";
            
            int? tagId = PageParameter( "tagId" ).AsIntegerOrNull();
            if (tagId.HasValue)
            {
                Tag tag = new TagService( new RockContext() ).Get( tagId.Value );
                if (tag != null)
                {
                    pageTitle = tag.Name;
                    breadCrumbs.Add( new BreadCrumb( tag.Name, pageReference ) );
                }
            }

            RockPage.Title = pageTitle;

            return breadCrumbs;
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var tag = new TagService( new RockContext() ).Get( int.Parse( hfId.Value ) );
            ShowEditDetails( tag );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var tagService = new Rock.Model.TagService( rockContext );
            var tag = tagService.Get( int.Parse( hfId.Value ) );

            if ( tag != null )
            {
                string errorMessage;
                if ( !tagService.CanDelete( tag, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                tagService.Delete( tag );
                rockContext.SaveChanges();

                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var tagService = new Rock.Model.TagService( rockContext );
            Tag tag = null;

            int tagId = int.Parse( hfId.Value );

            if ( tagId != 0 )
            {
                tag = tagService.Get( tagId );
            }

            if ( tag == null )
            {
                tag = new Tag();
                tag.IsSystem = false;
                tagService.Add( tag );
            }

            string name = tbName.Text;
            int? ownerId = ppOwner.PersonId;
            int? entityTypeId = ddlEntityType.SelectedValueAsId();
            string qualifierCol = tbEntityTypeQualifierColumn.Text;
            string qualifierVal = tbEntityTypeQualifierValue.Text;

            // Verify tag with same name does not already exist
            if (tagService.Queryable()
                    .Where( t =>
                        t.Id != tagId &&
                        t.Name == name &&
                        ( 
                            ( t.OwnerPersonAlias == null && !ownerId.HasValue ) || 
                            ( t.OwnerPersonAlias != null && ownerId.HasValue && t.OwnerPersonAlias.PersonId == ownerId.Value ) 
                        ) &&
                        ( !t.EntityTypeId.HasValue || (
                            t.EntityTypeId.Value == entityTypeId &&
                            t.EntityTypeQualifierColumn == qualifierCol &&
                            t.EntityTypeQualifierValue == qualifierVal )
                        ) )
                    .Any())
            {
                nbEditError.Heading = "Tag Already Exists";
                nbEditError.Text = string.Format("A '{0}' tag already exists for the selected scope, owner, and entity type.", name);
                nbEditError.Visible = true;
            }
            else
            {
                int? ownerPersonAliasId = null;
                if (ownerId.HasValue)
                {
                    ownerPersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( ownerId.Value );
                }
                tag.Name = name;
                tag.Description = tbDescription.Text;
                tag.IsActive = cbIsActive.Checked;

                if ( _canConfigure )
                {
                    tag.CategoryId = cpCategory.SelectedValueAsInt();
                    tag.OwnerPersonAliasId = ownerPersonAliasId;
                    tag.EntityTypeId = entityTypeId;
                    tag.EntityTypeQualifierColumn = qualifierCol;
                    tag.EntityTypeQualifierValue = qualifierVal;
                }

                rockContext.SaveChanges();

                var qryParams = new Dictionary<string, string>();
                qryParams["tagId"] = tag.Id.ToString();

                NavigateToPage( RockPage.Guid, qryParams );
            }

        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfId.Value.Equals( "0" ) )
            {
                NavigateToParentPage();
            }
            else
            {
                var tag = new TagService( new RockContext() ).Get( int.Parse( hfId.Value ) );
                ShowReadonlyDetails( tag );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblScope control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblScope_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( rblScope.SelectedValue == "Personal" && CurrentPerson != null )
            {
                ppOwner.SetValue( CurrentPerson );
                ppOwner.Visible = _canConfigure;
            }
            else
            {
                ppOwner.SetValue( null );
                ppOwner.Visible = false;
            }

        }
        #endregion

        #region Methods

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
        /// Shows the detail.
        /// </summary>
        /// <param name="tagId">The tag identifier.</param>
        public void ShowDetail( int tagId )
        {
            ShowDetail( tagId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="tagId">The tag identifier.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        public void ShowDetail( int tagId, int? entityTypeId )
        {
            pnlDetails.Visible = false;

            Tag tag = null;

            if ( !tagId.Equals( 0 ) )
            {
                tag = new TagService( new RockContext() ).Get( tagId );
                pdAuditDetails.SetEntity( tag, ResolveRockUrl( "~" ) );
            }
            
            if ( tag == null )
            {
                tag = new Tag {
                    Id = 0,
                    CategoryId = PageParameter( "CategoryId" ).AsIntegerOrNull(),
                    OwnerPersonAliasId = CurrentPersonAliasId,
                    OwnerPersonAlias = CurrentPersonAlias,
                    EntityTypeId = entityTypeId
                };

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            bool canView = _canConfigure || tag.IsAuthorized( Authorization.VIEW, CurrentPerson );

            if ( canView )
            {
                bool canEdit = _canConfigure || tag.IsAuthorized( Authorization.EDIT, CurrentPerson );

                pnlDetails.Visible = true;

                hfId.Value = tag.Id.ToString();

                bool readOnly = false;

                if ( !canEdit )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Tag.FriendlyTypeName );
                }

                if ( tag.IsSystem )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( Group.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    btnEdit.Visible = false;
                    btnDelete.Visible = false;
                    ShowReadonlyDetails( tag );
                }
                else
                {
                    btnEdit.Visible = true;
                    btnDelete.Visible = true;
                    if ( tag.Id > 0 )
                    {
                        ShowReadonlyDetails( tag );
                    }
                    else
                    {
                        ShowEditDetails( tag );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void ShowEditDetails( Tag tag )
        {
            if ( tag.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( Tag.FriendlyTypeName ).FormatAsHtmlTitle();
                hlStatus.Visible = false;
            }
            else
            {
                lReadOnlyTitle.Text = tag.Name.FormatAsHtmlTitle();
                SetLabel( tag );
            }

            SetEditMode( true );

            tbName.Text = tag.Name;
            tbDescription.Text = tag.Description;

            pnlAdvanced.Visible = _canConfigure;

            cpCategory.SetValue( tag.CategoryId );

            if ( tag.OwnerPersonAlias != null )
            {
                rblScope.SelectedValue = "Personal";
                ppOwner.SetValue( tag.OwnerPersonAlias.Person );
                ppOwner.Visible = true;
            }
            else
            {
                rblScope.SelectedValue = "Organization";
                ppOwner.SetValue( null );
                ppOwner.Visible = false;
            }

            cbIsActive.Checked = tag.IsActive;

            ddlEntityType.Items.Clear();
            ddlEntityType.Items.Add( new System.Web.UI.WebControls.ListItem() );
            new EntityTypeService( new RockContext() ).GetEntityListItems().ForEach( l => ddlEntityType.Items.Add( l ) );
            ddlEntityType.SelectedValue = tag.EntityTypeId.ToString();

            tbEntityTypeQualifierColumn.Text = tag.EntityTypeQualifierColumn;
            tbEntityTypeQualifierValue.Text = tag.EntityTypeQualifierValue;

        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowReadonlyDetails( Tag tag )
        {
            SetEditMode( false );
            lReadOnlyTitle.Text = tag.Name.FormatAsHtmlTitle();

            SetLabel( tag );

            lDescription.Text = tag.Description;
            hlEntityType.Text = tag.EntityType != null ? tag.EntityType.FriendlyName : "None";

            lScope.Text = tag.OwnerPersonAliasId.HasValue ? "Personal" : "Organizational";
            lOwner.Visible = tag.OwnerPersonAlias != null;
            if ( tag.OwnerPersonAlias != null && tag.OwnerPersonAlias.Person != null )
            {
                lOwner.Text = tag.OwnerPersonAlias.Person.FullName;
            }

            btnSecurity.Visible = !tag.OwnerPersonAliasId.HasValue && ( _canConfigure || tag.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) );
            
            btnSecurity.EntityId = tag.Id;
        }

        /// <summary>
        /// Sets the Active/Inactive label.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void SetLabel( Tag tag )
        {
            if ( tag.IsActive )
            {
                hlStatus.Text = "Active";
                hlStatus.LabelType = LabelType.Success;
            }
            else
            {
                hlStatus.Text = "Inactive";
                hlStatus.LabelType = LabelType.Danger;
            }
        }

        #endregion

    }
}