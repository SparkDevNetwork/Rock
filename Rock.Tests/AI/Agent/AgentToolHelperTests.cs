using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.AI.Agent;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Field.Types;
using Rock.Lava;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Tests.AI.Agent
{
    [TestClass]
    public class AgentToolHelperTests : MockDatabaseTestsBase
    {
        #region Constructor

        [TestMethod]
        public void Constructor_WithNullAgentRequestContext_ThrowsArgumentNullException()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var logger = new Mock<ILogger>().Object;

            Assert.ThrowsExactly<ArgumentNullException>( () =>
            {
                var helper = new AgentToolHelper( null, logger );
            } );

            Assert.ThrowsExactly<ArgumentNullException>( () =>
            {
                var helper = new AgentToolHelper( rockContext, null, logger );
            } );
        }

        [TestMethod]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

            Assert.ThrowsExactly<ArgumentNullException>( () =>
            {
                var helper = new AgentToolHelper( agentRequestContext, null );
            } );

            Assert.ThrowsExactly<ArgumentNullException>( () =>
            {
                var helper = new AgentToolHelper( rockContext, agentRequestContext, null );
            } );
        }

        [TestMethod]
        public void Constructor_WithNullRockContext_ThrowsArgumentNullException()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            Assert.ThrowsExactly<ArgumentNullException>( () =>
            {
                var helper = new AgentToolHelper( null, agentRequestContext, null );
            } );
        }

        #endregion

        #region GetErrorResult

        [TestMethod]
        public void GetErrorResult_WithoutErrors_ThrowsInvalidOperationException()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            Assert.ThrowsExactly<InvalidOperationException>( () =>
            {
                var result = helper.ErrorResult;
            } );
        }

        [TestMethod]
        public void GetErrorResult_WithErrors_IncludesAllErrors()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            helper.AddError( "First error." );
            helper.AddError( "Second error." );

            var result = helper.ErrorResult;

            Assert.Contains( "First error.", result.ErrorMessages );
            Assert.Contains( "Second error.", result.ErrorMessages );
        }

        [TestMethod]
        public void GetErrorResult_WithInstructions_IncludesAllInstructions()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            helper.AddError( "error." );
            helper.AddInstructions( "First instructions." );
            helper.AddInstructions( "Second instructions." );

            var result = helper.ErrorResult;

            Assert.Contains( "First instructions.", result.Instructions );
            Assert.Contains( "Second instructions.", result.Instructions );
        }

        [TestMethod]
        public void GetErrorResult_WithMetadata_IncludesAllMetadata()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            helper.AddError( "error." );
            helper.AddMetadata( "one", 1 );
            helper.AddMetadata( "two", 2 );

            var result = helper.ErrorResult;

            Assert.Contains( "one", result.Meta.Keys );
            Assert.AreEqual( 1, result.Meta["one"] );

            Assert.Contains( "two", result.Meta.Keys );
            Assert.AreEqual( 2, result.Meta["two"] );
        }

        #endregion

        #region GetPaginatedResult

        [TestMethod]
        public void GetPaginatedResult_WithEmptyItems_ReturnsNoDataResult()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetPaginatedResult( new PaginatedResult<string> { Items = new string[0] } );

            Assert.AreEqual( ToolStatus.NoData, result.Status );
        }

        [TestMethod]
        public void GetPaginatedResult_WithItems_ReturnsSuccessResult()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( rockContext, agentRequestContext, logger );

            var result = helper.GetPaginatedResult( new PaginatedResult<string> { Items = new string[] { "item 1" } } );

            Assert.AreEqual( ToolStatus.Success, result.Status );
        }

        [TestMethod]
        public void GetPaginatedResult_WithInstructions_IncludesAllInstructions()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( rockContext, agentRequestContext, logger );

            helper.AddInstructions( "First instructions." );
            helper.AddInstructions( "Second instructions." );

            var result = helper.GetPaginatedResult( new PaginatedResult<string> { Items = new string[] { "item 1" } } );

            Assert.Contains( "First instructions.", result.Instructions );
            Assert.Contains( "Second instructions.", result.Instructions );
        }

        [TestMethod]
        public void GetPaginatedResult_WithMetadata_IncludesAllMetadata()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( rockContext, agentRequestContext, logger );

            helper.AddMetadata( "one", 1 );
            helper.AddMetadata( "two", 2 );

            var result = helper.GetPaginatedResult( new PaginatedResult<string> { Items = new string[] { "item 1" } } );

            Assert.Contains( "one", result.Meta.Keys );
            Assert.AreEqual( 1, result.Meta["one"] );

            Assert.Contains( "two", result.Meta.Keys );
            Assert.AreEqual( 2, result.Meta["two"] );
        }

        [TestMethod]
        public void GetPaginatedResult_WithQueryableAndSanitize_CallsSanitizeMethod()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var itemMock = new Mock<EntityResultBase>();
            itemMock.Setup( m => m.Sanitize( It.IsAny<AgentRequestContext>() ) ).Returns( true );

            var originalItems = new[] { itemMock.Object };

            var result = helper.GetPaginatedResult( new PaginatedResult<EntityResultBase> { Items = originalItems }, sanitizeForSecurity: true );

            itemMock.Verify( m => m.Sanitize( It.IsAny<AgentRequestContext>() ), Times.Once );
        }

        [TestMethod]
        public void GetPaginatedResult_WithQueryableAndNoSanitize_DoesNotCallSanitizeMethod()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var itemMock = new Mock<EntityResultBase>();
            itemMock.Setup( m => m.Sanitize( It.IsAny<AgentRequestContext>() ) ).Returns( true );

            var originalItems = new[] { itemMock.Object };

            var result = helper.GetPaginatedResult( new PaginatedResult<EntityResultBase> { Items = originalItems }, sanitizeForSecurity: false );

            itemMock.Verify( m => m.Sanitize( It.IsAny<AgentRequestContext>() ), Times.Never );
        }

        #endregion

        #region AddError

        [TestMethod]
        public void AddError_WithValue_IncludesErrorInResult()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            helper.AddError( "text" );

            var result = helper.ErrorResult;

            Assert.Contains( "text", result.ErrorMessages );
        }

        #endregion

        #region AddInstructions

        [TestMethod]
        public void AddInstructions_WithValue_IncludesInstructionsInResult()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            helper.AddError( "junk" );
            helper.AddInstructions( "text" );

            var result = helper.ErrorResult;

            Assert.Contains( "text", result.Instructions );
        }

        #endregion

        #region AddMetadata

        [TestMethod]
        public void AddMetadata_WithValue_IncludesMetadataInResult()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            helper.AddError( "junk" );
            helper.AddMetadata( "key", "value" );

            var result = helper.ErrorResult;

            Assert.Contains( "key", result.Meta.Keys );
            Assert.AreEqual( "value", result.Meta["key"] );
        }

        #endregion

        #region GetPaginatedItems

        [TestMethod]
        public void GetPaginatedItems_WithQueryableAndPageNumber_ReturnsExpectedItems()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var originalItems = Enumerable.Range( 1, 15 )
                .AsQueryable();

            var result = helper.GetPaginatedItems( originalItems, 2, 5 );

            Assert.HasCount( 5, result.Items );
            Assert.AreEqual( 6, result.Items[0] );
            Assert.AreEqual( 7, result.Items[1] );
            Assert.AreEqual( 8, result.Items[2] );
            Assert.AreEqual( 9, result.Items[3] );
            Assert.AreEqual( 10, result.Items[4] );
        }

        [TestMethod]
        public void GetPaginatedItems_WithEnumerableAndPageNumber_ReturnsExpectedItems()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var originalItems = Enumerable.Range( 1, 15 );

            var result = helper.GetPaginatedItems( originalItems, 2, 5 );

            Assert.HasCount( 5, result.Items );
            Assert.AreEqual( 6, result.Items[0] );
            Assert.AreEqual( 7, result.Items[1] );
            Assert.AreEqual( 8, result.Items[2] );
            Assert.AreEqual( 9, result.Items[3] );
            Assert.AreEqual( 10, result.Items[4] );
        }

        #endregion

        #region GetCursorPaginatedItems

        [TestMethod]
        public void GetCursorPaginatedItems_WithCursor_ReturnsExpectedItems()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var originalItems = Enumerable.Range( 1, 15 )
                .Select( i => new Campus { Id = i } )
                .AsQueryable();

            var paginator = new CursorPaginator<Campus>( qry => qry.OrderBy( c => c.Id ) );
            var page = paginator.GetNextPage( originalItems, null, 5, false );

            var result = helper.GetCursorPaginatedItems( originalItems, paginator, cursor: page.NextCursor, pageSize: 5 );

            Assert.HasCount( 5, result.Items );
            Assert.AreEqual( 6, result.Items[0].Id );
            Assert.AreEqual( 7, result.Items[1].Id );
            Assert.AreEqual( 8, result.Items[2].Id );
            Assert.AreEqual( 9, result.Items[3].Id );
            Assert.AreEqual( 10, result.Items[4].Id );
        }

        #endregion

        #region GetOptionalEntity

        [TestMethod]
        public void GetOptionalEntity_WithoutParameterExpression_ThrowsArgumentNullException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            Assert.ThrowsExactly<ArgumentNullException>( () =>
            {
                helper.GetOptionalEntity<Campus>( string.Empty, parameterExpression: null );
            } );
        }

        [TestMethod]
        public void GetOptionalEntity_WithoutParameter_ReturnsNullWithoutError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetOptionalEntity<Campus>( string.Empty, parameterExpression: "test" );

            Assert.IsNull( result );
            Assert.IsFalse( helper.HasErrors );
        }

        [TestMethod]
        public void GetOptionalEntity_WithMissingEntity_ReturnsNullWithError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetOptionalEntity<Campus>( key, parameterExpression: "test" );

            Assert.IsNull( result );
            Assert.IsTrue( helper.HasErrors );
        }

        [TestMethod]
        public void GetOptionalEntity_WithoutAuthorization_ReturnsNullWithError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( Authorization.VIEW, null ) ).Returns( false );
            campusMock.Object.Id = 1;
            rockContextMock.Object.Set<Campus>().Add( campusMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetOptionalEntity<Campus>( key, parameterExpression: "test" );

            Assert.IsNull( result );
            Assert.IsTrue( helper.HasErrors );

            campusMock.Verify( m => m.IsAuthorized( Authorization.VIEW, null ), "IsAuthorized was not called." );
        }

        [TestMethod]
        public void GetOptionalEntity_WithAuthorization_ReturnsEntity()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( Authorization.VIEW, null ) ).Returns( true );
            campusMock.Object.Id = 1;
            rockContextMock.Object.Set<Campus>().Add( campusMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetOptionalEntity<Campus>( key, parameterExpression: "test" );

            Assert.IsNotNull( result );
            Assert.IsFalse( helper.HasErrors );

            campusMock.Verify( m => m.IsAuthorized( Authorization.VIEW, null ), "IsAuthorized was not called." );
        }

        [TestMethod]
        public void GetOptionalEntity_WithoutCheckSecurity_ReturnsEntity()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( Authorization.VIEW, null ) ).Returns( false );
            campusMock.Object.Id = 1;
            rockContextMock.Object.Set<Campus>().Add( campusMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetOptionalEntity<Campus>( key, checkSecurity: false, parameterExpression: "test" );

            Assert.IsNotNull( result );
            Assert.IsFalse( helper.HasErrors );

            campusMock.Verify( m => m.IsAuthorized( Authorization.VIEW, null ), Times.Never, "IsAuthorized was unexpectedly called." );
        }

        [TestMethod]
        public void GetOptionalEntity_WithPersonMissingPrimaryAliasId_ReturnsNullWithError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var personMock = new Mock<Person>();
            personMock.Object.Id = 1;
            personMock.Object.PrimaryAliasId = null;
            rockContextMock.Object.Set<Person>().Add( personMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetOptionalEntity<Person>( key, checkSecurity: false, parameterExpression: "test" );

            Assert.IsNull( result );
            Assert.IsTrue( helper.HasErrors );
        }

        [TestMethod]
        public void GetOptionalEntity_WithPersonPrimaryAliasId_ReturnsEntity()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var personMock = new Mock<Person>();
            personMock.Object.Id = 1;
            personMock.Object.PrimaryAliasId = 2;
            rockContextMock.Object.Set<Person>().Add( personMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetOptionalEntity<Person>( key, checkSecurity: false, parameterExpression: "test" );

            Assert.IsNotNull( result );
            Assert.IsFalse( helper.HasErrors );
        }

        #endregion

        #region TryGetOptionalEntity

        [TestMethod]
        public void TryGetOptionalEntity_WithoutParameterExpression_ThrowsArgumentNullException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            Assert.ThrowsExactly<ArgumentNullException>( () =>
            {
                helper.TryGetOptionalEntity<Campus>( string.Empty, out _, parameterExpression: null );
            } );
        }

        [TestMethod]
        public void TryGetOptionalEntity_WithoutParameter_ReturnsFalseWithoutError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.TryGetOptionalEntity<Campus>( string.Empty, out var entity, parameterExpression: "test" );

            Assert.IsFalse( result );
            Assert.IsNull( entity );
            Assert.IsFalse( helper.HasErrors );
        }

        [TestMethod]
        public void TryGetOptionalEntity_WithMissingEntity_ReturnsFalseWithError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.TryGetOptionalEntity<Campus>( key, out var entity, parameterExpression: "test" );

            Assert.IsFalse( result );
            Assert.IsNull( entity );
            Assert.IsTrue( helper.HasErrors );
        }

        [TestMethod]
        public void TryGetOptionalEntity_WithoutAuthorization_ReturnsFalseWithError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( Authorization.VIEW, null ) ).Returns( false );
            campusMock.Object.Id = 1;
            rockContextMock.Object.Set<Campus>().Add( campusMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.TryGetOptionalEntity<Campus>( key, out var entity, parameterExpression: "test" );

            Assert.IsFalse( result );
            Assert.IsNull( entity );
            Assert.IsTrue( helper.HasErrors );

            campusMock.Verify( m => m.IsAuthorized( Authorization.VIEW, null ), "IsAuthorized was not called." );
        }

        [TestMethod]
        public void TryGetOptionalEntity_WithAuthorization_ReturnsTrueWithEntity()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( Authorization.VIEW, null ) ).Returns( true );
            campusMock.Object.Id = 1;
            rockContextMock.Object.Set<Campus>().Add( campusMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.TryGetOptionalEntity<Campus>( key, out var entity, parameterExpression: "test" );

            Assert.IsTrue( result );
            Assert.IsNotNull( entity );
            Assert.IsFalse( helper.HasErrors );

            campusMock.Verify( m => m.IsAuthorized( Authorization.VIEW, null ), "IsAuthorized was not called." );
        }

        [TestMethod]
        public void TryGetOptionalEntity_WithoutCheckSecurity_ReturnsTrueWithEntity()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( Authorization.VIEW, null ) ).Returns( false );
            campusMock.Object.Id = 1;
            rockContextMock.Object.Set<Campus>().Add( campusMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.TryGetOptionalEntity<Campus>( key, out var entity, checkSecurity: false, parameterExpression: "test" );

            Assert.IsTrue( result );
            Assert.IsNotNull( entity );
            Assert.IsFalse( helper.HasErrors );

            campusMock.Verify( m => m.IsAuthorized( Authorization.VIEW, null ), Times.Never, "IsAuthorized was unexpectedly called." );
        }

        [TestMethod]
        public void TryGetOptionalEntity_WithPersonMissingPrimaryAliasId_ReturnsFalseWithError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var personMock = new Mock<Person>();
            personMock.Object.Id = 1;
            personMock.Object.PrimaryAliasId = null;
            rockContextMock.Object.Set<Person>().Add( personMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.TryGetOptionalEntity<Person>( key, out var entity, checkSecurity: false, parameterExpression: "test" );

            Assert.IsFalse( result );
            Assert.IsNull( entity );
            Assert.IsTrue( helper.HasErrors );
        }

        [TestMethod]
        public void TryGetOptionalEntity_WithPersonPrimaryAliasId_ReturnsTrueWithEntity()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var personMock = new Mock<Person>();
            personMock.Object.Id = 1;
            personMock.Object.PrimaryAliasId = 2;
            rockContextMock.Object.Set<Person>().Add( personMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.TryGetOptionalEntity<Person>( key, out var entity, checkSecurity: false, parameterExpression: "test" );

            Assert.IsTrue( result );
            Assert.IsNotNull( entity );
            Assert.IsFalse( helper.HasErrors );
        }

        #endregion

        #region GetRequiredEntity

        [TestMethod]
        public void GetRequiredEntity_WithoutParameterExpression_ThrowsArgumentNullException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            Assert.ThrowsExactly<ArgumentNullException>( () =>
            {
                helper.GetRequiredEntity<Campus>( string.Empty, parameterExpression: null );
            } );
        }

        [TestMethod]
        public void GetRequiredEntity_WithoutParameter_ReturnsNullWithError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetRequiredEntity<Campus>( string.Empty, parameterExpression: "test" );

            Assert.IsNull( result );
            Assert.IsTrue( helper.HasErrors );
        }

        [TestMethod]
        public void GetRequiredEntity_WithMissingEntity_ReturnsNullWithError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetRequiredEntity<Campus>( key, parameterExpression: "test" );

            Assert.IsNull( result );
            Assert.IsTrue( helper.HasErrors );
        }

        [TestMethod]
        public void GetRequiredEntity_WithoutAuthorization_ReturnsNullWithError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();

            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( Authorization.VIEW, null ) ).Returns( false );
            campusMock.Object.Id = 1;
            campusMock.Object.Guid = new Guid( "56f3aa9d-62d4-4925-af11-2d2cbe3fc647" );

            rockContextMock.Object.Set<Campus>().Add( campusMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetRequiredEntity<Campus>( key, parameterExpression: "test" );

            Assert.IsNull( result );
            Assert.IsTrue( helper.HasErrors );

            campusMock.Verify( m => m.IsAuthorized( Authorization.VIEW, null ), "IsAuthorized was not called." );
        }

        [TestMethod]
        public void GetRequiredEntity_WithAuthorization_ReturnsEntity()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();

            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( Authorization.VIEW, null ) ).Returns( true );
            campusMock.Object.Id = 1;
            campusMock.Object.Guid = new Guid( "56f3aa9d-62d4-4925-af11-2d2cbe3fc647" );

            rockContextMock.Object.Set<Campus>().Add( campusMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetRequiredEntity<Campus>( key, parameterExpression: "test" );

            Assert.IsNotNull( result );
            Assert.IsFalse( helper.HasErrors );

            campusMock.Verify( m => m.IsAuthorized( Authorization.VIEW, null ), "IsAuthorized was not called." );
        }

        [TestMethod]
        public void GetRequiredEntity_WithoutCheckSecurity_ReturnsEntity()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();

            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( Authorization.VIEW, null ) ).Returns( false );
            campusMock.Object.Id = 1;
            campusMock.Object.Guid = new Guid( "56f3aa9d-62d4-4925-af11-2d2cbe3fc647" );

            rockContextMock.Object.Set<Campus>().Add( campusMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetRequiredEntity<Campus>( key, checkSecurity: false, parameterExpression: "test" );

            Assert.IsNotNull( result );
            Assert.IsFalse( helper.HasErrors );

            campusMock.Verify( m => m.IsAuthorized( Authorization.VIEW, null ), Times.Never, "IsAuthorized was unexpectedly called." );
        }

        [TestMethod]
        public void GetRequiredEntity_WithPersonMissingPrimaryAliasId_ReturnsNullWithError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();

            var person = new Person
            {
                Id = 1,
                Guid = new Guid( "56f3aa9d-62d4-4925-af11-2d2cbe3fc647" ),
                PrimaryAliasId = null,
            };

            rockContextMock.Object.Set<Person>().Add( person );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetRequiredEntity<Person>( key, checkSecurity: false, parameterExpression: "test" );

            Assert.IsNull( result );
            Assert.IsTrue( helper.HasErrors );
        }

        [TestMethod]
        public void GetRequiredEntity_WithPersonPrimaryAliasId_ReturnsEntity()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();

            var person = new Person
            {
                Id = 1,
                Guid = new Guid( "56f3aa9d-62d4-4925-af11-2d2cbe3fc647" ),
                PrimaryAliasId = 2,
            };

            rockContextMock.Object.Set<Person>().Add( person );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetRequiredEntity<Person>( key, checkSecurity: false, parameterExpression: "test" );

            Assert.IsNotNull( result );
            Assert.IsFalse( helper.HasErrors );
        }

        #endregion

        #region TryGetRequiredEntity

        [TestMethod]
        public void TryGetRequiredEntity_WithoutParameterExpression_ThrowsArgumentNullException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            Assert.ThrowsExactly<ArgumentNullException>( () =>
            {
                helper.TryGetRequiredEntity<Campus>( string.Empty, out _, parameterExpression: null );
            } );
        }

        [TestMethod]
        public void TryGetRequiredEntity_WithoutParameter_ReturnsFalseWithError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.TryGetRequiredEntity<Campus>( string.Empty, out var entity, parameterExpression: "test" );

            Assert.IsFalse( result );
            Assert.IsNull( entity );
            Assert.IsTrue( helper.HasErrors );
        }

        [TestMethod]
        public void TryGetRequiredEntity_WithMissingEntity_ReturnsFalseWithError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.TryGetRequiredEntity<Campus>( key, out var entity, parameterExpression: "test" );

            Assert.IsFalse( result );
            Assert.IsNull( entity );
            Assert.IsTrue( helper.HasErrors );
        }

        [TestMethod]
        public void TryGetRequiredEntity_WithoutAuthorization_ReturnsFalseWithError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();

            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( Authorization.VIEW, null ) ).Returns( false );
            campusMock.Object.Id = 1;
            campusMock.Object.Guid = new Guid( "56f3aa9d-62d4-4925-af11-2d2cbe3fc647" );

            rockContextMock.Object.Set<Campus>().Add( campusMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.TryGetRequiredEntity<Campus>( key, out var entity, parameterExpression: "test" );

            Assert.IsFalse( result );
            Assert.IsNull( entity );
            Assert.IsTrue( helper.HasErrors );

            campusMock.Verify( m => m.IsAuthorized( Authorization.VIEW, null ), "IsAuthorized was not called." );
        }

        [TestMethod]
        public void TryGetRequiredEntity_WithAuthorization_ReturnsTrueWithEntity()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();

            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( Authorization.VIEW, null ) ).Returns( true );
            campusMock.Object.Id = 1;
            campusMock.Object.Guid = new Guid( "56f3aa9d-62d4-4925-af11-2d2cbe3fc647" );

            rockContextMock.Object.Set<Campus>().Add( campusMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.TryGetRequiredEntity<Campus>( key, out var entity, parameterExpression: "test" );

            Assert.IsTrue( result );
            Assert.IsNotNull( entity );
            Assert.IsFalse( helper.HasErrors );

            campusMock.Verify( m => m.IsAuthorized( Authorization.VIEW, null ), "IsAuthorized was not called." );
        }

        [TestMethod]
        public void TryGetRequiredEntity_WithoutCheckSecurity_ReturnsTrueWithEntity()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();

            var campusMock = new Mock<Campus>();
            campusMock.Setup( m => m.IsAuthorized( Authorization.VIEW, null ) ).Returns( false );
            campusMock.Object.Id = 1;
            campusMock.Object.Guid = new Guid( "56f3aa9d-62d4-4925-af11-2d2cbe3fc647" );

            rockContextMock.Object.Set<Campus>().Add( campusMock.Object );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.TryGetRequiredEntity<Campus>( key, out var entity, checkSecurity: false, parameterExpression: "test" );

            Assert.IsTrue( result );
            Assert.IsNotNull( entity );
            Assert.IsFalse( helper.HasErrors );

            campusMock.Verify( m => m.IsAuthorized( Authorization.VIEW, null ), Times.Never, "IsAuthorized was unexpectedly called." );
        }

        [TestMethod]
        public void TryGetRequiredEntity_WithPersonMissingPrimaryAliasId_ReturnsFalseWithError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();

            var person = new Person
            {
                Id = 1,
                Guid = new Guid( "56f3aa9d-62d4-4925-af11-2d2cbe3fc647" ),
                PrimaryAliasId = null,
            };

            rockContextMock.Object.Set<Person>().Add( person );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.TryGetRequiredEntity<Person>( key, out var entity, checkSecurity: false, parameterExpression: "test" );

            Assert.IsFalse( result );
            Assert.IsNull( entity );
            Assert.IsTrue( helper.HasErrors );
        }

        [TestMethod]
        public void TryGetRequiredEntity_WithPersonPrimaryAliasId_ReturnsTrueWithEntity()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();

            var person = new Person
            {
                Id = 1,
                Guid = new Guid( "56f3aa9d-62d4-4925-af11-2d2cbe3fc647" ),
                PrimaryAliasId = 2,
            };

            rockContextMock.Object.Set<Person>().Add( person );

            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;
            var key = IdHasher.Instance.GetHash( 1 );

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.TryGetRequiredEntity<Person>( key, out var entity, checkSecurity: false, parameterExpression: "test" );

            Assert.IsTrue( result );
            Assert.IsNotNull( entity );
            Assert.IsFalse( helper.HasErrors );
        }

        #endregion

        #region GetAvailableAttributes

        [TestMethod]
        public void GetAvailableAttributes_WithNullEntity_ReturnsEmpty()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            var result = helper.GetAvailableAttributes( null );

            Assert.IsEmpty( result );
        }

        [TestMethod]
        public void GetAvailableAttributes_WithoutPreLoadedAttributes_LoadsAttributes()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "TestAttribute",
                    IsPublic = true,
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                var result = helper.GetAvailableAttributes( campus, enforceSecurity: false );

                Assert.IsNotEmpty( result );
                Assert.AreEqual( "TestAttribute", result.First().Key );
            }
        }

        [TestMethod]
        public void GetAvailableAttributes_WithInternalAudience_IncludesNonPublicAttributes()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "PrivateAttribute",
                    IsPublic = false,
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext )
                {
                    AudienceType = AudienceType.Internal
                };

                var helper = new AgentToolHelper( agentRequestContext, logger );
                campus.LoadAttributes( rockContext );

                var result = helper.GetAvailableAttributes( campus, enforceSecurity: false );

                Assert.IsNotEmpty( result );
                Assert.AreEqual( "PrivateAttribute", result.First().Key );
            }
        }

        [TestMethod]
        public void GetAvailableAttributes_WithPublicAudience_DoesNotIncludeNonPublicAttributes()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "PrivateAttribute",
                    IsPublic = false,
                } );

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 2,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "PublicAttribute",
                    IsPublic = true,
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext )
                {
                    AudienceType = AudienceType.Public
                };

                var helper = new AgentToolHelper( agentRequestContext, logger );
                campus.LoadAttributes( rockContext );

                var result = helper.GetAvailableAttributes( campus, enforceSecurity: false );

                Assert.HasCount( 1, result );
                Assert.AreEqual( "PublicAttribute", result.First().Key );
            }
        }

        [TestMethod]
        public void GetAvailableAttributes_WithoutEnforceSecurity_IncludesSecuredAttributes()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "SecuredAttribute",
                } );

                rockContext.Set<Auth>().Add( new Auth
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>( true, rockContext ).Id,
                    EntityId = 1,
                    SpecialRole = SpecialRole.AllUsers,
                    Action = Authorization.VIEW,
                    AllowOrDeny = "D",
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                campus.LoadAttributes( rockContext );

                var result = helper.GetAvailableAttributes( campus, enforceSecurity: false );

                Assert.HasCount( 1, result );
                Assert.AreEqual( "SecuredAttribute", result.First().Key );
            }
        }

        [TestMethod]
        public void GetAvailableAttributes_WithEnforceSecurity_DoesNotIncludeSecuredAttributes()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "SecuredAttribute",
                } );

                rockContext.Set<Auth>().Add( new Auth
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>( true, rockContext ).Id,
                    EntityId = 1,
                    SpecialRole = SpecialRole.AllUsers,
                    Action = Authorization.VIEW,
                    AllowOrDeny = "D",
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                campus.LoadAttributes( rockContext );

                var result = helper.GetAvailableAttributes( campus, enforceSecurity: true );

                Assert.IsEmpty( result );
            }
        }

        [TestMethod]
        public void GetAvailableAttributes_WithoutEditPermission_MarksAttributeAsReadOnly()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "SecuredAttribute",
                } );

                rockContext.Set<Auth>().Add( new Auth
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>( true, rockContext ).Id,
                    EntityId = 1,
                    SpecialRole = SpecialRole.AllUsers,
                    Action = Authorization.VIEW,
                    AllowOrDeny = "A",
                } );

                rockContext.Set<Auth>().Add( new Auth
                {
                    Id = 2,
                    EntityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>( true, rockContext ).Id,
                    EntityId = 1,
                    SpecialRole = SpecialRole.AllUsers,
                    Action = Authorization.EDIT,
                    AllowOrDeny = "D",
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                campus.LoadAttributes( rockContext );

                var result = helper.GetAvailableAttributes( campus, enforceSecurity: true );

                Assert.HasCount( 1, result );
                Assert.IsTrue( result.First().IsReadOnly );
            }
        }

        [TestMethod]
        public void GetAvailableAttributes_WithEditPermission_DoesNotMarkAttributeAsReadOnly()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "SecuredAttribute",
                } );

                rockContext.Set<Auth>().Add( new Auth
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>( true, rockContext ).Id,
                    EntityId = 1,
                    SpecialRole = SpecialRole.AllUsers,
                    Action = Authorization.VIEW,
                    AllowOrDeny = "A",
                } );

                rockContext.Set<Auth>().Add( new Auth
                {
                    Id = 2,
                    EntityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>( true, rockContext ).Id,
                    EntityId = 1,
                    SpecialRole = SpecialRole.AllUsers,
                    Action = Authorization.EDIT,
                    AllowOrDeny = "A",
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                campus.LoadAttributes( rockContext );

                var result = helper.GetAvailableAttributes( campus, enforceSecurity: true );

                Assert.HasCount( 1, result );
                Assert.IsFalse( result.First().IsReadOnly );
            }
        }

        [TestMethod]
        public void GetAvailableAttributes_WithFieldType_IncludesValueFormat()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var fieldType = new FieldType
                {
                    Id = 1,
                    Class = typeof( RatingFieldType ).FullName,
                    Assembly = typeof( RatingFieldType ).Assembly.GetName().Name,
                };

                rockContext.Set<FieldType>().Add( fieldType );
                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    FieldType = fieldType,
                    FieldTypeId = fieldType.Id,
                    Key = "RatingAttribute",
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                campus.LoadAttributes( rockContext );

                var result = helper.GetAvailableAttributes( campus, enforceSecurity: false );

                Assert.HasCount( 1, result );
                Assert.IsNotNull( result.First().ValueFormat );
                Assert.IsNotEmpty( result.First().ValueFormat );
            }
        }

        #endregion

        #region SetAttributeValues

        [TestMethod]
        public void SetAttributeValues_WithNullEntity_DoesNotThrowException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContextMock.Object );
            var logger = new Mock<ILogger>().Object;

            var helper = new AgentToolHelper( agentRequestContext, logger );

            helper.SetAttributeValues( null, new List<AttributeValueResult>() );
        }

        [TestMethod]
        public void SetAttributeValues_WithNullAttributeValues_DoesNotThrowException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                campus.LoadAttributes( rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                helper.SetAttributeValues( campus, null );
            }
        }

        [TestMethod]
        public void SetAttributeValues_WithoutPreLoadedAttributes_LoadsAttributes()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "TestAttribute",
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                helper.SetAttributeValues( campus, new List<AttributeValueResult>() );

                Assert.IsNotNull( campus.Attributes );
                Assert.IsNotEmpty( campus.Attributes );
            }
        }

        [TestMethod]
        public void SetAttributeValues_WithUnknownAttribute_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                campus.LoadAttributes( rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var attributeValues = new List<AttributeValueResult>
                {
                    new AttributeValueResult
                    {
                        Key = "UnknownAttribute",
                        Value = "SomeValue",
                    },
                };

                helper.SetAttributeValues( campus, attributeValues, enforceSecurity: false );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "does not exist" ) ) );
            }
        }

        [TestMethod]
        public void SetAttributeValues_WithInternalAudience_SetsNonPublicAttribute()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "PrivateAttribute",
                    IsPublic = false,
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext )
                {
                    AudienceType = AudienceType.Internal,
                };

                campus.LoadAttributes( rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var attributeValues = new List<AttributeValueResult>
                {
                    new AttributeValueResult
                    {
                        Key = "PrivateAttribute",
                        Value = "SomeValue",
                    },
                };

                helper.SetAttributeValues( campus, attributeValues, enforceSecurity: false );

                Assert.IsFalse( helper.HasErrors );
                Assert.AreEqual( "SomeValue", campus.GetAttributeValue( "PrivateAttribute" ) );
            }
        }

        [TestMethod]
        public void SetAttributeValues_WithPublicAudienceAndNonPublicAttribute_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "PrivateAttribute",
                    IsPublic = false,
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext )
                {
                    AudienceType = AudienceType.Public,
                };

                campus.LoadAttributes( rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var attributeValues = new List<AttributeValueResult>
                {
                    new AttributeValueResult
                    {
                        Key = "PrivateAttribute",
                        Value = "SomeValue",
                    },
                };

                helper.SetAttributeValues( campus, attributeValues, enforceSecurity: false );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is not available" ) ) );
            }
        }

        [TestMethod]
        public void SetAttributeValues_WithPublicAudience_SetsPublicAttribute()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "PublicAttribute",
                    IsPublic = true,
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext )
                {
                    AudienceType = AudienceType.Public,
                };

                campus.LoadAttributes( rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var attributeValues = new List<AttributeValueResult>
                {
                    new AttributeValueResult
                    {
                        Key = "PublicAttribute",
                        Value = "SomeValue",
                    },
                };

                helper.SetAttributeValues( campus, attributeValues, enforceSecurity: false );

                Assert.IsFalse( helper.HasErrors );
                Assert.AreEqual( "SomeValue", campus.GetAttributeValue( "PublicAttribute" ) );
            }
        }

        [TestMethod]
        public void SetAttributeValues_WithEnforceSecurityAndNoEditAuthorization_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "SecuredAttribute",
                } );

                rockContext.Set<Auth>().Add( new Auth
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>( true, rockContext ).Id,
                    EntityId = 1,
                    SpecialRole = SpecialRole.AllUsers,
                    Action = Authorization.EDIT,
                    AllowOrDeny = "D",
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                campus.LoadAttributes( rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var attributeValues = new List<AttributeValueResult>
                {
                    new AttributeValueResult
                    {
                        Key = "SecuredAttribute",
                        Value = "SomeValue",
                    },
                };

                helper.SetAttributeValues( campus, attributeValues, enforceSecurity: true );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "do not have permission" ) ) );
            }
        }

        [TestMethod]
        public void SetAttributeValues_WithEnforceSecurityAndEditAuthorization_SetsAttributeValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "SecuredAttribute",
                } );

                rockContext.Set<Auth>().Add( new Auth
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>( true, rockContext ).Id,
                    EntityId = 1,
                    SpecialRole = SpecialRole.AllUsers,
                    Action = Authorization.EDIT,
                    AllowOrDeny = "A",
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                campus.LoadAttributes( rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var attributeValues = new List<AttributeValueResult>
                {
                    new AttributeValueResult
                    {
                        Key = "SecuredAttribute",
                        Value = "SomeValue",
                    },
                };

                helper.SetAttributeValues( campus, attributeValues, enforceSecurity: true );

                Assert.IsFalse( helper.HasErrors );
                Assert.AreEqual( "SomeValue", campus.GetAttributeValue( "SecuredAttribute" ) );
            }
        }

        [TestMethod]
        public void SetAttributeValues_WithNullAttributeValue_SetsValueToEmptyString()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "TestAttribute",
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                campus.LoadAttributes( rockContext );
                campus.SetAttributeValue( "TestAttribute", "some value" );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var attributeValues = new List<AttributeValueResult>
                {
                    new AttributeValueResult
                    {
                        Key = "TestAttribute",
                        Value = null,
                    },
                };

                helper.SetAttributeValues( campus, attributeValues, enforceSecurity: false );

                // Check the AttributeValues collection directly. The GetAttributeValue()
                // method will convert null to empty string.
                Assert.IsFalse( helper.HasErrors );
                Assert.AreEqual( string.Empty, campus.AttributeValues["TestAttribute"].Value );
            }
        }

        [TestMethod]
        public void SetAttributeValues_WithMissingRequiredValue_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "TestAttribute",
                    IsRequired = true,
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                campus.LoadAttributes( rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var attributeValues = new List<AttributeValueResult>();

                helper.SetAttributeValues( campus, attributeValues, enforceSecurity: false );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is required" ) ) );
            }
        }

        [TestMethod]
        public void SetAttributeValues_WithProvidedRequiredValue_ReportsNoErrors()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "TestAttribute",
                    IsRequired = true,
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                campus.LoadAttributes( rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var attributeValues = new List<AttributeValueResult>
                {
                    new AttributeValueResult
                    {
                        Key = "TestAttribute",
                        Value = "SomeValue",
                    },
                };

                helper.SetAttributeValues( campus, attributeValues, enforceSecurity: false );

                Assert.IsFalse( helper.HasErrors );
            }
        }

        [TestMethod]
        public void SetAttributeValues_WithEnforceSecurityAndNoEditAuthorization_DoesNotReportRequiredAttribute()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "SecuredAttribute",
                    IsRequired = true,
                } );

                rockContext.Set<Auth>().Add( new Auth
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>( true, rockContext ).Id,
                    EntityId = 1,
                    SpecialRole = SpecialRole.AllUsers,
                    Action = Authorization.EDIT,
                    AllowOrDeny = "D",
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                campus.LoadAttributes( rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var attributeValues = new List<AttributeValueResult>();

                helper.SetAttributeValues( campus, attributeValues, enforceSecurity: true );

                Assert.IsFalse( helper.HasErrors );
            }
        }

        [TestMethod]
        public void SetAttributeValues_WithEnforceSecurityAndEditAuthorization_ReportsRequiredAttribute()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "SecuredAttribute",
                    IsRequired = true,
                } );

                rockContext.Set<Auth>().Add( new Auth
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>( true, rockContext ).Id,
                    EntityId = 1,
                    SpecialRole = SpecialRole.AllUsers,
                    Action = Authorization.EDIT,
                    AllowOrDeny = "A",
                } );

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                campus.LoadAttributes( rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var attributeValues = new List<AttributeValueResult>();

                helper.SetAttributeValues( campus, attributeValues, enforceSecurity: true );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is required" ) ) );
            }
        }

        #endregion

        #region UpdateProperty

        [TestMethod]
        public void UpdateProperty_WithNullParameterExpression_ThrowsArgumentNullException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.UpdateProperty( campus, c => c.Name, ( SetOrClear<string> ) null, parameterExpression: null );
                } );

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.UpdateProperty( campus, c => c.Name, ( string ) null, parameterExpression: null );
                } );

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.UpdateProperty( campus, c => c.Id, null, parameterExpression: null );
                } );

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.UpdateProperty( campus, c => c.LeaderPersonAliasId, ( int? ) null, parameterExpression: null );
                } );

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.UpdateProperty( campus, c => c.LeaderPersonAliasId, ( SetOrClear<int> ) null, parameterExpression: null );
                } );

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.UpdateProperty( campus, c => c.LeaderPersonAliasId, ( SetOrClear<int?> ) null, parameterExpression: null );
                } );
            }
        }

        #region Nullable Int Property

        [TestMethod]
        public void UpdateProperty_WithNullableIntPropertyAndNullSetOrClearParameter_DoesNotChangeValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.LeaderPersonAliasId = 2;

                helper.UpdateProperty( campus, c => c.LeaderPersonAliasId, ( SetOrClear<int?> ) null, parameterExpression: "parameterExpression" );
                Assert.AreEqual( 2, campus.LeaderPersonAliasId );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithNullableIntPropertyAndNullIntParameter_DoesNotChangeValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.LeaderPersonAliasId = 2;

                helper.UpdateProperty( campus, c => c.LeaderPersonAliasId, ( int? ) null, parameterExpression: "parameterExpression" );
                Assert.AreEqual( 2, campus.LeaderPersonAliasId );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithNullableIntPropertyAndClearValue_ClearsValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.LeaderPersonAliasId = 2;
                var value = new SetOrClear<int?> { ClearValue = true };

                helper.UpdateProperty( campus, c => c.LeaderPersonAliasId, value, parameterExpression: "parameterExpression" );
                Assert.IsNull( campus.LeaderPersonAliasId );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithNullableIntPropertyAndSetValue_UpdatesValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.LeaderPersonAliasId = 2;
                var value = new SetOrClear<int?> { Value = 3 };

                helper.UpdateProperty( campus, c => c.LeaderPersonAliasId, value, parameterExpression: "parameterExpression" );
                Assert.AreEqual( 3, campus.LeaderPersonAliasId );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithNullableIntPropertyAndNullValue_ClearsValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.LeaderPersonAliasId = 2;
                var value = new SetOrClear<int?> { Value = null };

                helper.UpdateProperty( campus, c => c.LeaderPersonAliasId, value, parameterExpression: "parameterExpression" );
                Assert.IsNull( campus.LeaderPersonAliasId );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithNullableIntPropertyAndSetOrClearNullableIntExceptionOnSet_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var errorClass = new HelperErrorClass();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var value = new SetOrClear<int?> { Value = 0 };

                helper.UpdateProperty( errorClass, e => e.NullableIntProperty, value, parameterExpression: "parameterExpression" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is not valid" ) ) );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithNullableIntPropertyAndSetOrClearIntExceptionOnSet_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var errorClass = new HelperErrorClass();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var value = new SetOrClear<int> { Value = 0 };

                helper.UpdateProperty( errorClass, e => e.NullableIntProperty, value, parameterExpression: "parameterExpression" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is not valid" ) ) );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithNullableIntPropertyAndNullableIntExceptionOnSet_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var errorClass = new HelperErrorClass();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var value = ( int? ) 2;

                helper.UpdateProperty( errorClass, e => e.NullableIntProperty, value, parameterExpression: "parameterExpression" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is not valid" ) ) );
            }
        }

        #endregion

        #region Int Property

        [TestMethod]
        public void UpdateProperty_WithIntPropertyAndNullSetOrClearParameter_DoesNotChangeValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.Id = 1;

                helper.UpdateProperty( campus, c => c.Id, ( SetOrClear<int> ) null, parameterExpression: "parameterExpression" );
                Assert.AreEqual( 1, campus.Id );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithIntPropertyAndNullIntParameter_DoesNotChangeValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.Id = 1;

                helper.UpdateProperty( campus, c => c.Id, null, parameterExpression: "parameterExpression" );
                Assert.AreEqual( 1, campus.Id );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithIntPropertyAndClearValue_SetsDefaultValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.Id = 2;
                var value = new SetOrClear<int?> { ClearValue = true };

                helper.UpdateProperty( campus, c => c.Id, value, parameterExpression: "parameterExpression" );
                Assert.AreEqual( default, campus.Id );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithIntPropertyAndSetOrClearWithValue_UpdatesValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.LeaderPersonAliasId = 2;
                var value = new SetOrClear<int> { Value = 3 };

                helper.UpdateProperty( campus, c => c.LeaderPersonAliasId, value, parameterExpression: "parameterExpression" );
                Assert.AreEqual( 3, campus.LeaderPersonAliasId );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithIntPropertyAndSetOrClearNullValue_SetsDefaultValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.Id = 2;
                var value = new SetOrClear<int?> { Value = null };

                helper.UpdateProperty( campus, c => c.Id, value, parameterExpression: "parameterExpression" );
                Assert.AreEqual( default, campus.Id );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithIntPropertyAndSetNullableValue_UpdatesValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.Id = 2;

                helper.UpdateProperty( campus, c => c.Id, 3, parameterExpression: "parameterExpression" );
                Assert.AreEqual( 3, campus.Id );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithIntPropertyAndSetIntExceptionOnSet_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var errorClass = new HelperErrorClass();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                helper.UpdateProperty( errorClass, e => e.IntProperty, 3, parameterExpression: "parameterExpression" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is not valid" ) ) );
            }
        }

        #endregion

        #region String Property

        [TestMethod]
        public void UpdateProperty_WithStringPropertyAndNullSetOrClearParameter_DoesNotChangeValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.Name = "Test";

                helper.UpdateProperty( campus, c => c.Name, ( SetOrClear<string> ) null, parameterExpression: "parameterExpression" );
                Assert.AreEqual( "Test", campus.Name );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithStringPropertyAndNullStringParameter_DoesNotChangeValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.Name = "Test";

                helper.UpdateProperty( campus, c => c.Name, ( string ) null, parameterExpression: "parameterExpression" );
                Assert.AreEqual( "Test", campus.Name );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithStringPropertyAndClearValueParameter_ClearsValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.Name = "Test";
                var value = new SetOrClear<string> { ClearValue = true };

                helper.UpdateProperty( campus, c => c.Name, value, parameterExpression: "parameterExpression" );
                Assert.IsNull( campus.Name );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithStringPropertyAndSetOrClearValueParameter_SetsValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.Name = "Test";
                var value = new SetOrClear<string> { Value = "new value" };

                helper.UpdateProperty( campus, c => c.Name, value, parameterExpression: "parameterExpression" );

                Assert.AreEqual( "new value", campus.Name );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithStringPropertyAndStringParameter_SetsValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.Name = "Test";

                helper.UpdateProperty( campus, c => c.Name, "new value", parameterExpression: "parameterExpression" );

                Assert.AreEqual( "new value", campus.Name );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithStringPropertyAndSetStringExceptionOnSet_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var errorClass = new HelperErrorClass();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                helper.UpdateProperty( errorClass, e => e.StringProperty, "value", parameterExpression: "parameterExpression" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is not valid" ) ) );
            }
        }

        [TestMethod]
        public void UpdateProperty_WithStringPropertyAndSetOrClearValueExceptionOnSet_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var errorClass = new HelperErrorClass();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var value = new SetOrClear<string> { Value = "value" };

                helper.UpdateProperty( errorClass, e => e.StringProperty, value, parameterExpression: "parameterExpression" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is not valid" ) ) );
            }
        }

        #endregion

        #endregion

        #region UpdateNavigationProperty

        [TestMethod]
        public void UpdateNavigationProperty_WithNullParameterExpression_ThrowsArgumentNullException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.UpdateNavigationProperty( campus, c => c.LeaderPersonAlias, ( SetOrClear<string> ) null, parameterExpression: null );
                } );

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.UpdateNavigationProperty( campus, c => c.LeaderPersonAlias, ( string ) null, parameterExpression: null );
                } );
            }
        }

        [TestMethod]
        public void UpdateNavigationProperty_WithNullSetOrClearParameter_DoesNotChangeValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.LeaderPersonAlias = new PersonAlias { Id = 2 };
                campus.LeaderPersonAliasId = 2;

                helper.UpdateNavigationProperty( campus, c => c.LeaderPersonAlias, ( SetOrClear<string> ) null, parameterExpression: "parameterExpression" );

                Assert.AreEqual( 2, campus.LeaderPersonAliasId );
                Assert.AreEqual( 2, campus.LeaderPersonAlias.Id );
            }
        }

        [TestMethod]
        public void UpdateNavigationProperty_WithNullStringParameter_DoesNotChangeValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.LeaderPersonAlias = new PersonAlias { Id = 2 };
                campus.LeaderPersonAliasId = 2;

                helper.UpdateNavigationProperty( campus, c => c.LeaderPersonAlias, ( string ) null, parameterExpression: "parameterExpression" );

                Assert.AreEqual( 2, campus.LeaderPersonAliasId );
                Assert.AreEqual( 2, campus.LeaderPersonAlias.Id );
            }
        }

        [TestMethod]
        public void UpdateNavigationProperty_WithEmptyStringParameter_DoesNotChangeValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.LeaderPersonAlias = new PersonAlias { Id = 2 };
                campus.LeaderPersonAliasId = 2;

                helper.UpdateNavigationProperty( campus, c => c.LeaderPersonAlias, string.Empty, parameterExpression: "parameterExpression" );

                Assert.AreEqual( 2, campus.LeaderPersonAliasId );
                Assert.AreEqual( 2, campus.LeaderPersonAlias.Id );
            }
        }

        [TestMethod]
        public void UpdateNavigationProperty_WithMissingForeignKeyParameter_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var errorClass = new HelperErrorClass();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var value = new SetOrClear<string> { Value = "123" };

                var exception = Assert.Throws<Exception>( () =>
                {
                    helper.UpdateNavigationProperty( errorClass, e => e.MissingForeignKey, value, parameterExpression: "parameterExpression" );
                } );

                Assert.Contains( "MissingForeignKey is not valid.", exception.Message );
            }
        }

        [TestMethod]
        public void UpdateNavigationProperty_WithInvalidForeignKeyParameter_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var errorClass = new HelperErrorClass();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var value = new SetOrClear<string> { Value = "123" };

                var exception = Assert.Throws<Exception>( () =>
                {
                    helper.UpdateNavigationProperty( errorClass, e => e.InvalidForeignKey, value, parameterExpression: "parameterExpression" );
                } );

                Assert.Contains( "InvalidForeignKeyId is not valid.", exception.Message );
            }
        }

        [TestMethod]
        public void UpdateNavigationProperty_WithRequiredForeignKeyParameterAndClearValue_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var errorClass = new HelperErrorClass();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var value = new SetOrClear<string> { ClearValue = true };

                helper.UpdateNavigationProperty( errorClass, e => e.RequiredForeignKey, value, parameterExpression: "parameterExpression" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is required" ) ) );
            }
        }

        [TestMethod]
        public void UpdateNavigationProperty_WithOptionalForeignKeyAndClearValue_ClearsValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var value = new SetOrClear<string> { ClearValue = true };

                campus.LeaderPersonAlias = new PersonAlias { Id = 2 };
                campus.LeaderPersonAliasId = 2;

                helper.UpdateNavigationProperty( campus, c => c.LeaderPersonAlias, value, parameterExpression: "parameterExpression" );

                Assert.IsNull( campus.LeaderPersonAliasId );
                Assert.IsNull( campus.LeaderPersonAlias );
            }
        }

        [TestMethod]
        public void UpdateNavigationProperty_WithMissingTarget_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                helper.UpdateNavigationProperty( campus, c => c.TeamGroup, 123.AsIdKey(), parameterExpression: "parameterExpression" );

                Assert.IsTrue( helper.HasErrors );
            }
        }

        [TestMethod]
        public void UpdateNavigationProperty_WithValidTarget_SetsValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                rockContext.Set<Group>().Add( new Group
                {
                    Id = 123,
                } );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                helper.UpdateNavigationProperty( campus, c => c.TeamGroup, 123.AsIdKey(), parameterExpression: "parameterExpression" );

                Assert.IsFalse( helper.HasErrors );
                Assert.AreEqual( 123, campus.TeamGroupId );
                Assert.AreEqual( 123, campus.TeamGroup.Id );
            }
        }

        [TestMethod]
        public void UpdateNavigationProperty_WithMissingPersonAliasTarget_SetsValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                helper.UpdateNavigationProperty( campus, c => c.LeaderPersonAlias, 456.AsIdKey(), parameterExpression: "parameterExpression" );

                Assert.IsTrue( helper.HasErrors );
            }
        }

        [TestMethod]
        public void UpdateNavigationProperty_WithValidPersonAliasTarget_SetsValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );
                var personAlias = new PersonAlias
                {
                    Id = 123,
                    AliasPersonId = 456,
                };

                rockContext.Set<PersonAlias>().Add( personAlias );
                rockContext.Set<Person>().Add( new Person
                {
                    Id = 456,
                    Aliases = new List<PersonAlias> { personAlias },
                    PrimaryAliasId = personAlias.Id,
                } );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                helper.UpdateNavigationProperty( campus, c => c.LeaderPersonAlias, 456.AsIdKey(), parameterExpression: "parameterExpression" );

                Assert.IsFalse( helper.HasErrors );
                Assert.AreEqual( personAlias.Id, campus.LeaderPersonAliasId );
                Assert.AreEqual( personAlias.Id, campus.LeaderPersonAlias.Id );
            }
        }

        #endregion

        #region UpdateDefinedValueProperty

        [TestMethod]
        public void UpdateDefinedValueProperty_WithNullParameterExpression_ThrowsArgumentNullException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.UpdateDefinedValueProperty( campus, c => c.CampusTypeValue, ( SetOrClear<string> ) null, parameterExpression: null );
                } );

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.UpdateDefinedValueProperty( campus, c => c.CampusTypeValue, ( string ) null, parameterExpression: null );
                } );
            }
        }

        [TestMethod]
        public void UpdateDefinedValueProperty_WithNullSetOrClearParameter_DoesNotChangeValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.CampusTypeValue = new DefinedValue { Id = 2 };
                campus.CampusTypeValueId = 2;

                helper.UpdateDefinedValueProperty( campus, c => c.CampusTypeValue, ( SetOrClear<string> ) null, parameterExpression: "parameterExpression" );

                Assert.AreEqual( 2, campus.CampusTypeValueId );
                Assert.AreEqual( 2, campus.CampusTypeValue.Id );
            }
        }

        [TestMethod]
        public void UpdateDefinedValueProperty_WithNullStringParameter_DoesNotChangeValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.CampusTypeValue = new DefinedValue { Id = 2 };
                campus.CampusTypeValueId = 2;

                helper.UpdateDefinedValueProperty( campus, c => c.CampusTypeValue, ( string ) null, parameterExpression: "parameterExpression" );

                Assert.AreEqual( 2, campus.CampusTypeValueId );
                Assert.AreEqual( 2, campus.CampusTypeValue.Id );
            }
        }

        [TestMethod]
        public void UpdateDefinedValueProperty_WithEmptyStringParameter_DoesNotChangeValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                campus.CampusTypeValue = new DefinedValue { Id = 2 };
                campus.CampusTypeValueId = 2;

                helper.UpdateDefinedValueProperty( campus, c => c.CampusTypeValue, string.Empty, parameterExpression: "parameterExpression" );

                Assert.AreEqual( 2, campus.CampusTypeValueId );
                Assert.AreEqual( 2, campus.CampusTypeValue.Id );
            }
        }

        [TestMethod]
        public void UpdateDefinedValueProperty_WithMissingForeignKeyParameter_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var errorClass = new HelperErrorClass();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var value = new SetOrClear<string> { Value = "123" };

                var exception = Assert.Throws<Exception>( () =>
                {
                    helper.UpdateDefinedValueProperty( errorClass, e => e.MissingDefinedValueForeignKey, value, parameterExpression: "parameterExpression" );
                } );

                Assert.Contains( "MissingDefinedValueForeignKey is not valid.", exception.Message );
            }
        }

        [TestMethod]
        public void UpdateDefinedValueProperty_WithInvalidForeignKeyParameter_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var errorClass = new HelperErrorClass();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var value = new SetOrClear<string> { Value = "123" };

                var exception = Assert.Throws<Exception>( () =>
                {
                    helper.UpdateDefinedValueProperty( errorClass, e => e.InvalidDefinedValueForeignKey, value, parameterExpression: "parameterExpression" );
                } );

                Assert.Contains( "InvalidDefinedValueForeignKeyId is not valid.", exception.Message );
            }
        }

        [TestMethod]
        public void UpdateDefinedValueProperty_WithRequiredForeignKeyParameterAndClearValue_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var errorClass = new HelperErrorClass();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var value = new SetOrClear<string> { ClearValue = true };

                helper.UpdateDefinedValueProperty( errorClass, e => e.RequiredDefinedValueForeignKey, value, parameterExpression: "parameterExpression" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is required" ) ) );
            }
        }

        [TestMethod]
        public void UpdateDefinedValueProperty_WithOptionalForeignKeyAndClearValue_ClearsValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var value = new SetOrClear<string> { ClearValue = true };

                campus.CampusTypeValue = new DefinedValue { Id = 2 };
                campus.CampusTypeValueId = 2;

                helper.UpdateDefinedValueProperty( campus, c => c.CampusTypeValue, value, parameterExpression: "parameterExpression" );

                Assert.IsNull( campus.CampusTypeValueId );
                Assert.IsNull( campus.CampusTypeValue );
            }
        }

        [TestMethod]
        public void UpdateDefinedValueProperty_WithMissingTarget_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                helper.UpdateDefinedValueProperty( campus, c => c.CampusTypeValue, 123.AsIdKey(), parameterExpression: "parameterExpression" );

                Assert.IsTrue( helper.HasErrors );
            }
        }

        [TestMethod]
        public void UpdateDefinedValueProperty_WithValidTarget_SetsValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                rockContext.Set<DefinedType>().Add( new DefinedType
                {
                    Id = 456,
                    Guid = SystemGuid.DefinedType.CAMPUS_TYPE.AsGuid(),
                } );

                rockContext.Set<DefinedValue>().Add( new DefinedValue
                {
                    Id = 123,
                    DefinedTypeId = 456,
                } );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                helper.UpdateDefinedValueProperty( campus, c => c.CampusTypeValue, 123.AsIdKey(), parameterExpression: "parameterExpression" );

                Assert.IsFalse( helper.HasErrors );
                Assert.AreEqual( 123, campus.CampusTypeValueId );
                Assert.AreEqual( 123, campus.CampusTypeValue.Id );
            }
        }

        [TestMethod]
        public void UpdateDefinedValueProperty_WithValidTargetAndNoConstraintAttribute_SetsValue()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var errorClass = new HelperErrorClass();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                rockContext.Set<DefinedValue>().Add( new DefinedValue
                {
                    Id = 123,
                    DefinedTypeId = 456,
                } );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                helper.UpdateDefinedValueProperty( errorClass, e => e.RequiredDefinedValueForeignKey, 123.AsIdKey(), parameterExpression: "parameterExpression" );

                Assert.IsFalse( helper.HasErrors );
                Assert.AreEqual( 123, errorClass.RequiredDefinedValueForeignKeyId );
                Assert.AreEqual( 123, errorClass.RequiredDefinedValueForeignKey.Id );
            }
        }

        [TestMethod]
        public void UpdateDefinedValueProperty_WithMismatchedDefinedValue_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var campus = new Campus();
                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                rockContext.Set<DefinedType>().Add( new DefinedType
                {
                    Id = 456,
                    Guid = SystemGuid.DefinedType.CAMPUS_TYPE.AsGuid(),
                } );

                rockContext.Set<DefinedValue>().Add( new DefinedValue
                {
                    Id = 123,
                    DefinedTypeId = 321,
                } );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                helper.UpdateDefinedValueProperty( campus, c => c.CampusTypeValue, 123.AsIdKey(), parameterExpression: "parameterExpression" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is not valid" ) ) );
            }
        }

        #endregion

        #region WhereOptionalIdKey

        [TestMethod]
        public void WhereOptionalIdKey_WithIntPropertyAndNullParameterExpression_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.WhereOptionalIdKey( query, c => c.Id, string.Empty, parameterExpression: null );
                } );
            }
        }

        [TestMethod]
        public void WhereOptionalIdKey_WithIntPropertyAndEmptyParameter_ReturnsSameQueryable()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                var result = helper.WhereOptionalIdKey( query, c => c.Id, string.Empty, parameterExpression: "parameter" );

                Assert.AreSame( query, result );
            }
        }

        [TestMethod]
        public void WhereOptionalIdKey_WithIntPropertyAndNonIdKeyParameter_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { Id = 1 },
                }.AsQueryable();

                var result = helper.WhereOptionalIdKey( query, c => c.Id, "bad", parameterExpression: "parameter" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is not valid." ) ) );
                Assert.IsEmpty( result );
            }
        }

        [TestMethod]
        public void WhereOptionalIdKey_WithIntPropertyAndIdKeyParameter_UpdatesQuery()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { Id = 1 },
                    new Campus { Id = 2 },
                }.AsQueryable();

                var result = helper.WhereOptionalIdKey( query, c => c.Id, 1.AsIdKey(), parameterExpression: "parameter" );

                Assert.IsFalse( helper.HasErrors );
                Assert.HasCount( 1, result );
            }
        }

        [TestMethod]
        public void WhereOptionalIdKey_WithNullableIntPropertyAndNullParameterExpression_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.WhereOptionalIdKey( query, c => c.TeamGroupId, string.Empty, parameterExpression: null );
                } );
            }
        }

        [TestMethod]
        public void WhereOptionalIdKey_WithNullableIntPropertyAndEmptyParameter_ReturnsSameQueryable()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                var result = helper.WhereOptionalIdKey( query, c => c.TeamGroupId, string.Empty, parameterExpression: "parameter" );

                Assert.AreSame( query, result );
            }
        }

        [TestMethod]
        public void WhereOptionalIdKey_WithNullableIntPropertyAndNonIdKeyParameter_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { TeamGroupId = 1 },
                }.AsQueryable();

                var result = helper.WhereOptionalIdKey( query, c => c.TeamGroupId, "bad", parameterExpression: "parameter" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is not valid." ) ) );
                Assert.IsEmpty( result );
            }
        }

        [TestMethod]
        public void WhereOptionalIdKey_WithNullableIntPropertyAndIdKeyParameter_UpdatesQuery()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { TeamGroupId = 1 },
                    new Campus { TeamGroupId = 2 },
                }.AsQueryable();

                var result = helper.WhereOptionalIdKey( query, c => c.TeamGroupId, 1.AsIdKey(), parameterExpression: "parameter" );

                Assert.IsFalse( helper.HasErrors );
                Assert.HasCount( 1, result );
            }
        }

        #endregion

        #region WhereRequiredIdKey

        [TestMethod]
        public void WhereRequiredIdKey_WithIntPropertyAndNullParameterExpression_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.WhereRequiredIdKey( query, c => c.Id, string.Empty, parameterExpression: null );
                } );
            }
        }

        [TestMethod]
        public void WhereRequiredIdKey_WithIntPropertyAndEmptyParameter_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { Id = 1 },
                }.AsQueryable();

                var result = helper.WhereRequiredIdKey( query, c => c.Id, string.Empty, parameterExpression: "parameter" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is required." ) ) );
                Assert.IsEmpty( result );
            }
        }

        [TestMethod]
        public void WhereRequiredIdKey_WithIntPropertyAndNonIdKeyParameter_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { Id = 1 },
                }.AsQueryable();

                var result = helper.WhereRequiredIdKey( query, c => c.Id, "bad", parameterExpression: "parameter" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is not valid." ) ) );
                Assert.IsEmpty( result );
            }
        }

        [TestMethod]
        public void WhereRequiredIdKey_WithIntPropertyAndIdKeyParameter_UpdatesQuery()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { Id = 1 },
                    new Campus { Id = 2 },
                }.AsQueryable();

                var result = helper.WhereRequiredIdKey( query, c => c.Id, 1.AsIdKey(), parameterExpression: "parameter" );

                Assert.IsFalse( helper.HasErrors );
                Assert.HasCount( 1, result );
            }
        }

        [TestMethod]
        public void WhereRequiredIdKey_WithNullableIntPropertyAndNullParameterExpression_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.WhereRequiredIdKey( query, c => c.TeamGroupId, string.Empty, parameterExpression: null );
                } );
            }
        }

        [TestMethod]
        public void WhereRequiredIdKey_WithNullableIntPropertyAndEmptyParameter_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { TeamGroupId = 1 },
                }.AsQueryable();

                var result = helper.WhereRequiredIdKey( query, c => c.TeamGroupId, string.Empty, parameterExpression: "parameter" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is required." ) ) );
                Assert.IsEmpty( result );
            }
        }

        [TestMethod]
        public void WhereRequiredIdKey_WithNullableIntPropertyAndNonIdKeyParameter_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { TeamGroupId = 1 },
                }.AsQueryable();

                var result = helper.WhereRequiredIdKey( query, c => c.TeamGroupId, "bad", parameterExpression: "parameter" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is not valid." ) ) );
                Assert.IsEmpty( result );
            }
        }

        [TestMethod]
        public void WhereRequiredIdKey_WithNullableIntPropertyAndIdKeyParameter_UpdatesQuery()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { TeamGroupId = 1 },
                    new Campus { TeamGroupId = 2 },
                }.AsQueryable();

                var result = helper.WhereRequiredIdKey( query, c => c.TeamGroupId, 1.AsIdKey(), parameterExpression: "parameter" );

                Assert.IsFalse( helper.HasErrors );
                Assert.HasCount( 1, result );
            }
        }

        #endregion

        #region WhereOptionalProperty

        [TestMethod]
        public void WhereOptionalProperty_WithIntPropertyAndNullParameterExpression_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.WhereOptionalProperty( query, c => c.Id, ( int? ) 0, parameterExpression: null );
                } );
            }
        }

        [TestMethod]
        public void WhereOptionalProperty_WithIntPropertyAndNullParameter_ReturnsSameQueryable()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                var result = helper.WhereOptionalProperty( query, c => c.Id, ( int? ) null, parameterExpression: "parameter" );

                Assert.AreSame( query, result );
            }
        }

        [TestMethod]
        public void WhereOptionalProperty_WithIntPropertyAndValueParameter_UpdatesQuery()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { Id = 1 },
                    new Campus { Id = 2 },
                }.AsQueryable();

                var result = helper.WhereOptionalProperty( query, c => c.Id, ( int? ) 1, parameterExpression: "parameter" );

                Assert.IsFalse( helper.HasErrors );
                Assert.HasCount( 1, result );
            }
        }

        [TestMethod]
        public void WhereOptionalProperty_WithNullableIntPropertyAndNullParameterExpression_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.WhereOptionalProperty( query, c => c.TeamGroupId, ( int? ) 0, parameterExpression: null );
                } );
            }
        }

        [TestMethod]
        public void WhereOptionalProperty_WithNullableIntPropertyAndNullParameter_ReturnsSameQueryable()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                var result = helper.WhereOptionalProperty( query, c => c.TeamGroupId, ( int? ) null, parameterExpression: "parameter" );

                Assert.AreSame( query, result );
            }
        }

        [TestMethod]
        public void WhereOptionalProperty_WithNullableIntPropertyAndValueParameter_UpdatesQuery()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { TeamGroupId = 1 },
                    new Campus { TeamGroupId = 2 },
                }.AsQueryable();

                var result = helper.WhereOptionalProperty( query, c => c.TeamGroupId, ( int? ) 1, parameterExpression: "parameter" );

                Assert.IsFalse( helper.HasErrors );
                Assert.HasCount( 1, result );
            }
        }

        [TestMethod]
        public void WhereOptionalProperty_WithStringPropertyAndNullParameterExpression_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.WhereOptionalProperty( query, c => c.Name, null, parameterExpression: null );
                } );
            }
        }

        [TestMethod]
        public void WhereOptionalProperty_WithStringPropertyAndNullParameter_ReturnsSameQueryable()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                var result = helper.WhereOptionalProperty( query, c => c.Name, null, parameterExpression: "parameter" );

                Assert.AreSame( query, result );
            }
        }

        [TestMethod]
        public void WhereOptionalProperty_WithStringPropertyAndValueParameter_UpdatesQuery()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { Name = "One" },
                    new Campus { Name = "Two" },
                }.AsQueryable();

                var result = helper.WhereOptionalProperty( query, c => c.Name, "One", parameterExpression: "parameter" );

                Assert.IsFalse( helper.HasErrors );
                Assert.HasCount( 1, result );
            }
        }

        #endregion

        #region WhereRequiredProperty

        [TestMethod]
        public void WhereRequiredProperty_WithIntPropertyAndNullParameterExpression_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.WhereRequiredProperty( query, c => c.Id, ( int? ) 0, parameterExpression: null );
                } );
            }
        }

        [TestMethod]
        public void WhereRequiredProperty_WithIntPropertyAndNullParameter_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { Id = 1 },
                }.AsQueryable();

                var result = helper.WhereRequiredProperty( query, c => c.Id, ( int? ) null, parameterExpression: "parameter" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is required." ) ) );
                Assert.IsEmpty( result );
            }
        }

        [TestMethod]
        public void WhereRequiredProperty_WithIntPropertyAndValueParameter_UpdatesQuery()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { Id = 1 },
                    new Campus { Id = 2 },
                }.AsQueryable();

                var result = helper.WhereRequiredProperty( query, c => c.Id, ( int? ) 1, parameterExpression: "parameter" );

                Assert.IsFalse( helper.HasErrors );
                Assert.HasCount( 1, result );
            }
        }

        [TestMethod]
        public void WhereRequiredProperty_WithNullableIntPropertyAndNullParameterExpression_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.WhereRequiredProperty( query, c => c.TeamGroupId, ( int? ) 0, parameterExpression: null );
                } );
            }
        }

        [TestMethod]
        public void WhereRequiredProperty_WithNullableIntPropertyAndNullParameter_ReturnsSameQueryable()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { TeamGroupId = 1 },
                }.AsQueryable();

                var result = helper.WhereRequiredProperty( query, c => c.TeamGroupId, ( int? ) null, parameterExpression: "parameter" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is required." ) ) );
                Assert.IsEmpty( result );
            }
        }

        [TestMethod]
        public void WhereRequiredProperty_WithNullableIntPropertyAndValueParameter_UpdatesQuery()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { TeamGroupId = 1 },
                    new Campus { TeamGroupId = 2 },
                }.AsQueryable();

                var result = helper.WhereRequiredProperty( query, c => c.TeamGroupId, ( int? ) 1, parameterExpression: "parameter" );

                Assert.IsFalse( helper.HasErrors );
                Assert.HasCount( 1, result );
            }
        }

        [TestMethod]
        public void WhereRequiredProperty_WithStringPropertyAndNullParameterExpression_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>().AsQueryable();

                Assert.ThrowsExactly<ArgumentNullException>( () =>
                {
                    helper.WhereRequiredProperty( query, c => c.Name, null, parameterExpression: null );
                } );
            }
        }

        [TestMethod]
        public void WhereRequiredProperty_WithStringPropertyAndNullParameter_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { Name = "One" },
                }.AsQueryable();

                var result = helper.WhereRequiredProperty( query, c => c.Name, null, parameterExpression: "parameter" );

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "is required." ) ) );
                Assert.IsEmpty( result );
            }
        }

        [TestMethod]
        public void WhereRequiredProperty_WithStringPropertyAndValueParameter_UpdatesQuery()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );
                var query = new List<Campus>()
                {
                    new Campus { Name = "One" },
                    new Campus { Name = "Two" },
                }.AsQueryable();

                var result = helper.WhereRequiredProperty( query, c => c.Name, "One", parameterExpression: "parameter" );

                Assert.IsFalse( helper.HasErrors );
                Assert.HasCount( 1, result );
            }
        }

        #endregion

        #region SaveChanges

        [TestMethod]
        public void SaveChanges_WithReadOnlyContext_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                Assert.ThrowsExactly<InvalidOperationException>( () =>
                {
                    helper.SaveChanges();
                } );
            }
        }

        [TestMethod]
        public void SaveChanges_WithSaveException_ReportsError()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            rockContextMock.Setup( m => m.SaveChanges() ).Throws( new Exception( "Test Exception" ) );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( rockContext, agentRequestContext, logger );

                helper.SaveChanges();

                Assert.IsTrue( helper.HasErrors );
                Assert.IsTrue( helper.ErrorResult.ErrorMessages.Any( e => e.Contains( "error occurred" ) ) );
            }
        }

        [TestMethod]
        public void SaveChanges_WithUpdatedAttributes_SavesAttributeValues()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    EntityTypeId = EntityTypeCache.Get<Campus>( true, rockContext ).Id,
                    Key = "TestAttribute",
                } );

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var campus = new Campus();
                rockContext.Set<Campus>().Add( campus );
                campus.LoadAttributes( rockContext );

                var helper = new AgentToolHelper( rockContext, agentRequestContext, logger );
                var attributeValues = new List<AttributeValueResult>
                {
                    new AttributeValueResult
                    {
                        Key = "TestAttribute",
                        Value = "SomeValue",
                    },
                };

                helper.SetAttributeValues( campus, attributeValues, enforceSecurity: false );
                helper.SaveChanges();

                Assert.IsFalse( helper.HasErrors );
                Assert.AreNotEqual( 0, campus.Id );
                Assert.IsNotEmpty( rockContext.Set<AttributeValue>().Where( av => av.Value == "SomeValue" ) );
            }
        }

        #endregion

        #region SaveChangesIfNoErrors

        [TestMethod]
        public void SaveChangesIfNoErrors_WithReadOnlyContext_ThrowsException()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( agentRequestContext, logger );

                Assert.ThrowsExactly<InvalidOperationException>( () =>
                {
                    helper.SaveChangesIfNoErrors();
                } );
            }
        }

        [TestMethod]
        public void SaveChangesIfNoErrors_WithHelperErrors_DoesNotCallSaveChanges()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            rockContextMock.Setup( m => m.SaveChanges() ).Returns( 0 );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( rockContext, agentRequestContext, logger );

                helper.AddError( "Test error" );
                helper.SaveChangesIfNoErrors();

                rockContextMock.Verify( m => m.SaveChanges(), Times.Never );
            }
        }

        [TestMethod]
        public void SaveChangesIfNoErrors_WithNoErrors_CallsSaveChanges()
        {
            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            rockContextMock.Setup( m => m.SaveChanges() ).Returns( 0 );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var rockContext = rockContextMock.Object;

                var logger = new Mock<ILogger>().Object;
                var agentRequestContext = new AgentRequestContext( new RockRequestContext(), rockContext );

                var helper = new AgentToolHelper( rockContext, agentRequestContext, logger );

                helper.SaveChangesIfNoErrors();

                rockContextMock.Verify( m => m.SaveChanges(), Times.Once );
            }
        }

        #endregion

        #region Support

        private class HelperErrorClass : Entity<HelperErrorClass>
        {
            public const string Error = "Test exception for property access.";

            public int IntProperty
            {
                get => throw new Exception( Error );
                set => throw new Exception( Error );
            }

            public int? NullableIntProperty
            {
                get => throw new Exception( Error );
                set => throw new Exception( Error );
            }

            public string StringProperty
            {
                get => throw new Exception( Error );
                set => throw new Exception( Error );
            }

            public PersonAlias MissingForeignKey { get; set; }

            public PersonAlias InvalidForeignKey { get; set; }

            public string InvalidForeignKeyId { get; set; }

            public PersonAlias RequiredForeignKey { get; set; }

            public int RequiredForeignKeyId { get; set; }

            public DefinedValue MissingDefinedValueForeignKey { get; set; }

            public DefinedValue InvalidDefinedValueForeignKey { get; set; }

            public string InvalidDefinedValueForeignKeyId { get; set; }

            public DefinedValue RequiredDefinedValueForeignKey { get; set; }

            public int RequiredDefinedValueForeignKeyId { get; set; }
        }

        #endregion
    }
}
