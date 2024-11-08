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

using System.ComponentModel;

namespace Rock.Model
{
    /// <summary>
    /// see <see cref="RelatedEntity.PurposeKey" />
    /// </summary>
    public static class RelatedEntityPurposeKey
    {
        /// <summary>
        /// Gets the purpose key description (Friendly Name)
        /// </summary>
        /// <param name="relatedEntityPurposeKey">The related entity purpose key.</param>
        /// <returns>System.String.</returns>
        public static string GetPurposeKeyFriendlyName( string relatedEntityPurposeKey )
        {
            return Reflection.GetDescriptionOfStringConstant( typeof( RelatedEntityPurposeKey ), relatedEntityPurposeKey ) ?? relatedEntityPurposeKey.Replace( '-', ' ' ).FixCase();
        }

        /// <summary>
        /// <para>The group placement for a specific Registration Instance.</para>
        /// <para>NOTE: Use methods on <seealso cref="RegistrationInstanceService" /> such as <seealso cref="RegistrationInstanceService.GetRegistrationInstancePlacementGroups"/>
        /// instead of <seealso cref="RelatedEntityService"/> to make this easier to use.</para>
        /// <para>For this, the <see cref="RelatedEntity"/> fields would be...</para>
        /// <list>
        /// <item>
        ///     <term><see cref="RelatedEntity.PurposeKey" /></term>
        ///     <description>PLACEMENT</description>
        /// </item>
        /// <item>
        ///     <term><see cref="RelatedEntity.SourceEntityType"/></term>
        ///     <description><see cref="RegistrationInstance"/></description>
        /// </item>
        /// <item>
        ///     <term><see cref="RelatedEntity.TargetEntityType"/></term>
        ///     <description><see cref="Rock.Model.Group"/></description>
        /// </item>
        /// <item>
        ///     <term><see cref="RelatedEntity.QualifierValue"/></term>
        ///     <description><see cref="RegistrationTemplatePlacement"/> Id</description>
        /// </item>
        /// </list>
        /// </summary>
        [Description( "Registration Instance Placement" )]
        public const string RegistrationInstanceGroupPlacement = "PLACEMENT";

        /// <summary>
        /// <para>The group placement for a Registration Template ('Shared' for all of the RegistrationTemplate's Registration Instances)</para>
        /// <para>NOTE: Use methods on <seealso cref="RegistrationTemplatePlacementService" /> such as <seealso cref="RegistrationTemplatePlacementService.GetRegistrationTemplatePlacementPlacementGroups"/>
        /// instead of <seealso cref="RelatedEntityService"/> to make this easier to use.</para>
        /// <para>For this, the RelatedEntity fields would be...</para>
        /// <list>
        /// <item>
        ///     <term><see cref="RelatedEntity.PurposeKey" /></term>
        ///     <description>PLACEMENT-TEMPLATE</description>
        /// </item>
        /// <item>
        ///     <term><see cref="RelatedEntity.SourceEntityType"/></term>
        ///     <description><see cref="RegistrationTemplatePlacement"/></description>
        /// </item>
        /// <item>
        ///     <term><see cref="RelatedEntity.TargetEntityType"/></term>
        ///     <description><see cref="Rock.Model.Group"/></description>
        /// </item>
        /// <item>
        ///     <term><see cref="RelatedEntity.QualifierValue"/></term>
        ///     <description><c>null</c></description>
        /// </item>
        /// </list>
        /// </summary>
        [Description( "Registration Template Placement" )]
        public const string RegistrationTemplateGroupPlacementTemplate = "PLACEMENT-TEMPLATE";

        /// <summary>
        /// <para>The relationship between a Person (PersonAlias) and the FinancialAccount they want Giving Alerts for.</para>
        /// <para>NOTE: Use methods on <seealso cref="FinancialAccountService"/> such as ....
        /// instead of <seealso cref="RelatedEntityService"/> to make this easier to use.</para>
        /// <para>For this, the related entity fields would be...</para>
        ///
        /// 
        /// <list>
        /// <item>
        ///     <term><see cref="RelatedEntity.PurposeKey" /></term>
        ///     <description>ACCOUNT-GIVING-ALERT</description>
        /// </item>
        /// <item>
        ///     <term><see cref="RelatedEntity.SourceEntityType"/></term>
        ///     <description><see cref="FinancialAccount"/></description>
        /// </item>
        /// <item>
        ///     <term><see cref="RelatedEntity.TargetEntityType"/></term>
        ///     <description><see cref="PersonAlias"/></description>
        /// </item>
        /// <item>
        ///     <term><see cref="RelatedEntity.QualifierValue"/></term>
        ///     <description><c>null</c></description>
        /// </item>
        /// </list>
        /// </summary>
        [Description( "Giving Alerts" )]
        public const string FinancialAccountGivingAlert = "ACCOUNT-GIVING-ALERT";

        /// <summary>
        /// <para>
        /// The relationship between a check-in area (GroupType) and a check-in
        /// label that should be printed when someone checks in or out of the
        /// area.
        /// </para>
        /// <para>For this, the related entity fields would be...</para>
        /// 
        /// <list>
        /// <item>
        ///     <term><see cref="RelatedEntity.PurposeKey" /></term>
        ///     <description>AREA-CHECK-IN-LABEL</description>
        /// </item>
        /// <item>
        ///     <term><see cref="RelatedEntity.SourceEntityType"/></term>
        ///     <description><see cref="GroupType"/></description>
        /// </item>
        /// <item>
        ///     <term><see cref="RelatedEntity.TargetEntityType"/></term>
        ///     <description><see cref="CheckInLabel"/></description>
        /// </item>
        /// <item>
        ///     <term><see cref="RelatedEntity.QualifierValue"/></term>
        ///     <description><c>null</c></description>
        /// </item>
        /// </list>
        /// </summary>
        [Description( "Check-in Label" )]
        public const string AreaCheckInLabel = "AREA-CHECK-IN-LABEL";
    }
}
