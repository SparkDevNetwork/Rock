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
using System.Linq;
using System.Web;
using System.IO;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Web;
using Rock.Web.UI;
using Rock.Communication;

using Microsoft.Exchange.WebServices;
using Microsoft.Exchange.WebServices.Data;
using System.Text.RegularExpressions;

namespace com.centralaz.GeneralJobs.Jobs
{

    /// <summary>
    /// Connects to an Exchange mailbox, reads the emails, finds a matching person and updates the person's attribute with the value found in the matching Field to Store regular expression.
    /// The contents of the email message must have a person's name, birthdate, and email address to find a matching person record in Rock.
    /// 
    /// It will move the messages it processes into one of these four folders:
    ///  * Done - if it able to find a single matching person record and the person passed
    ///  * Pending - if it was unable to find a single matching person record
    ///  * Other - if the message did not have the proper Subject (probably means it's spam)
    ///  * Failed - if the score was below a passing percentage.
    /// </summary>

    [TextField( "AutoDiscoverUrl", "Email address of the user on the mail server (Ex: responder@domain.com).", category: "Mail Server Settings", order: 1 )]
    [TextField( "Mail Username", "Exchange account to login to.", category: "Mail Server Settings", order: 2)]
    [TextField( "Mail Password", "Password of the account.", isPassword: true, category: "Mail Server Settings", order: 3 )]

    [TextField( "Subject Contains", "A String filter that the subject must contain.", true, "Mandated Reporting Quiz Results", category: "Email Content Settings", order: 1 )]
    [IntegerField( "Passing Percent Score", "If applicable, the percentage of correctly answered questions necessary to pass the test and record the result in the person's attribute. Leave blank if no score is required to save the results.", false, 95, category: "Email Content Settings", order: 2 )]
    [TextField( "Score Regex", "The regular expression for the score match. If using the Passing Percent Score, the Regex should have two groups, one for the person's score and the other for the total. (e.g., 78 out of 100)", false, @"Score:[\r\n]*(\d+) out of (\d+)[\r\n]*", category: "Email Content Settings", order: 3 )]
    [TextField( "Name Regex", "The regular expression for the person name match.", true, @"Name:[\r\n]*(.+)[\r\n]*", category: "Email Content Settings", order: 4 )]
    [TextField( "Date of Birth Regex", "The regular expression for the person's data of birth match.", true, @"Date of Birth:[\r\n]*[^\d]*(\d+/\d+/\d+)[\r\n]*", category: "Email Content Settings", order: 5 )]
    [TextField( "Email Regex", "The regular expression for the person's email match.", true, @"Email:[\r\n]*(\S+)[\r\n]*", category: "Email Content Settings", order: 6 )]
    [TextField( "Field to Store Regex", "The regular expression for the field (value) you wish to match and store in the person attribute. The Regex must contain ONLY ONE grouping.", true, @"Date Completed:[\r\n]*(.+)", category: "Email Content Settings", order: 7 )]

    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Person Attribute", "The attribute where you want to store the results of the value that matches the 'Field to Store Regex' found in the email.", true, false, category:"Storage", order: 6 )]

    [IntegerField( "Message Batch Size", "Max number of e-mails to process with each running of the agent (recommended 30).", category: "Processing Settings", order: 7 )]
    [BooleanField( "Enable Logging", "Enable logging", false, category: "Processing Settings", order:8 )]

    [DisallowConcurrentExecution]
    public class EmailToAttribute : IJob
    {
        private bool _loggingActive = false;
        private string _pathName;
        private AttributeCache _personAttribute = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailToAttribute"/> class.
        /// </summary>
        public EmailToAttribute()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            String root = System.Web.Hosting.HostingEnvironment.MapPath( "~/App_Data/Logs/" );
            String now = RockDateTime.Now.ToString( "yyyy-MM-dd-HH-mm-ss" );
            _pathName = String.Format( "{0}{1}-EmailToAttribute.log", root, now );
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            int messagesProcessed = 0;
            int recordsUpdated = 0;
            int errors = 0;

