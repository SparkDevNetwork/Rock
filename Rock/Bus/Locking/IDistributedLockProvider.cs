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
/// A provider that coordinates named locks across every Rock instance in
/// a farm. Consumers acquire a lock keyed by a marker type plus a resource
/// identifier, do the protected work while holding the returned handle,
/// and dispose the handle to release the lock.
/// </summary>
/// <remarks>
/// <para>
/// The provider is registered in Rock's DI container as a singleton.
/// Callers obtain it via constructor injection where possible, or via
/// <c>RockApp.Current.GetRequiredService&lt;IDistributedLockProvider&gt;()</c>
/// for code paths that cannot yet participate in constructor injection.
/// </para>
/// <para>
/// The <c>Type</c>-taking method is the primitive. The generic overload
/// exposed by <see cref="DistributedLockProviderExtensions"/> is a thin
/// wrapper over it; both surfaces enforce identical validation.
/// </para>
/// <para>
/// Reentrancy is tracked per <em>logical flow</em> using
/// <see cref="System.Threading.AsyncLocal{T}"/>. Async continuations
/// inherit the tracking automatically, and sibling tasks spawned from a
/// common parent flow (for example the members of a
/// <see cref="Task.WhenAll(Task[])"/>) share it too. Consequently, two
/// siblings that both attempt to acquire the same lock key do not race
/// at the SQL layer; one throws
/// <see cref="DistributedLockReentrancyException"/> and the other
/// proceeds. This is deliberate: it surfaces a genuine coordination
/// bug (the same logical operation trying to hold the same lock twice)
/// rather than masking it as a random SQL contention loss. When the
/// intent really is per-item work in parallel, key each acquisition to
/// a different <c>resourceId</c> so the siblings coordinate on distinct
/// locks.
/// </para>
/// </remarks>
public interface IDistributedLockProvider
{
    /// <summary>
    /// Attempts to acquire a distributed lock keyed by
    /// <paramref name="markerType"/> plus <paramref name="resourceId"/>.
    /// Returns a handle whose <see cref="ILockHandle.IsAcquired"/> reports
    /// whether the lock was obtained within the timeout. The handle MUST
    /// be disposed even if it was not acquired.
    /// </summary>
    /// <param name="markerType">
    /// The type that identifies the subsystem coordinating on this lock.
    /// By convention the class that acquires the lock uses itself as the
    /// marker. MUST NOT be a generic (constructed or open) type: doing so
    /// throws <see cref="ArgumentException"/>. See the Distributed Locking
    /// spec's Lock Key Namespace section for the selection guidance.
    /// </param>
    /// <param name="resourceId">
    /// A caller-supplied identifier that scopes the lock inside the marker
    /// type's namespace (typically the entity Id being coordinated). MUST
    /// be printable ASCII (letters, digits, hyphen, underscore, period,
    /// colon; no whitespace) and MUST NOT push the total lock key past
    /// 255 characters. Invalid inputs throw <see cref="ArgumentException"/>.
    /// </param>
    /// <param name="timeout">
    /// How long to wait for the lock to become available. Pass
    /// <see cref="TimeSpan.Zero"/> for "try acquire, skip if unavailable"
    /// semantics; positive values wait up to the specified duration.
    /// </param>
    /// <returns>
    /// A handle whose <see cref="ILockHandle.IsAcquired"/> is <c>true</c>
    /// on success. If acquisition failed for any reason, <c>IsAcquired</c>
    /// is <c>false</c> and the caller MUST skip the protected work.
    /// Callers MUST dispose the returned handle in all cases (it is safe
    /// to dispose an unacquired handle).
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="markerType"/> is generic, or
    /// <paramref name="resourceId"/> is null, empty, contains invalid
    /// characters, or produces a total lock key over 255 characters.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The caller already holds this lock in the same logical flow.
    /// Reentrancy is intentionally rejected.
    /// </exception>
    ILockHandle TryAcquire( Type markerType, string resourceId, TimeSpan timeout );

    /// <summary>
    /// Asynchronous form of
    /// <see cref="TryAcquire(Type, string, TimeSpan)"/>. Same validation,
    /// same semantics; the acquisition wait respects
    /// <paramref name="cancellationToken"/>.
    /// </summary>
    /// <param name="markerType">See <see cref="TryAcquire(Type, string, TimeSpan)"/>.</param>
    /// <param name="resourceId">See <see cref="TryAcquire(Type, string, TimeSpan)"/>.</param>
    /// <param name="timeout">See <see cref="TryAcquire(Type, string, TimeSpan)"/>.</param>
    /// <param name="cancellationToken">
    /// Fires cancellation while the provider is waiting for the lock to
    /// become available. If the token fires before the lock is granted,
    /// the returned task's result reports <see cref="ILockHandle.IsAcquired"/>
    /// as <c>false</c>; the token does NOT release an already-granted
    /// lock.
    /// </param>
    Task<ILockHandle> TryAcquireAsync( Type markerType, string resourceId, TimeSpan timeout, CancellationToken cancellationToken = default );
}
