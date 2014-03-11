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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "Template List" )]
    [Category( "Communication" )]
    [Description( "Lists the available communicaiton templates that can used when creating new communications." )]

    [LinkedPage("Detail Page")]
    public partial class TemplateList : Rock.Web.UI.RockBlock
    {

        #region Fields

        private bool canEdit = false;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            BindFilter();
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;

            gCommunication.DataKeyNames = new string[] { "Id" };
            gCommunication.Actions.ShowAdd = true;
            gCommunication.Actions.AddClick += Actions_AddClick;
            gCommunication.GridRebind += gCommunication_GridRebind;

            // The created by column/filter should only be displayed if user is allowed to approve
            canEdit = this.IsUserAuthorized( "Edit" );
            ppOwner.Visible = canEdit;
            gCommunication.Columns[0].Visible = canEdit;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Channel", cpChannel.SelectedValue );
            if ( canEdit )
            {
                rFilter.SaveUserPreference( "Owner", ppOwner.PersonId.ToString() );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the filter display for each saved user value
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void rFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Channel":

                    int entityTypeId = 0;
                    if ( int.TryParse( e.Value, out entityTypeId ) )
                    {
                        var entity = EntityTypeCache.Read( entityTypeId );
                        if ( entity != null )
                        {
                            e.Value = entity.FriendlyName;
                        }
                    }

                    break;

                case "Owner":

                    int personId = 0;
                    if ( int.TryParse( e.Value, out personId ) && personId != 0 )
                    {
                        var personService = new PersonService();
                        var person = personService.Get( personId );
                        if ( person != null )
                        {
                            e.Value = person.FullName;
                        }
                    }

                    break;
            }
        }

        void Actions_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "TemplateId", 0 );
        }

        /// <summary>
        /// Handles the RowSelected event of the gCommunication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void gCommunication_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "TemplateId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gCommunication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gCommunication_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                var service = new CommunicationTemplateService();
                var template = service.Get( e.RowKeyId );
                if ( template != null )
                {
                    string errorMessage;
                    if ( !service.CanDelete( template, out errorMessage ) )
                    {
                        maGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    service.Delete( template, CurrentPersonAlias );
                    service.Save( template, CurrentPersonAlias );
                }
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gCommunication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void gCommunication_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        private void BindFilter()
        {
            if ( cpChannel.Items[0].Value != string.Empty )
            {
                cpChannel.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
            }

            if ( !Page.IsPostBack )
            {
                if ( !canEdit )
                {
                    rFilter.SaveUserPreference( "Owner", string.Empty );
                }

                cpChannel.SelectedValue = rFilter.GetUserPreference( "Channel" );

                int personId = 0;
                if ( int.TryParse( rFilter.GetUserPreference( "Owner" ), out personId ) )
                {
                    var personService = new PersonService();
                    var person = personService.Get( personId );
                    if ( person != null )
                    {
                        ppOwner.SetValue( person );
                    }
                }
            }
        }

        private void BindGrid()
        {
            using ( new UnitOfWorkScope() )
            {
                var communications = new CommunicationTemplateService()
                    .Queryable("OwnerPersonAlias.Person");

                Guid entityTypeGuid = Guid.Empty;
                if ( Guid.TryParse( rFilter.GetUserPreference( "Channel" ), out entityTypeGuid ) )
                {
                    communications = communications
                        .Where( c => 
                            c.ChannelEntityType != null && 
                            c.ChannelEntityType.Guid.Equals( entityTypeGuid ) );
                }

                if ( canEdit )
                {
                    int personId = 0;
                    if ( int.TryParse( rFilter.GetUserPreference( "Created By" ), out personId ) && personId != 0 )
                    {
                        communications = communications
                            .Where( c => 
                                c.OwnerPersonAlias != null &&
                                c.OwnerPersonAlias.PersonId == personId );
                    }
                }
                else
                {
                    communications = communications
                        .Where( c => 
                            c.OwnerPersonAlias != null &&
                            c.OwnerPersonAlias.PersonId == CurrentPersonId );
                }

                var sortProperty = gCommunication.SortProperty;

                if ( sortProperty != null )
                {
                    communications = communications.Sort( sortProperty );
                }
                else
                {
                    communications = communications.OrderBy( c => c.Name );
                }

                gCommunication.DataSource = communications.ToList();
                gCommunication.DataBind();
            }

        }

        #endregion

    }
}