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
using System.Data.Entity;
using System.Data.SqlClient;
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

using System.Text.RegularExpressions;

namespace com.centralaz.GeneralJobs.Jobs
{
    /// <summary>
    /// Auto subscribes people (who are in the given DataView) to topics based on our rules:
    ///  * Cares - no auto-subscribe
    ///  * Children - if adult has children (age 0-13)
    ///  * Students - if adult has students (grade 7th-12th)
    ///  * Young Adults - if age 18-29
    ///  * Life Groups - everyone
    ///  * Serving - everyone
    ///  * Global - everyone
    ///  * Prayer - everyone
    ///  * Financial - everyone
    ///  * Men - if male over 18
    ///  * Women - if female over 18
    /// </summary>
    [DataViewField( "DataView", "The dataview that contains the people to auto-topic subscribe. This Data View should really exclude people who have already had their topics set or have a 'Disable Auto Topic Subscription' boolean attribute set to true.", true, "", "Rock.Model.Person" )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Disable Auto-Topic Subscription Attribute", "The person attribute that indicates auto-topic subscription should be disabled after successfully performing auto-topic subscription.", required: true, key: "DisableAutoTopicSubscriptionAttribute", order: 1 )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Subscription Preference Attribute", "The person attribute that holds a person's subscribed topics.", required: true, defaultValue: "1E372FF6-93D9-4D42-9107-4FAD1E452218", order: 0 )]
    [IntegerField( "Database Timeout", "The number of seconds to wait before reporting a database timeout.", false, 180 )]

    [DefinedValueField( CustomGuids.SUBSCRIPTION_TOPICS, "Children Topic", "The topic representing Children news and information.", true, false, "11AE292E-C9D2-4871-BC60-A0FDC6D84213", "Topic Matching", 1 )]
    [DefinedValueField( CustomGuids.SUBSCRIPTION_TOPICS, "Students Topic", "The topic representing Students news and information.", true, false, "D9BF69B1-C910-4C11-8CCC-0D311908E410", "Topic Matching", 2 )]
    [DefinedValueField( CustomGuids.SUBSCRIPTION_TOPICS, "Young Adults Topic", "The topic representing Young Adults news and information.", true, false, "5887F362-8520-4FF9-8EC5-8E10CB394375", "Topic Matching", 3 )]
    [DefinedValueField( CustomGuids.SUBSCRIPTION_TOPICS, "Life Groups Topic", "The topic representing Life Groups news and information.", true, false, "67BC8D13-2231-4DF5-A79E-F40D58788177", "Topic Matching", 4 )]
    [DefinedValueField( CustomGuids.SUBSCRIPTION_TOPICS, "Serving Topic", "The topic representing Serving news and information.", true, false, "978C2CC0-EE42-406E-AADB-D91ACFD4F740", "Topic Matching", 5 )]
    [DefinedValueField( CustomGuids.SUBSCRIPTION_TOPICS, "Global Topic", "The topic representing Global news and information.", true, false, "4E1496F0-FF4E-4547-9238-0C84F25BF4B2", "Topic Matching", 6 )]
    [DefinedValueField( CustomGuids.SUBSCRIPTION_TOPICS, "Prayer Topic", "The topic representing Prayer news and information.", true, false, "EFB02F5F-8BFD-4B58-8D0F-56EB235DA2D9", "Topic Matching", 7 )]
    [DefinedValueField( CustomGuids.SUBSCRIPTION_TOPICS, "Financial Topic", "The topic representing Financial news and information.", true, false, "56CB084C-D9CC-4DAD-9DF3-DD2B1961A216", "Topic Matching", 8 )]
    [DefinedValueField( CustomGuids.SUBSCRIPTION_TOPICS, "Care Topic", "The topic representing Care news and information.", true, false, "547EAEB4-5E95-4576-8DA2-33EB02F6870F", "Topic Matching", 9 )]
    [DefinedValueField( CustomGuids.SUBSCRIPTION_TOPICS, "Men Topic", "The topic representing Mens news and information.", true, false, "B3C5CD31-381A-4D79-9848-44E40C033208", "Topic Matching", 10 )]
    [DefinedValueField( CustomGuids.SUBSCRIPTION_TOPICS, "Women Topic", "The topic representing Womens news and information.", true, false, "721EB8A4-4846-470E-8B10-0DBFBBB092EB", "Topic Matching", 11 )]

    [BooleanField( "Enable Logging", "Enable logging", false, "", 12 )]

    [DisallowConcurrentExecution]
    public class AutoTopicSubscribe : IJob
    {
        private bool _loggingActive = false;
        private string _pathName;
        private AttributeCache _personAttribute = null;
        private AttributeCache _personDisableAutoTopicSubscriptionAttribute = null;
        private string _personAttributeKey;
        private Guid _adultRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();

        private Guid? _childTopicGuid;
        private Guid? _studentsTopicGuid;
        private Guid? _youngAdultsTopicGuid;
        private Guid? _lifeGroupsTopicGuid;
        private Guid? _servingTopicGuid;
        private Guid? _globalTopicGuid;
        private Guid? _prayerTopicGuid;
        private Guid? _financeTopicGuid;
        private Guid? _careTopicGuid;
        private Guid? _menTopicGuid;
        private Guid? _womenTopicGuid;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoTopicSubscribe"/> class.
        /// </summary>
        public AutoTopicSubscribe()
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
            _pathName = String.Format( "{0}{1}-AutoTopicSubscribe.log", root, now );
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            _childTopicGuid = dataMap.Get( "ChildrenTopic" ).ToStringSafe().AsGuidOrNull();
            _studentsTopicGuid = dataMap.Get( "StudentsTopic" ).ToStringSafe().AsGuidOrNull();
            _youngAdultsTopicGuid = dataMap.Get( "YoungAdultsTopic" ).ToStringSafe().AsGuidOrNull();
            _lifeGroupsTopicGuid = dataMap.Get( "LifeGroupsTopic" ).ToStringSafe().AsGuidOrNull();
            _servingTopicGuid = dataMap.Get( "ServingTopic" ).ToStringSafe().AsGuidOrNull();
            _globalTopicGuid = dataMap.Get( "GlobalTopic" ).ToStringSafe().AsGuidOrNull();
            _prayerTopicGuid = dataMap.Get( "PrayerTopic" ).ToStringSafe().AsGuidOrNull();
            _financeTopicGuid = dataMap.Get( "FinancialTopic" ).ToStringSafe().AsGuidOrNull();
            _careTopicGuid = dataMap.Get( "CareTopic" ).ToStringSafe().AsGuidOrNull();
            _menTopicGuid = dataMap.Get( "MenTopic" ).ToStringSafe().AsGuidOrNull();
            _womenTopicGuid = dataMap.Get( "WomenTopic" ).ToStringSafe().AsGuidOrNull();

            var errorMessages = new List<string>();

            int recordsUpdated = 0;
            int errors = 0;

            var _personAttributeGuid = dataMap.Get( "SubscriptionPreferenceAttribute" ).ToString().AsGuidOrNull();

            if ( _personAttributeGuid != null )
            {
                _personAttribute = AttributeCache.Read( _personAttributeGuid.Value );
                _personAttributeKey = _personAttribute.Key;
            }
            else
            {
                context.Result = "Error. Job configuration has an invalid 'Subscription Preference Attribute' setting.";
                return;
            }

            var _personDisableAutoTopicSubscriptionGuid = dataMap.Get( "DisableAutoTopicSubscriptionAttribute" ).ToString().AsGuidOrNull();
            if ( _personDisableAutoTopicSubscriptionGuid != null )
            {
                _personDisableAutoTopicSubscriptionAttribute = AttributeCache.Read( _personDisableAutoTopicSubscriptionGuid.Value );
            }
            else
            { 
                context.Result = "Error. Job configuration has an invalid 'Disable Auto-Topic Subscription Attribute' setting.";
                return;
            }

            if ( ! ( _childTopicGuid.HasValue && _studentsTopicGuid.HasValue && _youngAdultsTopicGuid.HasValue &&
                _lifeGroupsTopicGuid.HasValue && _servingTopicGuid.HasValue && _globalTopicGuid.HasValue &&
                _prayerTopicGuid.HasValue && _financeTopicGuid.HasValue && _careTopicGuid.HasValue &&
                _menTopicGuid.HasValue && _womenTopicGuid.HasValue ) )
            {
                context.Result = "Error. Job configuration has an missing a configured matching topic setting.";
                return;
            }

            var dataViewGuid = dataMap.GetString( "DataView" ).AsGuidOrNull();
            if ( dataViewGuid == null )
            {
                context.Result = "Error. Job configuration has an invalid 'Data View' setting.";
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
                using ( var rockContext = new RockContext() )
                {
                    var dataView = new DataViewService( rockContext ).Get( ( Guid ) dataViewGuid );

                    List<IEntity> resultSet = null;
                    var dataTimeout = dataMap.GetString( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180;

                    var qry = dataView.GetQuery( null, rockContext, dataTimeout, out errorMessages );
                    if ( qry != null )
                    {
                        resultSet = qry.AsNoTracking().ToList();
                    }

                    if ( resultSet.Any() )
                    {
                        foreach ( Person person in resultSet )
                        {
                            // process each person
                            if ( SetPersonSubscriptions( rockContext, person ) )
                            {
                                recordsUpdated++;
                            }

                            // Only save after every 500 items
                            if ( recordsUpdated % 500 == 0 )
                            {
                                rockContext.SaveChanges();
                            }
                        }

                        // Save the remaining changes.
                        rockContext.SaveChanges();
                    }

                    qry = null;
                    resultSet = null;
                    dataView = null;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
                while ( ex != null )
                {
                    if ( ex is SqlException && ( ex as SqlException ).Number == -2 )
                    {
                        // if there was a SQL Server Timeout, have the warning be a friendly message about that.
                        errorMessages.Add( "This dataview did not complete in a timely manner. You can try again or adjust the timeout setting of this block." );
                        ex = ex.InnerException;
                    }
                    else
                    {
                        errorMessages.Add( ex.Message );
                        ex = ex.InnerException;
                    }
                }

                LogToFile( "An error occured while processing auto-topic subscriptions.\n\nMessage\n------------------------\n" + ex.Message + "\n\nStack Trace\n------------------------\n" + ex.StackTrace );
                errors++;
            }

            var resultMessage = string.Empty;
            if ( recordsUpdated == 0 )
            {
                resultMessage = "No items processed";
            }
            else
            {
                resultMessage = string.Format( "{0} people records were updated",  recordsUpdated );
            }

            if ( errors > 0 )
            {
                resultMessage += string.Format( "; but {0} errors were encountered", errors );
            }

            context.Result = resultMessage;
        }

        /// <summary>
        /// Sets the person subscriptions.  They are saved as a comma deliminted list of DefinedValue guids.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        private bool SetPersonSubscriptions( RockContext rockContext, Person person )
        {
            var personRole = person.GetFamilyRole( rockContext );

            person.LoadAttributes( rockContext );
            //person.LoadAttributes();
            var personAttributeValueGuid = person.GetAttributeValue( _personAttributeKey );

            List<Guid> guidList = personAttributeValueGuid.SplitDelimitedValues().AsGuidList();

            if ( person.Age >= 18 || ( personRole != null && personRole.Guid == _adultRoleGuid ) )
            {
                // Men
                if ( person.Gender == Gender.Male )
                {
                    AddIfNotExists( guidList, _menTopicGuid.Value );
                }
                else if ( person.Gender == Gender.Female )
                {
                    AddIfNotExists( guidList, _womenTopicGuid.Value );
                }

                // Young Adults
                if ( person.Age >= 18 && person.Age <= 29 )
                {
                    AddIfNotExists( guidList, _youngAdultsTopicGuid.Value );
                }

                // We must ToList() this in order to check Age properties below...
                var familyMembers = person.GetFamilyMembers( includeSelf: false, rockContext: rockContext ).ToList();

                // Parents of Children age 0-13
                if ( familyMembers.Where( fm => fm.Person.Age <= 13 ).Any() )
                {
                    AddIfNotExists( guidList, _childTopicGuid.Value );
                }

                // Parents of Children grade 7-12 (offset 6-0)
                if ( familyMembers.Where( fm => fm.Person.GradeOffset <= 6 && fm.Person.GradeOffset >= 0 ).Any() )
                {
                    AddIfNotExists( guidList, _studentsTopicGuid.Value );
                }

                AddIfNotExists( guidList, _globalTopicGuid.Value );
                AddIfNotExists( guidList, _lifeGroupsTopicGuid.Value );
                AddIfNotExists( guidList, _servingTopicGuid.Value );
                AddIfNotExists( guidList, _prayerTopicGuid.Value );
                AddIfNotExists( guidList, _financeTopicGuid.Value );

                // Set their attribute with the selected guids.
                person.SetAttributeValue( _personAttributeKey, guidList.AsDelimited( "," ) );

                // Set the disable auto-topic subscription setting to "true"
                person.SetAttributeValue( _personDisableAutoTopicSubscriptionAttribute.Key, "True" );

                // super slow:
                //Rock.Attribute.Helper.SaveAttributeValues( person, true, rockContext );
                // much faster:
                person.SaveAttributeValues();
            }
            else
            {
                return false;
            }

            return true;
        }

        private void AddIfNotExists( List<Guid> guidList, Guid guid )
        {
            if ( ! guidList.Contains( guid ) )
            {
                guidList.Add( guid );
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

    public class CustomGuids
    {
        /// <summary>
        /// Guid for the types of Subscriptions Topics a person can have.
        /// </summary>
        public const string SUBSCRIPTION_TOPICS = "71CFF7D6-1B5A-4EC3-971B-AB9ABB85E40C";
    }

}