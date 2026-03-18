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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.Attribute;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Provides data lookup and analytics functions focused on attendance data.
    /// </summary>

    [Description( "This skill provides access for a person to view their own information." )]
    [AgentUsage( "These tools provide access for a person to view their own information." )]

    [GroupTypesField( "My Group Types",
        Description = "The group types that will be checked when retrieving the current person's groups.",
        IsRequired = false,
        EnhancedSelection = true,
        Key = ConfigurationKey.GroupTypes,
        Order = 0 )]

    [BooleanField( "Disable Name Edit",
        Description = "When true, the person will not be allowed to edit their name.",
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = ConfigurationKey.DisableNameEdit,
        Order = 1 )]

    [BooleanField( "Disable Email Edit",
        Description = "When true, the person will not be allowed to edit their email address.",
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = ConfigurationKey.DisableEmailEdit,
        Order = 2 )]

    [BooleanField( "Disable Birthdate Edit",
        Description = "When true, the person will not be allowed to edit their birthdate.",
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = ConfigurationKey.DisableBirthdateEdit,
        Order = 3 )]

    [BooleanField( "Disable Phone Number Edit",
        Description = "When true, the person will not be allowed to edit their phone numbers.",
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = ConfigurationKey.DisablePhoneNumberEdit,
        Order = 4 )]

    [DefinedValueField( "Phone Number Types",
        Description = "The types of phone numbers that can be edited.",
        DefinedTypeGuid = DefinedType.PERSON_PHONE_TYPE,
        DefaultValue = DefinedValue.PERSON_PHONE_TYPE_HOME,
        IsRequired = false,
        AllowMultiple = true,
        Key = ConfigurationKey.PhoneNumberTypes,
        Order = 5 )]

    [BooleanField( "Disable Address Edit",
        Description = "When true, the person will not be allowed to edit their addresses.",
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = ConfigurationKey.DisableAddressEdit,
        Order = 6 )]

    [GroupLocationTypeField( "Address Type",
        Description = "The type of address to be edited.",
        GroupTypeGuid = GroupType.GROUPTYPE_FAMILY,
        DefaultValue = DefinedValue.GROUP_LOCATION_TYPE_HOME,
        IsRequired = false,
        Key = ConfigurationKey.AddressType,
        Order = 7 )]

    [BooleanField( "Disable Campus Edit",
        Description = "When true, the person will not be allowed to edit their campus.",
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = ConfigurationKey.DisableCampusEdit,
        Order = 8 )]

    [BooleanField( "Disable Demographics Edit",
        Description = "When true, the person will not be allowed to edit their demographic information.",
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = ConfigurationKey.DisableDemographicsEdit,
        Order = 9 )]

    [AttributeField( "Viewable Person Attributes",
        EntityTypeGuid = EntityType.PERSON,
        Description = "The person attributes that should be made available when retrieving the person profile.",
        IsRequired = false,
        AllowMultiple = true,
        Key = ConfigurationKey.ViewablePersonAttributes,
        Order = 11 )]

    [AttributeField( "Editable Person Attributes",
        EntityTypeGuid = EntityType.PERSON,
        Description = "The person attributes that should be made available for editing.",
        IsRequired = false,
        AllowMultiple = true,
        Key = ConfigurationKey.EditablePersonAttributes,
        Order = 11 )]

    [AgentSkillGuid( "86d579e8-2a4f-4bb7-baf4-edfff792d3f9" )]
    [EntityTypeGuid( "36e0b376-e06d-4e65-8f0b-e9021662e8bd" )]
    internal sealed partial class AttendeeSkill : AgentSkillComponent
    {
        #region Keys

        private static class ConfigurationKey
        {
            public const string GroupTypes = "GroupTypes";
            public const string DisableNameEdit = "DisableNameEdit";
            public const string DisableEmailEdit = "DisableEmailEdit";
            public const string DisableBirthdateEdit = "DisableBirthdateEdit";
            public const string DisablePhoneNumberEdit = "DisablePhoneNumberEdit";
            public const string PhoneNumberTypes = "PhoneTypes";
            public const string DisableAddressEdit = "DisableAddressEdit";
            public const string AddressType = "AddressType";
            public const string DisableCampusEdit = "DisableCampusEdit";
            public const string DisableDemographicsEdit = "DisableDemographicsEdit";
            public const string ViewablePersonAttributes = "ViewablePersonAttributes";
            public const string EditablePersonAttributes = "EditablePersonAttributes";
        }

        #endregion

        #region Fields

        /// <summary>
        /// The logger for this instance.
        /// </summary>
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// The constructor for the Attendee Skill.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public AttendeeSkill( ILogger<ConnectionSkill> logger )
        {
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Methods

        private IEnumerable<GroupTypeCache> GetConfiguredGroupTypes()
        {
            var groupTypeGuids = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.GroupTypes, string.Empty )
                .SplitDelimitedValues()
                .AsGuidList();

            if ( groupTypeGuids.Count == 0 )
            {
                return [];
            }

            return GroupTypeCache.GetMany( groupTypeGuids, AgentRequestContext.RockContext )
                .Where( gt => gt.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) );
        }

        #endregion
    }
}
