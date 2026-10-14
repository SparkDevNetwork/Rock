using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Security;

namespace Rock.Tests.Security
{
    /// <summary>
    /// Tests for <see cref="PersonTokenScope"/>.
    /// </summary>
    [TestClass]
    public class PersonTokenScopeTests
    {
        private static readonly Person _allowedPerson = new Person { Id = 1001 };

        private static readonly Person _otherPerson = new Person { Id = 1002 };

        [TestMethod]
        public void IsTokenAllowed_WithNoActiveScope_AllowsAnyone()
        {
            Assert.IsTrue( PersonTokenScope.IsTokenAllowed( _otherPerson.Id ) );
        }

        [TestMethod]
        public void IsTokenAllowed_InsideScope_AllowsOnlyTheScopedPerson()
        {
            using ( PersonTokenScope.RestrictTo( _allowedPerson ) )
            {
                Assert.IsTrue( PersonTokenScope.IsTokenAllowed( _allowedPerson.Id ) );
                Assert.IsFalse( PersonTokenScope.IsTokenAllowed( _otherPerson.Id ) );
            }
        }

        [TestMethod]
        public void IsTokenAllowed_InsideScopeForNullPerson_AllowsNoOne()
        {
            using ( PersonTokenScope.RestrictTo( null ) )
            {
                Assert.IsFalse( PersonTokenScope.IsTokenAllowed( _allowedPerson.Id ) );
            }
        }

        [TestMethod]
        public void IsTokenAllowed_AfterScopeIsDisposed_AllowsAnyoneAgain()
        {
            using ( PersonTokenScope.RestrictTo( _allowedPerson ) )
            {
                Assert.IsFalse( PersonTokenScope.IsTokenAllowed( _otherPerson.Id ) );
            }

            Assert.IsTrue( PersonTokenScope.IsTokenAllowed( _otherPerson.Id ) );
        }

        [TestMethod]
        public void IsTokenAllowed_InNestedScopes_AllowsOnlyPeopleEveryScopeAllows()
        {
            using ( PersonTokenScope.RestrictTo( _allowedPerson ) )
            {
                using ( PersonTokenScope.RestrictTo( _otherPerson ) )
                {
                    Assert.IsFalse( PersonTokenScope.IsTokenAllowed( _allowedPerson.Id ) );
                    Assert.IsFalse( PersonTokenScope.IsTokenAllowed( _otherPerson.Id ) );
                }

                Assert.IsTrue( PersonTokenScope.IsTokenAllowed( _allowedPerson.Id ) );
            }
        }

        [TestMethod]
        public async Task IsTokenAllowed_InTaskStartedInsideScope_HonorsTheScope()
        {
            using ( PersonTokenScope.RestrictTo( _allowedPerson ) )
            {
                var isAllowed = await Task.Run( () => PersonTokenScope.IsTokenAllowed( _otherPerson.Id ) );

                Assert.IsFalse( isAllowed );
            }
        }

        [TestMethod]
        public void Dispose_CalledAgainAfterANewerScopeBegins_LeavesTheNewerScopeActive()
        {
            using ( PersonTokenScope.RestrictTo( _allowedPerson ) )
            {
                var staleScope = PersonTokenScope.RestrictTo( _allowedPerson );
                staleScope.Dispose();

                using ( PersonTokenScope.RestrictTo( null ) )
                {
                    staleScope.Dispose();

                    Assert.IsFalse( PersonTokenScope.IsTokenAllowed( _allowedPerson.Id ) );
                }
            }
        }
    }
}
