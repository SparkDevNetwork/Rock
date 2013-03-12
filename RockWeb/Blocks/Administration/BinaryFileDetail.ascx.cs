//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    [BooleanField("Show Binary File Type")]
    public partial class BinaryFileDetail : RockBlock, IDetailBlock
    {

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            fsFile.FileUploaded += fsFile_FileUploaded;
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
                string itemId = PageParameter( "BinaryFileId" );
                string typeId = PageParameter( "BinaryFileTypeId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    if ( string.IsNullOrWhiteSpace( typeId ) )
                    {

                        ShowDetail( "BinaryFileId", int.Parse( itemId ) );
                    }
                    else
                    {
                        ShowDetail( "BinaryFileId", int.Parse( itemId ), int.Parse( typeId ) );
                    }
                }
                else
                {
                    pnlDetails.Visible = false;
                }

                bool showBinaryFileType = false;
                if ( Boolean.TryParse( GetAttributeValue( "ShowBinaryFileType" ), out showBinaryFileType ) && showBinaryFileType )
                {
                    ddlBinaryFileType.Visible = true;
                }
                else
                {
                    ddlBinaryFileType.Visible = false;
                }
            }
            else
            {
                if ( pnlDetails.Visible )
                {
                    var binaryFile = new BinaryFile { BinaryFileTypeId = ddlBinaryFileType.SelectedValueAsInt() ?? 0 };
                    if ( binaryFile.BinaryFileTypeId > 0 )
                    {
                        binaryFile.LoadAttributes();
                        phAttributes.Controls.Clear();
                        Rock.Attribute.Helper.AddEditControls( binaryFile, phAttributes, false );
                    }
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
            ShowDetail( itemKey, itemKeyValue, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        /// <param name="binaryFileTypeId">The binary file type id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue, int? binaryFileTypeId )
        {
            if ( !itemKey.Equals( "BinaryFileId" ) )
            {
                return;
            }

            var binaryFileService = new BinaryFileService();
            BinaryFile BinaryFile = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                BinaryFile = binaryFileService.Get( itemKeyValue );
                lActionTitle.Text = ActionTitle.Edit( BinaryFile.FriendlyTypeName );
            }
            else
            {
                BinaryFile = new BinaryFile { Id = 0, IsSystem = false, BinaryFileTypeId = binaryFileTypeId };
                lActionTitle.Text = ActionTitle.Add( BinaryFile.FriendlyTypeName );
            }

            pnlDetails.Visible = true;
            hfBinaryFileId.SetValue( BinaryFile.Id );

            fsFile.BinaryFileId = BinaryFile.Id;
            tbName.Text = BinaryFile.FileName;
            tbDescription.Text = BinaryFile.Description;
            tbMimeType.Text = BinaryFile.MimeType;
            ddlBinaryFileType.SetValue( BinaryFile.BinaryFileTypeId );

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( BinaryFile.FriendlyTypeName );
            }

            phAttributes.Controls.Clear();
            BinaryFile.LoadAttributes();

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( BinaryFile.FriendlyTypeName );
                btnCancel.Text = "Close";
                Rock.Attribute.Helper.AddDisplayControls( BinaryFile, phAttributes );
            }
            else
            {
                Rock.Attribute.Helper.AddEditControls( BinaryFile, phAttributes, true );
            }

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
            tbMimeType.ReadOnly = readOnly;
            ddlBinaryFileType.Enabled = !readOnly;

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
            BinaryFile BinaryFile;
            BinaryFileService BinaryFileService = new BinaryFileService();
            AttributeService attributeService = new AttributeService();

            int BinaryFileId = int.Parse( hfBinaryFileId.Value );

            if ( BinaryFileId == 0 )
            {
                BinaryFile = new BinaryFile();
                BinaryFileService.Add( BinaryFile, CurrentPersonId );
            }
            else
            {
                BinaryFile = BinaryFileService.Get( BinaryFileId );
            }

            BinaryFile.IsTemporary = false;
            BinaryFile.FileName = tbName.Text;
            BinaryFile.Description = tbDescription.Text;
            BinaryFile.MimeType = tbMimeType.Text;
            BinaryFile.BinaryFileTypeId = ddlBinaryFileType.SelectedValueAsInt();

            BinaryFile.LoadAttributes();
            Rock.Attribute.Helper.GetEditValues( phAttributes, BinaryFile );
            Rock.Attribute.Helper.SetErrorIndicators( phAttributes, BinaryFile );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !BinaryFile.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                BinaryFileService.Save( BinaryFile, CurrentPersonId );
                Rock.Attribute.Helper.SaveAttributeValues( BinaryFile, CurrentPersonId );
            } );

            NavigateToParentPage();
        }

        #endregion

        #region Activities and Actions

        /// <summary>
        /// Handles the FileUploaded event of the fsFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void fsFile_FileUploaded( object sender, EventArgs e )
        {
            var service = new BinaryFileService();
            var binaryFile = service.Get(fsFile.BinaryFileId);
            if (binaryFile != null)
            {
                hfBinaryFileId.SetValue( binaryFile.Id );
                tbName.Text = binaryFile.FileName;
                tbMimeType.Text = binaryFile.MimeType;
            }
        }

        #endregion
    }
}