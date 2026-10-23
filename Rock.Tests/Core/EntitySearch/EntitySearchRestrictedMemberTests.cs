using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Core.EntitySearch;
using Rock.Model;
using Rock.ViewModels.Core;

namespace Rock.Tests.Core.EntitySearch
{
    /// <summary>
    /// Tests that entity search expressions can not use the entity members
    /// that create sign-in tokens or other credentials.
    /// </summary>
    [TestClass]
    public class EntitySearchRestrictedMemberTests
    {
        #region Tests

        [TestMethod]
        [DataRow( null, "ImpersonationParameter", null )]
        [DataRow( null, "impersonationparameter", null )]
        [DataRow( null, "UrlEncodedKey", null )]
        [DataRow( null, "EncryptedKey", null )]
        [DataRow( null, "new { Id, EncryptedKey }", null )]
        [DataRow( null, "np( UrlEncodedKey )", null )]
        [DataRow( null, "it[\"ImpersonationParameter\"]", null )]
        [DataRow( null, "it[\"Imperson\" + \"ationParameter\"]", null )]
        [DataRow( null, "object( it ).ImpersonationParameter", null )]
        [DataRow( null, "new { object( it ).UrlEncodedKey as Key }", null )]
        [DataRow( null, "Aliases.Select( AliasEncryptedKey )", null )]
        [DataRow( null, "Users.Select( ConfirmationCode )", null )]
        [DataRow( "1", "it.Select( ImpersonationParameter )", null )]
        [DataRow( null, null, "ImpersonationParameter != null" )]
        public void RestrictedMember_IsRejectedWithoutReadingIt( string groupBy, string select, string where )
        {
            var userQuery = new EntitySearchQueryBag
            {
                GroupBy = groupBy,
                Select = select,
                Where = where
            };

            TokenProbePerson.Reads.Clear();

            AssertSearchIsRejected( userQuery );
            Assert.AreEqual( 0, TokenProbePerson.Reads.Count );
        }

        [TestMethod]
        public void RestrictedMember_InCountOnlySearch_IsRejectedWithoutReadingIt()
        {
            var userQuery = new EntitySearchQueryBag
            {
                Where = "ImpersonationParameter != null",
                IsCountOnly = true
            };

            TokenProbePerson.Reads.Clear();

            AssertSearchIsRejected( userQuery );
            Assert.AreEqual( 0, TokenProbePerson.Reads.Count );
        }

        [TestMethod]
        public void OtherMembers_AreAllowed()
        {
            var userQuery = new EntitySearchQueryBag
            {
                Where = "Id == 2",
                Select = "new { Id, NickName }"
            };

            var results = Search( userQuery );
            var items = results.Items.Cast<dynamic>().ToList();

            Assert.AreEqual( 1, items.Count );
            Assert.AreEqual( "Cindy", ( string ) items[0].NickName );
        }

        [TestMethod]
        public void GroupByWithoutSelect_IsAllowed()
        {
            var userQuery = new EntitySearchQueryBag
            {
                GroupBy = "LastName"
            };

            var results = Search( userQuery );

            Assert.AreEqual( 1, results.Items.Count );
        }

        [TestMethod]
        public void EncryptedKeysOnOtherEntities_AreAllowed()
        {
            var groups = new List<Group>
            {
                new Group { Id = 1, Guid = new Guid( "9D3F7A4B-1C2E-4F5A-8B6C-0D1E2F3A4B5C" ), Name = "Group 1" }
            };

            var userQuery = new EntitySearchQueryBag
            {
                Select = "new { Id, UrlEncodedKey, EncryptedKey }"
            };

            var results = EntitySearchHelper.GetSearchResults( groups.AsQueryable(), new EntitySearchSystemQuery { IsRefinementAllowed = true }, userQuery, null );

            Assert.AreEqual( 1, results.Items.Count );
        }

        [TestMethod]
        public void DatabaseColumnWithRestrictedName_IsAllowed()
        {
            var entities = new List<ColumnTestEntity>
            {
                new ColumnTestEntity { Id = 1, ConfirmationCode = "ABC" }
            };

            var userQuery = new EntitySearchQueryBag
            {
                Select = "ConfirmationCode"
            };

            var results = EntitySearchHelper.GetSearchResults( entities.AsQueryable(), new EntitySearchSystemQuery { IsRefinementAllowed = true }, userQuery, null );

            Assert.AreEqual( "ABC", ( string ) results.Items[0] );
        }

        #endregion

        #region Support Methods

        private static void AssertSearchIsRejected( EntitySearchQueryBag userQuery )
        {
            string message = null;

            try
            {
                Search( userQuery );
            }
            catch ( Exception ex )
            {
                message = ex.Message;
            }

            Assert.IsNotNull( message, "The search was not rejected." );
            Assert.IsTrue( message.Contains( "can not be used in an entity search" ), message );
        }

        private static EntitySearchResultsBag Search( EntitySearchQueryBag userQuery )
        {
            var people = new List<TokenProbePerson>
            {
                new TokenProbePerson { Id = 1, NickName = "Ted", LastName = "Decker" },
                new TokenProbePerson { Id = 2, NickName = "Cindy", LastName = "Decker" }
            };

            var systemQuery = new EntitySearchSystemQuery
            {
                CurrentPerson = people[0],
                IsRefinementAllowed = true
            };

            var results = EntitySearchHelper.GetSearchResults( people.AsQueryable(), systemQuery, userQuery, null );

            // Serialize the results so anything evaluated late is also read.
            Newtonsoft.Json.JsonConvert.SerializeObject( results.Items );

            return results;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// An entity with a database column that has the same name as a
        /// restricted property.
        /// </summary>
        public class ColumnTestEntity : Rock.Data.Entity<ColumnTestEntity>
        {
            public string ConfirmationCode { get; set; }
        }

        /// <summary>
        /// A person that records when a restricted property is read instead
        /// of creating a token.
        /// </summary>
        public class TokenProbePerson : Person
        {
            public static readonly List<string> Reads = new List<string>();

            public override string ImpersonationParameter => Read( nameof( ImpersonationParameter ) );

            public override string UrlEncodedKey => Read( nameof( UrlEncodedKey ) );

            public override string EncryptedKey => Read( nameof( EncryptedKey ) );

            private static string Read( string name )
            {
                Reads.Add( name );
                return name;
            }
        }

        #endregion
    }
}
