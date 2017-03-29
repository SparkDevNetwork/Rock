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
using Rock.Security;

namespace RockWeb.Plugins.com_centralaz.Utility
{
    [DisplayName( "Email Topic Subscription Preference" )]
    [Category( "com_centralaz > Utility" )]
    [Description( "Displays and or sets the user's email subscriptions" )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Subscription Preference Attribute", "The person attribute that holds each person's subscribed topics.", required: true, defaultValue: "1E372FF6-93D9-4D42-9107-4FAD1E452218", order: 0 )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Disable Auto-Topic Subscription Attribute", "The person attribute that controls whether auto-topic subscription will attempt to subscribe a person to topics.", required: true, key: "DisableAutoTopicSubscriptionAttribute", order: 1 )]
    [CodeEditorField( "Lava Template", "The lava template to use for the results", CodeEditorMode.Lava, CodeEditorTheme.Rock, defaultValue: "Are you sure you want to unsubscribe to these?", order: 2 )]
    [TextField( "Popup Text", "The Text that will be displayed as the checkboxlist's label.", true, "Are you sure you want to unsubscribe to these?", order: 3 )]
    [TextField( "Popup Values", required: false, order: 4 )]
    public partial class EmailTopicSubscriptionPreference : Rock.Web.UI.RockBlockCustomSettings
    {
        #region Fields

        private Person _person = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the settings tool tip.
        /// </summary>
        /// <value>
        /// The settings tool tip.
        /// </value>
        public override string SettingsToolTip
        {
            get
            {
                return "Edit Header Image";
            }
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

            var key = PageParameter( "Person" );
            if ( !string.IsNullOrWhiteSpace( key ) )
            {
                var service = new PersonService( new RockContext() );
                _person = service.GetByUrlEncodedKey( key );
            }

            if ( _person == null && CurrentPerson != null )
            {
                _person = CurrentPerson;
            }

            if ( _person == null )
            {
                nbSuccess.NotificationBoxType = NotificationBoxType.Danger;
                nbSuccess.Text = "Unfortunately, we're unable to update your email preference, as we're not sure who you are.";
                nbSuccess.Visible = true;
                lbEditPreferences.Visible = false;
            }

            cblSubscriptions.ClientIDMode = ClientIDMode.AutoID;

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
                if ( _person != null )
                {
                    ShowDetail();
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSaveSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveSettings_Click( object sender, EventArgs e )
        {
            SetAttributeValue( "SubscriptionPreferenceAttribute", ddlPersonAttribute.SelectedValue );
            SetAttributeValue( "PopupValues", cblPopupValues.SelectedValues.AsDelimited( "," ) );
            SetAttributeValue( "PopupText", tbPopupText.Text );
            SetAttributeValue( "LavaTemplate", htmlEditor.Text );
            SaveAttributeValues();

            ShowDetail();
        }

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
                if ( _person != null )
                {
                    _person.LoadAttributes();

                    DefinedTypeCache definedType = GetDefinedType();
                    if ( definedType == null )
                    {
                        pnlViewPreferences.Visible = false;
                        nbConfigurationError.Visible = true;
                        return;
                    }

                    var personAttributeValueGuid = _person.GetAttributeValue( hfAttributeKey.Value );
                    if ( personAttributeValueGuid != null )
                    {
                        List<DefinedValueCache> popupRequiredList = definedType.DefinedValues.Where( dv => GetAttributeValue( "PopupValues" ).ToLower().SplitDelimitedValues().Contains( dv.Guid.ToString().ToLower() ) ).ToList();
                        List<DefinedValueCache> personAttributeList = definedType.DefinedValues.Where( dv => personAttributeValueGuid.SplitDelimitedValues().AsGuidList().Contains( dv.Guid ) ).ToList();
                        List<DefinedValueCache> popupList = new List<DefinedValueCache>();

                        foreach ( var popupRequiredValue in popupRequiredList )
                        {
                            if ( personAttributeList.Contains( popupRequiredValue ) && !cblSubscriptions.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value ).AsGuidList().Contains( popupRequiredValue.Guid ) )
                            {
                                popupList.Add( popupRequiredValue );
                            }
                        }

                        if ( popupList.Count == 0 )
                        {
                            SaveSubscriptionSettings();
                        }
                        else
                        {
                            lWarning.Text = GetAttributeValue( "PopupText" );
                            mdConfirmUnsubscribe.Show();
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
            SaveSubscriptionSettings();
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

        protected void ddlPersonAttribute_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdatePopupValues();
        }

        protected void grpEmailPreference_CheckedChanged( object sender, EventArgs e )
        {
            if ( radDoNotEmail.Checked || radNoMassEmail.Checked )
            {
                foreach ( ListItem item in cblSubscriptions.Items )
                {
                    item.Selected = false;
                }
            }
        }

        protected void cblSubscriptions_SelectedIndexChanged( object sender, EventArgs e )
        {
            radDoNotEmail.Checked = false;
            radNoMassEmail.Checked = false;
            radEmailAllowed.Checked = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the settings.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void ShowSettings()
        {
            ddlPersonAttribute.Items.Clear();
            var personGuid = Rock.SystemGuid.EntityType.PERSON.AsGuid();
            var definedValueGuid = Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid();
            var attributes = new AttributeService( new RockContext() ).Queryable().Where( a =>
                  a.EntityType.Guid == personGuid &&
                  a.FieldType.Guid == definedValueGuid )
                  .OrderBy( a => a.Name )
                .ToList();
            foreach ( var attribute in attributes )
            {
                ddlPersonAttribute.Items.Add( new ListItem( attribute.Name, attribute.Guid.ToString().ToLower() ) );
            }
            ddlPersonAttribute.SetValue( GetAttributeValue( "SubscriptionPreferenceAttribute" ).ToLower() );
            UpdatePopupValues();
            tbPopupText.Text = GetAttributeValue( "PopupText" );
            htmlEditor.Text = GetAttributeValue( "LavaTemplate" );
            pnlEditModal.Visible = true;
            mdEdit.Show();
        }

        private void UpdatePopupValues()
        {
            var definedType = GetDefinedType( ddlPersonAttribute.SelectedValue.AsGuid() );
            if ( definedType != null )
            {
                cblPopupValues.Items.Clear();
                foreach ( var definedValue in definedType.DefinedValues )
                {
                    cblPopupValues.Items.Add( new ListItem( definedValue.Value, definedValue.Guid.ToString().ToLower() ) );
                }

                cblPopupValues.SetValues( GetAttributeValue( "PopupValues" ).ToLower().SplitDelimitedValues() );
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            mdEdit.Hide();
            pnlEditModal.Visible = false;
            pnlEditPreferences.Visible = false;
            pnlViewPreferences.Visible = true;

            DefinedTypeCache definedType = GetDefinedType();
            if ( definedType == null )
            {
                pnlViewPreferences.Visible = false;
                nbConfigurationError.Visible = true;
                return;
            }

            List<Guid> subscribedGuidList = new List<Guid>();
            _person.LoadAttributes();
            var subscribedDefinedValueGuids = _person.GetAttributeValue( hfAttributeKey.Value );
            if ( !String.IsNullOrWhiteSpace( subscribedDefinedValueGuids ) )
            {
                subscribedGuidList = subscribedDefinedValueGuids.SplitDelimitedValues().AsGuidList();
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "SubscribedTopics", definedType.DefinedValues.Where( dv => subscribedGuidList.Contains( dv.Guid ) ) );
            mergeFields.Add( "EmailPreference", _person.EmailPreference.ConvertToInt() );

            string template = GetAttributeValue( "LavaTemplate" );
            lContent.Text = template.ResolveMergeFields( mergeFields );

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }
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
                    pnlViewPreferences.Visible = false;
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

                cblSubscriptions.SelectedIndex = -1;
                cblSubscriptions.DataSource = ds;
                cblSubscriptions.DataTextField = "Name";
                cblSubscriptions.DataValueField = "Guid";
                cblSubscriptions.TextAlign = TextAlign.Left;
                cblSubscriptions.DataBind();

                if ( _person.EmailPreference == EmailPreference.EmailAllowed )
                {
                    radEmailAllowed.Checked = true;

                    // set the person's current value as the selected one
                    _person.LoadAttributes();

                    // The Guids that are put into the data list will be in lowercase as per
                    // https://msdn.microsoft.com/en-us/library/97af8hh4(v=vs.110).aspx
                    // so we need to make sure we're comparing with the lowercase of whatever
                    // was stored.
                    var personAttributeValueGuid = _person.GetAttributeValue( hfAttributeKey.Value );
                    if ( personAttributeValueGuid != null )
                    {
                        try
                        {
                            cblSubscriptions.SetValues( personAttributeValueGuid.SplitDelimitedValues() );
                        }
                        catch
                        { }
                    }
                }

                else if ( _person.EmailPreference == EmailPreference.NoMassEmails )
                {
                    radNoMassEmail.Checked = true;
                }

                else if ( _person.EmailPreference == EmailPreference.DoNotEmail )
                {
                    radDoNotEmail.Checked = true;
                }

            }
            catch ( Exception ex )
            {
                pnlViewPreferences.Visible = false;
                nbConfigurationError.Visible = true;
                nbConfigurationError.Text = "There appears to be a problem with something we did not expect. We've reported this to the system administrators.";
                ExceptionLogService.LogException( new Exception( "Unable to load Email Topic Subscription Preference block.", ex ), null );
            }
        }

        /// <summary>
        /// Gets the type of the defined.
        /// </summary>
        /// <returns></returns>
        private DefinedTypeCache GetDefinedType( Guid? attributeGuid = null )
        {
            DefinedTypeCache definedType = null;

            nbConfigurationError.Visible = false;
            if ( attributeGuid == null )
            {
                attributeGuid = GetAttributeValue( "SubscriptionPreferenceAttribute" ).AsGuidOrNull();
            }

            if ( !attributeGuid.HasValue )
            {
                return definedType;
            }

            var personAttribute = Rock.Web.Cache.AttributeCache.Read( attributeGuid.Value );
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
        /// Saves the subscription settings.
        /// </summary>
        private void SaveSubscriptionSettings()
        {
            using ( var rockContext = new RockContext() )
            {
                _person = new PersonService( rockContext ).Get( _person.Id );
                _person.LoadAttributes();
                _person.SetAttributeValue( hfAttributeKey.Value, cblSubscriptions.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value ).ToList().AsDelimited( "," ) );

                // Check the disable auto-topic subscription setting and if it's set, save "true" to the person's attribute. 
                Guid? theAttributesGuid = GetAttributeValue( "DisableAutoTopicSubscriptionAttribute" ).AsGuidOrNull();
                if ( theAttributesGuid != null && theAttributesGuid.HasValue )
                {
                    var theDisableAutoTopicAttribute = Rock.Web.Cache.AttributeCache.Read( theAttributesGuid.Value );
                    _person.SetAttributeValue( theDisableAutoTopicAttribute.Key, "True" );
                }

                _person.SaveAttributeValues();

                if ( radEmailAllowed.Checked )
                {
                    _person.EmailPreference = EmailPreference.EmailAllowed;
                }

                else if ( radNoMassEmail.Checked )
                {
                    _person.EmailPreference = EmailPreference.NoMassEmails;
                }

                else if ( radDoNotEmail.Checked )
                {
                    _person.EmailPreference = EmailPreference.DoNotEmail;
                }

                rockContext.SaveChanges();
            }

            nbSuccess.Visible = true;
            nbSuccess.NotificationBoxType = NotificationBoxType.Success;
            nbSuccess.Text = "Your preferences were saved.";
            ShowDetail();
        }

        #endregion        
    }
}