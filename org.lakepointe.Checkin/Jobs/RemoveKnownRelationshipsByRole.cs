using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace org.lakepointe.Checkin.Jobs
{
    [GroupRoleField( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS, "Known Relationship To Remove", "Group Role of the known relationship types that should be removed from people's known relationship groups.", true, "", Order = 0)]
    [BooleanField( "Remove Inverse Relationship", "Remove the inverse known relationship type from people's known relationship groups", true, Order = 1 )]
    [IntegerField( "Relationship Age", "The minimum age (in days) that the relationship should be before it is removed. If 0 all relationships of this type are removed. Default is 0", false, 0, Order = 2 )]
    [DisallowConcurrentExecution]
    public class RemoveKnownRelationshipsByRole : IJob
    {
        public RemoveKnownRelationshipsByRole() { }

        
        public void Execute( IJobExecutionContext context )
        {
            var rockContext = new RockContext();
            var dataMap = context.JobDetail.JobDataMap;


            bool removeInverseRelationship = dataMap.GetBoolean( "RemoveInverseRelationship" );
            int relationshipAge = dataMap.GetInt( "RelationshipAge" );
            Guid? relationshipGuid = dataMap.GetString( "KnownRelationshipToRemove" ).AsGuidOrNull();

            GroupTypeRole relationship = null;

            if ( relationshipGuid.HasValue )
            {
                relationship = new GroupTypeRoleService( rockContext ).Get( relationshipGuid.Value );
            }

            if ( relationship == null )
            {
                context.Result = "Job Cancelled. Group Role not set.";
                return;
            }

            var groupMemberService = new GroupMemberService( rockContext );
            var qry = groupMemberService.Queryable()
                    .Where( gm => gm.GroupRoleId == relationship.Id );

            if ( relationshipAge > 0 )
            {
                
                relationshipAge--;

                DateTime maxDate = DateTime.Today.AddDays( -relationshipAge );
                qry = qry.Where( gm => gm.DateTimeAdded < maxDate );
            }

            var groupMembers = qry.ToList();

            int counter = 0;
            int batchSize = 5;
            foreach ( var gm in groupMembers )
            {
                GroupMember inverseGM = null;
                if ( removeInverseRelationship )
                {
                    inverseGM = groupMemberService.GetInverseRelationship( gm, false );
                }
                groupMemberService.Delete( gm );

                if ( inverseGM != null)
                {
                    groupMemberService.Delete( inverseGM );
                }

                counter++;

                if ( counter % batchSize == 0 || counter == groupMembers.Count )
                {
                    rockContext.SaveChanges();
                    counter = 0;
                }
            }

            context.Result = string.Format( "{0} relationships removed.", groupMembers.Count );
        }
    }
}