            int passingPercentage = dataMap.Get( "PassingPercentScore" ).ToString().AsInteger();
            var personAttributeGuid = dataMap.Get( "PersonAttribute" ).ToString().AsGuidOrNull();

            if ( personAttributeGuid != null )
            {
                _personAttribute = AttributeCache.Read( personAttributeGuid.Value );
            }
            else
            {
                context.Result = "Error. Job configuration has illegal 'person attribute' setting.";
                return;
            }

            if ( dataMap.Get( "EnableLogging" ).ToString().AsBoolean() )
            {
                _loggingActive = true;
            }
            else
            {
                _loggingActive = false;
            }

            try
            {
                PropertySet propSet = new PropertySet( BasePropertySet.IdOnly, ItemSchema.Body, EmailMessageSchema.Body );
                try
                {
                    string fullName = string.Empty;
                    string firstName = string.Empty;
                    string lastName = string.Empty;
                    string birthDate = string.Empty;
                    string email = string.Empty;
                    string fieldValue = string.Empty;
                    string percentScore = string.Empty;

                    Match matchFullName;
                    Match matchBirthDate;
                    Match matchEmail;
                    Match matchFieldToStore;
                    Match matchScore;

                    Regex reScore = new Regex( dataMap.Get( "ScoreRegex" ).ToString() , RegexOptions.IgnoreCase );
                    Regex reFullName = new Regex( dataMap.Get( "NameRegex" ).ToString(), RegexOptions.IgnoreCase );
                    Regex reBirthDate = new Regex( dataMap.Get( "DateofBirthRegex" ).ToString(), RegexOptions.IgnoreCase );
                    Regex reEmailAddress = new Regex( dataMap.Get( "EmailRegex" ).ToString(), RegexOptions.IgnoreCase );
                    Regex reFieldToStore = new Regex( dataMap.Get( "FieldtoStoreRegex" ).ToString(), RegexOptions.IgnoreCase );

                    Regex reEmptyLines = new Regex( @"^\s+$[\r\n]*", RegexOptions.Multiline );

                    bool performScoreMatching = ( ! string.IsNullOrEmpty( dataMap.Get( "ScoreRegex" ).ToString() ) );
                    bool foundScore = false;
                    bool isScorePass = false;

                    ExchangeService service = new ExchangeService( ExchangeVersion.Exchange2010_SP2 );
                    service.Credentials = new WebCredentials( dataMap.Get( "MailUsername" ).ToString(), dataMap.Get( "MailPassword" ).ToString() );
                    service.AutodiscoverUrl( dataMap.Get( "AutoDiscoverUrl" ).ToString() );

                    Folder otherFolder = GetFolder( "Other", service );
                    Folder doneFolder = GetFolder( "Done", service );
                    Folder pendingFolder = GetFolder( "Pending", service );
                    Folder failedFolder = GetFolder( "Failed", service );

                    int messageBatchSize = dataMap.Get( "MessageBatchSize" ).ToString().AsInteger();
                    ItemView view = new ItemView( messageBatchSize );
                    FindItemsResults<Item> findResults = service.FindItems( WellKnownFolderName.Inbox, new ItemView( messageBatchSize ) );

                    if ( findResults != null && findResults.Items != null && findResults.Items.Count > 0 )
                    {
                        foreach ( Item item in findResults.Items )
                        {
                            messagesProcessed++;
                            birthDate = string.Empty;
                            foundScore = false;
                            isScorePass = false;

                            EmailMessage message = EmailMessage.Bind( service, item.Id, propSet );

                            if ( item.Subject.Contains( dataMap.Get( "SubjectContains" ).ToString() ) )
                            {
                                var body = message.Body.Text.SanitizeHtml();
                                body = reEmptyLines.Replace( body, "" );
                                body = body.Replace( "\r", string.Empty );

                                matchFullName = reFullName.Match( body );
                                matchBirthDate = reBirthDate.Match( body );
                                matchEmail = reEmailAddress.Match( body );
                                matchFieldToStore = reFieldToStore.Match( body );

                                // Check if we're doing score checking...
                                if ( performScoreMatching )
                                {
                                    matchScore = reScore.Match( body );

                                    if ( matchScore.Success )
                                    {
                                        foundScore = true;
                                        percentScore = matchScore.Groups[1].Value;

                                        // Check their score and set isScorePass true if they passed.
                                        if ( int.Parse( percentScore ) >= passingPercentage )
                                        {
                                            isScorePass = true;
                                        }
                                    }
                                }

                                // If we don't match these things, then it must be moved to the pending folder 
                                if ( matchFullName.Success && matchEmail.Success && matchFieldToStore.Success && 
                                    ( ( performScoreMatching && foundScore ) || ! performScoreMatching ) )
                                {
                                    fullName = matchFullName.Groups[1].Value;
                                    if ( fullName.IndexOf( ' ' ) > 0 )
                                    {
                                        var nameParts = fullName.Split( ' ' );
                                        firstName = nameParts[0];
                                        lastName = nameParts[1];
                                    }
                                    email = matchEmail.Groups[1].Value;
                                    fieldValue = matchFieldToStore.Groups[1].Value;

                                    // Check their score and fail them if they didn't pass.
                                    if ( performScoreMatching && foundScore && ! isScorePass )
                                    {
                                        MoveMessageToFolder( message, failedFolder );
                                        continue;
                                    }
                                }
                                else
                                {
                                    MoveMessageToFolder( message, pendingFolder );
                                    continue;
                                }

                                // now check these optional values -- we only need them in the case of
                                // multiple matches of the same email, first and last name.
                                if ( matchFullName.Success && matchEmail.Success &&
                                    matchBirthDate.Success && matchFieldToStore.Success
                                    )
                                {
                                    birthDate = matchBirthDate.Groups[1].Value;
                                }

                                // find person records with this email address
                                List<Person> people = new List<Person>();

                                Person person = BestMatchingPerson( people, firstName, lastName, birthDate );
                                if ( person != null )
                                {
                                    UpdateDateAttributeOnPersonRecord( person, fieldValue, dataMap );
                                    MoveMessageToFolder( message, doneFolder );
                                    recordsUpdated++;
                                }
                                else
                                {
                                    MoveMessageToFolder( message, pendingFolder );
                                }
                            }
                            else
                            {
                                MoveMessageToFolder( message, otherFolder );
                            }
                        }
                    }
                    service = null;
                    otherFolder = null;
                    pendingFolder = null;
                }
                catch ( Exception e )
                {
                    LogToFile( "Error while reading Inbox of the configured Exchange account. " + e.Message );
                    errors++;
                }
                finally
                {
                    propSet = null;
                }
            }
            catch ( Exception ex )
            {
                LogToFile( "An error occured while processing the Inbox.\n\nMessage\n------------------------\n" + ex.Message + "\n\nStack Trace\n------------------------\n" + ex.StackTrace );
                errors++;
            }

