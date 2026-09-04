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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Bus.Locking;
using Rock.Configuration;

namespace Rock.Tests.Bus.Locking;

/// <summary>
/// Pure-logic tests for the internal validation and key-building logic
/// exposed by <see cref="SqlServerDistributedLockProvider"/>. These tests
/// exercise only in-memory string handling and reflection; they never
/// open a database connection.
/// </summary>
[TestClass]
public class SqlServerDistributedLockProviderTests
{
    #region Marker Types

    /// <summary>
    /// A plain non-generic marker used by the "happy path" tests. Nested
    /// under the test class so its FullName ends with a `+` separator,
    /// which we use to prove the nested-type handling documented in the
    /// spec.
    /// </summary>
    private sealed class HappyMarker { }

    /// <summary>
    /// A generic marker used by the rejection tests. Even in its
    /// constructed form (<c>GenericMarker&lt;int&gt;</c>) its FullName
    /// embeds the assembly-qualified type argument that changes across
    /// builds, which is why the provider rejects it at the API boundary.
    /// </summary>
    private sealed class GenericMarker<T> { }

    #endregion

    #region ValidateAndBuildKey - Happy Path

    [TestMethod]
    public void ValidateAndBuildKey_NonGenericMarker_ReturnsNamespacePlusName()
    {
        // The marker is nested under this test class, so FullName uses
        // "+" as the nested-type separator. That is deliberate: the spec
        // documents this form.
        var key = SqlServerDistributedLockProvider.ValidateAndBuildKey( typeof( HappyMarker ), "42" );

        var expected = typeof( HappyMarker ).FullName + ":42";

        Assert.AreEqual( expected, key );
        Assert.Contains( "+HappyMarker", key, "Nested type name should retain the '+' separator." );
    }

    [TestMethod]
    public void ValidateAndBuildKey_TopLevelMarker_UsesFullName()
    {
        // The provider uses FullName (Namespace.Name), NOT
        // AssemblyQualifiedName, because AQN embeds Version and
        // PublicKeyToken that drift across Rock builds.
        var key = SqlServerDistributedLockProvider.ValidateAndBuildKey( typeof( SqlServerDistributedLockProviderTests ), "1" );

        Assert.AreEqual( typeof( SqlServerDistributedLockProviderTests ).FullName + ":1", key );
        Assert.DoesNotContain( "Version=", key, "Lock key should not embed assembly version metadata." );
        Assert.DoesNotContain( "PublicKeyToken=", key, "Lock key should not embed public key token metadata." );
    }

    [TestMethod]
    [DataRow( "abc123", DisplayName = "Alphanumeric" )]
    [DataRow( "with-hyphen", DisplayName = "Hyphen" )]
    [DataRow( "with_underscore", DisplayName = "Underscore" )]
    [DataRow( "with.period", DisplayName = "Period" )]
    [DataRow( "with:colon", DisplayName = "Colon" )]
    [DataRow( "A1B2C3", DisplayName = "Mixed case + digits" )]
    [DataRow( "0", DisplayName = "Single digit" )]
    public void ValidateAndBuildKey_ValidResourceId_Accepts( string resourceId )
    {
        var key = SqlServerDistributedLockProvider.ValidateAndBuildKey( typeof( HappyMarker ), resourceId );

        Assert.EndsWith( ":" + resourceId , key);
    }

    [TestMethod]
    public void ValidateAndBuildKey_KeyExactly255Chars_Accepts()
    {
        // Build a resource id whose combined key is exactly 255 chars.
        // The marker's FullName eats a fixed prefix; pad the rest with
        // valid chars.
        var prefix = typeof( HappyMarker ).FullName + ":";
        var padCount = SqlServerDistributedLockProvider.MaxLockKeyLength - prefix.Length;
        var resourceId = new string( 'a', padCount );

        var key = SqlServerDistributedLockProvider.ValidateAndBuildKey( typeof( HappyMarker ), resourceId );

        Assert.AreEqual( SqlServerDistributedLockProvider.MaxLockKeyLength, key.Length );
    }

    #endregion

    #region ValidateAndBuildKey - Rejections

    [TestMethod]
    public void ValidateAndBuildKey_GenericMarker_Throws()
    {
        // Constructed generic. The spec calls this out specifically as
        // the class of markers that would embed assembly-qualified type
        // arguments and break coordination during rolling upgrades.
        var ex = Assert.ThrowsExactly<ArgumentException>( () =>
            SqlServerDistributedLockProvider.ValidateAndBuildKey( typeof( GenericMarker<int> ), "42" ) );

        Assert.Contains( "generic", ex.Message, "Message should indicate the generic-type rejection." );
    }

    [TestMethod]
    public void ValidateAndBuildKey_OpenGenericMarker_Throws()
    {
        // Open generic. Same rejection reason as a constructed generic,
        // and this variant is also more common in reflection-driven code
        // paths that forget to close the generic.
        Assert.ThrowsExactly<ArgumentException>( () =>
            SqlServerDistributedLockProvider.ValidateAndBuildKey( typeof( List<> ), "42" ) );
    }

