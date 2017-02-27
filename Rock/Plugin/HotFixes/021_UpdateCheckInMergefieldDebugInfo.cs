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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 21, "1.6.2" )]
    public class UpdateCheckInMergefieldDebugInfo : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
UPDATE [DefinedType]
SET [helptext] = '
Label merge fields are defined with a liquid syntax. Click the ''Show Merge Fields'' button below to view the available merge fields.
<p>
    <a data-toggle=''collapse''  href=''#collapseMergeFields'' class=''btn btn-action btn-xs''>Show/Hide Merge Fields</a>
</p>
<div id=''collapseMergeFields'' class=''panel-collapse collapse''>
<div class=''alert alert-info lava-debug''>
    <div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-e3fc71fd-4c95-44f7-bef8-29eabc81f76c''><h5 class=''panel-title pull-left''>Global Attribute</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-e3fc71fd-4c95-44f7-bef8-29eabc81f76c'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>Global attributes should be accessed using <code>{{ ''Global'' | Attribute:''[AttributeKey]'' }}</code>. Find out more about using Global Attributes in Lava at <a href=''http://www.rockrms.com/lava/globalattributes'' target=''_blank''>rockrms.com/lava/globalattributes</a>.</p>
                <ul>
                    <li><span class=''lava-debug-key''>ContentFiletypeBlacklist</span> <span class=''lava-debug-value''> - ascx, ashx, aspx, ascx.cs, ashx.cs, aspx.cs,...</span></li>
                    <li><span class=''lava-debug-key''>ContentImageFiletypeWhitelist</span> <span class=''lava-debug-value''> - jpg,png,gif,bmp,svg</span></li>
                    <li><span class=''lava-debug-key''>core.GradeLabel</span> <span class=''lava-debug-value''> - Grade</span></li>
                    <li><span class=''lava-debug-key''>core.LavaSupportLevel</span> <span class=''lava-debug-value''> - Legacy</span></li>
                    <li><span class=''lava-debug-key''>core.ValidUsernameCaption</span> <span class=''lava-debug-value''> - It must only contain letters, numbers, +, -,...</span></li>
                    <li><span class=''lava-debug-key''>core.ValidUsernameRegularExpression</span> <span class=''lava-debug-value''> - ^[A-Za-z0-9+.@_-]{3,128}$</span></li>
                    <li><span class=''lava-debug-key''>CurrencySymbol</span> <span class=''lava-debug-value''> - $</span></li>
                    <li><span class=''lava-debug-key''>DefaultEnabledLavaCommands</span> <span class=''lava-debug-value''> - RockEntity</span></li>
                    <li><span class=''lava-debug-key''>EmailExceptionsFilter</span> <span class=''lava-debug-value''> - HTTP_USER_AGENT: Googlebot</span></li>
                    <li><span class=''lava-debug-key''>EmailExceptionsList</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key''>EmailFooter</span> <span class=''lava-debug-value''>
                            -
                            ...
                        </span>
                    </li>
                    <li>
                        <span class=''lava-debug-key''>EmailHeader</span> <span class=''lava-debug-value''>
                            -
                            &lt;!DOCTYPE HTML PUBLIC &quot;-//W3C//DTD XHTML 1.0...
                        </span>
                    </li>
                    <li><span class=''lava-debug-key''>EmailHeaderLogo</span> <span class=''lava-debug-value''> - assets/images/email-header.jpg</span></li>
                    <li><span class=''lava-debug-key''>EnableAuditing</span> <span class=''lava-debug-value''> - No</span></li>
                    <li><span class=''lava-debug-key''>GoogleAPIKey</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GradeTransitionDate</span> <span class=''lava-debug-value''> - 6/1</span></li>
                    <li><span class=''lava-debug-key''>InternalApplicationRoot</span> <span class=''lava-debug-value''> - http://rock.organization.com/</span></li>
                    <li><span class=''lava-debug-key''>JobPulse</span> <span class=''lava-debug-value''> - 7/13/2012 4:58:30 PM</span></li>
                    <li><span class=''lava-debug-key''>Log404AsException</span> <span class=''lava-debug-value''> - No</span></li>
                    <li><span class=''lava-debug-key''>OrganizationAbbreviation</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key''>OrganizationAddress</span> <span class=''lava-debug-value''>
                            - 3120 W Cholla St
                            Phoenix, AZ 85029
                        </span>
                    </li>
                    <li><span class=''lava-debug-key''>OrganizationEmail</span> <span class=''lava-debug-value''> - info@organizationname.com</span></li>
                    <li><span class=''lava-debug-key''>OrganizationName</span> <span class=''lava-debug-value''> - Rock Solid Church</span></li>
                    <li><span class=''lava-debug-key''>OrganizationPhone</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>OrganizationWebsite</span> <span class=''lava-debug-value''> - www.organization.com</span></li>
                    <li><span class=''lava-debug-key''>PasswordRegexFriendlyDescription</span> <span class=''lava-debug-value''> - Invalid Password. Password must be at least 6...</span></li>
                    <li><span class=''lava-debug-key''>PasswordRegularExpression</span> <span class=''lava-debug-value''> - \w{6,255}</span></li>
                    <li><span class=''lava-debug-key''>PreferredEmailLinkType</span> <span class=''lava-debug-value''> - New Communication</span></li>
                    <li><span class=''lava-debug-key''>PublicApplicationRoot</span> <span class=''lava-debug-value''> - http://www.organization.com/</span></li>
                    <li><span class=''lava-debug-key''>SupportInternationalAddresses</span> <span class=''lava-debug-value''> - No</span></li>
                    <li><span class=''lava-debug-key''>UpdateServerUrl</span> <span class=''lava-debug-value''> - http://update.rockrms.com/F/rock/api/v2/</span></li>
                </ul>
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-abbdb3d7-1635-4d28-a23b-54aef048931d''><h5 class=''panel-title pull-left''>Page Parameter</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-abbdb3d7-1635-4d28-a23b-54aef048931d'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>PageParameter properties can be accessed by <code>{{ PageParameter.[PropertyKey] }}</code>.</p>
                <ul>
                    <li><span class=''lava-debug-key''>PageId</span> <span class=''lava-debug-value''> - 447</span></li>
                </ul>
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-a15744ad-bbce-434d-a7b2-7766219c15bc''><h5 class=''panel-title pull-left''>Current Person</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-a15744ad-bbce-434d-a7b2-7766219c15bc'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>CurrentPerson properties can be accessed by <code>{{ CurrentPerson.[PropertyKey] }}</code>. Find out more about using ''Person'' fields in Lava at <a href=''http://www.rockrms.com/lava/person'' target=''_blank''>rockrms.com/lava/person</a>.</p>
                <ul>
                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>RecordTypeValueId</span> <span class=''lava-debug-value''> - 1</span></li>
                    <li><span class=''lava-debug-key''>RecordStatusValueId</span> <span class=''lava-debug-value''> - 3</span></li>
                    <li><span class=''lava-debug-key''>RecordStatusLastModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>RecordStatusReasonValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ConnectionStatusValueId</span> <span class=''lava-debug-value''> - 66</span></li>
                    <li><span class=''lava-debug-key''>ReviewReasonValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IsDeceased</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>TitleValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>FirstName</span> <span class=''lava-debug-value''> - Admin</span></li>
                    <li><span class=''lava-debug-key''>NickName</span> <span class=''lava-debug-value''> - Admin</span></li>
                    <li><span class=''lava-debug-key''>MiddleName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>LastName</span> <span class=''lava-debug-value''> - Admin</span></li>
                    <li><span class=''lava-debug-key''>SuffixValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>PhotoId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>BirthDay</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>BirthMonth</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>BirthYear</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Gender</span> <span class=''lava-debug-value''> - Unknown</span></li>
                    <li><span class=''lava-debug-key''>MaritalStatusValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>AnniversaryDate</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GraduationYear</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GivingGroupId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GivingId</span> <span class=''lava-debug-value''> - P1</span></li>
                    <li><span class=''lava-debug-key''>GivingLeaderId</span> <span class=''lava-debug-value''> - 1</span></li>
                    <li><span class=''lava-debug-key''>Email</span> <span class=''lava-debug-value''> - admin@organization.com</span></li>
                    <li><span class=''lava-debug-key''>IsEmailActive</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>EmailNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>EmailPreference</span> <span class=''lava-debug-value''> - EmailAllowed</span></li>
                    <li><span class=''lava-debug-key''>ReviewReasonNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>InactiveReasonNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>SystemNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ViewedCount</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>PrimaryAlias</span>
                        <ul>
                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PersonId</span> <span class=''lava-debug-value''> - 1</span></li>
                            <li><span class=''lava-debug-key''>AliasPersonId</span> <span class=''lava-debug-value''> - 1</span></li>
                            <li><span class=''lava-debug-key''>AliasPersonGuid</span> <span class=''lava-debug-value''> - ad28da19-4af1-408f-9090-2672f8376f27</span></li>
                            <li><span class=''lava-debug-key''>Person</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 10</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 259629a1-3116-4780-a179-6612ca2e237a</span></li>
                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 226</span></li>
                            <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.PersonAlias</span></li>
                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAANqUp1aI5gmk8C!2fT!2fFrkcIVnpFhUg6VdkQoYh0s...</span></li>
                        </ul>
                    </li>
                    <li><span class=''lava-debug-key''>PrimaryAliasId</span> <span class=''lava-debug-value''> - 10</span></li>
                    <li><span class=''lava-debug-key''>FullName</span> <span class=''lava-debug-value''> - Admin Admin</span></li>
                    <li><span class=''lava-debug-key''>BirthdayDayOfWeek</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>BirthdayDayOfWeekShort</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>PhotoUrl</span> <span class=''lava-debug-value''> - /Assets/Images/person-no-photo-male.svg?</span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Users</span>
                        {<ul>
                            <li>
                                [0]
                                <ul>
                                    <li><span class=''lava-debug-key''>EntityTypeId</span> <span class=''lava-debug-value''> - 27</span></li>
                                    <li><span class=''lava-debug-key''>UserName</span> <span class=''lava-debug-value''> - admin</span></li>
                                    <li><span class=''lava-debug-key''>IsConfirmed</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>LastActivityDateTime</span> <span class=''lava-debug-value''> - 2/21/2017 1:10:26 PM</span></li>
                                    <li><span class=''lava-debug-key''>LastLoginDateTime</span> <span class=''lava-debug-value''> - 2/21/2017 1:10:21 PM</span></li>
                                    <li><span class=''lava-debug-key''>LastPasswordChangedDateTime</span> <span class=''lava-debug-value''> - 1/23/2012 3:43:25 AM</span></li>
                                    <li><span class=''lava-debug-key''>IsOnLine</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>IsLockedOut</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>IsPasswordChangeRequired</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>LastLockedOutDateTime</span> <span class=''lava-debug-value''> - 12/15/2011 2:45:54 AM</span></li>
                                    <li><span class=''lava-debug-key''>FailedPasswordAttemptCount</span> <span class=''lava-debug-value''> - 1</span></li>
                                    <li><span class=''lava-debug-key''>FailedPasswordAttemptWindowStartDateTime</span> <span class=''lava-debug-value''> - 6/7/2012 3:25:06 PM</span></li>
                                    <li><span class=''lava-debug-key''>LastPasswordExpirationWarningDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ApiKey</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>PersonId</span> <span class=''lava-debug-value''> - 1</span></li>
                                    <li><span class=''lava-debug-key''>ConfirmationCode</span> <span class=''lava-debug-value''> - EAAAANxRURJOQeLeemJEkLb6SYYkSAkjw4lAyeNN6N+SmZL...</span></li>
                                    <li><span class=''lava-debug-key''>ConfirmationCodeEncoded</span> <span class=''lava-debug-value''> - EAAAADgnlP8C%2fQc7r19xs6k7gVuIiODyAEzIQI%2bGXnK...</span></li>
                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - 3/19/2011 7:34:15 AM</span></li>
                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - 2/21/2017 1:11:20 PM</span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 1</span></li>
                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 7e10a764-ef6b-431f-87c7-861053c84131</span></li>
                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 112</span></li>
                                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.UserLogin</span></li>
                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAKI02!2b94hxbNJjTp1s16Lrd!2b3Bm8MW5sbYR1qoV...</span></li>
                                </ul>
                            </li>
                        </ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>PhoneNumbers</span>
                        {<ul></ul>}
                    </li>
                    <li><span class=''lava-debug-key''>MaritalStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>ConnectionStatusValue</span>
                        <ul>
                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - 4</span></li>
                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 2</span></li>
                            <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - Visitor</span></li>
                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - Used when a person first enters through your...</span></li>
                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 66</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - b91ba046-bc1e-400c-b85d-638c1f4e0ce2</span></li>
                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 31</span></li>
                            <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.DefinedValue</span></li>
                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAACZMfK7gieLjckFidWigq9Fe7qkgBjdjhA!2f9XooGm...</span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ CurrentPerson.ConnectionStatusValue | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                                <ul>
                                    <li><span class=''lava-debug-key''>Color</span> <span class=''lava-debug-value''> - #afd074</span></li>
                                </ul>
                            </li>
                        </ul>
                    </li>
                    <li><span class=''lava-debug-key''>ReviewReasonValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>RecordStatusValue</span>
                        <ul>
                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                            <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - 2</span></li>
                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                            <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - Active</span></li>
                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - Denotes an individual that is actively...</span></li>
                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 3</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 618f906c-c33d-4fa3-8aef-e58cb7b63f1e</span></li>
                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 31</span></li>
                            <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.DefinedValue</span></li>
                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAKRrgOicT9qL2iqrU!2fTHQzPNZNP2x9AxulfN8QsWa...</span></li>
                        </ul>
                    </li>
                    <li><span class=''lava-debug-key''>RecordStatusReasonValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>RecordTypeValue</span>
                        <ul>
                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                            <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - 1</span></li>
                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                            <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - Person</span></li>
                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - A Person Record</span></li>
                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 1</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 36cf10d6-c695-413d-8e7c-4546efef385e</span></li>
                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 31</span></li>
                            <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.DefinedValue</span></li>
                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAABCkrXN6NfICXj4v6ty5dzOR8qLExSal3TVGYYkoi5c...</span></li>
                        </ul>
                    </li>
                    <li><span class=''lava-debug-key''>SuffixValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>TitleValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Photo</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>BirthDate</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>DaysUntilBirthday</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Age</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>NextBirthDay</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>DaysToBirthday</span> <span class=''lava-debug-value''> - 2147483647</span></li>
                    <li><span class=''lava-debug-key''>NextAnniversary</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GradeOffset</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>HasGraduated</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GradeFormatted</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ImpersonationParameter</span> <span class=''lava-debug-value''> - rckipid=EAAAAAlY2dmZ4pTKxdCAKIP1rqrmabZx%2bGVJe...</span></li>
                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 1</span></li>
                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - ad28da19-4af1-408f-9090-2672f8376f27</span></li>
                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 15</span></li>
                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.Person</span></li>
                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAFnZqlnlpsW!2bwbhVtsqT!2bDCcvICA8VZvH03lFdV...</span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ CurrentPerson | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                        <ul>
                            <li><span class=''lava-debug-key''>BaptismDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>AbilityLevel</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Allergy</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>BaptizedHere</span> <span class=''lava-debug-value''> - No</span></li>
                            <li><span class=''lava-debug-key''>LegalNotes</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PreviousChurch</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>FirstVisit</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>SecondVisit</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>SourceofVisit</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>School</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Employer</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Position</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>MembershipDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Facebook</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Twitter</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Instagram</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>AdaptiveD</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>AdaptiveI</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>AdaptiveS</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>AdaptiveC</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>NaturalD</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>NaturalI</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>NaturalS</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>NaturalC</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>LastSaveDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>BackgroundChecked</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>BackgroundCheckDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>BackgroundCheckResult</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>BackgroundCheckDocument</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PersonalityType</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>com.sparkdevnetwork.DLNumber</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>LastDiscRequestDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_CurrentlyAnEra</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraStartDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraEndDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraFirstCheckin</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraLastCheckin</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraLastGave</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraFirstGave</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_TimesCheckedIn16Wks</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraTimesGiven52Wks</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>core_EraTimesGiven6Wks</span> <span class=''lava-debug-value''> - </span></li>
                        </ul>
                    </li>
                </ul>
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-43d2d1b7-86ae-46de-8e39-4520cdd78596''><h5 class=''panel-title pull-left''>Campuses</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-43d2d1b7-86ae-46de-8e39-4520cdd78596'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>Campuses properties can be accessed by <code>{% for campus in Campuses %}{{ campus.[PropertyKey] }}{% endfor %}</code>.</p>
                {<ul>
                    <li>
                        [0]
                        <ul>
                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Main Campus</span></li>
                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - True</span></li>
                            <li><span class=''lava-debug-key''>ShortCode</span> <span class=''lava-debug-value''> - MAIN</span></li>
                            <li><span class=''lava-debug-key''>Url</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>LocationId</span> <span class=''lava-debug-value''> - 2</span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-1''>Location</span>
                                <ul>
                                    <li><span class=''lava-debug-key''>Street1</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Street2</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>City</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>State</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>PostalCode</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Country</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Latitude</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Longitude</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ImageUrl</span> <span class=''lava-debug-value''> - </span></li>
                                </ul>
                            </li>
                            <li><span class=''lava-debug-key''>PhoneNumber</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>LeaderPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>RawServiceTimes</span> <span class=''lava-debug-value''> - </span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-1''>ServiceTimes</span>
                                {<ul></ul>}
                            </li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 1</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 76882ae3-1ce8-42a6-a2b6-8c0b29cf8cf8</span></li>
                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                        </ul>
                    </li>
                </ul>}
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-2b418e1b-6277-4dd1-aabf-6cab0cb8087e''><h5 class=''panel-title pull-left''>Location</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-2b418e1b-6277-4dd1-aabf-6cab0cb8087e'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>Location properties can be accessed by <code>{{ Location.[PropertyKey] }}</code>.</p>
                <ul>
                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                    <li><span class=''lava-debug-key''>Locations</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ParentLocationId</span> <span class=''lava-debug-value''> - 3</span></li>
                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Outpost Room</span></li>
                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>LocationTypeValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GeoPoint</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GeoFence</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Street1</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Street2</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>City</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>County</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>State</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Country</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>PostalCode</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Barcode</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>AssessorParcelId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>StandardizeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>StandardizeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>StandardizeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>StandardizedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GeocodeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GeocodeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GeocodeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GeocodedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IsGeoPointLocked</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>PrinterDeviceId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ImageId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>SoftRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>FirmRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IsNamedLocation</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>LocationTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>ChildLocations</span>
                        {<ul></ul>}
                    </li>
                    <li><span class=''lava-debug-key''>PrinterDevice</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Image</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>FormattedAddress</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>FormattedHtmlAddress</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Latitude</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Longitude</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GeoFenceCoordinates</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Distance</span> <span class=''lava-debug-value''> - 0</span></li>
                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 9</span></li>
                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - f9f33251-f829-4618-904e-42a1eda2b58e</span></li>
                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 93</span></li>
                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.Location</span></li>
                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAGoV7JnyfxU8sMW0WR5DTF5FZZUBkleZn5vH4NtO8ni...</span></li>
                </ul>
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-84709da5-f359-4303-bb36-64b824f20790''><h5 class=''panel-title pull-left''>Group</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-84709da5-f359-4303-bb36-64b824f20790'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>Group properties can be accessed by <code>{{ Group.[PropertyKey] }}</code>.</p>
                <ul>
                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Locations</span>
                        {<ul>
                            <li>
                                [0]
                                <ul>
                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                                    <li><span class=''lava-debug-key''>Locations</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ParentLocationId</span> <span class=''lava-debug-value''> - 3</span></li>
                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Outpost Room</span></li>
                                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>LocationTypeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GeoPoint</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GeoFence</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Street1</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Street2</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>City</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>County</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>State</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Country</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>PostalCode</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Barcode</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>AssessorParcelId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>StandardizeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>StandardizeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>StandardizeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>StandardizedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GeocodeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GeocodeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GeocodeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GeocodedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>IsGeoPointLocked</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>PrinterDeviceId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ImageId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>SoftRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>FirmRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>IsNamedLocation</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>LocationTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>ChildLocations</span>
                                        {<ul></ul>}
                                    </li>
                                    <li><span class=''lava-debug-key''>PrinterDevice</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Image</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>FormattedAddress</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>FormattedHtmlAddress</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Latitude</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Longitude</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GeoFenceCoordinates</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Distance</span> <span class=''lava-debug-value''> - 0</span></li>
                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 9</span></li>
                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - f9f33251-f829-4618-904e-42a1eda2b58e</span></li>
                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 93</span></li>
                                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.Location</span></li>
                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAO2b0JAMDlPpIQtxfRXf7iH4UZdpT1B8!2ftya1!2f!...</span></li>
                                </ul>
                            </li>
                        </ul>}
                    </li>
                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>ParentGroupId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GroupTypeId</span> <span class=''lava-debug-value''> - 20</span></li>
                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ScheduleId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Grades 4-6</span></li>
                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IsSecurityRole</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 6</span></li>
                    <li><span class=''lava-debug-key''>AllowGuests</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>WelcomeSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ExitSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>SyncDataViewId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>AddUserAccountsDuringSync</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>MustMeetRequirementsToAddMember</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IsPublic</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>GroupCapacity</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplateId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GroupType</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Campus</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Schedule</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>WelcomeSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ExitSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>SyncDataView</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplate</span> <span class=''lava-debug-value''> - </span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Groups</span>
                        {<ul></ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Members</span>
                        {<ul></ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>GroupLocations</span>
                        {<ul></ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>GroupRequirements</span>
                        {<ul></ul>}
                    </li>
                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 30</span></li>
                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 42c408ce-3d69-4d7d-b9ea-41087a8945a6</span></li>
                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 16</span></li>
                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.Group</span></li>
                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAAnh8Ija3Kr11a3cB7!2fkfr8DnBZ38sUBz9rPd9gUX...</span></li>
                </ul>
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-c30b2435-0b35-4fb1-82ea-e218bf397ae1''><h5 class=''panel-title pull-left''>Person</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-c30b2435-0b35-4fb1-82ea-e218bf397ae1'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>Person properties can be accessed by <code>{{ Person.[PropertyKey] }}</code>.</p>
                <ul>
                    <li><span class=''lava-debug-key''>FamilyMember</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                    <li><span class=''lava-debug-key''>FirstTime</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>SecurityCode</span> <span class=''lava-debug-value''> - 28G</span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>GroupTypes</span>
                        {<ul>
                            <li>
                                [0]
                                <ul>
                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Groups</span>
                                        {<ul>
                                            <li>
                                                [0]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Locations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ScheduleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsSecurityRole</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowGuests</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>WelcomeSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ExitSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SyncDataViewId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AddUserAccountsDuringSync</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>MustMeetRequirementsToAddMember</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsPublic</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacity</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplateId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Campus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Schedule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>WelcomeSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ExitSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SyncDataView</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplate</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Groups</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Members</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupRequirements</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                        </ul>}
                                    </li>
                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Elementary Area</span></li>
                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 39</span></li>
                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Kiosk</span></li>
                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 2</span></li>
                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - 17</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>InheritedGroupType</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Check in by Grade</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                                            <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                                            <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 36</span></li>
                                            <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Default</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                                            <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - 15</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>InheritedGroupType</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - 145</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>GroupTypePurposeValue</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>Roles</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>GroupScheduleExclusions</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>ChildGroupTypes</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>ParentGroupTypes</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>LocationTypeValues</span>
                                                {<ul></ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 17</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 4f9565a7-dd5a-41c3-b4e8-13f0b872b10b</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                        </ul>
                                    </li>
                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Roles</span>
                                        {<ul>
                                            <li>[0] <span class=''lava-debug-value''> - Member</span></li>
                                        </ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>GroupScheduleExclusions</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>ChildGroupTypes</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>ParentGroupTypes</span>
                                        {<ul>
                                            <li>
                                                [0]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li>
                                                        <span class=''lava-debug-key lava-debug-section level-1''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ Person.GroupTypes.ParentGroupTypes | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                                                        <ul>
                                                            <li><span class=''lava-debug-key''>core_checkin_CheckInType</span> <span class=''lava-debug-value''> - Family</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_EnableManagerOption</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_EnableOverride</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_SecurityCodeLength</span> <span class=''lava-debug-value''> - 3</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_ReuseSameCode</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_UseSameOptions</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_SearchType</span> <span class=''lava-debug-value''> - Phone Number</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_RegularExpressionFilter</span> <span class=''lava-debug-value''> - </span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_MaxSearchResults</span> <span class=''lava-debug-value''> - 100</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_MinimumPhoneSearchLength</span> <span class=''lava-debug-value''> - 4</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_MaximumPhoneSearchLength</span> <span class=''lava-debug-value''> - 10</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_PhoneSearchType</span> <span class=''lava-debug-value''> - Ends With</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_RefreshInterval</span> <span class=''lava-debug-value''> - 10</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_AgeRequired</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_DisplayLocationCount</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_AutoSelectDaysBack</span> <span class=''lava-debug-value''> - 10</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_GradeRequired</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_HidePhotos</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_PreventDuplicateCheckin</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_PreventInactivePeople</span> <span class=''lava-debug-value''> - No</span></li>
                                                        </ul>
                                                    </li>
                                                </ul>
                                            </li>
                                        </ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>LocationTypeValues</span>
                                        {<ul></ul>}
                                    </li>
                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 20</span></li>
                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - e3c8f7d6-5ceb-43bb-802f-66c3e734049e</span></li>
                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                </ul>
                            </li>
                            <li>
                                [1]
                                <ul>
                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Groups</span>
                                        {<ul>
                                            <li>
                                                [0]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Locations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ScheduleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsSecurityRole</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowGuests</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>WelcomeSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ExitSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SyncDataViewId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AddUserAccountsDuringSync</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>MustMeetRequirementsToAddMember</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsPublic</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacity</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplateId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Campus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Schedule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>WelcomeSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ExitSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SyncDataView</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplate</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Groups</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Members</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupRequirements</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                        </ul>}
                                    </li>
                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Serving Team</span></li>
                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - Used to track groups that serve in the...</span></li>
                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 19</span></li>
                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - fa fa-clock-o</span></li>
                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Kiosk</span></li>
                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - 29</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>InheritedGroupType</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Check In</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - A base group type that can be inherited from...</span></li>
                                            <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                                            <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                                            <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 42</span></li>
                                            <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Default</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                                            <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - 145</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>GroupTypePurposeValue</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>Roles</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>GroupScheduleExclusions</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>ChildGroupTypes</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>ParentGroupTypes</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>LocationTypeValues</span>
                                                {<ul></ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 29</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 6e7ad783-7614-4721-abc1-35842113ef59</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                        </ul>
                                    </li>
                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - Named</span></li>
                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - 184</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>GroupTypePurposeValue</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - 31</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 2</span></li>
                                            <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - Serving Area</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - Used to denote group types that count as serving</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>DefinedType</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FieldTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CategoryId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Category</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FieldType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 184</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 36a554ce-7815-41b9-a435-93f3d52a2828</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                        </ul>
                                    </li>
                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Roles</span>
                                        {<ul>
                                            <li>[0] <span class=''lava-debug-value''> - Leader</span></li>
                                            <li>[1] <span class=''lava-debug-value''> - Member</span></li>
                                        </ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>GroupScheduleExclusions</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>ChildGroupTypes</span>
                                        {<ul>
                                            <li>
                                                [0]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li>
                                                        <span class=''lava-debug-key lava-debug-section level-1''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ Person.GroupTypes.ChildGroupTypes | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                                                        <ul>
                                                            <li><span class=''lava-debug-key''>NameTag</span> <span class=''lava-debug-value''> - &lt;a...</span></li>
                                                        </ul>
                                                    </li>
                                                </ul>
                                            </li>
                                        </ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>ParentGroupTypes</span>
                                        {<ul>
                                            <li>
                                                [0]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li>
                                                        <span class=''lava-debug-key lava-debug-section level-1''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ Person.GroupTypes.ParentGroupTypes | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                                                        <ul>
                                                            <li><span class=''lava-debug-key''>NameTag</span> <span class=''lava-debug-value''> - &lt;a...</span></li>
                                                        </ul>
                                                    </li>
                                                </ul>
                                            </li>
                                            <li>
                                                [1]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li>
                                                        <span class=''lava-debug-key lava-debug-section level-1''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ Person.GroupTypes.ParentGroupTypes | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                                                        <ul>
                                                            <li><span class=''lava-debug-key''>core_checkin_CheckInType</span> <span class=''lava-debug-value''> - Individual</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_EnableManagerOption</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_EnableOverride</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_SecurityCodeLength</span> <span class=''lava-debug-value''> - 3</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_ReuseSameCode</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_UseSameOptions</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_SearchType</span> <span class=''lava-debug-value''> - Phone Number</span></li>
                                                            <li><span class=''lava-debug-key lava-debug-section level-2''>core_checkin_RegularExpressionFilter</span> </li>
                                                            <li><span class=''lava-debug-key''>core_checkin_MaxSearchResults</span> <span class=''lava-debug-value''> - 100</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_MinimumPhoneSearchLength</span> <span class=''lava-debug-value''> - 4</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_MaximumPhoneSearchLength</span> <span class=''lava-debug-value''> - 10</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_PhoneSearchType</span> <span class=''lava-debug-value''> - Ends With</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_RefreshInterval</span> <span class=''lava-debug-value''> - 10</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_AgeRequired</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_DisplayLocationCount</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_AutoSelectDaysBack</span> <span class=''lava-debug-value''> - 10</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_GradeRequired</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_HidePhotos</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_PreventDuplicateCheckin</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_PreventInactivePeople</span> <span class=''lava-debug-value''> - No</span></li>
                                                        </ul>
                                                    </li>
                                                </ul>
                                            </li>
                                        </ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>LocationTypeValues</span>
                                        {<ul>
                                            <li>
                                                [0]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li>
                                                        <span class=''lava-debug-key lava-debug-section level-1''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ Person.GroupTypes.LocationTypeValues | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                                                        <ul>
                                                            <li><span class=''lava-debug-key''>AllowMapSelection</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>AllowNamedSelection</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>AllowAddressSelection</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                        </ul>
                                                    </li>
                                                </ul>
                                            </li>
                                        </ul>}
                                    </li>
                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 23</span></li>
                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 2c42b2d4-1c5f-4ad5-a9ad-08631b872ac4</span></li>
                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                </ul>
                            </li>
                        </ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>SelectedOptions</span>
                        {<ul>
                            <li>
                                [0]
                                <ul>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Schedule</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>StartTime</span> <span class=''lava-debug-value''> - 2/21/2017 12:01:00 AM</span></li>
                                            <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - 4:30 (test)</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                            <li>
                                                <span class=''lava-debug-key''>iCalendarContent</span> <span class=''lava-debug-value''>
                                                    - BEGIN:VCALENDAR
                                                    BEGIN:VEVENT
                                                    DTEND:20130501T2...
                                                </span>
                                            </li>
                                            <li><span class=''lava-debug-key''>CheckInStartOffsetMinutes</span> <span class=''lava-debug-value''> - 0</span></li>
                                            <li><span class=''lava-debug-key''>CheckInEndOffsetMinutes</span> <span class=''lava-debug-value''> - 1439</span></li>
                                            <li><span class=''lava-debug-key''>EffectiveStartDate</span> <span class=''lava-debug-value''> - 5/1/2013 12:00:00 AM</span></li>
                                            <li><span class=''lava-debug-key''>EffectiveEndDate</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>WeeklyDayOfWeek</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>WeeklyTimeOfDay</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CategoryId</span> <span class=''lava-debug-value''> - 50</span></li>
                                            <li><span class=''lava-debug-key''>Category</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>FriendlyScheduleText</span> <span class=''lava-debug-value''> - Daily at 12:01 AM</span></li>
                                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 6</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - a5c81078-eb8c-46aa-bb91-1e2ba8ba76ae</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 54</span></li>
                                            <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.Schedule</span></li>
                                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAANr!2bLJFnLvhDrNIo1GpqG9!2fNzYYAx66zw9y14gP...</span></li>
                                        </ul>
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>GroupType</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>Groups</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Serving Team</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - Used to track groups that serve in the...</span></li>
                                            <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                                            <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                                            <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 19</span></li>
                                            <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - fa fa-clock-o</span></li>
                                            <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Kiosk</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                                            <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - 29</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>InheritedGroupType</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - Named</span></li>
                                            <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - 184</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>GroupTypePurposeValue</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>Roles</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                    <li>[1] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>GroupScheduleExclusions</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>ChildGroupTypes</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>ParentGroupTypes</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                    <li>[1] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>LocationTypeValues</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 23</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 2c42b2d4-1c5f-4ad5-a9ad-08631b872ac4</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                        </ul>
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Group</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>Locations</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ParentGroupId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GroupTypeId</span> <span class=''lava-debug-value''> - 23</span></li>
                                            <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ScheduleId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Ushers</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>IsSecurityRole</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                                            <li><span class=''lava-debug-key''>AllowGuests</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>WelcomeSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ExitSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>SyncDataViewId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>AddUserAccountsDuringSync</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>MustMeetRequirementsToAddMember</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>IsPublic</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>GroupCapacity</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplateId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GroupType</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Campus</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Schedule</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>WelcomeSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ExitSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>SyncDataView</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplate</span> <span class=''lava-debug-value''> - </span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>Groups</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>Members</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>GroupLocations</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>GroupRequirements</span>
                                                {<ul></ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - 1/19/2017 1:26:59 PM</span></li>
                                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - 1/19/2017 1:26:59 PM</span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - 10</span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - 10</span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 103</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 0ba93d66-21b1-4229-979d-f76ceb57666d</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 16</span></li>
                                            <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.Group</span></li>
                                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAACvVpRVmqIWc1rRV1y3TML1lHQUo2CglXYXkShl76!2...</span></li>
                                        </ul>
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Location</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                                            <li><span class=''lava-debug-key''>Locations</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ParentLocationId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Main Campus</span></li>
                                            <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>LocationTypeValueId</span> <span class=''lava-debug-value''> - 181</span></li>
                                            <li><span class=''lava-debug-key''>GeoPoint</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GeoFence</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Street1</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Street2</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>City</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>County</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>State</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Country</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>PostalCode</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Barcode</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>AssessorParcelId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>StandardizeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>StandardizeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>StandardizeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>StandardizedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GeocodeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GeocodeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GeocodeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GeocodedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>IsGeoPointLocked</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>PrinterDeviceId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ImageId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>SoftRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>FirmRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>IsNamedLocation</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>LocationTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>ChildLocations</span>
                                                {<ul></ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>PrinterDevice</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Image</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>FormattedAddress</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>FormattedHtmlAddress</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Latitude</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Longitude</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GeoFenceCoordinates</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - 1</span></li>
                                            <li><span class=''lava-debug-key''>Distance</span> <span class=''lava-debug-value''> - 0</span></li>
                                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 2</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - d5171d44-c801-4b7d-9335-d23fb4ea0e60</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 93</span></li>
                                            <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.Location</span></li>
                                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAP49IJq6MTj4TM2eg9ZC9J7N84OP9WzGjd5SkQaXaTb...</span></li>
                                        </ul>
                                    </li>
                                    <li><span class=''lava-debug-key''>GroupTypeConfiguredForLabel</span> <span class=''lava-debug-value''> - False</span></li>
                                </ul>
                            </li>
                            <li>
                                [1]
                                <ul>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Schedule</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>StartTime</span> <span class=''lava-debug-value''> - 2/21/2017 12:01:00 AM</span></li>
                                            <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - 6:00 (test)</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                            <li>
                                                <span class=''lava-debug-key''>iCalendarContent</span> <span class=''lava-debug-value''>
                                                    - BEGIN:VCALENDAR
                                                    BEGIN:VEVENT
                                                    DTEND:20130501T2...
                                                </span>
                                            </li>
                                            <li><span class=''lava-debug-key''>CheckInStartOffsetMinutes</span> <span class=''lava-debug-value''> - 0</span></li>
                                            <li><span class=''lava-debug-key''>CheckInEndOffsetMinutes</span> <span class=''lava-debug-value''> - 1439</span></li>
                                            <li><span class=''lava-debug-key''>EffectiveStartDate</span> <span class=''lava-debug-value''> - 5/1/2013 12:00:00 AM</span></li>
                                            <li><span class=''lava-debug-key''>EffectiveEndDate</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>WeeklyDayOfWeek</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>WeeklyTimeOfDay</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CategoryId</span> <span class=''lava-debug-value''> - 50</span></li>
                                            <li><span class=''lava-debug-key''>Category</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>FriendlyScheduleText</span> <span class=''lava-debug-value''> - Daily at 12:01 AM</span></li>
                                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 7</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - c8b7beb4-54e2-4473-822f-f5d0f8ce19d7</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 54</span></li>
                                            <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.Schedule</span></li>
                                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAD4Q30BG!2bPeLCk1gm1927ODNcYjAjHIIsMMAlMOux...</span></li>
                                        </ul>
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>GroupType</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>Groups</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Elementary Area</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                                            <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                                            <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 39</span></li>
                                            <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Kiosk</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 2</span></li>
                                            <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - 17</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>InheritedGroupType</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>Roles</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>GroupScheduleExclusions</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>ChildGroupTypes</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>ParentGroupTypes</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>LocationTypeValues</span>
                                                {<ul></ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 20</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - e3c8f7d6-5ceb-43bb-802f-66c3e734049e</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                        </ul>
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Group</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>Locations</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ParentGroupId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GroupTypeId</span> <span class=''lava-debug-value''> - 20</span></li>
                                            <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ScheduleId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Grades 4-6</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>IsSecurityRole</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 6</span></li>
                                            <li><span class=''lava-debug-key''>AllowGuests</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>WelcomeSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ExitSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>SyncDataViewId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>AddUserAccountsDuringSync</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>MustMeetRequirementsToAddMember</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>IsPublic</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>GroupCapacity</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplateId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GroupType</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Campus</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Schedule</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>WelcomeSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ExitSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>SyncDataView</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplate</span> <span class=''lava-debug-value''> - </span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>Groups</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>Members</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>GroupLocations</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>GroupRequirements</span>
                                                {<ul></ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 30</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 42c408ce-3d69-4d7d-b9ea-41087a8945a6</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 16</span></li>
                                            <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.Group</span></li>
                                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAMwgqDLZHcmZSjg4B!2fh2pWAxhxCdrASMtGUTdnXkn...</span></li>
                                        </ul>
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Location</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                                            <li><span class=''lava-debug-key''>Locations</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ParentLocationId</span> <span class=''lava-debug-value''> - 3</span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Outpost Room</span></li>
                                            <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>LocationTypeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GeoPoint</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GeoFence</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Street1</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Street2</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>City</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>County</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>State</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Country</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>PostalCode</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Barcode</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>AssessorParcelId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>StandardizeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>StandardizeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>StandardizeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>StandardizedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GeocodeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GeocodeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GeocodeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GeocodedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>IsGeoPointLocked</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>PrinterDeviceId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ImageId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>SoftRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>FirmRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>IsNamedLocation</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>LocationTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>ChildLocations</span>
                                                {<ul></ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>PrinterDevice</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Image</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>FormattedAddress</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>FormattedHtmlAddress</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Latitude</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Longitude</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GeoFenceCoordinates</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>Distance</span> <span class=''lava-debug-value''> - 0</span></li>
                                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 9</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - f9f33251-f829-4618-904e-42a1eda2b58e</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 93</span></li>
                                            <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.Location</span></li>
                                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAALR7O5xHKU!2fWP!2bv6RcrEUcCLkhSXMj2jNVcg6Tq...</span></li>
                                        </ul>
                                    </li>
                                    <li><span class=''lava-debug-key''>GroupTypeConfiguredForLabel</span> <span class=''lava-debug-value''> - True</span></li>
                                </ul>
                            </li>
                        </ul>}
                    </li>
                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>RecordTypeValueId</span> <span class=''lava-debug-value''> - 1</span></li>
                    <li><span class=''lava-debug-key''>RecordStatusValueId</span> <span class=''lava-debug-value''> - 3</span></li>
                    <li><span class=''lava-debug-key''>RecordStatusLastModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>RecordStatusReasonValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ConnectionStatusValueId</span> <span class=''lava-debug-value''> - 146</span></li>
                    <li><span class=''lava-debug-key''>ReviewReasonValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IsDeceased</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>TitleValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>FirstName</span> <span class=''lava-debug-value''> - Noah</span></li>
                    <li><span class=''lava-debug-key''>NickName</span> <span class=''lava-debug-value''> - Noah</span></li>
                    <li><span class=''lava-debug-key''>MiddleName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>LastName</span> <span class=''lava-debug-value''> - Decker</span></li>
                    <li><span class=''lava-debug-key''>SuffixValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>PhotoId</span> <span class=''lava-debug-value''> - 59</span></li>
                    <li><span class=''lava-debug-key''>BirthDay</span> <span class=''lava-debug-value''> - 10</span></li>
                    <li><span class=''lava-debug-key''>BirthMonth</span> <span class=''lava-debug-value''> - 3</span></li>
                    <li><span class=''lava-debug-key''>BirthYear</span> <span class=''lava-debug-value''> - 2006</span></li>
                    <li><span class=''lava-debug-key''>Gender</span> <span class=''lava-debug-value''> - Male</span></li>
                    <li><span class=''lava-debug-key''>MaritalStatusValueId</span> <span class=''lava-debug-value''> - 144</span></li>
                    <li><span class=''lava-debug-key''>AnniversaryDate</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GraduationYear</span> <span class=''lava-debug-value''> - 2024</span></li>
                    <li><span class=''lava-debug-key''>GivingGroupId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GivingId</span> <span class=''lava-debug-value''> - P5</span></li>
                    <li><span class=''lava-debug-key''>GivingLeaderId</span> <span class=''lava-debug-value''> - 0</span></li>
                    <li><span class=''lava-debug-key''>Email</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IsEmailActive</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>EmailNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>EmailPreference</span> <span class=''lava-debug-value''> - EmailAllowed</span></li>
                    <li><span class=''lava-debug-key''>ReviewReasonNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>InactiveReasonNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>SystemNote</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ViewedCount</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>PrimaryAlias</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>PrimaryAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>FullName</span> <span class=''lava-debug-value''> - Noah Decker</span></li>
                    <li><span class=''lava-debug-key''>BirthdayDayOfWeek</span> <span class=''lava-debug-value''> - Friday</span></li>
                    <li><span class=''lava-debug-key''>BirthdayDayOfWeekShort</span> <span class=''lava-debug-value''> - Fri</span></li>
                    <li><span class=''lava-debug-key''>PhotoUrl</span> <span class=''lava-debug-value''> - /GetImage.ashx?id=59</span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Users</span>
                        {<ul></ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>PhoneNumbers</span>
                        {<ul></ul>}
                    </li>
                    <li><span class=''lava-debug-key''>MaritalStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ConnectionStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ReviewReasonValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>RecordStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>RecordStatusReasonValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>RecordTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>SuffixValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>TitleValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Photo</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>BirthDate</span> <span class=''lava-debug-value''> - 3/10/2006 12:00:00 AM</span></li>
                    <li><span class=''lava-debug-key''>DaysUntilBirthday</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>Age</span> <span class=''lava-debug-value''> - 10</span></li>
                    <li><span class=''lava-debug-key''>NextBirthDay</span> <span class=''lava-debug-value''> - 3/10/2017 12:00:00 AM</span></li>
                    <li><span class=''lava-debug-key''>DaysToBirthday</span> <span class=''lava-debug-value''> - 17</span></li>
                    <li><span class=''lava-debug-key''>NextAnniversary</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GradeOffset</span> <span class=''lava-debug-value''> - 7</span></li>
                    <li><span class=''lava-debug-key''>HasGraduated</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>GradeFormatted</span> <span class=''lava-debug-value''> - 5th Grade</span></li>
                    <li><span class=''lava-debug-key''>ImpersonationParameter</span> <span class=''lava-debug-value''> - rckipid=EAAAAFH%2fdvpeD6TBEr%2fJy8xXw0RdqFrq5AR...</span></li>
                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - 1/19/2017 1:25:12 PM</span></li>
                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - 10</span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 5</span></li>
                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 32aab9e4-970d-4551-a17e-385e66113bd5</span></li>
                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 15</span></li>
                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.Person</span></li>
                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAELYGxb4IwwwvFT8jCZqkCN2XKa1LoE5unsuVCJQ0P1...</span></li>
                </ul>
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-e591cd60-44de-4a28-922d-3b43fa78159a''><h5 class=''panel-title pull-left''>People</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-e591cd60-44de-4a28-922d-3b43fa78159a'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>People properties can be accessed by <code>{% for person in People %}{{ person.[PropertyKey] }}{% endfor %}</code>.</p>
                {<ul>
                    <li>
                        [0]
                        <ul>
                            <li><span class=''lava-debug-key''>FamilyMember</span> <span class=''lava-debug-value''> - True</span></li>
                            <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                            <li><span class=''lava-debug-key''>FirstTime</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>SecurityCode</span> <span class=''lava-debug-value''> - 28G</span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-1''>GroupTypes</span>
                                {<ul>
                                    <li>
                                        [0]
                                        <ul>
                                            <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>Groups</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Elementary Area</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                                            <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                                            <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 39</span></li>
                                            <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Kiosk</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 2</span></li>
                                            <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - 17</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>InheritedGroupType</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>Roles</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>GroupScheduleExclusions</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>ChildGroupTypes</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>ParentGroupTypes</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>LocationTypeValues</span>
                                                {<ul></ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 20</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - e3c8f7d6-5ceb-43bb-802f-66c3e734049e</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                        </ul>
                                    </li>
                                    <li>
                                        [1]
                                        <ul>
                                            <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>Groups</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Serving Team</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - Used to track groups that serve in the...</span></li>
                                            <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                                            <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                                            <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 19</span></li>
                                            <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - fa fa-clock-o</span></li>
                                            <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Kiosk</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                                            <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - 29</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>InheritedGroupType</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - Named</span></li>
                                            <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - 184</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>GroupTypePurposeValue</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>Roles</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                    <li>[1] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>GroupScheduleExclusions</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>ChildGroupTypes</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>ParentGroupTypes</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                    <li>[1] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>LocationTypeValues</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 23</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 2c42b2d4-1c5f-4ad5-a9ad-08631b872ac4</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                        </ul>
                                    </li>
                                </ul>}
                            </li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-1''>SelectedOptions</span>
                                {<ul>
                                    <li>
                                        [0]
                                        <ul>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>Schedule</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>StartTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>iCalendarContent</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CheckInStartOffsetMinutes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CheckInEndOffsetMinutes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EffectiveStartDate</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EffectiveEndDate</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>WeeklyDayOfWeek</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>WeeklyTimeOfDay</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CategoryId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Category</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FriendlyScheduleText</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>GroupType</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Groups</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>Group</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Locations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ScheduleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsSecurityRole</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowGuests</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>WelcomeSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ExitSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SyncDataViewId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AddUserAccountsDuringSync</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>MustMeetRequirementsToAddMember</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsPublic</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacity</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplateId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Campus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Schedule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>WelcomeSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ExitSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SyncDataView</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplate</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Groups</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Members</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupRequirements</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>Location</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Locations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentLocationId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeoPoint</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeoFence</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Street1</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Street2</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>City</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>County</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>State</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Country</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>PostalCode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Barcode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AssessorParcelId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsGeoPointLocked</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>PrinterDeviceId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ImageId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SoftRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FirmRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsNamedLocation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>PrinterDevice</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Image</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FormattedAddress</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FormattedHtmlAddress</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Latitude</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Longitude</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeoFenceCoordinates</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Distance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>GroupTypeConfiguredForLabel</span> <span class=''lava-debug-value''> - False</span></li>
                                        </ul>
                                    </li>
                                    <li>
                                        [1]
                                        <ul>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>Schedule</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>StartTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>iCalendarContent</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CheckInStartOffsetMinutes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CheckInEndOffsetMinutes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EffectiveStartDate</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EffectiveEndDate</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>WeeklyDayOfWeek</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>WeeklyTimeOfDay</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CategoryId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Category</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FriendlyScheduleText</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>GroupType</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Groups</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>Group</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Locations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ScheduleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsSecurityRole</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowGuests</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>WelcomeSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ExitSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SyncDataViewId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AddUserAccountsDuringSync</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>MustMeetRequirementsToAddMember</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsPublic</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacity</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplateId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Campus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Schedule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>WelcomeSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ExitSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SyncDataView</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplate</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Groups</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Members</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupRequirements</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>Location</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Locations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentLocationId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeoPoint</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeoFence</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Street1</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Street2</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>City</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>County</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>State</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Country</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>PostalCode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Barcode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AssessorParcelId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsGeoPointLocked</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>PrinterDeviceId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ImageId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SoftRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FirmRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsNamedLocation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>PrinterDevice</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Image</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FormattedAddress</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FormattedHtmlAddress</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Latitude</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Longitude</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeoFenceCoordinates</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Distance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>GroupTypeConfiguredForLabel</span> <span class=''lava-debug-value''> - True</span></li>
                                        </ul>
                                    </li>
                                </ul>}
                            </li>
                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>RecordTypeValueId</span> <span class=''lava-debug-value''> - 1</span></li>
                            <li><span class=''lava-debug-key''>RecordStatusValueId</span> <span class=''lava-debug-value''> - 3</span></li>
                            <li><span class=''lava-debug-key''>RecordStatusLastModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>RecordStatusReasonValueId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ConnectionStatusValueId</span> <span class=''lava-debug-value''> - 146</span></li>
                            <li><span class=''lava-debug-key''>ReviewReasonValueId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>IsDeceased</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>TitleValueId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>FirstName</span> <span class=''lava-debug-value''> - Noah</span></li>
                            <li><span class=''lava-debug-key''>NickName</span> <span class=''lava-debug-value''> - Noah</span></li>
                            <li><span class=''lava-debug-key''>MiddleName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>LastName</span> <span class=''lava-debug-value''> - Decker</span></li>
                            <li><span class=''lava-debug-key''>SuffixValueId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PhotoId</span> <span class=''lava-debug-value''> - 59</span></li>
                            <li><span class=''lava-debug-key''>BirthDay</span> <span class=''lava-debug-value''> - 10</span></li>
                            <li><span class=''lava-debug-key''>BirthMonth</span> <span class=''lava-debug-value''> - 3</span></li>
                            <li><span class=''lava-debug-key''>BirthYear</span> <span class=''lava-debug-value''> - 2006</span></li>
                            <li><span class=''lava-debug-key''>Gender</span> <span class=''lava-debug-value''> - Male</span></li>
                            <li><span class=''lava-debug-key''>MaritalStatusValueId</span> <span class=''lava-debug-value''> - 144</span></li>
                            <li><span class=''lava-debug-key''>AnniversaryDate</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>GraduationYear</span> <span class=''lava-debug-value''> - 2024</span></li>
                            <li><span class=''lava-debug-key''>GivingGroupId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>GivingId</span> <span class=''lava-debug-value''> - P5</span></li>
                            <li><span class=''lava-debug-key''>GivingLeaderId</span> <span class=''lava-debug-value''> - 0</span></li>
                            <li><span class=''lava-debug-key''>Email</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>IsEmailActive</span> <span class=''lava-debug-value''> - True</span></li>
                            <li><span class=''lava-debug-key''>EmailNote</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>EmailPreference</span> <span class=''lava-debug-value''> - EmailAllowed</span></li>
                            <li><span class=''lava-debug-key''>ReviewReasonNote</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>InactiveReasonNote</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>SystemNote</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ViewedCount</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PrimaryAlias</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>PrimaryAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>FullName</span> <span class=''lava-debug-value''> - Noah Decker</span></li>
                            <li><span class=''lava-debug-key''>BirthdayDayOfWeek</span> <span class=''lava-debug-value''> - Friday</span></li>
                            <li><span class=''lava-debug-key''>BirthdayDayOfWeekShort</span> <span class=''lava-debug-value''> - Fri</span></li>
                            <li><span class=''lava-debug-key''>PhotoUrl</span> <span class=''lava-debug-value''> - /GetImage.ashx?id=59</span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-1''>Users</span>
                                {<ul></ul>}
                            </li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-1''>PhoneNumbers</span>
                                {<ul></ul>}
                            </li>
                            <li><span class=''lava-debug-key''>MaritalStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ConnectionStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ReviewReasonValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>RecordStatusValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>RecordStatusReasonValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>RecordTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>SuffixValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>TitleValue</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Photo</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>BirthDate</span> <span class=''lava-debug-value''> - 3/10/2006 12:00:00 AM</span></li>
                            <li><span class=''lava-debug-key''>DaysUntilBirthday</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>Age</span> <span class=''lava-debug-value''> - 10</span></li>
                            <li><span class=''lava-debug-key''>NextBirthDay</span> <span class=''lava-debug-value''> - 3/10/2017 12:00:00 AM</span></li>
                            <li><span class=''lava-debug-key''>DaysToBirthday</span> <span class=''lava-debug-value''> - 17</span></li>
                            <li><span class=''lava-debug-key''>NextAnniversary</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>GradeOffset</span> <span class=''lava-debug-value''> - 7</span></li>
                            <li><span class=''lava-debug-key''>HasGraduated</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>GradeFormatted</span> <span class=''lava-debug-value''> - 5th Grade</span></li>
                            <li><span class=''lava-debug-key''>ImpersonationParameter</span> <span class=''lava-debug-value''> - rckipid=EAAAAOI1z6P2ato38IUmY1%2b%2bRi5W2I7J34b...</span></li>
                            <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - 1/19/2017 1:25:12 PM</span></li>
                            <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - 10</span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 5</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 32aab9e4-970d-4551-a17e-385e66113bd5</span></li>
                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 15</span></li>
                            <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.Person</span></li>
                            <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAAK8l!2f5nx435DNa2RoZr5u3O39Yr!2fTCUfzzh5mby...</span></li>
                        </ul>
                    </li>
                </ul>}
            </div>
        </div>
    </div><div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-c3d45a0c-81f6-460c-8b1d-aabe651987a2''><h5 class=''panel-title pull-left''>Group Type</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-c3d45a0c-81f6-460c-8b1d-aabe651987a2'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>GroupType properties can be accessed by <code>{{ GroupType.[PropertyKey] }}</code>.</p>
                <ul>
                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Groups</span>
                        {<ul>
                            <li>
                                [0]
                                <ul>
                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - 2/21/2017 12:44:28 PM</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Locations</span>
                                        {<ul>
                                            <li>
                                                [0]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>LastCheckIn</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Locations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentLocationId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeoPoint</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeoFence</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Street1</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Street2</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>City</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>County</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>State</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Country</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>PostalCode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Barcode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AssessorParcelId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>StandardizedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodeAttemptedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodeAttemptedServiceType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodeAttemptedResult</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeocodedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsGeoPointLocked</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>PrinterDeviceId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ImageId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SoftRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FirmRoomThreshold</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IsNamedLocation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>PrinterDevice</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Image</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FormattedAddress</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FormattedHtmlAddress</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Latitude</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Longitude</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GeoFenceCoordinates</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Distance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                        </ul>}
                                    </li>
                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ParentGroupId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GroupTypeId</span> <span class=''lava-debug-value''> - 20</span></li>
                                    <li><span class=''lava-debug-key''>CampusId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ScheduleId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Grades 4-6</span></li>
                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>IsSecurityRole</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>IsActive</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 6</span></li>
                                    <li><span class=''lava-debug-key''>AllowGuests</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>WelcomeSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ExitSystemEmailId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>SyncDataViewId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>AddUserAccountsDuringSync</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>MustMeetRequirementsToAddMember</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>IsPublic</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>GroupCapacity</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplateId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GroupType</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Campus</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>Schedule</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>WelcomeSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ExitSystemEmail</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>SyncDataView</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>RequiredSignatureDocumentTemplate</span> <span class=''lava-debug-value''> - </span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Groups</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Members</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>GroupLocations</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>GroupRequirements</span>
                                        {<ul></ul>}
                                    </li>
                                    <li><span class=''lava-debug-key''>CreatedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedDateTime</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonAliasId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>CreatedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedByPersonName</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ModifiedAuditValuesAlreadyUpdated</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 30</span></li>
                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 42c408ce-3d69-4d7d-b9ea-41087a8945a6</span></li>
                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>TypeId</span> <span class=''lava-debug-value''> - 16</span></li>
                                    <li><span class=''lava-debug-key''>TypeName</span> <span class=''lava-debug-value''> - Rock.Model.Group</span></li>
                                    <li><span class=''lava-debug-key''>UrlEncodedKey</span> <span class=''lava-debug-value''> - EAAAADVhCvk!2bsL!2fUd2YmlafGm3!2bnuKIQZWdbTxbFG...</span></li>
                                </ul>
                            </li>
                        </ul>}
                    </li>
                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Elementary Area</span></li>
                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 39</span></li>
                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - False</span></li>
                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - None</span></li>
                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Kiosk</span></li>
                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 2</span></li>
                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - 17</span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>InheritedGroupType</span>
                        <ul>
                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Check in by Grade</span></li>
                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                            <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                            <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 36</span></li>
                            <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - True</span></li>
                            <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - False</span></li>
                            <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                            <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - None</span></li>
                            <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Default</span></li>
                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                            <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - 15</span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>InheritedGroupType</span>
                                <ul>
                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Check in by Age</span></li>
                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 34</span></li>
                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Default</span></li>
                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - 145</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-3''>GroupTypePurposeValue</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - 31</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 1</span></li>
                                            <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - Check-in Filter</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - A Group Type where the purpose is for check-in...</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-4''>DefinedType</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FieldTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CategoryId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Category</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FieldType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 145</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 6bced84c-69ad-4f5a-9197-5c0f9c02dd34</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                        </ul>
                                    </li>
                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-3''>Roles</span>
                                        {<ul>
                                            <li>[0] <span class=''lava-debug-value''> - Member</span></li>
                                        </ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-3''>GroupScheduleExclusions</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-3''>ChildGroupTypes</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-3''>ParentGroupTypes</span>
                                        {<ul>
                                            <li>
                                                [0]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li>
                                                        <span class=''lava-debug-key lava-debug-section level-1''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ GroupType.InheritedGroupType.InheritedGroupType.ParentGroupTypes | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                                                        <ul>
                                                            <li><span class=''lava-debug-key''>core_checkin_CheckInType</span> <span class=''lava-debug-value''> - Family</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_EnableManagerOption</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_EnableOverride</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_SecurityCodeLength</span> <span class=''lava-debug-value''> - 3</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_ReuseSameCode</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_UseSameOptions</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_SearchType</span> <span class=''lava-debug-value''> - Phone Number</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_RegularExpressionFilter</span> <span class=''lava-debug-value''> - </span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_MaxSearchResults</span> <span class=''lava-debug-value''> - 100</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_MinimumPhoneSearchLength</span> <span class=''lava-debug-value''> - 4</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_MaximumPhoneSearchLength</span> <span class=''lava-debug-value''> - 10</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_PhoneSearchType</span> <span class=''lava-debug-value''> - Ends With</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_RefreshInterval</span> <span class=''lava-debug-value''> - 10</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_AgeRequired</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_DisplayLocationCount</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_AutoSelectDaysBack</span> <span class=''lava-debug-value''> - 10</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_GradeRequired</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_HidePhotos</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_PreventDuplicateCheckin</span> <span class=''lava-debug-value''> - No</span></li>
                                                            <li><span class=''lava-debug-key''>core_checkin_PreventInactivePeople</span> <span class=''lava-debug-value''> - No</span></li>
                                                        </ul>
                                                    </li>
                                                </ul>
                                            </li>
                                        </ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-3''>LocationTypeValues</span>
                                        {<ul></ul>}
                                    </li>
                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 15</span></li>
                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 0572a5fe-20a4-4bf1-95cd-c71db5281392</span></li>
                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                </ul>
                            </li>
                            <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                            <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                            <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - 145</span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>GroupTypePurposeValue</span>
                                <ul>
                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - 31</span></li>
                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 1</span></li>
                                    <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - Check-in Filter</span></li>
                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - A Group Type where the purpose is for check-in...</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-3''>DefinedType</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>FieldTypeId</span> <span class=''lava-debug-value''> - 1</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 16</span></li>
                                            <li><span class=''lava-debug-key''>CategoryId</span> <span class=''lava-debug-value''> - 150</span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Group Type Purpose</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - Determines the role (check-in template,...</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-4''>Category</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentCategoryId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EntityTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EntityTypeQualifierColumn</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EntityTypeQualifierValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>HighlightColor</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentCategory</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Categories</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-4''>FieldType</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Assembly</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Class</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Field</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-4''>DefinedValues</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                    <li>[1] <span class=''lava-debug-value''> - </span></li>
                                                    <li>[2] <span class=''lava-debug-value''> - ...</span></li>
                                                </ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 31</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - b23f1e45-bc26-4e82-beb3-9b191fe5ccc3</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                        </ul>
                                    </li>
                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 145</span></li>
                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 6bced84c-69ad-4f5a-9197-5c0f9c02dd34</span></li>
                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                </ul>
                            </li>
                            <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - False</span></li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>Roles</span>
                                {<ul>
                                    <li>[0] <span class=''lava-debug-value''> - Member</span></li>
                                </ul>}
                            </li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>GroupScheduleExclusions</span>
                                {<ul></ul>}
                            </li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>ChildGroupTypes</span>
                                {<ul></ul>}
                            </li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>ParentGroupTypes</span>
                                {<ul>
                                    <li>
                                        [0]
                                        <ul>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Weekly Service Check-in Area</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                                            <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                                            <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 33</span></li>
                                            <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - fa fa-child</span></li>
                                            <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Default</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                                            <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                                            <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - 142</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>GroupTypePurposeValue</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - False</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>Roles</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>GroupScheduleExclusions</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>ChildGroupTypes</span>
                                                {<ul>
                                                    <li>[0] <span class=''lava-debug-value''> - </span></li>
                                                    <li>[1] <span class=''lava-debug-value''> - </span></li>
                                                    <li>[2] <span class=''lava-debug-value''> - ...</span></li>
                                                </ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>ParentGroupTypes</span>
                                                {<ul></ul>}
                                            </li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>LocationTypeValues</span>
                                                {<ul></ul>}
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 14</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - fedd389a-616f-4a53-906c-63d8255631c5</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-1''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ GroupType.InheritedGroupType.ParentGroupTypes | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>core_checkin_CheckInType</span> <span class=''lava-debug-value''> - Family</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_EnableManagerOption</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_EnableOverride</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_SecurityCodeLength</span> <span class=''lava-debug-value''> - 3</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_ReuseSameCode</span> <span class=''lava-debug-value''> - No</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_UseSameOptions</span> <span class=''lava-debug-value''> - No</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_SearchType</span> <span class=''lava-debug-value''> - Phone Number</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_RegularExpressionFilter</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_MaxSearchResults</span> <span class=''lava-debug-value''> - 100</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_MinimumPhoneSearchLength</span> <span class=''lava-debug-value''> - 4</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_MaximumPhoneSearchLength</span> <span class=''lava-debug-value''> - 10</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_PhoneSearchType</span> <span class=''lava-debug-value''> - Ends With</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_RefreshInterval</span> <span class=''lava-debug-value''> - 10</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_AgeRequired</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_DisplayLocationCount</span> <span class=''lava-debug-value''> - Yes</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_AutoSelectDaysBack</span> <span class=''lava-debug-value''> - 10</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_GradeRequired</span> <span class=''lava-debug-value''> - No</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_HidePhotos</span> <span class=''lava-debug-value''> - No</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_PreventDuplicateCheckin</span> <span class=''lava-debug-value''> - No</span></li>
                                                    <li><span class=''lava-debug-key''>core_checkin_PreventInactivePeople</span> <span class=''lava-debug-value''> - No</span></li>
                                                </ul>
                                            </li>
                                        </ul>
                                    </li>
                                </ul>}
                            </li>
                            <li>
                                <span class=''lava-debug-key lava-debug-section level-2''>LocationTypeValues</span>
                                {<ul></ul>}
                            </li>
                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 17</span></li>
                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 4f9565a7-dd5a-41c3-b4e8-13f0b872b10b</span></li>
                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                        </ul>
                    </li>
                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - True</span></li>
                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - False</span></li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>Roles</span>
                        {<ul>
                            <li>[0] <span class=''lava-debug-value''> - Member</span></li>
                        </ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>GroupScheduleExclusions</span>
                        {<ul></ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>ChildGroupTypes</span>
                        {<ul></ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>ParentGroupTypes</span>
                        {<ul>
                            <li>
                                [0]
                                <ul>
                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - Weekly Service Check-in Area</span></li>
                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - Group</span></li>
                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - Member</span></li>
                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - 33</span></li>
                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - fa fa-child</span></li>
                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - True</span></li>
                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - Default</span></li>
                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - None</span></li>
                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - 142</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>GroupTypePurposeValue</span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - True</span></li>
                                            <li><span class=''lava-debug-key''>DefinedTypeId</span> <span class=''lava-debug-value''> - 31</span></li>
                                            <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - 0</span></li>
                                            <li><span class=''lava-debug-key''>Value</span> <span class=''lava-debug-value''> - Check-in Template</span></li>
                                            <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - A Group Type where the purpose is for check-in...</span></li>
                                            <li>
                                                <span class=''lava-debug-key lava-debug-section level-2''>DefinedType</span>
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FieldTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>CategoryId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Category</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>FieldType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefinedValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 142</span></li>
                                            <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - 4a406cb0-495b-4795-b788-52bdfde00b01</span></li>
                                            <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                        </ul>
                                    </li>
                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - False</span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Roles</span>
                                        {<ul>
                                            <li>[0] <span class=''lava-debug-value''> - Member</span></li>
                                        </ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>GroupScheduleExclusions</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>ChildGroupTypes</span>
                                        {<ul>
                                            <li>
                                                [0]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>
                                                [1]
                                                <ul>
                                                    <li><span class=''lava-debug-key''>IsSystem</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Name</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Description</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupMemberTerm</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>DefaultGroupRoleId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowMultipleLocations</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInGroupList</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowInNavigation</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IconCssClass</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>TakesAttendance</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceCountsAsWeekendService</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>SendAttendanceReminder</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ShowConnectionStatus</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendanceRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupCapacityRule</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AttendancePrintTo</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Order</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupTypeId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>InheritedGroupType</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>AllowedScheduleTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationSelectionMode</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>EnableLocationSchedules</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValueId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupTypePurposeValue</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>IgnorePersonInactivated</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Roles</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>GroupScheduleExclusions</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ChildGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ParentGroupTypes</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>LocationTypeValues</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                                </ul>
                                            </li>
                                            <li>[2] <span class=''lava-debug-value''> - ...</span></li>
                                        </ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>ParentGroupTypes</span>
                                        {<ul></ul>}
                                    </li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>LocationTypeValues</span>
                                        {<ul></ul>}
                                    </li>
                                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 14</span></li>
                                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - fedd389a-616f-4a53-906c-63d8255631c5</span></li>
                                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                                    <li>
                                        <span class=''lava-debug-key lava-debug-section level-1''>Attributes <p class=''attributes''>Below is a list of attributes that can be retrieved using <code>{{ GroupType.ParentGroupTypes | Attribute:''[AttributeKey]'' }}</code>.</p></span>
                                        <ul>
                                            <li><span class=''lava-debug-key''>core_checkin_CheckInType</span> <span class=''lava-debug-value''> - Family</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_EnableManagerOption</span> <span class=''lava-debug-value''> - Yes</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_EnableOverride</span> <span class=''lava-debug-value''> - Yes</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_SecurityCodeLength</span> <span class=''lava-debug-value''> - 3</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_ReuseSameCode</span> <span class=''lava-debug-value''> - No</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_UseSameOptions</span> <span class=''lava-debug-value''> - No</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_SearchType</span> <span class=''lava-debug-value''> - Phone Number</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_RegularExpressionFilter</span> <span class=''lava-debug-value''> - </span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_MaxSearchResults</span> <span class=''lava-debug-value''> - 100</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_MinimumPhoneSearchLength</span> <span class=''lava-debug-value''> - 4</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_MaximumPhoneSearchLength</span> <span class=''lava-debug-value''> - 10</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_PhoneSearchType</span> <span class=''lava-debug-value''> - Ends With</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_RefreshInterval</span> <span class=''lava-debug-value''> - 10</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_AgeRequired</span> <span class=''lava-debug-value''> - Yes</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_DisplayLocationCount</span> <span class=''lava-debug-value''> - Yes</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_AutoSelectDaysBack</span> <span class=''lava-debug-value''> - 10</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_GradeRequired</span> <span class=''lava-debug-value''> - No</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_HidePhotos</span> <span class=''lava-debug-value''> - No</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_PreventDuplicateCheckin</span> <span class=''lava-debug-value''> - No</span></li>
                                            <li><span class=''lava-debug-key''>core_checkin_PreventInactivePeople</span> <span class=''lava-debug-value''> - No</span></li>
                                        </ul>
                                    </li>
                                </ul>
                            </li>
                        </ul>}
                    </li>
                    <li>
                        <span class=''lava-debug-key lava-debug-section level-1''>LocationTypeValues</span>
                        {<ul></ul>}
                    </li>
                    <li><span class=''lava-debug-key''>Id</span> <span class=''lava-debug-value''> - 20</span></li>
                    <li><span class=''lava-debug-key''>Guid</span> <span class=''lava-debug-value''> - e3c8f7d6-5ceb-43bb-802f-66c3e734049e</span></li>
                    <li><span class=''lava-debug-key''>ForeignId</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ForeignGuid</span> <span class=''lava-debug-value''> - </span></li>
                    <li><span class=''lava-debug-key''>ForeignKey</span> <span class=''lava-debug-value''> - </span></li>
                </ul>
            </div>
        </div>
    </div>
    <div class=''panel panel-default panel-lavadebug''>
        <div class=''panel-heading clearfix collapsed'' data-toggle=''collapse'' data-target=''#collapse-eceae767-2854-4427-9e4e-9ef2e15eb3ee''><h5 class=''panel-title pull-left''>Group Members</h5> <div class=''pull-right''><i class=''fa fa-chevron-up''></i></div></div><div id=''collapse-eceae767-2854-4427-9e4e-9ef2e15eb3ee'' class=''panel-collapse collapse''>
            <div class=''panel-body''>
                <p>GroupMembers properties can be accessed by <code>{% for groupmember in GroupMembers %}{{ groupmember.[PropertyKey] }}{% endfor %}</code>.</p>
                {<ul></ul>}
            </div>
        </div>
    </div>
</div>
</div>'
WHERE[GUID] = 'E4D289A9-70FA-4381-913E-2A757AD11147'
" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
