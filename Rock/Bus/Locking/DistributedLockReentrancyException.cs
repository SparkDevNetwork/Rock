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

namespace Rock.Bus.Locking;

/// <summary>
/// Thrown when a caller attempts to acquire a distributed lock that is
/// already held by the same logical flow. Reentrancy is intentionally
/// rejected: silently succeeding on a re-acquire could mask coordination
/// bugs where a caller believes the second acquisition is a fresh
/// guarantee when it is really a no-op. If a caller genuinely needs the
/// same coordination scope twice, they should structure their code so
/// the outer acquisition covers both regions of protected work.
/// </summary>
public class DistributedLockReentrancyException : InvalidOperationException
{
    /// <summary>
    /// The fully-qualified lock key that the caller attempted to
    /// re-acquire. Provided for logging and debugging.
    /// </summary>
    public string LockKey { get; }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DistributedLockReentrancyException"/> class.
    /// </summary>
    /// <param name="lockKey">The lock key that was re-acquired.</param>
    public DistributedLockReentrancyException( string lockKey )
        : base( $"The distributed lock '{lockKey}' is already held by the current flow. Reentrancy is not supported." )
    {
        LockKey = lockKey;
    }
}
