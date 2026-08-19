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

namespace Rock.Bus.Locking;

/// <summary>
/// Represents a handle for a lock that has been requested from a provider.
/// The handle is disposable and MUST be disposed once the caller is done
/// with the protected work so the underlying resource (connection, row,
/// mutex, etc.) is released.
/// </summary>
/// <remarks>
/// This interface is intentionally general-purpose. Any lock mechanism
/// Rock might expose in the future (in-process, database-row, distributed)
/// can share this handle shape. The typical usage pattern is:
/// <code>
/// using ( var handle = _lockProvider.TryAcquire&lt;MyThing&gt;( id, TimeSpan.Zero ) )
/// {
///     if ( !handle.IsAcquired )
///     {
///         return;
///     }
///     // Do the protected work here.
/// }
/// </code>
/// </remarks>
public interface ILockHandle : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the lock was successfully acquired.
    /// Callers MUST check this value before performing the protected work.
    /// A value of <c>false</c> means the caller did not obtain the lock
    /// (another node holds it, the timeout expired, or an infrastructure
    /// issue prevented acquisition). In every case Dispose still MUST be
    /// called; it is safe to call on an unacquired handle.
    /// </summary>
    bool IsAcquired { get; }

    /// <summary>
    /// A cancellation token that fires if the underlying lock is lost while
    /// still nominally held (for example, the SQL connection dies mid-hold
    /// because of a network drop or database failover). Long-running work
    /// SHOULD observe this token so it can unwind cooperatively when the
    /// coordination guarantee no longer applies. For unacquired handles or
    /// providers that cannot lose a held lock, this returns
    /// <see cref="CancellationToken.None"/>.
    /// </summary>
    CancellationToken LostToken { get; }
}
