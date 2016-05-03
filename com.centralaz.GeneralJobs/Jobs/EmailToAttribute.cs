// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    /// Connects to an Exchange mailbox, reads the emails, finds a matching person and updates the date on the configured date based attribute.
    /// </summary>
    [IntegerField( "Attribute Id" )]
    [TextField( "AutoDiscoverUrl", "Hostname of the mail server." )]
    [TextField( "Mail Username", "Exchange account to login to." )]
    [TextField( "Subject Contains", "A String filter that the subject must contain.", true, "Mandated Reporting Quiz Results" )]
    [TextField( "Mail Password", "Password of the account.", isPassword: true )]
    [IntegerField( "Message Batch Size", "Max number of e-mails to process with each running of the agent (recommended 30)." )]
    [BooleanField( "Enable Logging", "Enable logging", false, "", 8 )]
    [DisallowConcurrentExecution]
    public class EmailToAttribute : IJob
    {
        private bool _loggingActive = false;
        private string _pathName;

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
            String now = DateTime.Now.ToString( "yyyy-MM-dd-HH-mm-ss" );
            _pathName = String.Format( "{0}{1}-EmailToAttribute.log", root, now );
            JobDataMap dataMap = context.JobDetail.JobDataMap;

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
                    Regex reFirstName = new Regex( @"Custom variable 1: (\w+)" );
                    Regex reLastName = new Regex( @"Custom variable 2: ([a-zA-Z -]+)" );
                    Regex reBirthDay = new Regex( @"Custom variable 3: (\d+)" );
                    Regex reBirthMonth = new Regex( @"Custom variable 4: (\S+)" );
                    Regex reBirthYear = new Regex( @"Custom variable 5: (\d+)" );
                    Regex reEmailAddress = new Regex( @"Custom variable 7: (\S+)" );
                    Regex reDateCompleted = new Regex( @"Date completed: (\d+/\d+/\d+)" );

                    Match matchFirstName;
                    Match matchLastName;
                    Match matchBirthDay;
                    Match matchBirthMonth;
                    Match matchBirthYear;
                    Match matchEmail;
                    Match matchDateCompleted;

                    string firstName = string.Empty;
                    string lastName = string.Empty;
                    string birthDay = string.Empty;
                    string birthMonth = string.Empty;
                    string birthYear = string.Empty;
                    string email = string.Empty;
                    string dateCompleted = string.Empty;

                    ExchangeService service = new ExchangeService( ExchangeVersion.Exchange2010_SP2 );
                    service.Credentials = new WebCredentials( dataMap.Get( "MailUsername" ).ToString(), dataMap.Get( "MailPassword" ).ToString() );
                    service.AutodiscoverUrl( dataMap.Get( "AutoDiscoverUrl" ).ToString() );

                    Folder otherFolder = GetFolder( "Other", service );
                    Folder doneFolder = GetFolder( "Done", service );
                    Folder pendingFolder = GetFolder( "Pending", service );

                    int messageBatchSize = dataMap.Get( "MessageBatchSize" ).ToString().AsInteger();
                    ItemView view = new ItemView( messageBatchSize );
                    FindItemsResults<Item> findResults = service.FindItems( WellKnownFolderName.Inbox, new ItemView( messageBatchSize ) );

                    if ( findResults != null && findResults.Items != null && findResults.Items.Count > 0 )
                    {
                        foreach ( Item item in findResults.Items )
                        {
                            birthDay = string.Empty;
                            birthMonth = string.Empty;
                            birthYear = string.Empty;

                            EmailMessage message = EmailMessage.Bind( service, item.Id, propSet );

                            if ( item.Subject.Contains( dataMap.Get( "SubjectContains" ).ToString() ) )
                            {
                                matchFirstName = reFirstName.Match( message.Body.Text );
                                matchLastName = reLastName.Match( message.Body.Text );
                                matchBirthDay = reBirthDay.Match( message.Body.Text );
                                matchBirthMonth = reBirthMonth.Match( message.Body.Text );
                                matchBirthYear = reBirthYear.Match( message.Body.Text );
                                matchEmail = reEmailAddress.Match( message.Body.Text );
                                matchDateCompleted = reDateCompleted.Match( message.Body.Text );

                                // If we don't match these things, then it must be moved to the pending folder 
                                if ( matchFirstName.Success && matchLastName.Success && matchEmail.Success &&
                                    matchDateCompleted.Success
                                    )
                                {
                                    firstName = matchFirstName.Groups[1].Value;
                                    lastName = matchLastName.Groups[1].Value;
                                    email = matchEmail.Groups[1].Value;
                                    dateCompleted = matchDateCompleted.Groups[1].Value;
                                }
                                else
                                {
                                    MoveMessageToFolder( message, pendingFolder );
                                    continue;
                                }

                                // now check these optional values -- we only need them in the case of
                                // multiple matches of the same email, first and last name.
                                if ( matchFirstName.Success && matchLastName.Success && matchEmail.Success &&
                                    matchBirthDay.Success && matchBirthMonth.Success && matchBirthYear.Success &&
                                    matchDateCompleted.Success
                                    )
                                {
                                    birthDay = matchBirthDay.Groups[1].Value;
                                    birthMonth = matchBirthMonth.Groups[1].Value;
                                    birthYear = matchBirthYear.Groups[1].Value;
                                }

                                // find person records with this email address
                                List<Person> people = new List<Person>();

                                Person person = BestMatchingPerson( people, firstName, lastName, string.Format( "{0}, {1} {2}", birthMonth, birthDay, birthYear ) );
                                if ( person != null )
                                {
                                    UpdateDateAttributeOnPersonRecord( person, dateCompleted, dataMap );
                                    MoveMessageToFolder( message, doneFolder );
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
                }
                catch ( Exception e )
                {
                    LogToFile( "Error while reading Inbox of the configured Exchange account." + e );
                }
            }
            catch ( Exception ex )
            {
                LogToFile( "An error occured while processing the Inbox.\n\nMessage\n------------------------\n" + ex.Message + "\n\nStack Trace\n------------------------\n" + ex.StackTrace );
            }
        }

        /// <summary>
        /// Updates the given person's person attribute with the given date value.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="aDate"></param>
        /// <exception cref="System.ArgumentNullException">If the given date can't be parsed.</exception>
        /// <exception cref="System.FormatException">If the given date can't be parsed.</exception>
        private void UpdateDateAttributeOnPersonRecord( Person person, string aDate, JobDataMap dataMap )
        {
            var rockContext = new RockContext();
            int attributeId = dataMap.Get( "AttributeId" ).ToString().AsInteger();
            AttributeValue av = new AttributeValueService( rockContext ).GetByAttributeIdAndEntityId( attributeId, person.Id );
            if ( av == null )
            {
                av = new AttributeValue { AttributeId = attributeId, EntityId = person.Id };
            }
            av.Value = aDate;
            rockContext.AttributeValues.Add( av );
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
            return null;
        }

        /// <summary>
        /// Moves the message to the given folder.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="otherFolder"></param>
        public void MoveMessageToFolder( EmailMessage message, Folder otherFolder )
        {
            // Move the specified mail to the JunkEmail folder and store the returned item.
            Item item = message.Move( otherFolder.Id );
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