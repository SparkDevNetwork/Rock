using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock;

namespace Rock.Tests.Integration.Follow
{
    [TestClass]
    public class InFollowedGroupTests
    {
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
        }

        [TestMethod]
        public void GetSuggestionsTest1()
        {
            var followerPersonIds = new List<int>() { 2 };
            var rockContext = new Rock.Data.RockContext();
            //Family Members
            var familyMemberSuggestionType = new Rock.Model.FollowingSuggestionTypeService( rockContext ).Get( new Guid( "8641F468-272B-4617-91ED-AB312D0F273C" ) );
            var suggestionComponent = familyMemberSuggestionType.GetSuggestionComponent();

            var suggestions = suggestionComponent.GetSuggestions( familyMemberSuggestionType, followerPersonIds );

            Assert.IsNotNull( suggestions );
        }

        [TestMethod]
        public void RunSendFollowingSuggestionsJob()
        {
            var job = new Rock.Model.ServiceJobService( new Rock.Data.RockContext() ).Get( 21 );
            var transaction = new Rock.Transactions.RunJobNowTransaction( job.Id );
            transaction.Execute();
        }
    }
}
