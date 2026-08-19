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

using Rock.Bus.Locking;

namespace Rock.Tests.Bus.Locking;

/// <summary>
/// Tests for <see cref="NoOpDistributedLockProvider"/>. The no-op
/// provider is the break-glass fallback registered when the
/// <c>DisableDistributedLocking</c> app setting is <c>true</c>. Its
/// contract is that every acquisition succeeds and disposal is a no-op,
/// which restores Rock's pre-distributed-locking behavior. Argument
/// validation is still enforced so bad keys don't lie dormant until the
/// kill switch is turned off again.
/// </summary>
[TestClass]
public class NoOpDistributedLockProviderTests
{
    public TestContext TestContext { get; set; }

    private sealed class TestMarker { }

    [TestMethod]
    public void TryAcquire_ReturnsHandleWithIsAcquiredTrue()
    {
        var provider = new NoOpDistributedLockProvider();

        using var handle = provider.TryAcquire( typeof( TestMarker ), "42", TimeSpan.Zero );

        Assert.IsTrue( handle.IsAcquired, "No-op handle must report acquired so callers do not silently skip." );
        Assert.AreEqual( CancellationToken.None, handle.LostToken, "No real lock exists to lose." );
    }

    [TestMethod]
    public async Task TryAcquireAsync_ReturnsHandleWithIsAcquiredTrue()
    {
        var provider = new NoOpDistributedLockProvider();

        using var handle = await provider.TryAcquireAsync( typeof( TestMarker ), "42", TimeSpan.Zero, TestContext.CancellationToken );

        Assert.IsTrue( handle.IsAcquired );
        Assert.AreEqual( CancellationToken.None, handle.LostToken );
    }

    [TestMethod]
    public void Dispose_IsSafeToCallMultipleTimes()
    {
        // The no-op handle is a shared singleton; disposing it multiple
        // times must be safe because callers may re-enter using blocks
        // and the same instance is handed out to every caller.
        var provider = new NoOpDistributedLockProvider();
        var handle = provider.TryAcquire( typeof( TestMarker ), "42", TimeSpan.Zero );

        handle.Dispose();
        handle.Dispose();
        handle.Dispose();

        // No throw = success. Nothing else to observe.
    }

    [TestMethod]
    public void TryAcquire_SameKeyMultipleCalls_AllAcquired()
    {
        // The no-op does not track state, so back-to-back acquires on the
        // same key both succeed. This is a shape difference from the SQL
        // provider (which rejects reentrancy) but is intentional — the
        // kill switch's purpose is to disable coordination, and adding
        // reentrancy detection here would break subsystems that
        // legitimately re-enter under the kill switch.
        var provider = new NoOpDistributedLockProvider();

        using var first = provider.TryAcquire( typeof( TestMarker ), "42", TimeSpan.Zero );
        using var second = provider.TryAcquire( typeof( TestMarker ), "42", TimeSpan.Zero );

        Assert.IsTrue( first.IsAcquired );
        Assert.IsTrue( second.IsAcquired );
    }

    [TestMethod]
    public void TryAcquire_InvalidKey_StillValidates()
    {
        // Argument validation runs even when the kill switch is on so a
        // caller who ships a bad key does not have their bug hidden
        // until the operator flips the switch off again.
        var provider = new NoOpDistributedLockProvider();

        Assert.ThrowsExactly<ArgumentException>( () =>
            provider.TryAcquire( typeof( TestMarker ), "has space", TimeSpan.Zero ) );
    }

    [TestMethod]
    public void TryAcquire_NullMarker_StillValidates()
    {
        var provider = new NoOpDistributedLockProvider();

        Assert.ThrowsExactly<ArgumentNullException>( () =>
            provider.TryAcquire( null, "42", TimeSpan.Zero ) );
    }
}
