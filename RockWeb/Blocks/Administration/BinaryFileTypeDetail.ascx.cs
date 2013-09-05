//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Administration
{
    public partial class BinaryFileTypeDetail : RockBlock, IDetailBlock
    {
        #region Child Grid Dictionarys

        /// <summary>
        /// Gets or sets the state of the attributes.
        /// </summary>
        /// <value>
        /// The state of the attributes.
        /// </value>
        private ViewStateList<Attribute> BinaryFileAttributesState
        {
            get
            {
                return ViewState["BinaryFileAttributesState"] as ViewStateList<Attribute>;
            }

            set
            {
                ViewState["BinaryFileAttributesState"] = value;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gBinaryFileAttributes.DataKeyNames = new[] { "Guid" };
            gBinaryFileAttributes.Actions.ShowAdd = true;
            gBinaryFileAttributes.Actions.AddClick += gBinaryFileAttributes_Add;
            gBinaryFileAttributes.GridRebind += gBinaryFileAttributes_GridRebind;
            gBinaryFileAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
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
                string itemId = PageParameter( "binaryFileTypeId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "binaryFileTypeId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "binaryFileTypeId" ) )
            {
                return;
            }

            pnlDetails.Visible = true;
            BinaryFileType binaryFileType;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                binaryFileType = new BinaryFileTypeService().Get( itemKeyValue );
                lActionTitle.Text = ActionTitle.Edit( BinaryFileType.FriendlyTypeName );
            }
            else
            {
                binaryFileType = new BinaryFileType { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( BinaryFileType.FriendlyTypeName );
            }

            BinaryFileAttributesState = new ViewStateList<Attribute>();

            hfBinaryFileTypeId.Value = binaryFileType.Id.ToString();
            tbName.Text = binaryFileType.Name;
            tbDescription.Text = binaryFileType.Description;
            tbIconCssClass.Text = binaryFileType.IconCssClass;
            imgIconSmall.BinaryFileId = binaryFileType.IconSmallFileId;
            imgIconLarge.BinaryFileId = binaryFileType.IconLargeFileId;
            cbAllowCaching.Checked = binaryFileType.AllowCaching;

            if ( binaryFileType.StorageEntityType != null )
            {
                cpStorageType.SelectedValue = binaryFileType.StorageEntityType.Guid.ToString();
            }

            AttributeService attributeService = new AttributeService();

            string qualifierValue = binaryFileType.Id.ToString();
            var qryBinaryFileAttributes = attributeService.GetByEntityTypeId( new BinaryFile().TypeId ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "BinaryFileTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( qualifierValue ) );

            BinaryFileAttributesState.AddAll( qryBinaryFileAttributes.ToList() );
            BindBinaryFileAttributesGrid();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( BinaryFileType.FriendlyTypeName );
            }

            if ( binaryFileType.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( BinaryFileType.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( BinaryFileType.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
            tbIconCssClass.ReadOnly = readOnly;
            imgIconLarge.Enabled = !readOnly;
            imgIconSmall.Enabled = !readOnly;
            gBinaryFileAttributes.Enabled = !readOnly;
            cbAllowCaching.Enabled = !readOnly;
            cpStorageType.Enabled = !readOnly;
            btnSave.Visible = !readOnly;

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
            using ( new UnitOfWorkScope() )
            {
                BinaryFileType binaryFileType;
                BinaryFileTypeService binaryFileTypeService = new BinaryFileTypeService();
                AttributeService attributeService = new AttributeService();

                int binaryFileTypeId = int.Parse( hfBinaryFileTypeId.Value );

                if ( binaryFileTypeId == 0 )
                {
                    binaryFileType = new BinaryFileType();
                    binaryFileTypeService.Add( binaryFileType, CurrentPersonId );
                }
                else
                {
                    binaryFileType = binaryFileTypeService.Get( binaryFileTypeId );
                }

                binaryFileType.Name = tbName.Text;
                binaryFileType.Description = tbDescription.Text;
                binaryFileType.IconCssClass = tbIconCssClass.Text;
                binaryFileType.IconSmallFileId = imgIconSmall.BinaryFileId;
                binaryFileType.IconLargeFileId = imgIconLarge.BinaryFileId;
                binaryFileType.AllowCaching = cbAllowCaching.Checked;

                if ( !string.IsNullOrWhiteSpace( cpStorageType.SelectedValue ) )
                {
                    var entityTypeService = new EntityTypeService();
                    var storageEntityType = entityTypeService.Get( new Guid( cpStorageType.SelectedValue ) );

                    if ( storageEntityType != null )
                    {
                        binaryFileType.StorageEntityTypeId = storageEntityType.Id;
                    }
                }

                if ( !binaryFileType.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                RockTransactionScope.WrapTransaction( () =>
                    {
                        binaryFileTypeService.Save( binaryFileType, CurrentPersonId );

                        // get it back to make sure we have a good Id for it for the Attributes
                        binaryFileType = binaryFileTypeService.Get( binaryFileType.Guid );

                        /* Take care of Binary File Attributes */

                        // delete BinaryFileAttributes that are no longer configured in the UI
                        string qualifierValue = binaryFileType.Id.ToString();
                        var binaryFileAttributesQry = attributeService.GetByEntityTypeId( new BinaryFile().TypeId ).AsQueryable()
                            .Where( a => a.EntityTypeQualifierColumn.Equals( "BinaryFileTypeId", StringComparison.OrdinalIgnoreCase )
                            && a.EntityTypeQualifierValue.Equals( qualifierValue ) );

                        var deletedBinaryFileAttributes = from attr in binaryFileAttributesQry
                                                          where !( from d in BinaryFileAttributesState
                                                                   select d.Guid ).Contains( attr.Guid )
                                                          select attr;

                        deletedBinaryFileAttributes.ToList().ForEach( a =>
                        {
                            var attr = attributeService.Get( a.Guid );
                            Rock.Web.Cache.AttributeCache.Flush( attr.Id );
                            attributeService.Delete( attr, CurrentPersonId );
                            attributeService.Save( attr, CurrentPersonId );
                        } );

                        // add/update the BinaryFileAttributes that are assigned in the UI
                        foreach ( var attributeState in BinaryFileAttributesState )
                        {
                            // remove old qualifiers in case they changed
                            var qualifierService = new AttributeQualifierService();
                            foreach ( var oldQualifier in qualifierService.GetByAttributeId( attributeState.Id ).ToList() )
                            {
                                qualifierService.Delete( oldQualifier, CurrentPersonId );
                                qualifierService.Save( oldQualifier, CurrentPersonId );
                            }

                            Attribute attribute = binaryFileAttributesQry.FirstOrDefault( a => a.Guid.Equals( attributeState.Guid ) );
                            if ( attribute == null )
                            {
                                attribute = attributeState.Clone() as Attribute;
                                attributeService.Add( attribute, CurrentPersonId );
                            }
                            else
                            {

                                attributeState.Id = attribute.Id;
                                attribute.FromDictionary( attributeState.ToDictionary() );

                                foreach ( var qualifier in attributeState.AttributeQualifiers )
                                {
                                    attribute.AttributeQualifiers.Add( qualifier.Clone() as AttributeQualifier );
                                }

                            }

                            attribute.EntityTypeQualifierColumn = "BinaryFileTypeId";
                            attribute.EntityTypeQualifierValue = binaryFileType.Id.ToString();
                            attribute.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( typeof( BinaryFile ) ).Id;
                            Rock.Web.Cache.AttributeCache.Flush( attribute.Id );
                            attributeService.Save( attribute, CurrentPersonId );
                        }
                    } );
            }

            NavigateToParentPage();
        }

        #endregion

        #region BinaryFileAttributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gBinaryFileAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBinaryFileAttributes_Add( object sender, EventArgs e )
        {
            gBinaryFileAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gBinaryFileAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBinaryFileAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gBinaryFileAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the binary file attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gBinaryFileAttributes_ShowEdit( Guid attributeGuid )
        {
            pnlDetails.Visible = false;
            pnlBinaryFileAttribute.Visible = true;
            
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                edtBinaryFileAttributes.ActionTitle = ActionTitle.Add( "attribute for binary files of type " + tbName.Text );
            }
            else
            {
                attribute = BinaryFileAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtBinaryFileAttributes.ActionTitle = ActionTitle.Edit( "attribute for binary files of type " + tbName.Text );
            }

            edtBinaryFileAttributes.SetAttributeProperties( attribute, typeof( BinaryFile ) );
        }

        /// <summary>
        /// Handles the Delete event of the gBinaryFileAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gBinaryFileAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            BinaryFileAttributesState.RemoveEntity( attributeGuid );

            BindBinaryFileAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBinaryFileAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBinaryFileAttributes_GridRebind( object sender, EventArgs e )
        {
            BindBinaryFileAttributesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveBinaryFileAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveBinaryFileAttribute_Click( object sender, EventArgs e )
        {
            Attribute attribute = new Attribute();
            edtBinaryFileAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            BinaryFileAttributesState.RemoveEntity( attribute.Guid );
            BinaryFileAttributesState.Add( attribute );

            pnlDetails.Visible = true;
            pnlBinaryFileAttribute.Visible = false;

            BindBinaryFileAttributesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelBinaryFileAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelBinaryFileAttribute_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlBinaryFileAttribute.Visible = false;
        }

        /// <summary>
        /// Binds the binary file type attributes grid.
        /// </summary>
        private void BindBinaryFileAttributesGrid()
        {
            gBinaryFileAttributes.DataSource = BinaryFileAttributesState.OrderBy( a => a.Name ).ToList();
            gBinaryFileAttributes.DataBind();
        }

        #endregion
    }
}