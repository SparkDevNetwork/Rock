using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Rock;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;

namespace org.lakepointe.MinistryPoint.Transactions
{


    public class SendMinistryPointNotificationTransaction : ITransaction
    {
        public int RegistrationId { get; set; }
        public string AppRoot { get; set; }
        public string ThemeRoot { get; set; }
        public Guid? NotificationCommunicationGuid { get; set; }

        private SendMinistryPointNotificationTransaction() { }

        public SendMinistryPointNotificationTransaction( int registrationId, Guid? communicationGuid = null )
        {
            RegistrationId = registrationId;
            NotificationCommunicationGuid = communicationGuid;
        }

        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var registration = new RegistrationService( rockContext )
                    .Queryable( "RegistrationInstance.RegistrationTemplate" ).AsNoTracking()
                    .FirstOrDefault( r => r.Id == RegistrationId );

                if ( registration != null && !string.IsNullOrEmpty( registration.ConfirmationEmail ) &&
                    registration.RegistrationInstance != null && registration.RegistrationInstance.RegistrationTemplate != null )
                {
                    var template = registration.RegistrationInstance.RegistrationTemplate;

                    var mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance );
                    mergeFields.Add( "Registration", registration );

                    var anonymousHash = new HashSet<string>();
                    var messageRecipients = new List<RockMessageRecipient>();

                    // Contact
                    if ( !string.IsNullOrWhiteSpace( registration.RegistrationInstance.ContactEmail ) &&
                        ( template.Notify & RegistrationNotify.RegistrationContact ) == RegistrationNotify.RegistrationContact )
                    {
                        var messageRecipient = registration.RegistrationInstance.GetContactRecipient( mergeFields );
                        if ( !anonymousHash.Contains( messageRecipient.To ) )
                        {
                            messageRecipients.Add( messageRecipient );
                            anonymousHash.Add( messageRecipient.To );
                        }
                    }

                    // Group Followers
                    if ( registration.GroupId.HasValue &&
                        ( template.Notify & RegistrationNotify.GroupFollowers ) == RegistrationNotify.GroupFollowers )
                    {
                        new GroupService( rockContext ).GetFollowers( registration.GroupId.Value ).AsNoTracking()
                            .Where( p =>
                                p.Email != null &&
                                p.Email != "" )
                            .ToList()
                            .ForEach( p => messageRecipients.Add( new RockEmailMessageRecipient( p, mergeFields ) ) );
                    }

                    // Group Leaders
                    if ( registration.GroupId.HasValue &&
                        ( template.Notify & RegistrationNotify.GroupLeaders ) == RegistrationNotify.GroupLeaders )
                    {
                        new GroupMemberService( rockContext ).GetLeaders( registration.GroupId.Value )
                            .Where( m =>
                                m.Person != null &&
                                m.Person.Email != null &&
                                m.Person.Email != "" )
                            .Select( m => m.Person )
                            .ToList()
                            .ForEach( p => messageRecipients.Add( new RockEmailMessageRecipient( p, mergeFields ) ) );
                    }

                    if ( messageRecipients.Any() )
                    {
                        var notificationCommunicationGuid = NotificationCommunicationGuid.HasValue ? NotificationCommunicationGuid.Value :
                            Rock.SystemGuid.SystemCommunication.REGISTRATION_NOTIFICATION.AsGuid();

                        var emailMessage = new RockEmailMessage( notificationCommunicationGuid );
                        emailMessage.AdditionalMergeFields = mergeFields;
                        emailMessage.SetRecipients( messageRecipients );
                        emailMessage.AppRoot = AppRoot;
                        emailMessage.ThemeRoot = ThemeRoot;
                        emailMessage.Send();
                    }
                }
            }
        }
    }
}