    [TestMethod]
    public void ValidateAndBuildKey_NullMarker_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>( () =>
            SqlServerDistributedLockProvider.ValidateAndBuildKey( null, "42" ) );
    }

    [TestMethod]
    public void ValidateAndBuildKey_NullResourceId_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>( () =>
            SqlServerDistributedLockProvider.ValidateAndBuildKey( typeof( HappyMarker ), null ) );
    }

    [TestMethod]
    public void ValidateAndBuildKey_EmptyResourceId_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>( () =>
            SqlServerDistributedLockProvider.ValidateAndBuildKey( typeof( HappyMarker ), string.Empty ) );
    }

    [TestMethod]
    [DataRow( "has space", DisplayName = "Space" )]
    [DataRow( "has\ttab", DisplayName = "Tab" )]
    [DataRow( "has\nnewline", DisplayName = "Newline" )]
    [DataRow( "has/slash", DisplayName = "Slash" )]
    [DataRow( "has\\backslash", DisplayName = "Backslash" )]
    [DataRow( "has'quote", DisplayName = "Single quote" )]
    [DataRow( "has\"quote", DisplayName = "Double quote" )]
    [DataRow( "has*wildcard", DisplayName = "Asterisk" )]
    [DataRow( "has(paren", DisplayName = "Paren" )]
    [DataRow( "hasüü", DisplayName = "Non-ASCII (umlaut)" )]
    [DataRow( "hascomplex®", DisplayName = "Non-ASCII (registered mark)" )]
    [DataRow( "has\x7Fdelete", DisplayName = "Control char" )]
    public void ValidateAndBuildKey_InvalidResourceIdChars_Throws( string resourceId )
    {
        // Anything outside [A-Za-z0-9-_.:] is rejected at the boundary
        // so we never trigger the library's SHA512 fallback (which would
        // destroy the readability of sys.dm_tran_locks.resource_description).
        var ex = Assert.ThrowsExactly<ArgumentException>( () =>
            SqlServerDistributedLockProvider.ValidateAndBuildKey( typeof( HappyMarker ), resourceId ) );

        Assert.Contains( "invalid character", ex.Message, "Message should indicate the invalid character." );
    }

    [TestMethod]
    public void ValidateAndBuildKey_KeyOver255Chars_Throws()
    {
        // Build a resource id that pushes the key one char past the
        // limit. The provider is expected to reject at the boundary
        // rather than silently truncating or hashing.
        var prefix = typeof( HappyMarker ).FullName + ":";
        var overflow = SqlServerDistributedLockProvider.MaxLockKeyLength - prefix.Length + 1;
        var resourceId = new string( 'a', overflow );

        var ex = Assert.ThrowsExactly<ArgumentException>( () =>
            SqlServerDistributedLockProvider.ValidateAndBuildKey( typeof( HappyMarker ), resourceId ) );

        Assert.Contains( "255", ex.Message, "Message should indicate the 255-char limit." );
    }

    [TestMethod]
    public void ValidateAndBuildKey_MarkerWithNullFullName_Throws()
    {
        // Reflection can produce Type instances whose FullName is null,
        // most commonly generic type parameters retrieved via
        // GetGenericArguments on an open generic. The provider must
        // reject these because a null FullName can't build a stable
        // lock key. This closes the "marker.FullName is null" branch
        // that's otherwise unreachable via ordinary typeof(T) syntax.
        var genericTypeParameter = typeof( List<> ).GetGenericArguments()[0];

        Assert.IsNull( genericTypeParameter.FullName, "Sanity: generic type parameters must have null FullName for this test to exercise the intended branch." );

        var ex = Assert.ThrowsExactly<ArgumentException>( () =>
            SqlServerDistributedLockProvider.ValidateAndBuildKey( genericTypeParameter, "42" ) );

        Assert.Contains( "FullName", ex.Message, "Message should indicate the null-FullName rejection." );
    }

    #endregion

    #region Constructor

    [TestMethod]
    public void Constructor_NullConnectionStringProvider_Throws()
    {
        // DI should never inject null, but the guard is here so a
        // misconfigured registration fails fast at construction rather
        // than deferring the NRE to the first TryAcquire call.
        Assert.ThrowsExactly<ArgumentNullException>( () =>
            new SqlServerDistributedLockProvider( null ) );
    }

    [TestMethod]
    public void Constructor_EmptyConnectionString_Throws()
    {
        // Rock's primary connection string is required to derive the
        // lock-pool connection string. An empty/missing value at
        // construction is an environment error the operator needs to
        // see immediately, not deferred to first acquire.
        var providerMock = new Mock<IConnectionStringProvider>();
        providerMock.SetupGet( p => p.ConnectionString ).Returns( string.Empty );

        var ex = Assert.ThrowsExactly<InvalidOperationException>( () =>
            new SqlServerDistributedLockProvider( providerMock.Object ) );

        Assert.Contains( "primary connection string", ex.Message, "Message should indicate the missing connection string." );
    }

    #endregion
}
