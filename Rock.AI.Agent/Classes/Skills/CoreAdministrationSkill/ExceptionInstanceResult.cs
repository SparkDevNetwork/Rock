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

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;

/// <summary>
/// A single logged exception occurrence. The large fields, the stack trace and
/// request context, are not carried here; call GetException for one occurrence in
/// full. CreatedDateTime, inherited from the base, is when the exception occurred.
/// </summary>
internal class ExceptionInstanceResult : EntityResultBase
{
    /// <summary>
    /// The exception class, such as <c>System.NullReferenceException</c>.
    /// </summary>
    public string ExceptionType { get; set; }

    /// <summary>
    /// The exception message.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The application or object that raised the exception.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// The HTTP status code returned with the exception, when there was one.
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// The site the exception occurred on, when it occurred on a site.
    /// </summary>
    public KeyNameResult Site { get; set; }

    /// <summary>
    /// The page the exception occurred on, when it occurred on a page.
    /// </summary>
    public KeyNameResult Page { get; set; }

    /// <summary>
    /// Indicates that this exception has an inner exception. Call GetException to
    /// walk the inner chain.
    /// </summary>
    public bool HasInnerException { get; set; }
}
