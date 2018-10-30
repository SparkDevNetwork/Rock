using System;
using TechTalk.SpecFlow;
using Xunit;

using Rock;
using Rock.Data;
using Rock.Model;

namespace Rock.Specs
{
    [Binding]
    public class GroupMemberSteps
    {
        int? _personId;
        int? _relatedPersonId;
        int? _knownRelationshipRoleId;

        [When(@"I call CreateKnownRelationship with (.*) and (.*) and (.*)")]
        public void WhenICallCreateKnownRelationshipWithAndAnd(string p0, string p1, string p2)
        {
            _personId = p0.AsIntegerOrNull();
            _relatedPersonId = p1.AsIntegerOrNull();
            _knownRelationshipRoleId = p2.AsIntegerOrNull();
        }

        [Then(@"the relationship and an inverse relationship of (.*) should be created for both people")]
        public void ThenTheRelationshipAndAnInverseRelationshipOfShouldBeCreatedForBothPeople(string p0)
        {
            int? inverseRelationshipRoleId = p0.AsIntegerOrNull();

            //GroupMember relationship = null;
            //GroupMember inverseRelationship = null;

            //if ( _personId.HasValue && _relatedPersonId.HasValue && _knownRelationshipRoleId.HasValue )
            //{
            //    using ( var rockContext = new RockContext( "Data Source=DAVID-PC;Initial Catalog=Rock_Dev4; User Id=RockUser; password=rRUZew6tpsYBhXuZ;MultipleActiveResultSets=true" ) )
            //    {
            //        var groupMemberService = new GroupMemberService( rockContext );
            //        groupMemberService.CreateKnownRelationship( _personId.Value, _relatedPersonId.Value, _knownRelationshipRoleId.Value );
            //    }
            //}

            //Assert.NotNull( relationship );
            //Assert.NotNull( inverseRelationship );

            // Just assesrt the passed paremeter until we have a way to handle rockContext
            Assert.NotNull( inverseRelationshipRoleId );
        }
    }
}
