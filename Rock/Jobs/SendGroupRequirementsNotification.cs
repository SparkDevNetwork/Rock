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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Sends a birthday email
    /// </summary>
    [SystemEmailField( "Notification Email Template", required: true, order: 0 )]
    [GroupTypesField("Group Types", "Group types use to check the group requirements on.", order: 1)]
    [CustomDropdownListField("Notify Parent Leaders", "Whether leaders in parent groups should also be notified. When 'Direct Parent' is selected only the leaders of the parent group will be notified. 'All Parents' will notify leaders all the way up the heirarchy.", "0^None,1^Direct Parent,2^All Parents", order: 2)]
    [TextField("Excluded Group Type Role Id's", "Optional comma delimited list of group type role Id's that should be excluded from the notification.", false, order: 3)]
    [DisallowConcurrentExecution]
    public class SendGroupRequirementsNotification : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendGroupRequirementsNotification()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var rockContext = new RockContext();
            
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            Guid? systemEmailGuid = dataMap.GetString( "NotificationEmailTemplate" ).AsGuidOrNull();

            var selectedGroupTypes = dataMap.GetString("GroupTypes").Split(',').Select(int.Parse).ToList();

            // get groups matching of the types provided
            GroupService groupService = new GroupService(rockContext);
            var groups = groupService.Queryable("GroupType, Members").AsNoTracking()
                            .Where(g => selectedGroupTypes.Contains(g.GroupTypeId) && g.IsActive == true);

            foreach ( var group in groups )
            {
                // check for members that don't meet requirements

                GroupsMissingRequirements groupMissingRequirements = new GroupsMissingRequirements();
                groupMissingRequirements.Id = group.Id;
                groupMissingRequirements.Name = group.Name;
                if ( group.GroupType != null )
                {
                    groupMissingRequirements.GroupTypeId = group.GroupTypeId;
                    groupMissingRequirements.GroupTypeName = group.GroupType.Name;
                }
                groupMissingRequirements.AncestorPathName = groupService.GroupAncestorPathName( group.Id );

            }
            
            /*


            var recipients = new List<RecipientData>();

            var personList = personQry.AsNoTracking().ToList();
            foreach ( var person in personList )
            {
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Person", person );

                var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( null );
                globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

                recipients.Add( new RecipientData( person.Email, mergeFields ) );
            }

            var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
            Email.Send( systemEmailGuid, recipients, appRoot );*/
        }
    }

    public class NotifyMatrix
    {
        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int GroupId { get; set; }
    }

    public class GroupsMissingRequirements
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<GroupMembersMissingRequirements> GroupMembersMissingRequirements { get; set; }
        public string AncestorPathName { get; set; }
        public int GroupTypeId { get; set; }
        public string GroupTypeName { get; set; }
    }

    public class GroupMembersMissingRequirements
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string FullName { get; set; }
        public List<MissingRequirement> MissingRequirements { get; set; }
    }

    public class MissingRequirement
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}
