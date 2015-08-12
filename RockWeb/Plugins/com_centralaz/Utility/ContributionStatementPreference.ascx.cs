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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_CentralAZ.Utility
{
    [DisplayName( "Contribution Statement Preference" )]
    [Category( "CentralAZ.com > Utility" )]
    [Description( "Displays and or sets the user's statement preference such as quarterly, yearly, none." )]

    // List of all possible/documented Block Atributes:
    // https://github.com/SparkDevNetwork/Rock/wiki/Block-Attributes

    [TextField( "Frequency Preference Attribute Guid", "Guid of the person attribute that holds each person's frequency choice.  Note: The attribute must be of type DefinedType.", true, "546f10c6-58e5-4e0b-99a9-1e7b85e1c121" )]
    public partial class ContributionStatementPreference : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        Person _person = null;

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

            var contextEntity = this.ContextEntity();
            if ( contextEntity != null )
            {
                _person = (Person)contextEntity;
            }

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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                if ( CurrentPerson != null )
                {
                    BindOptionsToList();
                }
            }
        }

        #endregion

        #region Events

        protected void rblPreference_CheckedChanged( object sender, EventArgs e )
        {
            nbFail.Visible = false;
            nbSuccess.Visible = false;
            try
            {
                if ( CurrentPerson != null )
                {
                    CurrentPerson.LoadAttributes();
                    CurrentPerson.SetAttributeValue( hfAttributeKey.Value, rblPreference.SelectedValue );
                    CurrentPerson.SaveAttributeValues();
                }

                nbSuccess.Visible = true;
                pnlPreferences.Visible = false;
            }
            catch
            {
                nbFail.Visible = true;
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            BindOptionsToList();
        }

        #endregion

        #region Methods

        private void BindOptionsToList()
        {
            // Check configuration to make sure the attribute is valid and has a DefinedType for it's qualifier.
            nbConfigurationError.Visible = false;
            var personAttribute = AttributeCache.Read( GetAttributeValue( "FrequencyPreferenceAttributeGuid" ).AsGuid() );
            if ( personAttribute == null )
            {
                nbConfigurationError.Visible = true;
                return;
            }

            hfAttributeKey.Value = personAttribute.Key;
            //hfAttributeId.Value = attribute.Id.ToStringSafe();

            var qualifierValue = personAttribute.QualifierValues.First().Value;
            if ( qualifierValue == null || qualifierValue.Value == null )
            {
                nbConfigurationError.Visible = true;
                return;
            }

            var definedType = DefinedTypeCache.Read( int.Parse( qualifierValue.Value ) );
            if ( definedType == null )
            {
                nbConfigurationError.Visible = true;
                return;
            }

            // Bind the definedType to our radio button list
            rblPreference.BindToDefinedType( definedType, useDescriptionAsText: true );

            // set the person's current value as the selected one
            CurrentPerson.LoadAttributes();
            var personAttributeValue = CurrentPerson.GetAttributeValue( hfAttributeKey.Value );
            rblPreference.SelectedValue = personAttributeValue;
        }

        #endregion
        protected void btnSave_Click( object sender, EventArgs e )
        {
            rblPreference_CheckedChanged( sender, e );
        }
}
}