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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Bus.Locking;

namespace Rock.Tests.Bus.Locking;

/// <summary>
/// Tests for <see cref="DistributedLockProviderExtensions"/>. The
/// extensions are a thin generic shim over the <c>Type</c>-taking
/// primitive, so these tests verify only that the generic dispatch
/// resolves the marker type correctly and forwards the other arguments
/// unchanged. The real acquisition semantics live in
/// <c>SqlServerDistributedLockProvider</c> and are covered separately.
/// </summary>
[TestClass]
public class DistributedLockProviderExtensionsTests
{
    public TestContext TestContext { get; set; }

    private sealed class TestMarker { }

    [TestMethod]
    public void TryAcquire_ResolvesGenericToTypeAndForwards()
    {
        var expectedTimeout = TimeSpan.FromSeconds( 3 );
        var expectedHandle = new Mock<ILockHandle>().Object;

        var providerMock = new Mock<IDistributedLockProvider>( MockBehavior.Strict );
        providerMock
            .Setup( p => p.TryAcquire( typeof( TestMarker ), "42", expectedTimeout ) )
            .Returns( expectedHandle );

        var handle = providerMock.Object.TryAcquire<TestMarker>( "42", expectedTimeout );

        Assert.AreSame( expectedHandle, handle, "Extension should forward the underlying handle." );
        providerMock.VerifyAll();
    }

    [TestMethod]
    public async Task TryAcquireAsync_ResolvesGenericToTypeAndForwards()
    {
        var expectedTimeout = TimeSpan.FromSeconds( 3 );
        using var cts = new CancellationTokenSource();

        var expectedHandle = new Mock<ILockHandle>().Object;
        var providerMock = new Mock<IDistributedLockProvider>( MockBehavior.Strict );
        providerMock
            .Setup( p => p.TryAcquireAsync( typeof( TestMarker ), "42", expectedTimeout, cts.Token ) )
            .ReturnsAsync( expectedHandle );

        var handle = await providerMock.Object.TryAcquireAsync<TestMarker>( "42", expectedTimeout, cts.Token );

        Assert.AreSame( expectedHandle, handle, "Async extension should forward the underlying handle." );
        providerMock.VerifyAll();
    }

    [TestMethod]
    public void TryAcquire_NullProvider_Throws()
    {
        IDistributedLockProvider provider = null;

        Assert.ThrowsExactly<ArgumentNullException>( () =>
            provider.TryAcquire<TestMarker>( "42", TimeSpan.Zero ) );
    }

    [TestMethod]
    public void TryAcquireAsync_NullProvider_Throws()
    {
        IDistributedLockProvider provider = null;

        Assert.ThrowsExactly<ArgumentNullException>( () =>
            provider.TryAcquireAsync<TestMarker>( "42", TimeSpan.Zero, TestContext.CancellationToken ) );
    }
}
