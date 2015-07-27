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

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Person Badge Detail" )]
    [Category( "CRM" )]
    [Description( "Shows the details of a particular person badge." )]

    public partial class PersonBadgeDetail : RockBlock
    {

        #region Properties

        public int PersonBadgeId
        {
            get { return ViewState["PersonBadgeId"] as int? ?? 0; }
            set { ViewState["PersonBadgeId"] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                lActionTitle.Text = ActionTitle.Add( PersonBadge.FriendlyTypeName ).FormatAsHtmlTitle();

                PersonBadgeId = PageParameter( "PersonBadgeId" ).AsInteger();
                if ( PersonBadgeId != 0 )
                {
                    var personBadge = new PersonBadgeService( new RockContext() ).Get( PersonBadgeId );
                    if ( personBadge != null )
                    {
                        lActionTitle.Text = ActionTitle.Edit( personBadge.Name ).FormatAsHtmlTitle();

                        tbName.Text = personBadge.Name;
                        tbDescription.Text = personBadge.Description;
                        if ( personBadge.EntityTypeId.HasValue )
                        {
                            var badgeType = EntityTypeCache.Read( personBadge.EntityTypeId.Value );
                            compBadgeType.SelectedValue = badgeType.Guid.ToString().ToUpper();
                        }

                        BuildEditControls( personBadge, true );
                    }
                }
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( compBadgeType.SelectedValue ) )
                {
                    var badgeType = EntityTypeCache.Read( compBadgeType.SelectedValue.AsGuid() );
                    if ( badgeType != null )
                    {
                        var personBadge = new PersonBadge { EntityTypeId = badgeType.Id };
                        BuildEditControls( personBadge, false );
                    }
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the compBadgeType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void compBadgeType_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( compBadgeType.SelectedValue ) )
            {
                var badgeType = EntityTypeCache.Read( compBadgeType.SelectedValue.AsGuid() );
                if ( badgeType != null )
                {
                    var personBadge = new PersonBadge { EntityTypeId = badgeType.Id };
                    BuildEditControls( personBadge, true );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            PersonBadge personBadge = null;
            var rockContext = new RockContext();
            PersonBadgeService personBadgeService = new PersonBadgeService( rockContext );

            if ( PersonBadgeId != 0 )
            {
                personBadge = personBadgeService.Get( PersonBadgeId );
            }

            if ( personBadge == null )
            {
                personBadge = new PersonBadge();
                personBadgeService.Add( personBadge );
            }

            personBadge.Name = tbName.Text;
            personBadge.Description = tbDescription.Text;

            if ( !string.IsNullOrWhiteSpace( compBadgeType.SelectedValue ) )
            {
                var badgeType = EntityTypeCache.Read( compBadgeType.SelectedValue.AsGuid() );
                if ( badgeType != null )
                {
                    personBadge.EntityTypeId = badgeType.Id;
                }
            }

            personBadge.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( phAttributes, personBadge );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !personBadge.IsValid )
            {
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                personBadge.SaveAttributeValues( rockContext );
            } );

            PersonBadgeCache.Flush( personBadge.Id );

            NavigateToParentPage();
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

        #endregion

        #region Methods

        private void BuildEditControls(PersonBadge personBadge, bool setValues)
        {
            personBadge.LoadAttributes();
            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( personBadge, phAttributes, setValues, "", new List<string> { "Active", "Order" } );
        }

        #endregion
    }
}