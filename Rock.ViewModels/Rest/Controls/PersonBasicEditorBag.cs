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
        /// <summary>
        /// 
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ListItemBag PersonTitle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ListItemBag PersonSuffix { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ListItemBag PersonMaritalStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ListItemBag PersonGradeOffset { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ListItemBag PersonGroupRole { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ListItemBag PersonConnectionStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Gender PersonGender { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ListItemBag PersonRace { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ListItemBag PersonEthnicity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DatePartsPickerValueBag PersonBirthDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MobilePhoneNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MobilePhoneCountryCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsMessagingEnabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> ValidProperties {get; set;}
    }
}
