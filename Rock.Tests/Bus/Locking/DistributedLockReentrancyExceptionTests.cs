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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Bus.Locking;

namespace Rock.Tests.Bus.Locking;

/// <summary>
/// Tests for <see cref="DistributedLockReentrancyException"/>.
/// </summary>
[TestClass]
public class DistributedLockReentrancyExceptionTests
{
    [TestMethod]
    public void Constructor_CapturesLockKey()
    {
        var ex = new DistributedLockReentrancyException( "Rock.Some.Marker:42" );

        Assert.AreEqual( "Rock.Some.Marker:42", ex.LockKey );
    }

    [TestMethod]
    public void Constructor_MessageMentionsLockKey()
    {
        // The message needs to name the key so log readers can see which
        // lock was re-acquired without having to look up the LockKey
        // property from a raw exception dump.
        var ex = new DistributedLockReentrancyException( "Rock.Some.Marker:42" );

        Assert.Contains( "Rock.Some.Marker:42" , ex.Message);
    }

    [TestMethod]
    public void IsInvalidOperationException()
    {
        // Callers who catch InvalidOperationException should also catch
        // the reentrancy variant. This is the base class chosen by the
        // spec so the reentrancy check surfaces the same way as other
        // "wrong state for this operation" errors in .NET.
        var ex = new DistributedLockReentrancyException( "key" );

        Assert.IsInstanceOfType( ex, typeof( InvalidOperationException ) );
    }
}
