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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Humanizer;
using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Block used to register for registration instance.
    /// </summary>
    [DisplayName( "Registration Entry" )]
    [Category( "Event" )]
    [Description( "Block used to register for registration instance." )]

    public partial class RegistrationEntry : RockBlock
    {
        #region Fields

        // Page (query string) parameter names
        private readonly const string REGISTRATION_ID_PARAM_NAME = "RegistrationId";
        private readonly const string REGISTRANT_INSTANCE_ID_PARAM_NAME = "RegistrationInstanceId";

        // Viewstate keys
        private readonly const string REGISTRATION_STATE_KEY = "RegistrationState";
        private readonly const string CURRENT_PANEL_KEY = "CurrentPanel";
        private readonly const string CURRENT_REGISTRANT_INDEX_KEY = "CurrentRegistrantIndex";
        private readonly const string CURRENT_FORM_INDEX_KEY = "CurrentFormIndex";

        #endregion

        #region Properties

        // The current registration in progress
        private Registration RegistrationState { get; set; }

        // The current panel to display ( HowMany
        private int CurrentPanel { get; set; }
        private int CurrentRegistrantIndex { get; set; }
        private int CurrentFormIndex { get; set; }

        /// <summary>
        /// If the registration template allows multiple registrants per registration, returns the maximum allowed
        /// </summary>
        private int MaxRegistrants
        {
            get
            {
                if ( RegistrationState != null &&
                    RegistrationState.RegistrationInstance != null &&
                    RegistrationState.RegistrationInstance.RegistrationTemplate != null )
                {
                    if ( RegistrationState.RegistrationInstance.RegistrationTemplate.AllowMultipleRegistrants )
                    {
                        return RegistrationState.RegistrationInstance.RegistrationTemplate.MaxRegistrants;
                    }
                }

                return 1;
            }
        }

        /// <summary>
        /// Gets the minimum number of registrants allowed. Most of the time this is one, except for an existing
        /// registration that has existing registrants. The minimum in this case is the number of existing registrants
        /// </summary>
        private int MinRegistrants
        {
            get
            {
                int existingRegistrants = 0;
                if ( RegistrationState != null )
                {
                    existingRegistrants = RegistrationState.Registrants.Where( r => r.Id > 0 ).Count();
                }

                return existingRegistrants > 0 ? existingRegistrants : 1;
            }
        }

        /// <summary>
        /// Gets the number of registrants for the current registration
        /// </summary>
        private int RegistrantCount
        {
            get
            {
                if ( RegistrationState != null &&
                    RegistrationState.Registrants != null )
                {
                    return RegistrationState.Registrants.Count;
                }
                return 0;
            }
        }


        /// <summary>
        /// Gets the number of forms for the current registration template.
        /// </summary>
        private int FormCount
        {
            get
            {
                if ( RegistrationState != null &&
                    RegistrationState.RegistrationInstance != null &&
                    RegistrationState.RegistrationInstance.RegistrationTemplate != null &&
                    RegistrationState.RegistrationInstance.RegistrationTemplate.Forms != null )
                {
                    return RegistrationState.RegistrationInstance.RegistrationTemplate.Forms.Count;
                }
                return 0;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState[REGISTRATION_STATE_KEY] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GetRegistration();
            }
            else
            {
                RegistrationState = JsonConvert.DeserializeObject<Registration>( json );
            }

            CurrentPanel = ViewState[CURRENT_PANEL_KEY] as int? ?? 0;
            CurrentRegistrantIndex = ViewState[CURRENT_REGISTRANT_INDEX_KEY] as int? ?? 0;
            CurrentFormIndex = ViewState[CURRENT_FORM_INDEX_KEY] as int? ?? 0;
        }

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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                GetRegistration();

                if ( RegistrationState != null &&
                    RegistrationState.RegistrationInstance != null &&
                    RegistrationState.RegistrationInstance.RegistrationTemplate != null )
                {
                    ShowHowMany();
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState[REGISTRATION_STATE_KEY] = JsonConvert.SerializeObject( RegistrationState, Formatting.None, jsonSetting );

            ViewState[CURRENT_PANEL_KEY] = CurrentPanel;
            ViewState[CURRENT_REGISTRANT_INDEX_KEY] = CurrentRegistrantIndex;
            ViewState[CURRENT_FORM_INDEX_KEY] = CurrentFormIndex;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        #region Navigation Events

        /// <summary>
        /// Handles the Click event of the lbHowManyNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbHowManyNext_Click( object sender, EventArgs e )
        {
            CurrentRegistrantIndex = 0;
            CurrentFormIndex = 0;

            CreateRegistrants( hfHowMany.ValueAsInt() );

            ShowRegistrant();
        }

        /// <summary>
        /// Handles the Click event of the lbRegistrantPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRegistrantPrev_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 2 )
            {
                CurrentFormIndex--;
                if ( CurrentFormIndex < 0 )
                {
                    CurrentRegistrantIndex--;
                    CurrentFormIndex = FormCount - 1;
                }
                ShowRegistrant();
            }
            else
            {
                ShowHowMany();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbRegistrantNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRegistrantNext_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 2 )
            {
                CurrentFormIndex++;
                if ( CurrentFormIndex >= FormCount )
                {
                    CurrentRegistrantIndex++;
                    CurrentFormIndex = 0;
                }
                ShowRegistrant();
            }
            else
            {
                ShowHowMany();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSummaryPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSummaryPrev_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 3 )
            {
                CurrentRegistrantIndex = RegistrantCount - 1;
                CurrentFormIndex = FormCount - 1;
                ShowRegistrant();
            }
            else
            {
                ShowHowMany();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSummaryNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSummaryNext_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 4 )
            {
                ShowSuccess();
            }
            else
            {
                ShowHowMany();
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Gets the a registration record. Will create one if neccessary
        /// </summary>
        private void GetRegistration()
        {
            int? RegistrationInstanceId = PageParameter( REGISTRANT_INSTANCE_ID_PARAM_NAME ).AsIntegerOrNull();
            int? RegistrationId = PageParameter( REGISTRATION_ID_PARAM_NAME ).AsIntegerOrNull();

            using ( var rockContext = new RockContext() )
            {
                if ( RegistrationId.HasValue )
                {
                    RegistrationState = new RegistrationService( rockContext )
                        .Queryable( "RegistrationInstance.RegistrationTemplate.Fees,RegistrationInstance.RegistrationTemplate.Discounts,RegistrationInstance.RegistrationTemplate.Forms.Fields.Attribute" )
                        .AsNoTracking()
                        .Where( r => r.Id == RegistrationId.Value )
                        .FirstOrDefault();
                }

                if ( RegistrationState == null && RegistrationInstanceId.HasValue )
                {
                    var registrationInstance = new RegistrationInstanceService( rockContext )
                        .Queryable( "RegistrationTemplate.Fees,RegistrationTemplate.Discounts,RegistrationTemplate.Forms.Fields.Attribute" )
                        .AsNoTracking()
                        .Where( r => r.Id == RegistrationInstanceId.Value )
                        .FirstOrDefault();

                    if ( registrationInstance != null )
                    {
                        RegistrationState = new Registration();
                        RegistrationState.RegistrationInstance = registrationInstance;
                    }
                }

                if ( RegistrationState != null && !RegistrationState.Registrants.Any() )
                {
                    var registrant = new RegistrationRegistrant();
                    RegistrationState.Registrants.Add( registrant );
                }
            }
        }

        /// <summary>
        /// Adds (or removes) registrants to or from the registration. Only newly added registrants can
        /// can be removed. Any existing (saved) registrants cannot be removed from the registration
        /// </summary>
        /// <param name="registrantCount">The number of registrants that registration should have.</param>
        private void CreateRegistrants( int registrantCount )
        {
            if ( RegistrationState != null )
            {
                // While the number of registrants belonging to registration is less than the selected count, addd another registrant
                while( RegistrantCount < registrantCount )
                {
                    RegistrationState.Registrants.Add( new RegistrationRegistrant() );
                }

                // Get the number of registrants that needs to be removed
                int removeCount = RegistrantCount - registrantCount;
                if ( removeCount > 0 )
                {
                    // If removing any, reverse the order of registrants, so that most recently added will be removed first
                    RegistrationState.Registrants.Reverse();

                    // Try to get the registrants to remove. Most recently added will be taken first
                    foreach ( var registrant in RegistrationState.Registrants.Where( r => r.Id == 0 ).Take( removeCount ).ToList() )
                    {
                        RegistrationState.Registrants.Remove( registrant );
                    }

                    // Reset the order after removing any registrants
                    RegistrationState.Registrants.Reverse();
                }
            }
        }

        /// <summary>
        /// Registers the client script.
        /// </summary>
        private void RegisterScript()
        {
            var template = RegistrationState.RegistrationInstance.RegistrationTemplate;

            string script = string.Format( @"
    function adjustHowMany( var adjustment ) {{

        int minRegistrants = parseInt($('#{0}').val(), 10);
        int maxRegistrants = parseInt($('#{1}').val(), 10);

        var $hfHowMany = $('#{2}');
        int howMany = parseInt($hfHowMany.val(), 10) + adjustment;

        var $lblHowMany = $('#{3}');
        if ( howMany > 0 && howMany <= maxRegistrants ) {{
            $hfHowMany.val(howMany);
            $lblHowMany.val(howMany);
        }}

        var $lbHowManyAdd = $('#{4}');
        if ( howMany >= maxRegistrants ) {{
            $lbHowManyAdd.addClass( 'disabled' );
        }} else {{
            $lbHowManyAdd.removeClass( 'disabled' );
        }}
        
        var $lbHowManySubtract = $('#{5}');
        if ( howMany <= $hfMinRegistrants ) {{
            $lbHowManySubtract.addClass( 'disabled' );
        }} else {{
            $lbHowManySubtract.removeClass( 'disabled' );
        }}

    }}
", 
                hfMinRegistrants.ClientID,      // {0}
                hfMaxRegistrants.ClientID,      // {1}
                hfHowMany.ClientID,             // {2}
                lblHowMany.ClientID,            // {3}
                lbHowManyAdd.ClientID,          // {4}
                lbHowManySubtract.ClientID );   // {5}

            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "adjustHowMany", script, true );
        }

        /// <summary>
        /// Shows the how many panel
        /// </summary>
        private void ShowHowMany()
        {
            if ( MaxRegistrants > MinRegistrants )
            {
                hfMaxRegistrants.Value = MaxRegistrants.ToString();
                hfMinRegistrants.Value = MinRegistrants.ToString();
                hfHowMany.Value = RegistrantCount.ToString();
                lblHowMany.Text = RegistrantCount.ToString();

                SetVisiblePanel( 0 );
            }
            else
            {
                CurrentRegistrantIndex = 0;
                CurrentFormIndex = 0;

                CreateRegistrants( MinRegistrants );

                ShowRegistrant();
            }
        }

        /// <summary>
        /// Shows the registrant panel
        /// </summary>
        private void ShowRegistrant()
        {
            if ( RegistrantCount > 0 )
            {
                if ( CurrentRegistrantIndex < 0 )
                {
                    ShowHowMany();
                }

                if ( CurrentRegistrantIndex >= RegistrantCount )
                {
                    ShowSummary();
                }

                string title = RegistrantCount <= 1 ? "Individual" : RegistrantCount.ToOrdinalWords().Humanize(LetterCasing.Title) + " Individual";
                if ( CurrentFormIndex > 0 )
                {
                    title += " (cont)";
                }
                lRegistrantTitle.Text = title;

                SetVisiblePanel( 1 );
            }
            else
            {
                // If for some reason there are not any registrants ( i.e. viewstate expired ), return to first screen
                ShowHowMany();
            }
        }

        /// <summary>
        /// Shows the summary panel
        /// </summary>
        private void ShowSummary()
        {
            SetVisiblePanel( 2 );
        }

        /// <summary>
        /// Shows the success panel
        /// </summary>
        private void ShowSuccess()
        {
            SetVisiblePanel( 3 );
        }

        /// <summary>
        /// Sets the correct panel to be visible
        /// </summary>
        /// <param name="currentPanel">The current panel.</param>
        private void SetVisiblePanel( int currentPanel )
        {
            CurrentPanel = currentPanel;

            pnlHowMany.Visible = CurrentPanel <= 1;
            pnlRegistrant.Visible = CurrentPanel == 1;
            pnlSummaryAndPayment.Visible = CurrentPanel == 1;
            pnlSuccess.Visible = CurrentPanel == 1;
        }

        #endregion


}
}