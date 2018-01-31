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
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Utility
{
    [DisplayName( "Entity Attribute Values" )]
    [Category( "Utility" )]
    [Description( "View and edit attribute values for an entity." )]

    [ContextAware]
    public partial class EntityAttributeValues : RockBlock, ISecondaryBlock
    {
        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            IHasAttributes entity = ContextEntity() as IHasAttributes;

            if ( entity == null || entity.Attributes.Count == 0 )
            {
                return;
            }

            if ( !IsPostBack )
            {
                var canEdit = IsUserAuthorized( Authorization.EDIT );

                pnlDetails.Visible = true;
                pnlView.Visible = true;
                btnEdit.Visible = canEdit;

                ltTitle.Text = string.Format( "{0} Attributes", entity.GetType().GetFriendlyTypeName() );
            }

            if ( pnlEdit.Visible )
            {
                ShowEdit( false );
            }
            else
            {
                ShowDetails();
            }
        }

        /// <summary>
        /// Allow primary blocks to hide this one. This is common when the primary block goes
        /// into edit mode.
        /// </summary>
        /// <param name="visible">true if this block should be visible, false if it should be hidden.</param>
        void ISecondaryBlock.SetVisible( bool visible )
        {
            pnlDetails.Visible = false;
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Shows the read-only attribute values.
        /// </summary>
        protected void ShowDetails()
        {
            IHasAttributes entity = ContextEntity() as IHasAttributes;

            if ( entity != null )
            {
                phAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddDisplayControls( entity, phAttributes, null, false, false );
            }
        }

        /// <summary>
        /// Shows the edit attributes controls.
        /// </summary>
        /// <param name="setValues">If true then the values are set from the database, otherwise the postback values will be used.</param>
        protected void ShowEdit( bool setValues )
        {
            IHasAttributes entity = ContextEntity() as IHasAttributes;

            if ( entity != null )
            {
                phEditAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( entity, phEditAttributes, true, BlockValidationGroup );
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            pnlEdit.Visible = false;
            pnlView.Visible = true;

            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            pnlView.Visible = false;
            pnlEdit.Visible = true;

            ShowEdit( true );
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            pnlView.Visible = true;
            pnlEdit.Visible = false;

            IHasAttributes entity = ContextEntity() as IHasAttributes;
            Rock.Attribute.Helper.GetEditValues( phEditAttributes, entity );
            entity.SaveAttributeValues();

            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlView.Visible = true;
            pnlEdit.Visible = false;

            ShowDetails();
        }

        #endregion
    }
}