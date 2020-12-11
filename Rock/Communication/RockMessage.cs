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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication
{
    /// <summary>
    /// Rock Message base class
    /// </summary>
    public abstract class RockMessage
    {
        /// <summary>
        /// The recipients
        /// </summary>
        protected List<RockMessageRecipient> Recipients { get; set; } = new List<RockMessageRecipient>();

        /// <summary>
        /// Gets the medium entity type identifier.
        /// </summary>
        /// <value>
        /// The medium entity type identifier.
        /// </value>
        public abstract int MediumEntityTypeId { get; }

        /// <summary>
        /// Gets or sets the application root.
        /// </summary>
        /// <value>
        /// The application root.
        /// </value>
        public string AppRoot { get; set; }

        /// <summary>
        /// Gets or sets the theme root.
        /// </summary>
        /// <value>
        /// The theme root.
        /// </value>
        public string ThemeRoot { get; set; }

        /// <summary>
        /// Gets or sets the person that was logged in when this message was created
        /// </summary>
        /// <value>
        /// The current person.
        /// </value>
        public Person CurrentPerson { get; set; }

        /// <summary>
        /// Gets or sets the enabled lava commands.
        /// </summary>
        /// <value>
        /// The enabled lava commands.
        /// </value>
        public string EnabledLavaCommands { get; set; }

        /// <summary>
        /// Gets or sets any additional merge fields that are appended to each recipient's merge fields
        /// </summary>
        /// <value>
        /// The merge fields.
        /// </value>
        public Dictionary<string, object> AdditionalMergeFields { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets a value indicating whether the message should be sent separately to each recipient. If merge fields are used, this is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [send separately to each recipient]; otherwise, <c>false</c>.
        /// </value>
        public bool SendSeperatelyToEachRecipient { get; set; } = true;

        /// <summary>
        /// Gets or sets the attachments.
        /// </summary>
        /// <value>
        /// The attachments.
        /// </value>
        public List<BinaryFile> Attachments { get; set; } = new List<BinaryFile>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RockMessage"/> class.
        /// </summary>
        public RockMessage()
        {
            CreateCommunicationRecord = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [create communication record].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [create communication record]; otherwise, <c>false</c>.
        /// </value>
        public bool CreateCommunicationRecord { get; set; }

        #region Obsolete

        /// <summary>
        /// Adds the recipient.
        /// </summary>
        /// <param name="to">To.</param>
        [RockObsolete( "1.10" )]
        [Obsolete( "This has a issue where the wrong person(s) might be logged as the recipient. Use the AddRecipient that takes RockMessageRecipient as a parameter" )]
        public void AddRecipient( string to )
        {
            if ( to.IsNotNullOrWhiteSpace() )
            {
                Recipients.Add( RockEmailMessageRecipient.CreateAnonymous( to, null ) );
            }
        }

        /// <summary>
        /// Adds the recipient.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        [RockObsolete( "1.10" )]
        [Obsolete( "This has a issue where the wrong person(s) might be logged as the recipient. Use the AddRecipient that takes RockMessageRecipient as a parameter" )]
        public void AddRecipient( RecipientData recipient )
        {
            Recipients.Add( RockEmailMessageRecipient.CreateAnonymous( recipient.To, recipient.MergeFields ) );
        }

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="toEmails">To emails.</param>
        [RockObsolete( "1.10" )]
        [Obsolete( "This has a issue where the wrong person(s) might be logged as the recipient. Use the SetRecipients that uses List<RockMessageRecipient> as a parameter" )]
        public void SetRecipients( string toEmails )
        {
            Recipients.AddRange( toEmails.SplitDelimitedValues().ToList().Select( a => RockEmailMessageRecipient.CreateAnonymous( a, null ) ) );
        }

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="recipientData">The recipient data.</param>
        [RockObsolete( "1.10" )]
        [Obsolete( "This has a issue where the wrong person(s) might be logged as the recipient. Use the SetRecipients that uses List<RockMessageRecipient> as a parameter" )]
        public void SetRecipients( List<RecipientData> recipientData )
        {
            this.Recipients = new List<RockMessageRecipient>();
            foreach ( var recipient in recipientData )
            {
                // assume it is an email recipient
                this.AddRecipient( RockEmailMessageRecipient.CreateAnonymous( recipient.To, recipient.MergeFields ) );
            }
        }

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="toEmails">To emails.</param>
        [RockObsolete( "1.10" )]
        [Obsolete( "This has a issue where the wrong person(s) might be logged as the recipient. Use the SetRecipients that uses List<RockMessageRecipient> as a parameter" )]
        public void SetRecipients( List<string> toEmails )
        {
            Recipients.AddRange( toEmails.Select( a => RockEmailMessageRecipient.CreateAnonymous( a, null ) ) );
        }

        /// <summary>
        /// Gets the recipient data.
        /// </summary>
        /// <returns></returns>
        [RockObsolete( "1.10" )]
        [Obsolete( "Use List<RockMessageRecipient> GetRecipients() instead" )]
        public List<RecipientData> GetRecipientData()
        {
            return Recipients.Select( a => new RecipientData( a.To, a.MergeFields ) ).ToList();
        }

        #endregion Obsolete

        /// <summary>
        /// Adds the recipient.
        /// </summary>
        /// <param name="messageRecipient">The message recipient.</param>
        public void AddRecipient( RockMessageRecipient messageRecipient )
        {
            this.Recipients.Add( messageRecipient );
        }

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="people">The people.</param>
        public void SetRecipients( IQueryable<Person> people )
        {
            Recipients = new List<RockMessageRecipient>();
            if ( people != null )
            {
                Recipients.AddRange( people.AsNoTracking().ToList().Select( p => new RockEmailMessageRecipient( p, null ) ).ToList() );
            }
        }

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="personIds">The person ids.</param>
        public void SetRecipients( List<int> personIds )
        {
            if ( personIds != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    SetRecipients( new PersonService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p => personIds.Contains( p.Id ) ) );
                }
            }
        }

        /// <summary>
        /// Sets the recipients from the active members of the Group
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        public void SetRecipients( int groupId )
        {
            using ( var rockContext = new RockContext() )
            {
                SetRecipients( new GroupMemberService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where(
                        m => m.GroupId == groupId &&
                        m.GroupMemberStatus == GroupMemberStatus.Active )
                    .Select( m => m.Person ) );
            }
        }

        /// <summary>
        /// Sets the recipients from the active members of the Group
        /// </summary>
        /// <param name="group">The group.</param>
        public void SetRecipients( Group group )
        {
            SetRecipients( group.Id );
        }

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        public void SetRecipients( List<RockMessageRecipient> recipients )
        {
            this.Recipients = recipients.ToList();
        }

        /// <summary>
        /// Gets the recipients.
        /// </summary>
        /// <returns></returns>
        public List<RockMessageRecipient> GetRecipients()
        {
            return Recipients;
        }

        /// <summary>
        /// Sends this message(Email, SMS, or PushDevice, depending which RockMessage class you are using).
        /// NOTE: Email exceptions will be logged to exception log, but won't throw an exception. To get the list of errorMessages, use Send( out List&lt;string&gt; errorMessages )
        /// </summary>
        /// <returns></returns>
        public virtual bool Send()
        {
            var errorMessages = new List<string>();
            return Send( out errorMessages );
        }

        /// <summary>
        /// Sends this message(Email, SMS, or PushDevice, depending which RockMessage class you are using).
        /// NOTE: Email exceptions will be logged to exception log, but won't throw an exception. Ensure you check for error messages and the boolean value to handle error causes where a communication may not be sent.
        /// </summary>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public virtual bool Send( out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                if ( this.Recipients.Any() )
                {
                    var mediumEntity = EntityTypeCache.Get( MediumEntityTypeId );
                    if ( mediumEntity != null )
                    {
                        var medium = MediumContainer.GetComponent( mediumEntity.Name );
                        if ( medium != null )
                        {
                            medium.Send( this, out errorMessages );
                            return !errorMessages.Any();
                        }
                    }

                    errorMessages.Add( "Could not find valid Medium" );
                    return false;
                }

                return true;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
                errorMessages.Add( ex.Message );
                return false;
            }
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<SendMessageResult> SendAsync()
        {
            var sendEmailResult = new SendMessageResult();

            try
            {
                if ( this.Recipients.Any() )
                {
                    var mediumEntity = EntityTypeCache.Get( MediumEntityTypeId );
                    if ( mediumEntity != null )
                    {
                        var medium = MediumContainer.GetComponent( mediumEntity.Name );
                        if ( medium != null )
                        {
                            var iAsyncMedium = medium as IAsyncMediumComponent;
                            if ( iAsyncMedium == null )
                            {
                                medium.Send( this, out var errorMessages );
                                sendEmailResult.Errors.AddRange( errorMessages );
                            }
                            else
                            {
                                sendEmailResult = await iAsyncMedium.SendAsync( this ).ConfigureAwait( false );
                            }
                            return sendEmailResult;
                        }
                    }

                    sendEmailResult.Errors.Add( "Could not find valid Medium" );
                    return sendEmailResult;
                }

                return sendEmailResult;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
                sendEmailResult.Errors.Add( ex.Message );
                return sendEmailResult;
            }
        }
    }
}