            var resultMessage = string.Empty;
            if ( messagesProcessed == 0 )
            {
                resultMessage = "No items processed";
            }
            else
            {
                resultMessage = string.Format( "{0} items were processed and {1} people records were updated", messagesProcessed, recordsUpdated );
            }

            if ( errors > 0 )
            {
                resultMessage += string.Format( "; but {0} errors were encountered.", errors );
            }

            context.Result = resultMessage;
        }

        /// <summary>
        /// Updates the given person's person attribute with the given value.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="attribValue">the attribute value</param>
        /// <exception cref="System.ArgumentNullException">If the given date can't be parsed.</exception>
        /// <exception cref="System.FormatException">If the given date can't be parsed.</exception>
        private void UpdateDateAttributeOnPersonRecord( Person person, string attribValue, JobDataMap dataMap )
        {
            var rockContext = new RockContext();

            AttributeValue av = new AttributeValueService( rockContext ).GetByAttributeIdAndEntityId( _personAttribute.Id, person.Id );
            if ( av == null )
            {
                av = new AttributeValue { AttributeId = _personAttribute.Id, EntityId = person.Id, Value = attribValue };
                rockContext.AttributeValues.Add( av );
            }
            else
            {
                av.Value = attribValue;
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Find the best matching person record from the collection for the
        /// given email address.
        /// </summary>
        /// <param name="people">A collection of people that have the given email address.</param>
        /// <param name="email">The email address used to find the best match.</param>
        /// <returns>A single, best match, person record.</returns>
        private Person BestMatchingPerson( List<Person> people, string firstName, string lastName, string birthDateString )
        {
            firstName = firstName.ToLower();
            lastName = lastName.ToLower();
            DateTime birthDate;

            Guid recordStatusActive = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid();

            // If there's only one person that matches the first and last name, return it; it's the match
            var peopleQuery = new PersonService( new RockContext() ).Queryable().Where( p =>
                                  ( p.FirstName.ToLower() == firstName || p.NickName.ToLower() == firstName )
                                   && p.LastName.ToLower() == lastName
                                   && p.RecordStatusValue.Guid == recordStatusActive );

            if ( peopleQuery.Count() == 1 )
            {
                return peopleQuery.FirstOrDefault();
            }
            else
            {
                // if we found multiple matches and we have a valid date, then try matching based on the birthdate
                if ( DateTime.TryParse( birthDateString, out birthDate ) )
                {
                    peopleQuery = peopleQuery.Where( p => p.BirthDate == birthDate );

                    if ( peopleQuery.Count() == 1 )
                    {
                        return peopleQuery.FirstOrDefault();
                    }
                }
            }

            // if we make it down here, then we've got no match.
            return null;
        }

        /// <summary>
        /// Gets the proper folder object for the given name.
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public Folder GetFolder( string folderName, ExchangeService service )
        {
            // Create a view with a page size of 10.
            FolderView view = new FolderView( 10 );

            // Identify the properties to return in the results set.
            view.PropertySet = new PropertySet( BasePropertySet.IdOnly );
            view.PropertySet.Add( FolderSchema.DisplayName );

            // Return only folders that contain items.
            SearchFilter searchFilter = new SearchFilter.ContainsSubstring( FolderSchema.DisplayName, folderName );

            // Unlike FindItem searches, folder searches can be deep traversals.
            view.Traversal = FolderTraversal.Deep;

            // Send the request to search the mailbox and get the results.
            FindFoldersResults findFolderResults = service.FindFolders( WellKnownFolderName.Root, searchFilter, view );

            // Process each item.
            foreach ( Folder myFolder in findFolderResults.Folders )
            {
                if ( myFolder.DisplayName == folderName )
                {
                    return myFolder;
                }
            }

            // If we get here, we could not find the older so we need to create a new one...

            // Create a custom  folder.
            Folder folder = new Folder( service );
            folder.DisplayName = folderName;

            // Save the folder as a child folder in the Inbox folder.
            // This method call results in a CreateFolder call to EWS.
            folder.Save( WellKnownFolderName.MsgFolderRoot );

            return folder;
        }

        /// <summary>
        /// Moves the message to the given folder.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="otherFolder"></param>
        public void MoveMessageToFolder( EmailMessage message, Folder otherFolder )
        {
            if ( otherFolder != null )
            {
                // Move the specified mail to the JunkEmail folder and store the returned item.
                Item item = message.Move( otherFolder.Id );
            }
            else
            {
                ExceptionLogService.LogException( new Exception( "One of the required folders is missing (Pending, Other, or Done)." ), null );
            }
        }

        /// <summary>
        /// Logs to file.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="pathName">Name of the path.</param>
        protected void LogToFile( String message )
        {
            if ( _loggingActive )
            {
                using ( var str = new StreamWriter( _pathName, true ) )
                {
                    str.WriteLine( message );
                    str.Flush();
                }
            }

        }
    }
}