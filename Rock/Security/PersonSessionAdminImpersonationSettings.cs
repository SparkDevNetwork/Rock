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

namespace Rock.Security;

/// <summary>
/// Restore state persisted on a <c>PersonSession</c> with
/// <c>CreationSource = Impersonation</c>.
/// </summary>
internal class PersonSessionAdminImpersonationSettings
{
    /// <summary>
    /// The impersonator's prior <c>PersonSession.Guid</c>. Used by
    /// <c>EndImpersonationAndRestore</c> to revert to the admin's session.
    /// </summary>
    public Guid ImpersonatorPersonSessionGuid { get; set; }

    /// <summary>
    /// The impersonator's prior <c>InteractionSession.Guid</c>. Used by
    /// <c>EndImpersonationAndRestore</c> to re-attach the admin's
    /// pre-impersonation activity trail. The new <c>InteractionSession</c>
    /// created at impersonation start remains in the database as a historical
    /// row but is no longer the "current" session for the admin's browser
    /// after restore.
    /// </summary>
    public Guid ImpersonatorInteractionSessionGuid { get; set; }
}
