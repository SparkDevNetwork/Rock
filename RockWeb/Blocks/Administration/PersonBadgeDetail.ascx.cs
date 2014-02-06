// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Collections.Generic;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.PersonProfile;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Administration
{
    [DisplayName( "Person Badge Detail" )]
    [Category( "Administration" )]
    [Description( "Shows the details of a particular person badge." )]

    public partial class PersonBadgeDetail : RockBlock, IDetailBlock
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
                string itemId = PageParameter( "PersonBadgeId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "PersonBadgeId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
            else
            {
                if ( pnlDetails.Visible )
                {
                    var badgeType = EntityTypeCache.Read( compBadgeType.SelectedValue.AsGuid() );
                    if ( badgeType  != null )
                    {
                        var personBadge = new PersonBadge { EntityTypeId = badgeType.Id };
                        personBadge.LoadAttributes();
                        phAttributes.Controls.Clear();
                        Rock.Attribute.Helper.AddEditControls( personBadge, phAttributes, false );
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
            if ( !itemKey.Equals( "PersonBadgeId" ) )
            {
                return;
            }

            var PersonBadgeService = new PersonBadgeService();
            PersonBadge PersonBadge = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                PersonBadge = PersonBadgeService.Get( itemKeyValue );
            }

            if ( PersonBadge != null )
            {
                lActionTitle.Text = ActionTitle.Edit( PersonBadge.Name).FormatAsHtmlTitle();
            }
            else
            {
                PersonBadge = new PersonBadge { Id = 0 };
                lActionTitle.Text = ActionTitle.Add(  PersonBadge.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            ShowPersonBadgeDetail( PersonBadge );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="PersonBadge">The binary file.</param>
        public void ShowPersonBadgeDetail( PersonBadge PersonBadge )
        {
            pnlDetails.Visible = true;
            hfPersonBadgeId.SetValue( PersonBadge.Id );
            
            if ( PersonBadge.EntityType != null )
            {
                compBadgeType.SelectedValue = PersonBadge.EntityType.Guid.ToString();
            }

            tbName.Text = PersonBadge.Name;
            tbDescription.Text = PersonBadge.Description;

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( PersonBadge.FriendlyTypeName );
            }

            phAttributes.Controls.Clear();
            PersonBadge.LoadAttributes();

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( PersonBadge.FriendlyTypeName );
                btnCancel.Text = "Close";
                Rock.Attribute.Helper.AddDisplayControls( PersonBadge, phAttributes );
            }
            else
            {
                Rock.Attribute.Helper.AddEditControls( PersonBadge, phAttributes, true );
            }

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;

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
            PersonBadge PersonBadge;
            PersonBadgeService PersonBadgeService = new PersonBadgeService();
            AttributeService attributeService = new AttributeService();

            int PersonBadgeId = int.Parse( hfPersonBadgeId.Value );

            if ( PersonBadgeId == 0 )
            {
                PersonBadge = new PersonBadge();
                PersonBadgeService.Add( PersonBadge, CurrentPersonAlias );
            }
            else
            {
                PersonBadge = PersonBadgeService.Get( PersonBadgeId );
            }

            PersonBadge.Name = tbName.Text;
            PersonBadge.Description = tbDescription.Text;

            if ( !string.IsNullOrWhiteSpace( compBadgeType.SelectedValue ) )
            {
                var entityTypeService = new EntityTypeService();
                var badgeType = entityTypeService.Get( new Guid( compBadgeType.SelectedValue ) );

                if ( badgeType != null )
                {
                    PersonBadge.EntityTypeId = badgeType.Id;
                }
            }

            PersonBadge.LoadAttributes();
            Rock.Attribute.Helper.GetEditValues( phAttributes, PersonBadge );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !PersonBadge.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                PersonBadgeService.Save( PersonBadge, CurrentPersonAlias );
                Rock.Attribute.Helper.SaveAttributeValues( PersonBadge, CurrentPersonAlias );
            } );

            NavigateToParentPage();
        }

        #endregion

    }
}