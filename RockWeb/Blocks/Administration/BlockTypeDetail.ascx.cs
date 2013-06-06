//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class BlockTypeDetail : RockBlock, IDetailBlock
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
                string itemId = PageParameter( "blockTypeId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "blockTypeId", int.Parse( itemId ) );
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
            BlockType blockType;
            BlockTypeService blockTypeService = new BlockTypeService();

            int blockTypeId = int.Parse( hfBlockTypeId.Value );

            if ( blockTypeId == 0 )
            {
                blockType = new BlockType();
                blockTypeService.Add( blockType, CurrentPersonId );
            }
            else
            {
                BlockTypeCache.Flush( blockTypeId );
                blockType = blockTypeService.Get( blockTypeId );
            }

            blockType.Name = tbName.Text;
            blockType.Path = tbPath.Text;
            blockType.Description = tbDescription.Text;

            if ( !blockType.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                blockTypeService.Save( blockType, CurrentPersonId );
            } );

            NavigateToParentPage();
        }

        /// <summary>
        /// Gets the name of the fully qualified page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        private string GetFullyQualifiedPageName( Rock.Model.Page page )
        {
            string result = page.Name;
            Rock.Model.Page parent = page.ParentPage;
            while ( parent != null )
            {
                result = parent.Name + " / " + result;
                parent = parent.ParentPage;
            }

            return result;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            // return if unexpected itemKey 
            if ( itemKey != "blockTypeId" )
            {
                return;
            }

            pnlDetails.Visible = true;

            // Load depending on Add(0) or Edit
            BlockType blockType = null;
            if ( !itemKeyValue.Equals( 0 ) )
            {
                blockType = new BlockTypeService().Get( itemKeyValue );
                lActionTitle.Text = ActionTitle.Edit( BlockType.FriendlyTypeName );
            }
            else
            {
                blockType = new BlockType { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( BlockType.FriendlyTypeName );
            }

            hfBlockTypeId.Value = blockType.Id.ToString();
            tbName.Text = blockType.Name;
            tbPath.Text = blockType.Path;
            tbDescription.Text = blockType.Description;
            foreach ( var fullPageName in blockType.Blocks.ToList().Where( a => a.Page != null ).Select(a => GetFullyQualifiedPageName(a.Page)).OrderBy(a => a))
            {
                lstPages.Items.Add( fullPageName );
            }

            if ( lstPages.Items.Count == 0 )
            {
                lstPages.Items.Add( Rock.Constants.None.Text );
            }

            string blockPath = Request.MapPath( blockType.Path );
            if ( !System.IO.File.Exists( blockPath ) )
            {
                lblStatus.Text = string.Format( "<span class='label label-important'>The file '{0}' [{1}] does not exist.</span>", blockType.Path, blockType.Guid );
            }
            else
            {
                lblStatus.Text = "<span class='label label-success'>OK</span>";
            }

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( BlockType.FriendlyTypeName );
            }

            if ( blockType.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( BlockType.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( BlockType.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            tbName.ReadOnly = readOnly;
            tbPath.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;

            btnSave.Visible = !readOnly;
        }

        #endregion
    }
}