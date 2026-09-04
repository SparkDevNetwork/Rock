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

using System;

using Rock.AI.Agent.Classes.Entity;
using Rock.Model;

namespace Rock.AI.Agent.Classes.Skills.ContentChannelSkill;

/// <summary>
/// Represents a single content channel item.
/// </summary>
internal class ContentChannelItemResult : EntityResultBase
{
    /// <summary>
    /// The title of the content channel item.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The type of content channel for this instance.
    /// </summary>
    public ContentChannelResult ContentChannel { get; set; }
    
    /// <summary>
    /// The HTML content of the content channel item.
    /// </summary>
    public string ContentAsHtml { get; set; }

    /// <summary>
    /// The approval status of the content channel item.
    /// </summary>
    public ContentChannelItemStatus? Status { get; set; }

    /// <summary>
    /// The person that approved the item.
    /// </summary>
    public PersonResult ApprovedByPerson { get; set; }

    /// <summary>
    /// The scheduled start date of the content channel item.
    /// </summary>
    public DateTime? StartDateTime { get; set; }

    /// <summary>
    /// The scheduled end date of the content channel item.
    /// </summary>
    public DateTime? ExpireDateTime { get; set; }

    /// <summary>
    /// The permanent URL link to view the content channel item
    /// on the website.
    /// </summary>
    public string PermanentLink { get; set; }
}
