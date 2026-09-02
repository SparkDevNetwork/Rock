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

namespace Rock.AI.Agent.Classes.Skills.CommunicationSkill;

/// <summary>
/// A single communication template in full detail.
/// </summary>
/// <remarks>
/// The email message body is never returned. It is large and no authoring task
/// needs its contents; <see cref="MessageLength"/> tells a caller whether the
/// template has content without paying for it.
/// </remarks>
internal class CommunicationTemplateDetailResult : EntityResultBase
{
    /// <summary>
    /// The name of the template.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the template.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The category the template is filed under, or <c>null</c> when it is
    /// uncategorized.
    /// </summary>
    public KeyNameResult Category { get; set; }

    /// <summary>
    /// Indicates that the template is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates that the template is part of Rock's core configuration and cannot
    /// be deleted.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// The email subject line.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// The display name shown on the sending address.
    /// </summary>
    public string FromName { get; set; }

    /// <summary>
    /// The sending email address.
    /// </summary>
    public string FromEmail { get; set; }

    /// <summary>
    /// The reply-to email address.
    /// </summary>
    public string ReplyToEmail { get; set; }

    /// <summary>
    /// The carbon copy recipients.
    /// </summary>
    public string Cc { get; set; }

    /// <summary>
    /// The blind carbon copy recipients.
    /// </summary>
    public string Bcc { get; set; }

    /// <summary>
    /// The character count of the email message body. This tells a caller whether
    /// the template has email content without returning the body itself.
    /// </summary>
    public int MessageLength { get; set; }

    /// <summary>
    /// Indicates that the template defines an SMS message.
    /// </summary>
    public bool HasSmsMessage { get; set; }

    /// <summary>
    /// The system phone number SMS is sent from, when one is configured.
    /// </summary>
    public KeyNameResult SmsFromSystemPhoneNumber { get; set; }

    /// <summary>
    /// Indicates that the template defines a push notification.
    /// </summary>
    public bool HasPushMessage { get; set; }

    /// <summary>
    /// The push notification title.
    /// </summary>
    public string PushTitle { get; set; }
}
