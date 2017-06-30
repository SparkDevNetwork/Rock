using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.Model;

namespace Rock.Communication
{
    public abstract class RockMessage
    {
        /// <summary>
        /// The recipients
        /// </summary>
        private List<RecipientData> _recipients = new List<RecipientData>();

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
        /// Gets or sets the current person.
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
        /// Gets or sets a value indicating whether the message should be sent seperately to each recipient. If merge fields are used, this is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [send seperately to each recipient]; otherwise, <c>false</c>.
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
        /// Gets or sets the recipients.
        /// </summary>
        /// <value>
        /// The recipients.
        /// </value>
        public List<string> Recipients
        {
            get
            {
                return _recipients.Select( r => r.To ).ToList();
            }
            set
            {
                _recipients = new List<RecipientData>();
                if ( value != null )
                {
                    value.ForEach( r => _recipients.Add( new RecipientData( r ) ) );
                }
            }
        }

        public bool CreateCommunicationRecord { get; set; }

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="people">The people.</param>
        public void SetRecipients( IQueryable<Person> people )
        {
            _recipients = new List<RecipientData>();

            if ( people != null )
            {
                people.ToList().ForEach( p => _recipients.Add( new RecipientData( p.Email ) ) );
            }
        }

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="personIds">The person ids.</param>
        public void SetRecipients( List<int> personIds )
        {
            _recipients = new List<RecipientData>();
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
        /// Sets the recipients.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        public void SetRecipients( int groupId )
        {
            _recipients = new List<RecipientData>();

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
        /// Sets the recipients.
        /// </summary>
        /// <param name="group">The group.</param>
        public void SetRecipients( Group group )
        {
            SetRecipients( group.Id );
        }

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="recipientData">The recipient data.</param>
        public void SetRecipients( List<RecipientData> recipientData )
        {
            _recipients = recipientData;
        }

        /// <summary>
        /// Gets the recipient data.
        /// </summary>
        /// <returns></returns>
        public List<RecipientData> GetRecipientData()
        {
            return _recipients;
        }

        /// <summary>
        /// Sends the specified error message.
        /// </summary>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public abstract bool Send( out List<string> errorMessages );

    }
}
