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
using Rock.Model;

namespace Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;

/// <summary>
/// The person entry block of a workflow form, which collects a real person rather
/// than plain attribute values and matches or creates the person record.
/// </summary>
/// <remarks>
/// <para>
/// Returned only when the form has person entry enabled, so a form without it costs
/// nothing. The serializer omits nulls, which is what makes that free.
/// </para>
/// <para>
/// Not an entity of its own. These are columns on <c>WorkflowActionForm</c>, grouped
/// here because they are configured and understood as one unit.
/// </para>
/// </remarks>
internal class WorkflowFormPersonEntryResult
{
    #region Presentation

    /// <summary>
    /// The heading shown above the person entry block.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Explanatory text shown beneath the heading.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Indicates that a horizontal rule is drawn beneath the heading.
    /// </summary>
    public bool IsHeadingSeparatorShown { get; set; }

    /// <summary>
    /// The section type controlling the block's visual treatment.
    /// </summary>
    public KeyNameResult SectionTypeValue { get; set; }

    /// <summary>
    /// Markup rendered above the person entry fields.
    /// </summary>
    public string PreHtml { get; set; }

    /// <summary>
    /// Indicates that <see cref="PreHtml"/> was clipped because it exceeded the length
    /// a tree read returns. Omitted when the value is complete.
    /// </summary>
    public bool? IsPreHtmlTruncated { get; set; }

    /// <summary>
    /// Markup rendered below the person entry fields.
    /// </summary>
    public string PostHtml { get; set; }

    /// <summary>
    /// Indicates that <see cref="PostHtml"/> was clipped. Omitted when the value is
    /// complete.
    /// </summary>
    public bool? IsPostHtmlTruncated { get; set; }

    #endregion

    #region Where the results land

    /// <summary>
    /// The workflow attribute the matched or created person is written to. Without
    /// this the block collects a person the workflow cannot reference.
    /// </summary>
    public KeyNameResult PersonAttribute { get; set; }

    /// <summary>
    /// The workflow attribute the spouse is written to, when spouse entry is shown.
    /// </summary>
    public KeyNameResult SpouseAttribute { get; set; }

    /// <summary>
    /// The workflow attribute the family group is written to.
    /// </summary>
    public KeyNameResult FamilyAttribute { get; set; }

    #endregion

    #region Which fields are asked for

    /// <summary>
    /// Whether the address is hidden, optional, or required.
    /// </summary>
    public WorkflowActionFormPersonEntryOption AddressOption { get; set; }

    /// <summary>
    /// Whether the birthdate is hidden, optional, or required.
    /// </summary>
    public WorkflowActionFormPersonEntryOption BirthdateOption { get; set; }

    /// <summary>
    /// Whether the email address is hidden, optional, or required.
    /// </summary>
    public WorkflowActionFormPersonEntryOption EmailOption { get; set; }

    /// <summary>
    /// Whether ethnicity is hidden, optional, or required.
    /// </summary>
    public WorkflowActionFormPersonEntryOption EthnicityOption { get; set; }

    /// <summary>
    /// Whether gender is hidden, optional, or required.
    /// </summary>
    public WorkflowActionFormPersonEntryOption GenderOption { get; set; }

    /// <summary>
    /// Whether marital status is hidden, optional, or required.
    /// </summary>
    public WorkflowActionFormPersonEntryOption MaritalStatusOption { get; set; }

    /// <summary>
    /// Whether the mobile phone number is hidden, optional, or required.
    /// </summary>
    public WorkflowActionFormPersonEntryOption MobilePhoneOption { get; set; }

    /// <summary>
    /// Whether race is hidden, optional, or required.
    /// </summary>
    public WorkflowActionFormPersonEntryOption RaceOption { get; set; }

    /// <summary>
    /// Whether the spouse fields are hidden, optional, or required.
    /// </summary>
    public WorkflowActionFormPersonEntryOption SpouseOption { get; set; }

    /// <summary>
    /// Whether the SMS opt-in checkbox is shown. A different enum from the others,
    /// because opt-in can only be shown or hidden, never required.
    /// </summary>
    public WorkflowActionFormShowHideOption SmsOptInOption { get; set; }

    /// <summary>
    /// The label shown above the spouse fields.
    /// </summary>
    public string SpouseLabel { get; set; }

    #endregion

    #region Behavior

    /// <summary>
    /// Indicates that the block is prefilled from the signed-in person.
    /// </summary>
    public bool IsAutofillCurrentPersonEnabled { get; set; }

    /// <summary>
    /// Indicates that the campus picker is shown.
    /// </summary>
    public bool IsCampusVisible { get; set; }

    /// <summary>
    /// Indicates that the campus picker offers inactive campuses.
    /// </summary>
    /// <remarks>
    /// The one person entry setting that is not a column on the form. It lives in the
    /// form's additional settings JSON, and Rock reads an absent value as <c>true</c>.
    /// </remarks>
    public bool IsInactiveCampusIncluded { get; set; }

    /// <summary>
    /// Indicates that the whole block is hidden when the person is already signed in.
    /// </summary>
    public bool IsHiddenIfCurrentPersonKnown { get; set; }

    #endregion

    #region Values applied to a created person

    /// <summary>
    /// The connection status given to a person this block creates.
    /// </summary>
    public KeyNameResult ConnectionStatusValue { get; set; }

    /// <summary>
    /// The record status given to a person this block creates.
    /// </summary>
    public KeyNameResult RecordStatusValue { get; set; }

    /// <summary>
    /// The record source recorded against a person this block creates.
    /// </summary>
    public KeyNameResult RecordSourceValue { get; set; }

    /// <summary>
    /// The location type an entered address is saved as.
    /// </summary>
    public KeyNameResult AddressTypeValue { get; set; }

    /// <summary>
    /// Limits the campus picker to campuses of this status.
    /// </summary>
    public KeyNameResult CampusStatusValue { get; set; }

    /// <summary>
    /// Limits the campus picker to campuses of this type.
    /// </summary>
    public KeyNameResult CampusTypeValue { get; set; }

    #endregion
}
