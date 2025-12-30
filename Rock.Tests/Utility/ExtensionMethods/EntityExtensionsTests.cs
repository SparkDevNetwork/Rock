using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Core;
using Rock.Data;

namespace Rock.Tests.Utility.ExtensionMethods
{
    [TestClass]
    public class EntityExtensionsTests
    {
        #region SummarizeLinkage Tests

        [TestMethod]
        public void SummarizeLinkage_WithGenericEntity_ReturnsToStringValue()
        {
            var expectedValue = "Test Entity";
            var entityMock = new Mock<IEntity>();

            entityMock.Setup( e => e.TypeId ).Returns( 1 );
            entityMock.Setup( e => e.Id ).Returns( 1 );
            entityMock.Setup( e => e.ToString() ).Returns( expectedValue );

            var summary = entityMock.Object.SummarizeLinkage( null );

            Assert.AreEqual( expectedValue, summary.Value );
        }

        [TestMethod]
        public void SummarizeLinkage_IncludesEntityTypeId()
        {
            var expectedValue = 42;
            var entityMock = new Mock<IEntity>();

            entityMock.Setup( e => e.TypeId ).Returns( expectedValue );
            entityMock.Setup( e => e.Id ).Returns( 1 );
            entityMock.Setup( e => e.ToString() ).Returns( "test" );

            var summary = entityMock.Object.SummarizeLinkage( null );

            Assert.AreEqual( expectedValue, summary.EntityTypeId );
        }

        [TestMethod]
        public void SummarizeLinkage_IncludesEntityId()
        {
            var expectedValue = 42;
            var entityMock = new Mock<IEntity>();

            entityMock.Setup( e => e.TypeId ).Returns( 1 );
            entityMock.Setup( e => e.Id ).Returns( expectedValue );
            entityMock.Setup( e => e.ToString() ).Returns( "test" );

            var summary = entityMock.Object.SummarizeLinkage( null );

            Assert.AreEqual( expectedValue, summary.EntityId );
        }

        [TestMethod]
        public void SummarizeLinkage_WithSummaryEntity_CallsSummaryValue()
        {
            var expectedValue = "Test Entity";
            var entityMock = new Mock<IEntity>();

            entityMock.Setup( e => e.TypeId ).Returns( 1 );
            entityMock.Setup( e => e.Id ).Returns( 1 );
            entityMock.Setup( e => e.ToString() ).Returns( "test" );

            var summaryMock = entityMock.As<IHasLinkageSummary>();
            summaryMock.Setup( s => s.SummaryValue( It.IsAny<RockContext>() ) ).Returns( expectedValue );
            summaryMock.Setup( s => s.SummaryParent( It.IsAny<RockContext>() ) ).Returns( null );

            var summary = entityMock.Object.SummarizeLinkage( null );

            Assert.AreEqual( expectedValue, summary.Value );
        }

        [TestMethod]
        public void SummarizeLinkage_WithSummaryEntity_IncludesParent()
        {
            var expectedValue = "Test Entity";
            var parentMock = new Mock<IEntity>();

            parentMock.Setup( e => e.TypeId ).Returns( 1 );
            parentMock.Setup( e => e.Id ).Returns( 1 );
            parentMock.Setup( e => e.ToString() ).Returns( expectedValue );

            var entityMock = new Mock<IEntity>();

            entityMock.Setup( e => e.TypeId ).Returns( 42 );
            entityMock.Setup( e => e.Id ).Returns( 1 );
            entityMock.Setup( e => e.ToString() ).Returns( "test" );

            var summaryMock = entityMock.As<IHasLinkageSummary>();
            summaryMock.Setup( s => s.SummaryValue( It.IsAny<RockContext>() ) ).Returns( "test" );
            summaryMock.Setup( s => s.SummaryParent( It.IsAny<RockContext>() ) ).Returns( parentMock.Object );

            var summary = entityMock.Object.SummarizeLinkage( null );

            Assert.IsNotNull( summary.Parent );
            Assert.AreEqual( expectedValue, summary.Parent.Value );
        }

        [TestMethod]
        [Timeout( 2000 )]
        public void SummarizeLinkage_WithInfiniteLoop_BreaksLoop()
        {
            var entityMock = new Mock<IEntity>();

            entityMock.Setup( e => e.TypeId ).Returns( 42 );
            entityMock.Setup( e => e.Id ).Returns( 1 );
            entityMock.Setup( e => e.ToString() ).Returns( "test" );

            var summaryMock = entityMock.As<IHasLinkageSummary>();
            summaryMock.Setup( s => s.SummaryValue( It.IsAny<RockContext>() ) ).Returns( "test" );
            summaryMock.Setup( s => s.SummaryParent( It.IsAny<RockContext>() ) ).Returns( () => entityMock.Object );

            var summary = entityMock.Object.SummarizeLinkage( null );

            int count = 0;
            while ( summary != null )
            {
                count++;
                summary = summary.Parent;
            }

            Assert.IsTrue( count > 1, "The linkage summary should have included at least one parent." );
        }

        #endregion
    }
}
