using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.Tests.AI.Agent
{
    [TestClass]
    public class EntityResultBaseTests
    {
        [TestMethod]
        public void Sanitize_WithListChildren_SanitizesEachChild()
        {
            var child = new TrackedResult();
            var result = new Container<List<TrackedResult>> { Children = new List<TrackedResult> { child } };

            result.Sanitize( null );

            Assert.AreEqual( 1, child.SanitizeCount );
        }

        [TestMethod]
        public void Sanitize_WithEnumerableChildren_SanitizesEachChild()
        {
            var child = new TrackedResult();
            var result = new Container<IEnumerable<TrackedResult>> { Children = Enumerable.Repeat( child, 1 ) };

            result.Sanitize( null );

            Assert.AreEqual( 1, child.SanitizeCount );
        }

        [TestMethod]
        public void Sanitize_WithDerivedListChildren_SanitizesEachChild()
        {
            var child = new TrackedResult();
            var result = new Container<ResultList> { Children = new ResultList { child } };

            result.Sanitize( null );

            Assert.AreEqual( 1, child.SanitizeCount );
        }

        [TestMethod]
        public void Sanitize_WithRejectedCollectionChildren_RemovesAfterEnumeration()
        {
            var first = new TrackedResult { IsSafe = false };
            var safe = new TrackedResult();
            var last = new TrackedResult { IsSafe = false };
            var result = new Container<ICollection<TrackedResult>>
            {
                Children = new List<TrackedResult> { first, safe, last }
            };

            result.Sanitize( null );

            CollectionAssert.AreEqual( new[] { safe }, result.Children.ToArray() );
            Assert.AreEqual( 1, first.SanitizeCount );
            Assert.AreEqual( 1, safe.SanitizeCount );
            Assert.AreEqual( 1, last.SanitizeCount );
        }

        [TestMethod]
        public void Sanitize_WithArrayChildren_SanitizesWithoutRemoving()
        {
            var child = new TrackedResult { IsSafe = false };
            var result = new Container<TrackedResult[]> { Children = new[] { child } };

            result.Sanitize( null );

            Assert.AreEqual( 1, child.SanitizeCount );
            Assert.AreEqual( 1, result.Children.Length );
        }

        [TestMethod]
        public void Sanitize_WithReadOnlyChildren_SanitizesWithoutRemoving()
        {
            var child = new TrackedResult { IsSafe = false };
            var result = new Container<ReadOnlyCollection<TrackedResult>>
            {
                Children = new List<TrackedResult> { child }.AsReadOnly()
            };

            result.Sanitize( null );

            Assert.AreEqual( 1, child.SanitizeCount );
            Assert.AreEqual( 1, result.Children.Count );
        }

        [TestMethod]
        public void Sanitize_WithIndividualChild_ClearsRejectedChild()
        {
            var child = new TrackedResult { IsSafe = false };
            var result = new Container<TrackedResult> { Children = child };

            result.Sanitize( null );

            Assert.IsNull( result.Children );
            Assert.AreEqual( 1, child.SanitizeCount );
        }

        [TestMethod]
        public void Sanitize_WithNullChildren_DoesNotThrow()
        {
            var result = new Container<List<TrackedResult>>();

            Assert.IsTrue( result.Sanitize( null ) );
        }

        private class Container<T> : EntityResultBase
        {
            public T Children { get; set; }
        }

        private class ResultList : List<TrackedResult>
        {
        }

        private class TrackedResult : EntityResultBase
        {
            public int SanitizeCount { get; private set; }

            public bool IsSafe { get; set; } = true;

            public override bool Sanitize( AgentRequestContext agentRequestContext )
            {
                SanitizeCount++;
                return IsSafe;
            }
        }
    }
}
