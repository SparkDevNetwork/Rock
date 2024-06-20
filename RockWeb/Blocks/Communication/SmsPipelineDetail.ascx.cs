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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Communication;
using Rock.Communication.Medium;
using Rock.Communication.SmsActions;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "SMS Pipeline Detail" )]
    [Category( "Communication" )]
    [Description( "Configures the pipeline that processes an incoming SMS message." )]
    [Rock.SystemGuid.BlockTypeGuid( "44C32EB7-4DA3-4577-AC41-E3517442E269" )]
    public partial class SmsPipelineDetail : RockBlock
    {
        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string EntityId = "SmsPipelineId";
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( "~/Styles/Blocks/Shared/DragPallet.css", true );
            RockPage.AddCSSLink( "~/Styles/Blocks/Communication/SmsPipelineDetail.css", true );

            RockPage.AddScriptLink( "~/Scripts/dragula.min.js" );
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", SmsPipeline.FriendlyTypeName );
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
                InitializeAdvancedSettingsToggle();

                int? smsPipelineId = GetSmsPipelineId();
                SmsPipeline smsPipeline = null;

                if ( smsPipelineId == null || smsPipelineId == 0 )
                {
                    BindEditDetails( null );
                }
                else
                {
                    var smsPipelineService = new SmsPipelineService( new RockContext() );
                    smsPipeline = GetSmsPipeline( smsPipelineId.Value, smsPipelineService, "SmsActions" );
                    BindReadOnlyDetails( smsPipeline );
                }

                BindActions( smsPipeline );

                // This must come after BindComponents so that the SmsActionContainer will
                // have been initialized already and any new attributes created.
                var attributes = AttributeCache.AllForEntityType<SmsAction>()
                    .Where( a => a.Key == "Order" || a.Key == "Active" );
                avcAttributes.ExcludedAttributes = attributes.ToArray();
                avcAttributes.ExcludedCategoryNames = new string[] { SmsActionComponent.BaseAttributeCategories.Filters };
                avcFilters.IncludedCategoryNames = new string[] { SmsActionComponent.BaseAttributeCategories.Filters };
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
        /// Binds the pipeline detail read only section.
        /// </summary>
        /// <param name="smsPipeline">The SMS pipeline.</param>
        private void BindReadOnlyDetails( SmsPipeline smsPipeline )
        {
            divSmsActionsPanel.Visible = true;
            divEditDetails.Visible = false;

            pdAuditDetails.Visible = true;
            pdAuditDetails.SetEntity( smsPipeline, ResolveRockUrl( "~" ) );

            hlInactive.Visible = !smsPipeline.IsActive;

            lSmsPipelineDescription.Text = smsPipeline.Description;
            lSmsName.Text = smsPipeline.Name;

            var smsMedium = new Sms();
            var smsTransport = smsMedium.Transport as ISmsPipelineWebhook;
            lWebhookUrl.Visible = false;
            if ( smsTransport != null )
            {
                var globalAttributes = GlobalAttributesCache.Get();
                string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" );
                lWebhookUrl.Text = string.Format( "{0}{1}?{2}={3}", publicAppRoot, smsTransport.SmsPipelineWebhookPath, PageParameterKey.EntityId, GetSmsPipelineId() );
                lWebhookUrl.Visible = true;
            }
        }

        /// <summary>
        /// Binds the pipeline detail edit section.
        /// </summary>
        /// <param name="smsPipeline">The SMS pipeline.</param>
        private void BindEditDetails( SmsPipeline smsPipeline )
        {
            divReadOnlyDetails.Visible = false;
            divSmsActionsPanel.Visible = false;

            if ( smsPipeline == null )
            {
                cbPipelineIsActive.Checked = true;
                pdAuditDetails.Visible = false;
                return;
            }

            pdAuditDetails.Visible = true;
            pdAuditDetails.SetEntity( smsPipeline, ResolveRockUrl( "~" ) );
            divEditDetails.Visible = true;
            hlInactive.Visible = true;

            tbPipelineName.Text = smsPipeline.Name;
            cbPipelineIsActive.Checked = smsPipeline.IsActive;
            tbPipelineDescription.Text = smsPipeline.Description;
        }

        /// <summary>
        /// Binds the pipeline actions section.
        /// </summary>
        private void BindActions()
        {
            var smsPipelineId = GetSmsPipelineId();
            if ( smsPipelineId == null || smsPipelineId == 0 )
            {
                return;
            }

            var smsPipelineService = new SmsPipelineService( new RockContext() );
            SmsPipeline smsPipeline = GetSmsPipeline( smsPipelineId.Value, smsPipelineService, "SmsActions" );
            BindActions( smsPipeline );
        }

        /// <summary>
        /// Binds the pipeline actions section for a specific Sms Pipeline.
        /// </summary>
        /// <param name="smsPipeline">The SMS pipeline.</param>
        private void BindActions( SmsPipeline smsPipeline )
        {
            if ( smsPipeline == null )
            {
                divSmsActionsPanel.Visible = false;
                return;
            }

            var actions = smsPipeline.SmsActions
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

            if ( actions.Any() )
            {
                olActions.RemoveCssClass( "drag-container-empty" );
            }
            else
            {
                olActions.AddCssClass( "drag-container-empty" );
            }

            rptrActions.DataSource = actions;
            rptrActions.DataBind();
        }

        /// <summary>
        /// Gets the SmsPipeline for the specified smsPipelineId
        /// </summary>
        /// <param name="smsPipelineId">The SMS pipeline identifier.</param>
        /// <param name="smsPipelineService">The SMS pipeline service.</param>
        /// <param name="includes">The includes.</param>
        /// <returns></returns>
        private SmsPipeline GetSmsPipeline( int smsPipelineId, SmsPipelineService smsPipelineService, string includes = "" )
        {
            return smsPipelineService
                        .Queryable( includes )
                        .Where( p => p.Id == smsPipelineId )
                        .FirstOrDefault();
        }

        /// <summary>
        /// Initializes the advanced settings toggle.
        /// </summary>
        private void InitializeAdvancedSettingsToggle()
        {
            var doShow = hfShowAdvancedSettings.Value.ToLower() == "true";
            divAdvanced.Visible = doShow;
            lbToggleAdvancedSettings.Text = doShow ? "Hide Advanced Settings" : "Advanced Settings";
        }

        private void ShowAdvancedSettings()
        {
            var doShow = hfShowAdvancedSettings.Value.ToLower() != "true";
            hfShowAdvancedSettings.Value = doShow ? "true" : string.Empty;

            InitializeAdvancedSettingsToggle();
        }

        /// <summary>
        /// Processes the drag events.
        /// </summary>
        private void ProcessDragEvents()
        {
            string argument = Request["__EVENTARGUMENT"].ToStringSafe();
            var segments = argument.SplitDelimitedValues();
            var smsPipelineId = GetSmsPipelineId();

            if ( smsPipelineId == null || segments.Length != 3 )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var smsActionService = new SmsActionService( rockContext );

                var actions = smsActionService
                    .Queryable()
                    .Where( a => a.SmsPipelineId == smsPipelineId )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Id )
                    .ToList();

                // Reset order actions to eliminate gaps.
                for ( var i = 0; i < actions.Count; i++ )
                {
                    actions[i].Order = i;
                }

                // Check for the event to add a new action.
                if ( segments[0] == "add-action" )
                {
                    var actionComponent = SmsActionContainer.GetComponent( segments[1] );
                    var order = segments[2].AsInteger();

                    var action = new SmsAction
                    {
                        SmsPipelineId = smsPipelineId.Value,
                        Name = actionComponent.Title,
                        IsActive = true,
                        Order = order,
                        SmsActionComponentEntityTypeId = actionComponent.TypeId
                    };

                    actions
                        .Where( a => a.Order >= order )
                        .ToList()
                        .ForEach( a => a.Order += 1 );

                    smsActionService.Add( action );
                }
                else if ( segments[0] == "reorder-action" )
                {
                    // Check for the event to drag-reorder actions.
                    smsActionService.Reorder( actions, segments[1].AsInteger(), segments[2].AsInteger() );
                }

                rockContext.SaveChanges();
                BindActions();
            }
        }

        /// <summary>
        /// Gets the SmsPipelineId and returns null if none is found.
        /// </summary>
        /// <returns></returns>
        private int? GetSmsPipelineId()
        {
            return PageParameter( PageParameterKey.EntityId ).AsIntegerOrNull();
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
            var smsActionService = new SmsActionService( new RockContext() );
            var action = smsActionService.Get( e.CommandArgument.ToString().AsInteger() );

            if ( e.CommandName == "EditAction" )
            {
                var component = SmsActionContainer.GetComponent( EntityTypeCache.Get( action.SmsActionComponentEntityTypeId ).Name );

                hfEditActionId.Value = action.Id.ToString();
                lActionType.Text = component.Title;
                tbName.Text = action.Name;
                cbActive.Checked = action.IsActive;
                cbContinue.Checked = action.ContinueAfterProcessing;
                dpExpireDate.SelectedDate = action.ExpireDate;
                cbLogInteraction.Checked = action.IsInteractionLoggedAfterProcessing;

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
            action.ExpireDate = dpExpireDate.SelectedDate;
            action.IsInteractionLoggedAfterProcessing = cbLogInteraction.Checked;

            avcFilters.GetEditValues( action );
            avcAttributes.GetEditValues( action );

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                action.SaveAttributeValues();
            } );

            pnlEditAction.Visible = false;
            hfEditActionId.Value = string.Empty;
            BindActions();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var smsPipelineId = GetSmsPipelineId();
            var rockContext = new RockContext();
            var smsPipelineService = new SmsPipelineService( rockContext );

            SmsPipeline smsPipeline = null;

            if ( smsPipelineId == null || smsPipelineId == 0 )
            {
                smsPipeline = new SmsPipeline();
                smsPipelineService.Add( smsPipeline );
            }
            else
            {
                smsPipeline = GetSmsPipeline( smsPipelineId.Value, smsPipelineService );
            }

            if ( smsPipeline == null )
            {
                return;
            }

            smsPipeline.Name = tbPipelineName.Text;
            smsPipeline.IsActive = cbPipelineIsActive.Checked;
            smsPipeline.Description = tbPipelineDescription.Text;

            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams[PageParameterKey.EntityId] = smsPipeline.Id.ToString();

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            var smsPipelineId = GetSmsPipelineId();
            if ( smsPipelineId == null || smsPipelineId == 0 )
            {
                NavigateToParentPage();
            }
            else
            {
                var qryParams = new Dictionary<string, string>();
                qryParams[PageParameterKey.EntityId] = smsPipelineId.ToString();

                NavigateToPage( RockPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var smsPipelineId = GetSmsPipelineId();
            if ( smsPipelineId == null )
            {
                return;
            }

            var smsPipelineService = new SmsPipelineService( new RockContext() );
            var smsPipeline = GetSmsPipeline( smsPipelineId.Value, smsPipelineService, "SmsActions" );

            BindEditDetails( smsPipeline );
            BindActions( smsPipeline );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var smsPipelineId = GetSmsPipelineId();

            if ( smsPipelineId == null || smsPipelineId == 0 )
            {
                return;
            }

            var rockContext = new RockContext();
            var smsPipelineService = new SmsPipelineService( rockContext );
            var smsPipeline = smsPipelineService.Get( smsPipelineId.Value );
            smsPipelineService.Delete( smsPipeline );
            rockContext.SaveChanges();

            NavigateToParentPage();
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

        /// <summary>
        /// Handles the Click event of the lbToggleAdvancedSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbToggleAdvancedSettings_Click( object sender, EventArgs e )
        {
            ShowAdvancedSettings();
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
            var smsPipelineId = GetSmsPipelineId();

            if ( !string.IsNullOrWhiteSpace( tbSendMessage.Text ) && smsPipelineId != null )
            {
                var message = new SmsMessage
                {
                    FromNumber = tbFromNumber.Text,
                    ToNumber = tbToNumber.Text,
                    Message = tbSendMessage.Text
                };

                if ( message.FromNumber.StartsWith( "+" ) )
                {
                    message.FromPerson = new PersonService( new RockContext() ).GetPersonFromMobilePhoneNumber( message.FromNumber.Substring( 1 ), true );
                }
                else
                {
                    message.FromPerson = new PersonService( new RockContext() ).GetPersonFromMobilePhoneNumber( message.FromNumber, true );
                }

                var outcomes = SmsActionService.ProcessIncomingMessage( message, smsPipelineId.Value );
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

                            if ( outcome.IsInteractionLogged )
                            {
                                stringBuilder.AppendLine( "\tInteraction Logged = True" );
                            }

                            if ( !outcome.ErrorMessage.IsNullOrWhiteSpace() )
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
                lResponse.Text = "--Empty Message or No SMS Pipeline Id--";
                preOutcomes.InnerText = string.Empty;
            }
        }

        #endregion
    }
}