//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Constants;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    public partial class AuditInformationDetail : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "auditEntryId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "auditEntryId", int.Parse( itemId ) );
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
        /// Shows the readonly details.
        /// </summary>
        /// <param name="financialBatch">The audit entry.</param>
        private void ShowReadOnly( Audit audit )
        {
            lActionTitle.Text = ActionTitle.View( Audit.FriendlyTypeName ); 
            lblDetails.Text = new DescriptionList()
                .Add( "EntityType", audit.EntityType )
                .Add( "EntityId", audit.EntityId)
                .Add( "Title", audit.Title )
                .Add( "AuditType", audit.AuditType )
                .Add( "DateTime", audit.DateTime )
                .Add( "Person", audit.Person )
                .Html;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "auditEntryId" ) )
            {
                return;
            }

            Audit audit = null;
            if ( !itemKeyValue.Equals( 0 ) )
            {
                audit = new AuditService().Get( itemKeyValue );
            }

            ShowReadOnly( audit );
        }

        #endregion
    }
}