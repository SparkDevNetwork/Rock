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
using System.Text;

namespace RockWeb.Plugins.com_centralaz.Utility
{
    [DisplayName( "Email Topic Subscription Preference" )]
    [Category( "com_centralaz > Utility" )]
    [Description( "Displays and or sets the user's email subscriptions" )]
    [TextField( "Label Text", "The Text that will be displayed as the checkboxlist's label.", true, "Subscribed Topics:", order: 0 )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Subscription Preference Attribute Guid", "Guid of the person attribute that holds each person's frequency choice.  Note: The attribute must be of type DefinedType.", required: true, defaultValue: "1E372FF6-93D9-4D42-9107-4FAD1E452218", order: 1 )]
    [TextField( "Popup Defined Values", "Any Defined Values with guids listed here will have a popup appear should someone uncheck them. Separate guids by commas.", order: 2 )]
    [CodeEditorField( "Lava Template", "The lava template to use for the results", CodeEditorMode.Lava, CodeEditorTheme.Rock, defaultValue: "Are you sure you want to unsubscribe to these?", order: 3 )]

    public partial class EmailTopicSubscriptionPreference : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

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
                    ShowDetail();
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            nbFail.Visible = false;
            nbSuccess.Visible = false;
            try
            {
                if ( CurrentPerson != null )
                {
                    CurrentPerson.LoadAttributes();
                    var personAttributeValueGuid = CurrentPerson.GetAttributeValue( hfAttributeKey.Value );
                    if ( personAttributeValueGuid != null )
                    {
                        List<Guid> guidPopupRequiredList = GetAttributeValue( "PopupDefinedValues" ).SplitDelimitedValues().AsGuidList();
                        List<Guid> guidPersonList = personAttributeValueGuid.SplitDelimitedValues().AsGuidList();
                        List<Guid> guidPopupList = new List<Guid>();

                        foreach ( var popupRequiredValue in guidPopupRequiredList )
                        {
                            if ( guidPersonList.Contains( popupRequiredValue ) && !cblPreference.SelectedValues.AsGuidList().Contains( popupRequiredValue ) )
                            {
                                guidPopupList.Add( popupRequiredValue );
                            }
                        }
                        if ( guidPopupList.Count == 0 )
                        {
                            SaveAttribute();
                        }
                        else
                        {
                            ShowPopup( guidPopupList );
                        }
                    }
                }
            }
            catch
            {
                nbFail.Visible = true;
            }

        }

        /// <summary>
        /// Handles the SaveClick event of the mdConfirmUnsubscribe control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdConfirmUnsubscribe_SaveClick( object sender, EventArgs e )
        {
            mdConfirmUnsubscribe.Hide();
            SaveAttribute();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            nbSuccess.Visible = false;
            ShowDetail();
        }

        /// <summary>
        /// Handles the Click event of the lbEditPreferences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditPreferences_Click( object sender, EventArgs e )
        {
            BindOptionsToList();
            mdConfirmUnsubscribe.Hide();
            pnlViewPreferences.Visible = false;
            pnlEditPreferences.Visible = true;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail();
        }

        #endregion

        #region Methods
        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            pnlEditPreferences.Visible = false;
            pnlViewPreferences.Visible = true;

            DefinedTypeCache definedType = GetDefinedType();
            if ( definedType == null )
            {
                nbConfigurationError.Visible = true;
                return;
            }

            List<Guid> subscribedGuidList = new List<Guid>();
            CurrentPerson.LoadAttributes();
            var subscribedDefinedValueGuids = CurrentPerson.GetAttributeValue( hfAttributeKey.Value );
            if ( !String.IsNullOrWhiteSpace( subscribedDefinedValueGuids ) )
            {
                subscribedGuidList = subscribedDefinedValueGuids.SplitDelimitedValues().AsGuidList();
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine( String.Format( "<b>{0}</b><br>", GetAttributeValue( "LabelText" ) ) );
            foreach ( var value in definedType.DefinedValues )
            {
                sb.AppendLine( String.Format( "<i class='fa fa-{1}'></i>  {0}<br>", value.Value, subscribedGuidList.Contains( value.Guid ) ? "check-square-o" : "square-o" ) );
            }

            lPreferences.Text = sb.ToString();
        }

        /// <summary>
        /// Binds the options to list.
        /// </summary>
        private void BindOptionsToList()
        {
            try
            {
                DefinedTypeCache definedType = GetDefinedType();
                if ( definedType == null )
                {
                    nbConfigurationError.Visible = true;
                    return;
                }

                // Bind the definedType to our radio button list
                var ds = definedType.DefinedValues
                    .Select( v => new
                    {
                        Name = v.Value,
                        v.Description,
                        v.Guid
                    } );

                cblPreference.SelectedIndex = -1;
                cblPreference.DataSource = ds;
                cblPreference.DataTextField = "Name";
                cblPreference.DataValueField = "Guid";
                cblPreference.TextAlign = TextAlign.Left;
                cblPreference.Label = GetAttributeValue( "LabelText" );
                cblPreference.DataBind();

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
                        cblPreference.SetValues( personAttributeValueGuid.SplitDelimitedValues() );
                    }
                    catch
                    { }
                }

            }
            catch ( Exception ex )
            {
                nbConfigurationError.Visible = true;
                nbConfigurationError.Text = "There appears to be a problem with something we did not expect. We've reported this to the system administrators.";
                ExceptionLogService.LogException( new Exception( "Unable to load Email Topic Subscription Preference block.", ex ), null );
            }
        }

        /// <summary>
        /// Gets the type of the defined.
        /// </summary>
        /// <returns></returns>
        private DefinedTypeCache GetDefinedType()
        {
            DefinedTypeCache definedType = null;

            nbConfigurationError.Visible = false;
            Guid? frequencyAttribute = GetAttributeValue( "SubscriptionPreferenceAttributeGuid" ).AsGuidOrNull();
            if ( !frequencyAttribute.HasValue )
            {
                return definedType;
            }

            var personAttribute = Rock.Web.Cache.AttributeCache.Read( frequencyAttribute.Value );
            if ( personAttribute == null )
            {
                return definedType;
            }

            hfAttributeKey.Value = personAttribute.Key;

            if ( personAttribute.QualifierValues.Count > 0 )
            {
                var qualifierValue = personAttribute.QualifierValues.First( qv => qv.Key == "definedtype" ).Value;
                if ( qualifierValue == null || qualifierValue.Value == null )
                {
                    return definedType;
                }

                definedType = DefinedTypeCache.Read( int.Parse( qualifierValue.Value ) );
            }

            return definedType;
        }


        /// <summary>
        /// Shows the popup.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void ShowPopup( List<Guid> guidPopupList )
        {
            var definedValueList = new DefinedValueService( new RockContext() ).GetByGuids( guidPopupList ).ToList();
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "DefinedValues", definedValueList );

            lWarning.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );
            mdConfirmUnsubscribe.Show();
        }

        /// <summary>
        /// Saves the attribute.
        /// </summary>
        private void SaveAttribute()
        {
            CurrentPerson.LoadAttributes();
            CurrentPerson.SetAttributeValue( hfAttributeKey.Value, cblPreference.SelectedValues.AsDelimited( "," ) );
            CurrentPerson.SaveAttributeValues();
            nbSuccess.Visible = true;
            ShowDetail();
        }

        #endregion


    }
}