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
    [DisplayName( "SMS Pipeline" )]
    [Category( "Communication" )]
    [Description( "Configures the pipeline that processes an incoming SMS message." )]
    public partial class SmsPipeline : RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/dragula.min.js" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindComponents();
                BindActions();

                //
                // This must come after BindComponents so that the SmsActionContainer will
                // have been initialized already and any new attributes created.
                //
                var smsActionEntityTypeId = EntityTypeCache.Get( typeof( SmsAction ) ).Id;
                var attributes = AttributeCache.All()
                    .Where( a => a.EntityTypeId == smsActionEntityTypeId )
                    .Where( a => a.Key == "Order" || a.Key == "Active" );
                avcAttributes.ExcludedAttributes = attributes.ToArray();
                avcAttributes.ExcludedCategoryNames = new string[] { "Filter" };
                avcFilters.IncludedCategoryNames = new string[] { "Filter" };

                tbFromNumber.Text = "+16235553322"; // Ted Decker's cell
                tbToNumber.Text = "+15559991234"; // Fake church number
            }
            else
            {
                if ( Request["__EVENTTARGET"].ToStringSafe() == lbDragCommand.ClientID )
                {
                    ProcessDragEvents();
                }

                if ( hfIsTestingDrawerOpen.Value.AsBoolean() )
                {
                    divTestingDrawer.Style.Add( "display", null );
                }
                else
                {
                    divTestingDrawer.Style.Add( "display", "none" );
                }
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
                    a.Name,
                    a.SmsActionComponentEntityTypeId,
                    a.IsActive,
                    a.ContinueAfterProcessing,
                    Component = SmsActionContainer.GetComponent( EntityTypeCache.Get( a.SmsActionComponentEntityTypeId ).Name )
                } )
                .ToList();

            rptrActions.DataSource = actions;
            rptrActions.DataBind();
        }

        /// <summary>
        /// Processes the drag events.
        /// </summary>
        private void ProcessDragEvents()
        {
            string argument = Request["__EVENTARGUMENT"].ToStringSafe();
            var segments = argument.SplitDelimitedValues();

            //
            // Check for the event to add a new action.
            //
            if ( segments.Length == 3 && segments[0] == "add-action" )
            {
                var actionComponent = SmsActionContainer.GetComponent( segments[1] );
                var order = segments[2].AsInteger();

                var rockContext = new RockContext();
                var smsActionService = new SmsActionService( rockContext );

                var action = new SmsAction
                {
                    Name = actionComponent.Title,
                    IsActive = true,
                    Order = order,
                    SmsActionComponentEntityTypeId = actionComponent.TypeId
                };

                smsActionService.Queryable()
                    .Where( a => a.Order >= order )
                    .ToList()
                    .ForEach( a => a.Order += 1 );

                smsActionService.Add( action );

                rockContext.SaveChanges();

                BindActions();

                SmsActionCache.Clear();
            }

            //
            // Check for the event to drag-reorder actions.
            //
            else if ( segments.Length == 3 && segments[0] == "reorder-action" )
            {
                var rockContext = new RockContext();
                var smsActionService = new SmsActionService( rockContext );

                var actions = smsActionService.Queryable()
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Id )
                    .ToList();

                smsActionService.Reorder( actions, segments[1].AsInteger(), segments[2].AsInteger() );
                rockContext.SaveChanges();

                BindActions();

                SmsActionCache.Clear();
            }
        }

        #endregion

        #region Event Handlers

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
                tbName.Text = action.Name;
                cbActive.Checked = action.IsActive;
                cbContinue.Checked = action.ContinueAfterProcessing;

                avcFilters.AddEditControls( action );
                avcAttributes.AddEditControls( action );

                pnlEditAction.Visible = true;

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

            action.Name = tbName.Text;
            action.IsActive = cbActive.Checked;
            action.ContinueAfterProcessing = cbContinue.Checked;

            avcFilters.GetEditValues( action );
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

        #region Test Code

        /// <summary>
        /// Handles the Click event of the lbSendMessage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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

                var outcomes = SmsActionService.ProcessIncomingMessage( message );
                var response = SmsActionService.GetResponseFromOutcomes( outcomes );

                var stringBuilder = new StringBuilder();

                if ( outcomes != null )
                {
                    foreach ( var outcome in outcomes )
                    {
                        if ( outcome != null )
                        {
                            stringBuilder.AppendLine( outcome.ActionName );
                            stringBuilder.AppendLine( string.Format( "\tShould Process = {0}", outcome.ShouldProcess ) );

                            if ( outcome.Response != null && !outcome.Response.Message.IsNullOrWhiteSpace() )
                            {
                                stringBuilder.AppendLine( string.Format( "\tResponse = {0}", outcome.Response.Message ) );
                            }

                            if (!outcome.ErrorMessage.IsNullOrWhiteSpace())
                            {
                                stringBuilder.AppendLine( string.Format( "\tError = {0}", outcome.ErrorMessage ) );
                            }

                            if ( outcome.Exception != null )
                            {
                                stringBuilder.AppendLine( string.Format( "\tException = {0}", outcome.Exception.Message ) );
                            }

                            stringBuilder.AppendLine();
                        }
                    }
                }

                preOutcomes.InnerText = stringBuilder.ToString();

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
                preOutcomes.InnerText = string.Empty;
            }
        }

        #endregion
    }
}
