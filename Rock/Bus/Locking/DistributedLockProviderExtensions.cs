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
/// Convenience overloads on <see cref="IDistributedLockProvider"/> that let
/// callers specify the marker type as a generic parameter instead of a
/// runtime <see cref="Type"/>. Every overload delegates to the underlying
/// <see cref="IDistributedLockProvider.TryAcquire(Type, string, TimeSpan)"/>
/// method, so validation and behavior are identical.
/// </summary>
public static class DistributedLockProviderExtensions
{
    /// <summary>
    /// Attempts to acquire a distributed lock using <typeparamref name="T"/>
    /// as the marker type. See
    /// <see cref="IDistributedLockProvider.TryAcquire(Type, string, TimeSpan)"/>
    /// for full semantics.
    /// </summary>
    /// <typeparam name="T">
    /// A non-generic marker type. Constructed generics (for example
    /// <c>Dictionary&lt;string, int&gt;</c>) throw
    /// <see cref="ArgumentException"/> at the boundary.
    /// </typeparam>
    /// <param name="provider">The provider to acquire from.</param>
    /// <param name="resourceId">The resource identifier within the marker's namespace.</param>
    /// <param name="timeout">How long to wait for the lock.</param>
    public static ILockHandle TryAcquire<T>( this IDistributedLockProvider provider, string resourceId, TimeSpan timeout )
        where T : class
    {
        if ( provider == null )
        {
            throw new ArgumentNullException( nameof( provider ) );
        }

        return provider.TryAcquire( typeof( T ), resourceId, timeout );
    }

    /// <summary>
    /// Asynchronous form of
    /// <see cref="TryAcquire{T}(IDistributedLockProvider, string, TimeSpan)"/>.
    /// See <see cref="IDistributedLockProvider.TryAcquireAsync(Type, string, TimeSpan, CancellationToken)"/>
    /// for full semantics.
    /// </summary>
    public static Task<ILockHandle> TryAcquireAsync<T>( this IDistributedLockProvider provider, string resourceId, TimeSpan timeout, CancellationToken cancellationToken = default )
        where T : class
    {
        if ( provider == null )
        {
            throw new ArgumentNullException( nameof( provider ) );
        }

        return provider.TryAcquireAsync( typeof( T ), resourceId, timeout, cancellationToken );
    }
}
