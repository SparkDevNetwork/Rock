// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using org.secc.RecurringCommunications.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Web.Cache;

namespace RockWeb.Plugins.org_secc.RecurringCommunications
{
    [DisplayName( "Recurring Communications Detail" )]
    [Category( "SECC > Communication" )]
    [Description( "Block for the creation and editing of recurring communications." )]

    [CustomCheckboxListField(
        "Enabled Communication Types",
        "Select the communication types users will be allowed to send from",
        "0^Recipient Preference,1^Email,2^SMS,3^Push Notification",
        Key = AttributeKey.EnabledCommunicationTypes,
        DefaultValue = "0,1,2" )]
    public partial class RecurringCommunicationsDetail : Rock.Web.UI.RockBlock
    {
        internal class AttributeKey
        {
            public const string EnabledCommunicationTypes = "EnabledCommunicationTypes";
        }


        internal class PageParameterKey
        {
            public const string RecurringCommunicationId = "RecurringCommunicationId";
        }

        EntityTypeCache _personEntityType = null;

        private EntityTypeCache PersonEntityType
        {
            get
            {
                if (_personEntityType == null)
                {
                    _personEntityType = EntityTypeCache.Get(Rock.SystemGuid.EntityType.PERSON.AsGuid());
                }

                return _personEntityType;
            }
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.BlockUpdated += Block_BlockUpdated;
            var personEntityId = EntityTypeCache.Get( typeof( Person ) ).Id;
            dvpDataview.EntityTypeId = personEntityId;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                DisplayDetails();

            }
        }

        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            DisplayDetails();
        }

        private void BindTransformationTypes()
        {
            ddlTransformTypes.Items.Clear();
            
            foreach ( var component in DataTransformContainer.GetComponentsByTransformedEntityName( PersonEntityType.Name ).OrderBy( c => c.Title ) )
            {
                if (component.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    var transformEntityType = EntityTypeCache.Get(component.TypeName);
                    ListItem li = new ListItem(component.Title, transformEntityType.Id.ToString());
                    ddlTransformTypes.Items.Add(li);
                }               
            }

            ddlTransformTypes.Items.Insert(0, new ListItem("", ""));
        }

        private void DisplayDetails()
        {
            var values = GetAttributeValue( AttributeKey.EnabledCommunicationTypes ).SplitDelimitedValues();
            var communicationDictionary = Enum
                .GetValues( typeof( CommunicationType ) )
                .Cast<CommunicationType>()
                .ToDictionary( t => ( ( int ) t ).ToString(), t => t.ToString() )
                .Where( d => values.Contains( d.Key ) )
                .ToDictionary( k => k.Key, v => v.Value );
            rblCommunicationType.DataSource = communicationDictionary;
            rblCommunicationType.DataBind();

            dvpPhoneNumber.DefinedTypeId = DefinedTypeCache.Get(Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid()).Id;

            BindTransformationTypes();

            RecurringCommunication recurringCommunication = GetRecurringCommunication();

            tbName.Text = recurringCommunication.Name;
            dvpDataview.SetValue( recurringCommunication.DataView );
            if ( recurringCommunication.Schedule != null )
            {
                sbScheduleBuilder.iCalendarContent = recurringCommunication.Schedule.iCalendarContent;
                DisplayScheduleDetails();
            }
            rblCommunicationType.SetValue( recurringCommunication.CommunicationType.ConvertToInt() );

            if (recurringCommunication.TransformationEntityTypeId.HasValue)
            {
                if (ddlTransformTypes.Items.FindByValue(recurringCommunication.TransformationEntityTypeId.Value.ToString()) != null)
                {
                    ddlTransformTypes.SelectedValue = recurringCommunication.TransformationEntityTypeId.Value.ToString();
                }
                else
                {
                    var transformComponent = DataTransformContainer.GetComponentsByTransformedEntityName(PersonEntityType.Name)
                        .SingleOrDefault(c => c.Id == recurringCommunication.TransformationEntityTypeId.Value);

                    if (transformComponent != null)
                    {
                        var li = new ListItem(transformComponent.Title, transformComponent.Id.ToString());
                        ddlTransformTypes.Items.Insert(1, li);
                        ddlTransformTypes.SelectedIndex = 1;
                    }
                }
            }

            UpdateCommunicationTypeUI( recurringCommunication.CommunicationType );

            tbFromName.Text = recurringCommunication.FromName;
            tbFromEmail.Text = recurringCommunication.FromEmail;
            tbSubject.Text = recurringCommunication.Subject;
            ceEmailBody.Text = recurringCommunication.EmailBody;

            dvpPhoneNumber.SetValue(recurringCommunication.PhoneNumberValueId);
            tbSMSBody.Text = recurringCommunication.SMSBody;

            tbPushNotificationTitle.Text = recurringCommunication.PushTitle;
            tbPushNotificationBody.Text = recurringCommunication.PushMessage;
            cbPlaySound.Checked = recurringCommunication.PushSound.IsNotNullOrWhiteSpace();
        }

        private void DisplayScheduleDetails()
        {
            lScheduleDescription.Text = GetScheduleDescription();
        }

        private string GetScheduleDescription()
        {
            var iCal = sbScheduleBuilder.iCalendarContent;
            if ( iCal.IsNullOrWhiteSpace() )
            {
                lScheduleDescription.Visible = false;
                return "";
            }

            lScheduleDescription.Visible = true;
            var schedule = new Schedule();
            schedule.iCalendarContent = sbScheduleBuilder.iCalendarContent;
            return schedule.ToFriendlyScheduleText();
        }

        private void UpdateCommunicationTypeUI( CommunicationType communicationType )
        {
            switch ( communicationType )
            {
                case CommunicationType.RecipientPreference:
                    DisplaySMS( true );
                    DisplayEMail( true );
                    DisplayPushNotification( false );
                    break;
                case CommunicationType.Email:
                    DisplayEMail( true );
                    DisplaySMS( false );
                    DisplayPushNotification( false );
                    break;
                case CommunicationType.SMS:
                    DisplaySMS( true );
                    DisplayEMail( false );
                    DisplayPushNotification( false );
                    break;
                case CommunicationType.PushNotification:
                    DisplaySMS( false );
                    DisplayEMail( false );
                    DisplayPushNotification( true );
                    break;
                default:
                    break;
            }
        }

        private void DisplayEMail( bool shouldDisplay )
        {
            pnlEmail.Visible = shouldDisplay;
            tbFromEmail.Required = shouldDisplay;
            tbFromName.Required = shouldDisplay;
            tbSubject.Required = shouldDisplay;
            ceEmailBody.Required = shouldDisplay;
        }

        private void DisplaySMS( bool shouldDisplay )
        {
            pnlSMS.Visible = shouldDisplay;
            dvpPhoneNumber.Required = shouldDisplay;
            tbSMSBody.Required = shouldDisplay;
        }

        private void DisplayPushNotification( bool shouldDisplay )
        {
            pnlPushNofitication.Visible = shouldDisplay;
            tbPushNotificationBody.Required = shouldDisplay;
            tbPushNotificationTitle.Required = shouldDisplay;
        }

        private RecurringCommunication GetRecurringCommunication()
        {
            return GetRecurringCommunication( new RecurringCommunicationService( new RockContext() ) );
        }

        private RecurringCommunication GetRecurringCommunication( RecurringCommunicationService recurringCommunicationService )
        {
            var rucurringCommunicationId = PageParameter( PageParameterKey.RecurringCommunicationId ).AsInteger();
            var recurringCommunication = recurringCommunicationService.Get( rucurringCommunicationId );
            if ( recurringCommunication == null )
            {
                recurringCommunication = new RecurringCommunication
                {
                    CommunicationType = CommunicationType.Email,
                    EmailBody = "{{ 'Global' | Attribute:'EmailHeader' }}\n<br>\n<br>\n{{ 'Global' | Attribute:'EmailFooter' }}",
                };
            }
            return recurringCommunication;
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            RecurringCommunicationService recurringCommunicationService = new RecurringCommunicationService( rockContext );
            var recurringCommunication = GetRecurringCommunication( recurringCommunicationService );

            if ( recurringCommunication.Schedule == null )
            {
                recurringCommunication.Schedule = new Schedule();
                ScheduleService scheduleService = new ScheduleService( rockContext );
                scheduleService.Add( recurringCommunication.Schedule );
            }
            recurringCommunication.Name = tbName.Text;
            recurringCommunication.DataViewId = dvpDataview.SelectedValue.AsInteger();
            recurringCommunication.Schedule.iCalendarContent = sbScheduleBuilder.iCalendarContent;
            recurringCommunication.CommunicationType = rblCommunicationType.SelectedValueAsEnum<CommunicationType>();
            recurringCommunication.FromName = tbFromName.Text;
            recurringCommunication.FromEmail = tbFromEmail.Text;
            recurringCommunication.Subject = tbSubject.Text;
            recurringCommunication.EmailBody = ceEmailBody.Text;
            recurringCommunication.PhoneNumberValueId = dvpPhoneNumber.SelectedDefinedValueId > 0 ? dvpPhoneNumber.SelectedDefinedValueId : null;
            recurringCommunication.SMSBody = tbSMSBody.Text;
            recurringCommunication.PushTitle = tbPushNotificationTitle.Text;
            recurringCommunication.PushMessage = tbPushNotificationBody.Text;
            recurringCommunication.PushSound = cbPlaySound.Checked ? "default" : string.Empty;
            recurringCommunication.TransformationEntityTypeId = ddlTransformTypes.SelectedValue.AsIntegerOrNull();


            if ( recurringCommunication.Id == 0 )
            {
                recurringCommunicationService.Add( recurringCommunication );
            }
            recurringCommunication.ScheduleDescription = GetScheduleDescription();
            rockContext.SaveChanges();
            NavigateToParentPage();
        }

        protected void rblCommunicationType_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateCommunicationTypeUI( rblCommunicationType.SelectedValueAsEnum<CommunicationType>() );
        }

        protected void sbScheduleBuilder_SaveSchedule( object sender, EventArgs e )
        {
            DisplayScheduleDetails();
        }
    }
}