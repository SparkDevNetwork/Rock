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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;

namespace Rock.Tests.Security;

/// <summary>
/// Plain unit tests for the <see cref="PersonSession"/> entity. Anything
/// that needs a real database lives in
/// <c>Rock.Tests.Integration.Security.PersonSessionTests</c>.
/// </summary>
[TestClass]
public class PersonSessionTests
{
    /// <summary>
    /// New <see cref="PersonSession"/> instances default to
    /// <see cref="PersonSession.IsActive"/> = <c>true</c>. Inverting this
    /// default would silently break every <c>Start*Session</c> path in
    /// <c>PersonSessionService</c> (none of them stamp the flag themselves).
    /// </summary>
    [TestMethod]
    public void IsActive_DefaultsToTrue_OnConstruction()
    {
        var session = new PersonSession();

        Assert.IsTrue( session.IsActive );
    }
}
