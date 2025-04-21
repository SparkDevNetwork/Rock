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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Model;
using Rock.Web.Cache;
using Rock.SystemKey;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Person Preferences" )]
    [Category( "CRM" )]
    [Description( "Allows the person to set their personal preferences." )]
    [Rock.SystemGuid.BlockTypeGuid( "D2049782-C286-4EE1-94E8-039111E16794" )]
    public partial class PersonPreferences : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ConfigurePreferences();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ConfigurePreferences();
        }

        #endregion

        #region Methods

        private void ConfigurePreferences()
        {
            var pbxComponent = Rock.Pbx.PbxContainer.GetAllowedActiveComponentWithOriginationSupport( CurrentPerson );
            var preferences = GetGlobalPersonPreferences();

            if ( pbxComponent == null )
            {
                dvpOriginateCallSource.Visible = false;
                this.Visible = false; // hide the entire block since this is the currently the only setting
                return;
            }

            // configure the pbx call origination features
            var phoneTypeDefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() ).Id;
            dvpOriginateCallSource.DefinedTypeId = phoneTypeDefinedTypeId;

            var preferredOriginationPhoneTypeId = preferences.GetValue( PersonPreferenceKey.ORIGINATE_CALL_SOURCE ).AsIntegerOrNull();
            if ( preferredOriginationPhoneTypeId.HasValue )
            {
                dvpOriginateCallSource.SelectedValue = preferredOriginationPhoneTypeId.ToString();
            }
            else
            {
                // use default preference
                var defaultPhoneTypeId = pbxComponent.GetAttributeValue( "InternalPhoneType" );
                dvpOriginateCallSource.SelectedValue = defaultPhoneTypeId.ToString();
            }
        }

        #endregion

        protected void btnSave_Click( object sender, EventArgs e )
        {
            var pbxComponent = Rock.Pbx.PbxContainer.GetAllowedActiveComponentWithOriginationSupport( CurrentPerson );
            var preferences = GetGlobalPersonPreferences();

            var selectedOriginateCallSource = dvpOriginateCallSource.SelectedValue.AsIntegerOrNull();
            if ( selectedOriginateCallSource.HasValue )
            {
                // check to see if the default value was selected
                // if so delete it rather than saving so that if the administrator changes the default that change will be respected
                var defaultPhoneTypeId = pbxComponent.GetAttributeValue( "InternalPhoneType" ).AsIntegerOrNull();

                if (selectedOriginateCallSource == defaultPhoneTypeId )
                {
                    preferences.SetValue( PersonPreferenceKey.ORIGINATE_CALL_SOURCE, string.Empty );
                }
                else
                {
                    preferences.SetValue( PersonPreferenceKey.ORIGINATE_CALL_SOURCE, selectedOriginateCallSource.ToString() );
                }
            }
            else
            {
                // delete any preference so the default value is used again
                preferences.SetValue( PersonPreferenceKey.ORIGINATE_CALL_SOURCE, string.Empty );
            }

            preferences.Save();
        }
    }
}