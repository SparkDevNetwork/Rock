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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls.Communication;

namespace Rock.Communication.Medium
{

    /// <summary>
    /// A push notification communication
    /// </summary>
    [Description( "A push notification communication" )]
    [Export( typeof( MediumComponent ))]
    [ExportMetadata( "ComponentName", "Push Notification")]
    class PushNotification : MediumComponent
    {
        const int TOKEN_REUSE_DURATION = 30; // number of days between token reuse

        /// <summary>
        /// Gets the control.
        /// </summary>
        /// <param name="useSimpleMode">if set to <c>true</c> [use simple mode].</param>
        /// <returns></returns>
        public override MediumControl GetControl( bool useSimpleMode )
        {
            return new Rock.Web.UI.Controls.Communication.PushNotification();
        }

        /// <summary>
        /// Gets the HTML preview.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override string GetHtmlPreview( Model.Communication communication, Person person )
        {
            var rockContext = new RockContext();

            // Requery the Communication object
            communication = new CommunicationService( rockContext ).Get( communication.Id );

            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

            if ( person != null )
            {
                mergeFields.Add( "Person", person );

                var recipient = new CommunicationRecipientService( rockContext ).Queryable().Where(a => a.CommunicationId == communication.Id).Where( r => r.PersonAlias != null && r.PersonAlias.PersonId == person.Id ).FirstOrDefault();
                if ( recipient != null )
                {
                    // Add any additional merge fields created through a report
                    foreach ( var mergeField in recipient.AdditionalMergeValues )
                    {
                        if ( !mergeFields.ContainsKey( mergeField.Key ) )
                        {
                            mergeFields.Add( mergeField.Key, mergeField.Value );
                        }
                    }
                }
            }

            string message = communication.GetMediumDataValue( "Message" );
            return message.ResolveMergeFields( mergeFields, communication.EnabledLavaCommands );
        }

        /// <summary>
        /// Gets the read-only message details.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns></returns>
        public override string GetMessageDetails( Model.Communication communication )
        {
            StringBuilder sb = new StringBuilder();

            AppendMediumData( communication, sb, "Title" );
            AppendMediumData( communication, sb, "Message" );
            AppendMediumData( communication, sb, "Sound" );

            return sb.ToString();
        }

        private void AppendMediumData( Model.Communication communication, StringBuilder sb, string key )
        {
            string value = communication.GetMediumDataValue( key );
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                AppendMediumData( sb, key, value );
            }
        }

        private void AppendMediumData( StringBuilder sb, string key, string value )
        {
            sb.AppendFormat( "<div class='form-group'><label class='control-label'>{0}</label><p class='form-control-static'>{1}</p></div>",
                key.SplitCase(), value );
        }

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public override void Send( Model.Communication communication )
        {
            var rockContext = new RockContext();
            var communicationService = new CommunicationService( rockContext );

            communication = communicationService.Get( communication.Id );

            if ( communication != null &&
                communication.Status == Model.CommunicationStatus.Approved &&
                communication.HasPendingRecipients( rockContext ) &&
                ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value.CompareTo( RockDateTime.Now ) <= 0 ) )
            {
                // Update any recipients that should not get sent the communication
                var recipientService = new CommunicationRecipientService( rockContext );
                foreach ( var recipient in recipientService.Queryable( "PersonAlias.Person" )
                    .Where( r =>
                        r.CommunicationId == communication.Id &&
                        r.Status == CommunicationRecipientStatus.Pending )
                    .ToList() )
                {
                    var person = recipient.PersonAlias.Person;
                    if ( person.IsDeceased )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Person is deceased!";
                    }
                }

                rockContext.SaveChanges();
            }

            base.Send( communication );
        }

        /// <summary>
        /// Creates a new communication.
        /// </summary>
        /// <param name="fromPersonAliasId">From person alias identifier.</param>
        /// <param name="fromPersonName">Name of from person.</param>
        /// <param name="toPersonAliasId">To person alias identifier.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="title">The title of the notification</param>
        /// <param name="sound">If this notifications should play a sound</param>
        /// <param name="responseCode">The reponseCode to use for tracking the conversation.</param>
        /// <param name="rockContext">A context to use for database calls.</param>
        private void CreateCommunication( int fromPersonAliasId, string fromPersonName, int toPersonAliasId, string message, string title, string sound, string responseCode, Rock.Data.RockContext rockContext )
        {

            // add communication for reply
            var communication = new Rock.Model.Communication();
            communication.IsBulkCommunication = false;
            communication.Status = CommunicationStatus.Approved;
            communication.SenderPersonAliasId = fromPersonAliasId;
            communication.Subject = string.Format( "From: {0}", fromPersonName );

            communication.SetMediumDataValue( "Message", message );
            communication.SetMediumDataValue( "Title", title );
            communication.SetMediumDataValue( "Sound", sound );

            communication.MediumEntityTypeId = EntityTypeCache.Read( "Rock.Communication.Medium.PushNotification" ).Id;

            var recipient = new Rock.Model.CommunicationRecipient();
            recipient.Status = CommunicationRecipientStatus.Pending;
            recipient.PersonAliasId = toPersonAliasId;
            recipient.ResponseCode = responseCode;
            communication.Recipients.Add( recipient );

            var communicationService = new Rock.Model.CommunicationService( rockContext );
            communicationService.Add( communication );
            rockContext.SaveChanges();

            // queue the sending
            var transaction = new Rock.Transactions.SendCommunicationTransaction();
            transaction.CommunicationId = communication.Id;
            transaction.PersonAlias = null;
            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
        }

        /// <summary>
        /// Creates a recipient token to help track conversations.
        /// </summary>
        /// <param name="rockContext">A context to use for database calls.</param>
        /// <returns>String token</returns>
        private string GenerateResponseCode( Rock.Data.RockContext rockContext )
        {
            bool isUnique = false;
            int randomNumber = -1;
            DateTime tokenStartDate = RockDateTime.Now.Subtract( new TimeSpan( TOKEN_REUSE_DURATION, 0, 0, 0 ) );

            Random rnd = new Random();

            while ( isUnique == false )
            {
                randomNumber = rnd.Next( 100, 1000 );

                if ( randomNumber != 666 ) // just because
                {

                    // check if token has been used recently
                    var communication = new CommunicationRecipientService( rockContext ).Queryable()
                                            .Where( c => c.ResponseCode == "@" + randomNumber.ToString() && c.CreatedDateTime > tokenStartDate )
                                            .FirstOrDefault();
                    if ( communication == null )
                    {
                        isUnique = true;
                    }
                }
            }

            return "@" + randomNumber.ToString();
        }

        /// <summary>
        /// Gets a value indicating whether [supports bulk communication].
        /// </summary>
        /// <value>
        /// <c>true</c> if [supports bulk communication]; otherwise, <c>false</c>.
        /// </value>
        public override bool SupportsBulkCommunication
        {
            get
            {
                return true;
            }
        }
    }
}
