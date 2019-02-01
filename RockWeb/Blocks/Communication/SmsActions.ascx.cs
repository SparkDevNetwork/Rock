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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Communication.SmsActions;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "SMS Actions" )]
    [Category( "Communication" )]
    [Description( "Configures the SMS Actions that run when an incoming message is received." )]
    public partial class SmsActions : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                var smsActionEntityTypeId = EntityTypeCache.Get( typeof( SmsAction ) ).Id;
                var attributes = AttributeCache.All()
                    .Where( a => a.EntityTypeId == smsActionEntityTypeId )
                    .Where( a => a.Key == "Order" || a.Key == "Active" );
                avcAttributes.ExcludedAttributes = attributes.ToArray();

                BindComponents();
                BindActions();

                tbFromNumber.Text = "+15551234567";
                tbToNumber.Text = "+15559991234";
            }

            base.OnLoad( e );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the components repeater.
        /// </summary>
        private void BindComponents()
        {
            var components = SmsActionContainer.Instance.Components.Values
                .Select( a => a.Value )
                .Select( a => new
                {
                    a.Title,
                    a.Description,
                    a.IconCssClass,
                    Id = a.TypeName
                } );

            rptrComponents.DataSource = components;
            rptrComponents.DataBind();
        }

        /// <summary>
        /// Binds the actions repeater.
        /// </summary>
        private void BindActions()
        {
            var rockContext = new RockContext();
            var smsActionService = new SmsActionService( rockContext );

            var actions = smsActionService.Queryable()
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Id )
                .ToList()
                .Select( a => new
                {
                    a.Id,
                    a.Title,
                    a.SmsActionComponentEntityTypeId,
                    a.IsActive,
                    a.ContinueAfterProcessing,
                    Component = SmsActionContainer.GetComponent( EntityTypeCache.Get( a.SmsActionComponentEntityTypeId ).Name )
                } )
                .ToList();

            rptrActions.DataSource = actions;
            rptrActions.DataBind();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the ItemCommand event of the rptrComponents control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptrComponents_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var actionComponent = SmsActionContainer.GetComponent( e.CommandArgument.ToString() );

            if ( actionComponent != null )
            {
                if ( e.CommandName == "AddComponent" )
                {
                    var rockContext = new RockContext();
                    var smsActionService = new SmsActionService( rockContext );

                    int? lastOrder = smsActionService.Queryable()
                        .OrderByDescending( a => a.Order )
                        .Select( a => a.Order )
                        .FirstOrDefault();

                    var action = new SmsAction
                    {
                        Title = actionComponent.Title,
                        IsActive = true,
                        Order = ( lastOrder ?? -1 ) + 1,
                        SmsActionComponentEntityTypeId = actionComponent.TypeId
                    };

                    smsActionService.Add( action );
                    rockContext.SaveChanges();

                    BindActions();
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptrActions control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptrActions_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var rockContext = new RockContext();
            var smsActionService = new SmsActionService( rockContext );
            var action = smsActionService.Get( e.CommandArgument.ToString().AsInteger() );

            if ( e.CommandName == "EditAction" )
            {
                var component = SmsActionContainer.GetComponent( EntityTypeCache.Get( action.SmsActionComponentEntityTypeId ).Name );

                hfEditActionId.Value = action.Id.ToString();
                lActionType.Text = component.Title;
                tbTitle.Text = action.Title;
                cbActive.Checked = action.IsActive;
                cbContinue.Checked = action.ContinueAfterProcessing;

                avcAttributes.AddEditControls( action );

                pnlEditAction.Visible = true;

                BindActions();
            }
            else if ( e.CommandName == "MoveUp" )
            {
                var actions = smsActionService.Queryable()
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Id )
                    .ToList();
                int index = actions.IndexOf( action );

                smsActionService.Reorder( actions, index, index - 1 );
                rockContext.SaveChanges();

                BindActions();
            }
            else if ( e.CommandName == "MoveDown" )
            {
                var actions = smsActionService.Queryable()
                    .OrderBy( a => a.Order )
                    .OrderBy( a => a.Id )
                    .ToList();
                int index = actions.IndexOf( action );

                smsActionService.Reorder( actions, index, index + 1 );
                rockContext.SaveChanges();

                BindActions();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveActionSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveActionSettings_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var action = new SmsActionService( rockContext ).Get( hfEditActionId.Value.AsInteger() );

            action.Title = tbTitle.Text;
            action.IsActive = cbActive.Checked;
            action.ContinueAfterProcessing = cbContinue.Checked;

            avcAttributes.GetEditValues( action );

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                action.SaveAttributeValues();
            } );

            SmsActionCache.Clear();

            pnlEditAction.Visible = false;
            hfEditActionId.Value = string.Empty;
            BindActions();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelActionSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelActionSettings_Click( object sender, EventArgs e )
        {
            hfEditActionId.Value = string.Empty;
            BindActions();

            pnlEditAction.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteAction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDeleteAction_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var smsActionService = new SmsActionService( rockContext );
            var action = smsActionService.Get( hfEditActionId.Value.AsInteger() );

            smsActionService.Delete( action );
            rockContext.SaveChanges();

            pnlEditAction.Visible = false;

            hfEditActionId.Value = string.Empty;
            BindActions();
        }

        #endregion

        protected void lbSendMessage_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( tbSendMessage.Text ) )
            {
                var message = new SmsMessage
                {
                    FromNumber = tbFromNumber.Text,
                    ToNumber = tbToNumber.Text,
                    Message = tbSendMessage.Text
                };

                if ( message.FromNumber.StartsWith( "+" ) )
                {
                    message.FromPerson = new PersonService( new RockContext() ).GetPersonFromMobilePhoneNumber( message.FromNumber.Substring(1) );
                }
                else
                {
                    message.FromPerson = new PersonService( new RockContext() ).GetPersonFromMobilePhoneNumber( message.FromNumber );
                }

                var response = SmsActionService.ProcessIncomingMessage( message );

                if ( response != null )
                {
                    lResponse.Text = response.Message;
                }
                else
                {
                    lResponse.Text = "--No Response--";
                }
            }
            else
            {
                lResponse.Text = "--Empty Message--";
            }
        }
    }
}