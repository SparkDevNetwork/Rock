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

namespace Rock.Bus.Locking;

/// <summary>
/// Break-glass fallback provider registered when the
/// <c>DisableDistributedLocking</c> app setting is <c>true</c>. Every
/// acquisition succeeds immediately and disposal is a no-op, so consuming
/// code paths continue to run but without cross-node coordination — which
/// restores Rock's pre-distributed-locking behavior. Not for normal use.
/// </summary>
/// <remarks>
/// <para>
/// The choice to report <see cref="ILockHandle.IsAcquired"/> as <c>true</c>
/// (rather than <c>false</c>) is deliberate: returning <c>false</c> would
/// cause every locked code path (Quartz jobs, communication sends, etc.)
/// to silently skip, which is worse than the pre-spec behavior. Reporting
/// <c>true</c> restores exactly the pre-spec behavior — work runs,
/// possibly duplicating during app pool overlap and across farm nodes,
/// just as it did before the primitive existed.
/// </para>
/// <para>
/// Argument validation is intentionally still performed so callers cannot
/// accidentally ship a bad lock key that would fail once the kill switch
/// is turned off again.
/// </para>
/// </remarks>
internal sealed class NoOpDistributedLockProvider : IDistributedLockProvider
{
    /// <inheritdoc/>
    public ILockHandle TryAcquire( Type markerType, string resourceId, TimeSpan timeout )
    {
        // Reuse the SQL Server provider's key validation so bad keys still
        // fail loudly even when the kill switch is on. This keeps
        // development bugs from lying dormant until the switch is flipped.
        SqlServerDistributedLockProvider.ValidateAndBuildKey( markerType, resourceId );

        return NoOpLockHandle.Instance;
    }

    /// <inheritdoc/>
    public Task<ILockHandle> TryAcquireAsync( Type markerType, string resourceId, TimeSpan timeout, CancellationToken cancellationToken = default )
    {
        SqlServerDistributedLockProvider.ValidateAndBuildKey( markerType, resourceId );

        return Task.FromResult<ILockHandle>( NoOpLockHandle.Instance );
    }

    /// <summary>
    /// A singleton handle representing "always acquired, disposal is a
    /// no-op." No state is retained between calls; every caller shares
    /// the same instance because there is nothing to release.
    /// </summary>
    private sealed class NoOpLockHandle : ILockHandle
    {
        public static readonly NoOpLockHandle Instance = new NoOpLockHandle();

        public bool IsAcquired => true;

        public CancellationToken LostToken => CancellationToken.None;

        public void Dispose()
        {
            // Nothing to release.
        }
    }
}
