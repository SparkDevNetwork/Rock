//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;

using Rock;
using Rock.Constants;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Block for administrating a tag
    /// </summary>
    [Description( "Block for administrating a tag" )]
    public partial class TagDetail : Rock.Web.UI.RockBlock
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

            _canConfigure = IsUserAuthorized( "Administrate" );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return confirmDelete(event, '{0}');", Group.FriendlyTypeName );

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
                string itemId = PageParameter( "tagId" );
                string entityTypeId = PageParameter( "entityTypeId" );

                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    if ( string.IsNullOrWhiteSpace( entityTypeId ) )
                    {
                        ShowDetail( "tagId", int.Parse( itemId ) );
                    }
                    else
                    {
                        ShowDetail( "tagId", int.Parse( itemId ), int.Parse( entityTypeId ) );
                    }
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
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
            var tag = new TagService().Get( int.Parse( hfId.Value ) );
            ShowEditDetails( tag );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var tagService = new Rock.Model.TagService();
            var tag = tagService.Get( int.Parse( hfId.Value ) );

            if ( tag != null )
            {
                string errorMessage;
                if ( !tagService.CanDelete( tag, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                tagService.Delete( tag, CurrentPersonId );
                tagService.Save( tag, CurrentPersonId );

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
            Tag tag;

            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var tagService = new Rock.Model.TagService();

                int tagId = int.Parse( hfId.Value );

                if ( tagId == 0 )
                {
                    tag = new Tag();
                    tag.IsSystem = false;
                    tagService.Add( tag, CurrentPersonId );
                }
                else
                {
                    tag = tagService.Get( tagId );
                }

                tag.Name = tbName.Text;
                tag.OwnerId = ppOwner.PersonId;
                tag.EntityTypeId = ddlEntityType.SelectedValueAsId().Value;
                tag.EntityTypeQualifierColumn = tbEntityTypeQualifierColumn.Text;
                tag.EntityTypeQualifierValue = tbEntityTypeQualifierValue.Text;

                tagService.Save( tag, CurrentPersonId );

            }

            var qryParams = new Dictionary<string, string>();
            qryParams["tagId"] = tag.Id.ToString();

            NavigateToPage( this.CurrentPage.Guid, qryParams );
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
                var tag = new TagService().Get( int.Parse( hfId.Value ) );
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
            if ( rblScope.SelectedValue == "Private" && CurrentPerson != null )
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

            this.DimOtherBlocks( editable );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            ShowDetail( itemKey, itemKeyValue, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The group id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue, int? entityTypeId )
        {
            pnlDetails.Visible = false;
            if ( !itemKey.Equals( "tagId" ) )
            {
                return;
            }

            Tag tag = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                tag = new TagService().Get( itemKeyValue );
            }
            else
            {
                tag = new Tag { Id = 0, OwnerId = CurrentPersonId };
                if ( entityTypeId.HasValue )
                {
                    tag.EntityTypeId = entityTypeId.Value;
                }
            }

            if ( tag == null )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfId.Value = tag.Id.ToString();

            bool readOnly = false;

            if ( !_canConfigure && tag.OwnerId != CurrentPersonId )
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

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void ShowEditDetails( Tag tag )
        {

            if ( tag.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( Tag.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = tag.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbName.Text = tag.Name;
            if ( tag.OwnerId.HasValue )
            {
                rblScope.SelectedValue = "Private";
            }
            else
            {
                rblScope.SelectedValue = "Public";
            }
            ppOwner.SetValue( tag.Owner );

            ddlEntityType.Items.Clear();
            new EntityTypeService().GetEntityListItems().ForEach( l => ddlEntityType.Items.Add( l ) );
            ddlEntityType.SelectedValue = tag.EntityTypeId.ToString();
            tbEntityTypeQualifierColumn.Text = tag.EntityTypeQualifierColumn;
            tbEntityTypeQualifierValue.Text = tag.EntityTypeQualifierValue;

            rblScope.Visible = _canConfigure;
            ppOwner.Visible = _canConfigure;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowReadonlyDetails( Tag tag )
        {
            SetEditMode( false );
            lReadOnlyTitle.Text = tag.Name.FormatAsHtmlTitle();
            hlEntityType.Text = tag.EntityType.FriendlyName;
        }

        #endregion

    }
}