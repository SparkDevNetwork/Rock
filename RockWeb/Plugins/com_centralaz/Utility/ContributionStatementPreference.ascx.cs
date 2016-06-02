// <copyright>
// Copyright by Central Christian Church
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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_centralaz.Utility
{
    [DisplayName( "Contribution Statement Preference" )]
    [Category( "com_centralaz > Utility" )]
    [Description( "Displays and or sets the user's statement preference such as quarterly, yearly, none." )]

    // List of all possible/documented Block Atributes:
    // https://github.com/SparkDevNetwork/Rock/wiki/Block-Attributes

    [TextField( "Frequency Preference Attribute Guid", "Guid of the person attribute that holds each person's frequency choice.  Note: The attribute must be of type DefinedType.", true, "546f10c6-58e5-4e0b-99a9-1e7b85e1c121" )]
    public partial class ContributionStatementPreference : Rock.Web.UI.RockBlock
    {
        #region Fields

        #endregion

        #region Properties

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
            try
            {
                // Check configuration to make sure the attribute is valid and has a DefinedType for it's qualifier.
                nbConfigurationError.Visible = false;
                Guid? frequencyAttribute = GetAttributeValue( "FrequencyPreferenceAttributeGuid" ).AsGuidOrNull();
                if ( !frequencyAttribute.HasValue )
                {
                    nbConfigurationError.Visible = true;
                    return;
                }

                var personAttribute = Rock.Web.Cache.AttributeCache.Read( frequencyAttribute.Value );
                if ( personAttribute == null )
                {
                    nbConfigurationError.Visible = true;
                    return;
                }

                hfAttributeKey.Value = personAttribute.Key;

                if ( personAttribute.QualifierValues.Count > 0 )
                {
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
                    BindToDefinedType( rblPreference, definedType, useDescriptionAsText: true );

                    // set the person's current value as the selected one
                    CurrentPerson.LoadAttributes();

                    // The Guids that are put into the data list will be in lowercase as per
                    // https://msdn.microsoft.com/en-us/library/97af8hh4(v=vs.110).aspx
                    // so we need to make sure we're comparing with the lowercase of whatever
                    // was stored.
                    var personAttributeValueGuid = CurrentPerson.GetAttributeValue( hfAttributeKey.Value );
                    if ( personAttributeValueGuid != null )
                    {
                        try
                        {
                            rblPreference.SelectedValue = personAttributeValueGuid.ToLower();
                        }
                        catch
                        { }
                    }
                }
            }
            catch ( Exception ex )
            {
                nbConfigurationError.Visible = true;
                nbConfigurationError.Text = "There appears to be a problem with something we did not expect. We've reported this to the system administrators.";
                ExceptionLogService.LogException( new Exception( "Unable to load Contribution Statement Preference block.", ex ), null );
            }
        }

        #endregion
        protected void btnSave_Click( object sender, EventArgs e )
        {
            rblPreference_CheckedChanged( sender, e );
        }

        /// <summary>
        /// Binds to the values of a definedType using the definedValue's Guid as the listitem value
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="definedType">Type of the defined.</param>
        /// <param name="insertBlankOption">if set to <c>true</c> [insert blank option].</param>
        /// <param name="useDescriptionAsText">if set to <c>true</c> [use description as text].</param>
        public void BindToDefinedType( ListControl listControl, Rock.Web.Cache.DefinedTypeCache definedType, bool insertBlankOption = false, bool useDescriptionAsText = false )
        {
            var ds = definedType.DefinedValues
                .Select( v => new
                {
                    Name = v.Value,
                    v.Description,
                    v.Guid
                } );

            listControl.SelectedIndex = -1;
            listControl.DataSource = ds;
            listControl.DataTextField = useDescriptionAsText ? "Description" : "Name";
            listControl.DataValueField = "Guid";
            listControl.DataBind();

            if ( insertBlankOption )
            {
                listControl.Items.Insert( 0, new ListItem() );
            }
        }

}
}