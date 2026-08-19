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
/// A single system communication in full detail.
/// </summary>
/// <remarks>
/// The message body is never returned by any tool in this skill. It is large and
/// no authoring task needs its contents; <see cref="BodyLength"/> tells a caller
/// whether the template has content without paying for it.
/// </remarks>
internal class SystemCommunicationDetailResult : EntityResultBase
{
    /// <summary>
    /// The title of the system communication.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The email subject line.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// The sending email address.
    /// </summary>
    public string From { get; set; }

    /// <summary>
    /// The display name shown on the sending address.
    /// </summary>
    public string FromName { get; set; }

    /// <summary>
    /// The configured recipients, when the template targets fixed addresses.
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// The configured carbon copy recipients.
    /// </summary>
    public string Cc { get; set; }

    /// <summary>
    /// The configured blind carbon copy recipients.
    /// </summary>
    public string Bcc { get; set; }

    /// <summary>
    /// The category the communication is filed under, or <c>null</c> when it is
    /// uncategorized.
    /// </summary>
    public KeyNameResult Category { get; set; }

    /// <summary>
    /// Indicates that the communication is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates that the communication is part of Rock's core configuration and
    /// cannot be deleted.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Indicates that the template defines an SMS message in addition to email.
    /// </summary>
    public bool HasSmsMessage { get; set; }

    /// <summary>
    /// Indicates that the template defines a push notification in addition to
    /// email.
    /// </summary>
    public bool HasPushMessage { get; set; }

    /// <summary>
    /// The character count of the email body. This tells a caller whether the
    /// template has content without returning the body itself.
    /// </summary>
    public int BodyLength { get; set; }
}
