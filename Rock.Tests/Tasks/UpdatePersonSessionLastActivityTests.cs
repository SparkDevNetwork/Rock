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

using Rock.Configuration;
using Rock.Enums.Security;
using Rock.Model;
using Rock.Tasks;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Tasks;

/// <summary>
/// Mocked-database unit tests for
/// <see cref="UpdatePersonSessionLastActivity"/>. Validates the throttle
/// re-check the task performs inside <c>Execute</c> against the database
/// value (the per-process throttle in <c>SendIfNeeded</c> is a separate,
/// in-memory short-circuit and is not exercised here).
/// </summary>
[TestClass]
public class UpdatePersonSessionLastActivityTests
{
    /// <summary>
    /// A message whose <c>LastActivityDateTime</c> falls inside the throttle
    /// window relative to the row's existing
    /// <c>LastActivityDateTime</c> must be a no-op. Repeated page hits do
    /// not produce repeated UPDATEs against the PersonSession row.
    /// </summary>
    [TestMethod]
    public void Execute_WithinThrottleWindow_DoesNotAdvanceLastActivityDateTime()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var existingActivity = new DateTime( 2026, 6, 9, 12, 0, 0 );
        var personSession = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            LastActivityDateTime = existingActivity,
        };
        rockContext.Set<PersonSession>().Add( personSession );

        // One minute later. Throttle window is 5 minutes, so this is well
        // inside the suppression window.
        var message = new UpdatePersonSessionLastActivity.Message
        {
            PersonSessionId = personSession.Id,
            LastActivityDateTime = existingActivity.AddMinutes( 1 ),
        };

        new UpdatePersonSessionLastActivity().Execute( message );

        Assert.AreEqual( existingActivity, personSession.LastActivityDateTime );
    }

    /// <summary>
    /// A message whose <c>LastActivityDateTime</c> falls outside the
    /// throttle window relative to the row's existing
    /// <c>LastActivityDateTime</c> must advance the row to the new value.
    /// </summary>
    [TestMethod]
    public void Execute_PastThrottleWindow_AdvancesLastActivityDateTime()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var existingActivity = new DateTime( 2026, 6, 9, 12, 0, 0 );
        var personSession = new PersonSession
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            PersonAliasId = 100,
            CreationSource = PersonSessionCreationSource.Component,
            IsActive = true,
            LastActivityDateTime = existingActivity,
        };
        rockContext.Set<PersonSession>().Add( personSession );

        // Ten minutes later. Throttle window is 5 minutes, so this is past
        // the suppression window and must produce an UPDATE.
        var newActivity = existingActivity.AddMinutes( 10 );
        var message = new UpdatePersonSessionLastActivity.Message
        {
            PersonSessionId = personSession.Id,
            LastActivityDateTime = newActivity,
        };

        new UpdatePersonSessionLastActivity().Execute( message );

        Assert.AreEqual( newActivity, personSession.LastActivityDateTime );
    }
}
