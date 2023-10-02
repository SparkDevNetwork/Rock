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
using Rock.ViewModels.Utility;
using Rock.ViewModels.Controls;
using Rock.Model;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetChildren API action of the AccountPicker control.
    /// </summary>
    public class PersonBasicEditorBag : IValidPropertiesBox
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ListItemBag PersonTitle { get; set; }
        public ListItemBag PersonSuffix { get; set; }
        public ListItemBag PersonMaritalStatus { get; set; }
        public ListItemBag PersonGradeOffset { get; set; }
        public ListItemBag PersonGroupRole { get; set; }
        public ListItemBag PersonConnectionStatus { get; set; }
        public Gender PersonGender { get; set; }
        public ListItemBag PersonRace { get; set; }
        public ListItemBag PersonEthnicity { get; set; }
        public DatePartsPickerValueBag PersonBirthDate { get; set; }
        public string Email { get; set; }
        public string MobilePhoneNumber { get; set; }
        public string MobilePhoneCountryCode { get; set; }
        public bool IsMessagingEnabled { get; set; }

        public List<string> ValidProperties {get; set;}
    }
}
