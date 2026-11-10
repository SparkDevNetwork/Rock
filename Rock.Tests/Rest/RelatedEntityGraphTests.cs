// <copyright>
// Copyright by the Spark Development Network
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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Rest
{
    /// <summary>
    /// Tests the REST API check that finds related records nested inside a
    /// posted record. Adding the posted record would insert those related
    /// records too, without a permission check of their own.
    /// </summary>
    /// <remarks>
    /// The helpers are read by reflection because ApiControllerBase derives
    /// from a Web API type that this project does not reference directly.
    /// </remarks>
    [TestClass]
    public class RelatedEntityGraphTests
    {
        private static readonly Type _apiControllerBaseType = Assembly.Load( "Rock.Rest" ).GetType( "Rock.Rest.ApiControllerBase", true );

        #region Helpers

        private static string GetUntrackedRelatedEntityPropertyName( object entity, Func<string, bool> isCollectionAllowed = null )
        {
            var method = _apiControllerBaseType.GetMethod( "GetUntrackedRelatedEntityPropertyName", BindingFlags.NonPublic | BindingFlags.Static );

            return ( string ) method.Invoke( null, new object[] { entity, null, isCollectionAllowed } );
        }

        private static string GetUntrackedRelatedEntityPath( object entity, Func<string, bool, bool> isRelatedEntityAllowed )
        {
            var method = _apiControllerBaseType.GetMethods( BindingFlags.NonPublic | BindingFlags.Static )
                .Single( m => m.Name == "GetUntrackedRelatedEntityPath" && m.GetParameters().Length == 3 );

            return ( string ) method.Invoke( null, new object[] { entity, null, isRelatedEntityAllowed } );
        }

        /// <summary>
        /// Gets the controller's IsRelatedEntityAllowed as a predicate. The controller is not
        /// constructed (that would need a database) because the overrides read no state.
        /// </summary>
        private static Func<string, bool, bool> GetIsRelatedEntityAllowed( string controllerName )
        {
            var controllerType = Assembly.Load( "Rock.Rest" ).GetType( $"Rock.Rest.Controllers.{controllerName}", true );
            var method = controllerType.GetMethod( "IsRelatedEntityAllowed", BindingFlags.NonPublic | BindingFlags.Instance );
            var controller = System.Runtime.Serialization.FormatterServices.GetUninitializedObject( controllerType );

            return ( path, isCollection ) => ( bool ) method.Invoke( controller, new object[] { path, isCollection } );
        }

        private static bool IsMappedEntityType( Type type )
        {
            var method = _apiControllerBaseType.GetMethod( "IsMappedEntityType", BindingFlags.NonPublic | BindingFlags.Static );

            return ( bool ) method.Invoke( null, new object[] { type } );
        }

        private static bool IsNavigationProperty( PropertyInfo property )
        {
            var method = _apiControllerBaseType.GetMethod( "IsNavigationProperty", BindingFlags.NonPublic | BindingFlags.Static );

            return ( bool ) method.Invoke( null, new object[] { property } );
        }

        #endregion

        #region Test Types

        [Table( "TestRecord" )]
        public class TestRecord
        {
            public int Id { get; set; }

            public TestRecord Parent { get; set; }

            [NotMapped]
            public Note IgnoredNote { get; set; }
        }

        #endregion

        [TestMethod]
        public void FlatRecord_IsAllowed()
        {
            var note = new Note { Text = "x", NoteTypeId = 1, EntityId = 1 };

            Assert.IsNull( GetUntrackedRelatedEntityPropertyName( note ) );
        }

        [TestMethod]
        public void NullRecord_IsAllowed()
        {
            Assert.IsNull( GetUntrackedRelatedEntityPropertyName( null ) );
        }

        [TestMethod]
        public void NestedEntityCollection_IsRejected()
        {
            var note = new Note();
            note.ChildNotes.Add( new Note() );

            Assert.AreEqual( nameof( Note.ChildNotes ), GetUntrackedRelatedEntityPropertyName( note ) );
        }

        [TestMethod]
        public void NestedEntity_IsRejected()
        {
            var groupMember = new GroupMember { Person = new Person() };

            Assert.AreEqual( nameof( GroupMember.Person ), GetUntrackedRelatedEntityPropertyName( groupMember ) );
        }

        [TestMethod]
        public void NestedCollectionOfNonEntityMappedType_IsRejected()
        {
            var groupLocation = new GroupLocation
            {
                GroupLocationScheduleConfigs = new List<GroupLocationScheduleConfig>
                {
                    new GroupLocationScheduleConfig { Schedule = new Schedule() }
                }
            };

            Assert.AreEqual( nameof( GroupLocation.GroupLocationScheduleConfigs ), GetUntrackedRelatedEntityPropertyName( groupLocation ) );
        }

        [TestMethod]
        public void NestedNonEntityMappedType_IsRejected()
        {
            var transaction = new FinancialTransaction { SettledSourceDate = new AnalyticsSourceDate() };

            Assert.AreEqual( nameof( FinancialTransaction.SettledSourceDate ), GetUntrackedRelatedEntityPropertyName( transaction ) );
        }

        [TestMethod]
        public void NestedRecordInNonEntityMappedType_IsRejected()
        {
            var config = new GroupLocationScheduleConfig { Schedule = new Schedule() };

            Assert.AreEqual( nameof( GroupLocationScheduleConfig.Schedule ), GetUntrackedRelatedEntityPropertyName( config ) );
        }

        [TestMethod]
        public void AllowedCollection_IsAllowed()
        {
            var transaction = new FinancialTransaction();
            transaction.TransactionDetails.Add( new FinancialTransactionDetail { AccountId = 1, Amount = 10 } );

            var propertyName = GetUntrackedRelatedEntityPropertyName( transaction, p => p == nameof( FinancialTransaction.TransactionDetails ) );

            Assert.IsNull( propertyName );
        }

        [TestMethod]
        public void AllowedCollection_ChildWithNestedRecord_IsRejected()
        {
            var transaction = new FinancialTransaction();
            transaction.TransactionDetails.Add( new FinancialTransactionDetail { Account = new FinancialAccount() } );

            var propertyName = GetUntrackedRelatedEntityPropertyName( transaction, p => p == nameof( FinancialTransaction.TransactionDetails ) );

            Assert.AreEqual( "TransactionDetails.Account", propertyName );
        }

        [TestMethod]
        public void ScheduledTransactionsController_AllowsOnlyScheduledTransactionDetails()
        {
            var controllerType = Assembly.Load( "Rock.Rest" ).GetType( "Rock.Rest.Controllers.FinancialScheduledTransactionsController", true );
            var method = controllerType.GetMethod( "IsRelatedEntityCollectionAllowed", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly );

            Assert.IsNotNull( method, "FinancialScheduledTransactionsController must allow ScheduledTransactionDetails to be posted inline." );

            // The override reads no state, so the controller is not constructed (that would need a database).
            var controller = System.Runtime.Serialization.FormatterServices.GetUninitializedObject( controllerType );
            Func<string, bool> isAllowed = p => ( bool ) method.Invoke( controller, new object[] { p } );

            var schedule = new FinancialScheduledTransaction();
            schedule.ScheduledTransactionDetails.Add( new FinancialScheduledTransactionDetail { AccountId = 1, Amount = 25 } );
            Assert.IsNull( GetUntrackedRelatedEntityPropertyName( schedule, isAllowed ) );

            schedule.ScheduledTransactionDetails.First().Account = new FinancialAccount();
            Assert.AreEqual( "ScheduledTransactionDetails.Account", GetUntrackedRelatedEntityPropertyName( schedule, isAllowed ) );

            schedule = new FinancialScheduledTransaction();
            schedule.Transactions.Add( new FinancialTransaction() );
            Assert.AreEqual( nameof( FinancialScheduledTransaction.Transactions ), GetUntrackedRelatedEntityPropertyName( schedule, isAllowed ) );
        }

        [TestMethod]
        public void ScheduledTransactionDetail_ParentAuthority_IsItsScheduledTransaction()
        {
            var detail = new FinancialScheduledTransactionDetail { ScheduledTransactionId = 5 };

            var parent = detail.ParentAuthority as FinancialScheduledTransaction;
            Assert.IsNotNull( parent );
            Assert.AreEqual( 5, parent.Id );

            var schedule = new FinancialScheduledTransaction { Id = 7 };
            detail = new FinancialScheduledTransactionDetail { ScheduledTransactionId = 7, ScheduledTransaction = schedule };
            Assert.AreSame( schedule, detail.ParentAuthority );

            // A detail not yet linked to a schedule keeps the default parent authority.
            detail = new FinancialScheduledTransactionDetail();
            Assert.IsInstanceOfType( detail.ParentAuthority, typeof( Rock.Security.GlobalDefault ) );
        }

        [TestMethod]
        public void PersonWithAllowedPhoneNumbers_IsAllowed_ButLoginsAreRejected()
        {
            Func<string, bool> isAllowed = p => p == nameof( Person.PhoneNumbers );

            var person = new Person { PhoneNumbers = new List<PhoneNumber> { new PhoneNumber { Number = "5555555555" } } };
            Assert.IsNull( GetUntrackedRelatedEntityPropertyName( person, isAllowed ) );

            person.PhoneNumbers.First().NumberTypeValue = new DefinedValue();
            Assert.AreEqual( "PhoneNumbers.NumberTypeValue", GetUntrackedRelatedEntityPropertyName( person, isAllowed ) );

            person = new Person { Users = new List<UserLogin> { new UserLogin() } };
            Assert.AreEqual( nameof( Person.Users ), GetUntrackedRelatedEntityPropertyName( person, isAllowed ) );
        }

        [TestMethod]
        public void EmptyCollectionsAndNullItems_AreAllowed()
        {
            var note = new Note();
            note.ChildNotes.Add( null );

            Assert.IsNull( GetUntrackedRelatedEntityPropertyName( note ) );
            Assert.IsNull( GetUntrackedRelatedEntityPropertyName( new GroupLocation { GroupLocationScheduleConfigs = new List<GroupLocationScheduleConfig>() } ) );
        }

        [TestMethod]
        public void NotMappedNavigation_IsIgnored_MappedNavigationIsRejected()
        {
            var record = new TestRecord { IgnoredNote = new Note() };
            Assert.IsNull( GetUntrackedRelatedEntityPropertyName( record ) );

            record.Parent = new TestRecord();
            Assert.AreEqual( nameof( TestRecord.Parent ), GetUntrackedRelatedEntityPropertyName( record ) );
        }

        [TestMethod]
        public void IsNavigationProperty_DetectsEntityAndNonEntityNavigations()
        {
            Assert.IsTrue( IsNavigationProperty( typeof( GroupLocation ).GetProperty( nameof( GroupLocation.GroupLocationScheduleConfigs ) ) ) );
            Assert.IsTrue( IsNavigationProperty( typeof( FinancialTransaction ).GetProperty( nameof( FinancialTransaction.SettledSourceDate ) ) ) );
            Assert.IsTrue( IsNavigationProperty( typeof( Note ).GetProperty( nameof( Note.ParentNote ) ) ) );
            Assert.IsFalse( IsNavigationProperty( typeof( Note ).GetProperty( nameof( Note.ParentNoteId ) ) ) );
            Assert.IsFalse( IsNavigationProperty( typeof( Note ).GetProperty( nameof( Note.Text ) ) ) );
        }

        [TestMethod]
        public void PathCheck_AllowedSingleRecord_ChecksItsOwnRelatedRecords()
        {
            Func<string, bool, bool> isAllowed = ( path, isCollection ) => !isCollection && path == nameof( GroupMember.Person );

            var groupMember = new GroupMember { Person = new Person() };
            Assert.IsNull( GetUntrackedRelatedEntityPath( groupMember, isAllowed ) );

            groupMember.Person.Users = new List<UserLogin> { new UserLogin() };
            Assert.AreEqual( "Person.Users", GetUntrackedRelatedEntityPath( groupMember, isAllowed ) );

            // A path allowed as a single record is not allowed as a collection, and the reverse.
            Assert.AreEqual( nameof( Note.ChildNotes ), GetUntrackedRelatedEntityPath( new Note { ChildNotes = new List<Note> { new Note() } }, ( path, isCollection ) => !isCollection ) );
            Assert.AreEqual( nameof( GroupMember.Person ), GetUntrackedRelatedEntityPath( new GroupMember { Person = new Person() }, ( path, isCollection ) => isCollection ) );
        }

        [TestMethod]
        public void ExistingCollectionOverrides_BehaveTheSame_ThroughPathCheck()
        {
            var isTransactionAllowed = GetIsRelatedEntityAllowed( "FinancialTransactionsController" );
            var transaction = new FinancialTransaction();
            transaction.TransactionDetails.Add( new FinancialTransactionDetail { AccountId = 1, Amount = 10 } );
            Assert.IsNull( GetUntrackedRelatedEntityPath( transaction, isTransactionAllowed ) );
            transaction.TransactionDetails.First().Account = new FinancialAccount();
            Assert.AreEqual( "TransactionDetails.Account", GetUntrackedRelatedEntityPath( transaction, isTransactionAllowed ) );
            Assert.AreEqual( nameof( FinancialTransaction.RefundDetails ), GetUntrackedRelatedEntityPath( new FinancialTransaction { RefundDetails = new FinancialTransactionRefund() }, isTransactionAllowed ) );
            Assert.IsFalse( isTransactionAllowed( nameof( FinancialTransaction.TransactionDetails ), false ) );

            var isScheduleAllowed = GetIsRelatedEntityAllowed( "FinancialScheduledTransactionsController" );
            var schedule = new FinancialScheduledTransaction();
            schedule.ScheduledTransactionDetails.Add( new FinancialScheduledTransactionDetail { AccountId = 1, Amount = 25 } );
            Assert.IsNull( GetUntrackedRelatedEntityPath( schedule, isScheduleAllowed ) );
            schedule.ScheduledTransactionDetails.First().Account = new FinancialAccount();
            Assert.AreEqual( "ScheduledTransactionDetails.Account", GetUntrackedRelatedEntityPath( schedule, isScheduleAllowed ) );
            schedule = new FinancialScheduledTransaction();
            schedule.Transactions.Add( new FinancialTransaction() );
            Assert.AreEqual( nameof( FinancialScheduledTransaction.Transactions ), GetUntrackedRelatedEntityPath( schedule, isScheduleAllowed ) );

            var isPersonAllowed = GetIsRelatedEntityAllowed( "PeopleController" );
            var person = new Person { PhoneNumbers = new List<PhoneNumber> { new PhoneNumber { Number = "5555555555" } } };
            Assert.IsNull( GetUntrackedRelatedEntityPath( person, isPersonAllowed ) );
            person.PhoneNumbers.First().NumberTypeValue = new DefinedValue();
            Assert.AreEqual( "PhoneNumbers.NumberTypeValue", GetUntrackedRelatedEntityPath( person, isPersonAllowed ) );
            Assert.AreEqual( nameof( Person.Users ), GetUntrackedRelatedEntityPath( new Person { Users = new List<UserLogin> { new UserLogin() } }, isPersonAllowed ) );

            // A controller with no override allows nothing.
            var isAttributeAllowed = GetIsRelatedEntityAllowed( "AttributesController" );
            var attribute = new Rock.Model.Attribute();
            attribute.AttributeQualifiers.Add( new AttributeQualifier { Key = "values", Value = "A,B" } );
            Assert.AreEqual( nameof( Rock.Model.Attribute.AttributeQualifiers ), GetUntrackedRelatedEntityPath( attribute, isAttributeAllowed ) );
        }

        [TestMethod]
        public void RefundsController_AllowsOnlyTransactionAndItsDetails()
        {
            var isAllowed = GetIsRelatedEntityAllowed( "FinancialTransactionRefundsController" );

            var refund = new FinancialTransactionRefund { OriginalTransactionId = 1, FinancialTransaction = new FinancialTransaction { FinancialPaymentDetailId = 1 } };
            refund.FinancialTransaction.TransactionDetails.Add( new FinancialTransactionDetail { AccountId = 1, Amount = -25 } );
            Assert.IsNull( GetUntrackedRelatedEntityPath( refund, isAllowed ) );

            refund.FinancialTransaction.TransactionDetails.First().Account = new FinancialAccount();
            Assert.AreEqual( "FinancialTransaction.TransactionDetails.Account", GetUntrackedRelatedEntityPath( refund, isAllowed ) );

            refund.FinancialTransaction.TransactionDetails.First().Account = null;
            refund.FinancialTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            Assert.AreEqual( "FinancialTransaction.FinancialPaymentDetail", GetUntrackedRelatedEntityPath( refund, isAllowed ) );

            refund.FinancialTransaction.FinancialPaymentDetail = null;
            refund.FinancialTransaction.Images.Add( new FinancialTransactionImage() );
            Assert.AreEqual( "FinancialTransaction.Images", GetUntrackedRelatedEntityPath( refund, isAllowed ) );

            refund = new FinancialTransactionRefund { OriginalTransaction = new FinancialTransaction() };
            Assert.AreEqual( nameof( FinancialTransactionRefund.OriginalTransaction ), GetUntrackedRelatedEntityPath( refund, isAllowed ) );

            refund = new FinancialTransactionRefund { RefundReasonValue = new DefinedValue() };
            Assert.AreEqual( nameof( FinancialTransactionRefund.RefundReasonValue ), GetUntrackedRelatedEntityPath( refund, isAllowed ) );

            Assert.IsFalse( isAllowed( nameof( FinancialTransactionRefund.FinancialTransaction ), true ) );
            Assert.IsFalse( isAllowed( "FinancialTransaction.TransactionDetails", false ) );
            Assert.IsFalse( isAllowed( "TransactionDetails", true ) );
        }

        /// <summary>
        /// Every table in the Rock model must be seen as a mapped entity type,
        /// or a nested record of that type would be inserted unchecked. The
        /// RockContext model is built from the EntityTypeConfiguration classes
        /// in the Rock assembly.
        /// </summary>
        [TestMethod]
        public void EveryRockContextTable_IsMappedEntityType()
        {
            Type[] rockTypes;
            try
            {
                rockTypes = typeof( RockContext ).Assembly.GetTypes();
            }
            catch ( ReflectionTypeLoadException ex )
            {
                rockTypes = ex.Types.Where( t => t != null ).ToArray();
            }

            var tableTypes = rockTypes
                .Where( t => !t.IsAbstract && t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof( EntityTypeConfiguration<> ) )
                .Select( t => t.BaseType.GetGenericArguments()[0] )
                .Distinct()
                .ToList();

            Assert.IsTrue( tableTypes.Count > 100 );

            var missingTypes = tableTypes.Where( t => !IsMappedEntityType( t ) ).Select( t => t.FullName ).ToList();

            Assert.AreEqual( 0, missingTypes.Count, string.Join( ", ", missingTypes ) );
        }
    }
}
