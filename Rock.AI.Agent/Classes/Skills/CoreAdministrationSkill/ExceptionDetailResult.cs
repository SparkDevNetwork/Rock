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

using System.Collections.Generic;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;

/// <summary>
/// A single logged exception in full detail, including its stack trace and the
/// chain of inner exceptions beneath it. CreatedDateTime, inherited from the base,
/// is when the exception occurred.
/// </summary>
internal class ExceptionDetailResult : EntityResultBase
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
    /// The stack trace captured when the exception occurred.
    /// </summary>
    public string StackTrace { get; set; }

    /// <summary>
    /// The relative URL of the page the exception occurred on.
    /// </summary>
    public string PageUrl { get; set; }

    /// <summary>
    /// The site the exception occurred on, when it occurred on a site.
    /// </summary>
    public KeyNameResult Site { get; set; }

    /// <summary>
    /// The page the exception occurred on, when it occurred on a page.
    /// </summary>
    public KeyNameResult Page { get; set; }

    /// <summary>
    /// The person who was logged in when the exception occurred, when known.
    /// </summary>
    public KeyNameResult CreatedByPerson { get; set; }

    /// <summary>
    /// The exception this one is an inner exception of, when it has a parent.
    /// </summary>
    public KeyNameResult ParentException { get; set; }

    /// <summary>
    /// The exceptions nested directly beneath this one. Each can be read in full
    /// with GetException to walk deeper into the chain.
    /// </summary>
    public List<KeyNameResult> InnerExceptions { get; set; }
}
