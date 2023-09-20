// <copyright>
// Copyright Pillars Inc.
// </copyright>
//
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace rocks.pillars.Jobs
{
    /// <summary>
    /// Job for sending Email and/or SMS messages.
    /// </summary>
    [PersonField( "Recipient Person", "A specific person that message should be sent to.", false, "", "", 0 )]
    [GroupField( "Recipient Group", "A group that message should be sent to.", false, "", "", 1 )]
    [CodeEditorField("Recipients SQL Query", "Optional SQL query to run for list recipients and/or optional merge data. All resulting rows will be available as a 'rows' Lava merge field when sending to a hard-coded recipient or group. If query includes a 'RecipientPersonId' column, any additional columns will be available as lava merge fields for the message to that recipient.", CodeEditorMode.Sql, CodeEditorTheme.Rock, 200, false, "", "", 2, "SQLQuery")]
    [DataViewsField( "Data Views", "Optional list of data views to use for the list of recipients. Note: any data returned by a SQL Query will not be available as Lava merge field for these recipients.", false, "", "Rock.Model.Person", "", 3 )]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL or DataView queries to complete. Leave blank to use the default (30 seconds).", false, 180, "", 4, "CommandTimeout" )]

    [CodeEditorField("Attachments SQL Query", "Optional SQL query to return a list of binary file attachments. The only column that is needed is a \"BinaryFileId\" column.", CodeEditorMode.Sql, CodeEditorTheme.Rock, 200, false, "", "Attachments", 0)]

    [CodeEditorField( "Email Message", "An email message to send to Recipients. If using a SQL Query a 'rows' Lava merge field will be available with all the resulting query rows. If the query includes a 'RecipientPersonId' column the message will be sent to each of those recipients and each additional column in the result set will be available as a Lava merge field.",
        CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false, "", "Email", 0 )]
    [TextField( "Subject", "Subject for email message", false, "", "Email", 1)]
    [TextField( "From Name", "Name that message should be sent from.", false, "", "Email", 2 )]
    [EmailField( "From Email", "Email that message should be sent from.", false, "", "Email", 3 )]
    [EmailField( "CC", "Comma-Delimited list of emails that message should be CC'd to.", false, "", "Email", 4 )]
    [EmailField( "BCC", "Comma-Delimited list of emails that message should be BCC'd to.", false, "", "Email", 4 )]

    [CodeEditorField( "SMS Message", "A SMS message to send to Recipients. If using a SQL Query a 'rows' Lava merge field will be available with all the resulting query rows. If the query includes a 'RecipientPersonId' column the message will be sent to each of those recipients. If the query includes a 'FromNumberGuid' column the message will be sent from that number. Each additional column in the result set will be available as a Lava merge field.",
        CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false, "", "SMS", 0 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM, "From Numbers", "The Twilio number(s) that message should be sent from. If selecting multiple, the recipients will be divided between the selected numbers. This allows for faster sending to a large number of recipients. If using a SQL query and result includes a 'FromNumberGuid' column, this setting will be ignored", false, true, "", "SMS", 1 )]

    [DisallowConcurrentExecution]
    public class SendMessages : IJob
    {
        private List<string> _imports = new List<string>();

        private int? _mobileNumberTypeValueId = null;

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendMessages()
        {
        }

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            _mobileNumberTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;

            using ( var rockContext = new RockContext() )
            {
                int? commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull();
                var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

                var personService = new PersonService( rockContext );
                var personAliasService = new PersonAliasService(rockContext);
                var dataViewService = new DataViewService( rockContext );
                var phoneService = new PhoneNumberService( rockContext );
                var binaryFileService = new BinaryFileService( rockContext );

                var hardCodedRecipients = new List<Recipient>();
                var dynamicRecipients = new List<Recipient>();

                var attachments = new List<BinaryFile>();

                // Get attachments
                string attachmentsQuery = dataMap.GetString("AttachmentsSQLQuery");
                if (attachmentsQuery.IsNotNullOrWhiteSpace())
                {
                    DataSet ds = DbService.GetDataSet(attachmentsQuery, System.Data.CommandType.Text, null, commandTimeout);
                    if (ds.Tables.Count == 0) throw new Exception("Atachments SQL Query did not return a result set.");

                    var dt = ds.Tables[0];

                    foreach (DataRow row in dt.Rows)
                    {
                        foreach (DataColumn col in dt.Columns)
                        {
                            if(col.ColumnName.Equals("BinaryFileId", StringComparison.OrdinalIgnoreCase))
                            {
                                object cell = row[col.ColumnName];
                                int? binaryFileId = cell.ToString().AsIntegerOrNull();

                                if(binaryFileId.HasValue)
                                {
                                    var binaryFile = binaryFileService.Get(binaryFileId.Value);
                                    if(binaryFile != null)
                                    {
                                        attachments.Add(binaryFile);
                                    }
                                }
                                
                            }
                        }
                    }
                }

                // Get hard-coded recipient Person
                Guid? personAliasGuid = dataMap.GetString( "RecipientPerson" ).AsGuidOrNull();
                if ( personAliasGuid.HasValue )
                {
                    var person = personAliasService.GetPerson( personAliasGuid.Value );
                    var recipient = GetRecipient( person );
                    if ( recipient != null )
                    {
                        recipient.MergeFields = new Dictionary<string, object>( commonMergeFields ) { { "Person", person } };
                        hardCodedRecipients.Add( recipient );
                    }
                }

                // Get hard-coded recipient group
                Guid? groupGuid = dataMap.GetString( "RecipientGroup" ).AsGuidOrNull();
                if ( groupGuid.HasValue )
                {
                    foreach ( var groupMember in new GroupMemberService( rockContext )
                        .GetByGroupGuid( groupGuid.Value )
                        .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ) )
                    {
                        var recipient = GetRecipient( groupMember.Person );
                        if ( recipient != null )
                        {
                            recipient.MergeFields = new Dictionary<string, object>( commonMergeFields ) { { "Person", groupMember.Person } };
                            hardCodedRecipients.Add( recipient );
                        }
                    }
                }

                // Get Recipients SQL Query next
                string recipientsQuery = dataMap.GetString("SQLQuery");
                if (recipientsQuery.IsNotNullOrWhiteSpace())
                {
                    DataSet ds = DbService.GetDataSet( recipientsQuery, System.Data.CommandType.Text, null, commandTimeout );
                    if ( ds.Tables.Count == 0 ) throw new Exception( "Recipients SQL Query did not return a result set." );

                    var dt = ds.Tables[0];

                    var rows = new List<object>();

                    if ( !personAliasGuid.HasValue && !groupGuid.HasValue && !dt.Columns.Contains( "RecipientPersonId" ) )
                    {
                        throw new Exception( "If a Recipient Person or Recipient Group was not specified, the SQL Query must return a result set with a [RecipientPersonId] column." );
                    }

                    foreach ( DataRow row in dt.Rows )
                    {
                        int? recipientPersonId = null;
                        var columns = new Dictionary<string, object>();
                        foreach ( DataColumn col in dt.Columns )
                        {
                            columns.Add( col.ColumnName, row[col.ColumnName] );
                            if ( col.ColumnName.Equals( "RecipientPersonId", StringComparison.OrdinalIgnoreCase ) )
                            {
                                object cell = row[col.ColumnName];
                                recipientPersonId = cell.ToString().AsIntegerOrNull();
                            }
                        }

                        rows.Add( columns );

                        if ( recipientPersonId.HasValue )
                        {
                            var person = personService.Get( recipientPersonId.Value );
                            var recipient = GetRecipient( person );
                            if ( recipient != null )
                            {
                                recipient.MergeFields = new Dictionary<string, object>( commonMergeFields ) { { "Person", person } };
                                foreach ( DataColumn col in dt.Columns )
                                {
                                    if ( col.ColumnName == "FromNumberGuid" )
                                    {
                                        recipient.FromNumberGuid = row[col.ColumnName].ToString().AsGuidOrNull();
                                    }
                                    
                                    if (col.ColumnName == "FromEmail" )
                                    {
                                        recipient.FromEmail = row[col.ColumnName].ToString();
                                    }
                                    
                                    if (col.ColumnName == "FromName")
                                    {
                                        recipient.FromName = row[col.ColumnName].ToString();
                                    }
                                    
                                    recipient.MergeFields.Add( col.ColumnName, row[col.ColumnName] );
                                }
                                dynamicRecipients.Add( recipient );
                            }
                        }
                    }

                    foreach ( var recipient in hardCodedRecipients )
                    {
                        recipient.MergeFields.Add( "rows", rows );
                    }
                }

                // Get recipients from Dataviews next
                var dataViewGuids = dataMap.GetString( "DataViews" ).SplitDelimitedValues().AsGuidList();
                foreach ( var guid in dataViewGuids )
                {
                    var dataView = dataViewService.Get( guid );
                    if ( dataView != null )
                    {
                        var dvArgs = new DataViewGetQueryArgs
                        {
                            DbContext = rockContext,
                            DatabaseTimeoutSeconds = commandTimeout
                        };
                        foreach ( var person in dataView.GetQuery( dvArgs ).OfType<Person>() )
                        {
                            var recipient = GetRecipient( person );
                            if ( recipient != null )
                            {
                                recipient.MergeFields = new Dictionary<string, object>( commonMergeFields ) { { "Person", person } };
                                dynamicRecipients.Add( recipient );
                            }
                        }
                    }
                }

                var allRecipients = new List<Recipient>( hardCodedRecipients );
                allRecipients.AddRange( dynamicRecipients );

                if ( allRecipients.Any() )
                {
                    var tasks = new ConcurrentBag<Task>();

                    // Send Email Message
                    string emailMessage = dataMap.GetString( "EmailMessage" );
                    if ( emailMessage.IsNotNullOrWhiteSpace() )
                    {
                        //Set the From Email and From Name to user defined if not returned on query
                        allRecipients.Where(r => r.FromEmail.IsNullOrWhiteSpace() )
                            .ToList().ForEach(r => 
                            {
                                r.FromEmail = dataMap.GetString("FromEmail");
                            });

                        allRecipients.Where( r => r.FromName.IsNullOrWhiteSpace() )
                            .ToList().ForEach( r =>
                            {
                                r.FromName = dataMap.GetString( "FromName" );
                            } );

                        var emailGroups = allRecipients.GroupBy(r => new { r.FromName, r.FromEmail } );

                        foreach(var emailGroup in emailGroups)
                        {
                            tasks.Add(Task.Run(() =>
                             {
                                 RockEmailMessage rockEmailMessage = new RockEmailMessage();

                                 foreach(var recipient in emailGroup)
                                 {
                                     rockEmailMessage.AddRecipient(new RockEmailMessageRecipient(recipient.Person, recipient.MergeFields));
                                 }

                                 rockEmailMessage.Subject = dataMap.GetString("Subject");
                                 rockEmailMessage.FromName = emailGroup.Key.FromName;
                                 rockEmailMessage.FromEmail = emailGroup.Key.FromEmail;
                                 rockEmailMessage.CCEmails = dataMap.GetString( "CC" ).SplitDelimitedValues( false ).ToList();
                                 rockEmailMessage.BCCEmails = dataMap.GetString( "BCC" ).SplitDelimitedValues( false ).ToList();
                                 rockEmailMessage.Message = emailMessage;
                                 rockEmailMessage.Attachments = attachments;
                                 rockEmailMessage.Send();
                             }));
                        }
                    }

                    // Send SMS Messages
                    string smsMessage = dataMap.GetString( "SMSMessage" );
                    if ( smsMessage.IsNotNullOrWhiteSpace() )
                    { 
                        var fromNumberGuids = dataMap.GetString( "FromNumbers" ).SplitDelimitedValues().AsGuidList();
                        if ( fromNumberGuids.Any() )
                        {
                            int counter = 0;
                            int fromNumbersCount = fromNumberGuids.Count();

                            foreach ( var recipient in allRecipients.Where( r => !r.FromNumberGuid.HasValue ) )
                            {
                                counter++;
                                int index = counter % fromNumbersCount;
                                recipient.FromNumberGuid = fromNumberGuids[index];
                            }
                        }

                        foreach ( var fromNumberGuid in allRecipients
                            .Where( r => r.FromNumberGuid.HasValue )
                            .Select( r => r.FromNumberGuid.Value )
                            .Distinct() )
                        {
                            var fromNumber = DefinedValueCache.Get(fromNumberGuid);
                            Person fromPerson = null;
                            if (fromNumber != null)
                            {
                                var fromPersonAliasGuid = fromNumber.GetAttributeValue("ResponseRecipient").AsGuidOrNull();
                                if (fromPersonAliasGuid.HasValue )
                                {
                                    fromPerson = personAliasService.GetPerson(fromPersonAliasGuid.Value);
                                }
                            }

                            // send an SMS message for each batch
                            tasks.Add( Task.Run( (Action)( () =>
                            {
                                RockSMSMessage rockSmsMessage = new RockSMSMessage();

                                foreach ( var recipient in allRecipients.Where( r => r.FromNumberGuid.HasValue && r.FromNumberGuid.Value == fromNumberGuid ) )
                                {
                                    rockSmsMessage.AddRecipient( new RockSMSMessageRecipient( recipient.Person, recipient.PhoneNumber, recipient.MergeFields ) );
                                }

                                rockSmsMessage.CurrentPerson = fromPerson;
                                rockSmsMessage.FromNumber = fromNumber;
                                rockSmsMessage.Message = smsMessage;
                                rockSmsMessage.Attachments = attachments;
                                rockSmsMessage.Send();
                            } ) ) );
                        }
                    }

                    Task.WaitAll( tasks.ToArray() );
                }
            }
        }

        private Recipient GetRecipient( Person person )
        {
            if ( person == null ) return null;

            var phoneNumber = person.PhoneNumbers
                .FirstOrDefault( n =>
                    n.NumberTypeValueId == _mobileNumberTypeValueId &&
                    n.IsMessagingEnabled );

            return new Recipient {
                Person = person,
                Email = person.Email,
                PhoneNumber = phoneNumber != null ? "+" + phoneNumber.CountryCode + phoneNumber.Number : string.Empty
            };
        }
    }

    public class Recipient
    {
        public Person Person { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public Dictionary<string, object> MergeFields { get; set; }
        public Guid? FromNumberGuid { get; set; }
	    public string FromEmail { get; set; }
        public string FromName { get; set; }
    }
}
