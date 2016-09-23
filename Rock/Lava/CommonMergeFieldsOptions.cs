﻿// <copyright>
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
namespace Rock.Lava
{
    /// <summary>
    /// Optional options for Rock.Lava.LavaHelper.GetCommonMergeFields
    /// </summary>
    public class CommonMergeFieldsOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonMergeFieldsOptions"/> class.
        /// </summary>
        public CommonMergeFieldsOptions()
        {
            this.GetPageContext = true;
            this.GetPageParameters = true;
            this.GetCurrentPerson = true;
            this.GetCampuses = true;
            this.GetDeviceFamily = false;
            this.GetOSFamily = false;
            this.GetLegacyGlobalMergeFields = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [get page context]. Defaults to True
        /// </summary>
        /// <value>
        ///   <c>true</c> if [get page context]; otherwise, <c>false</c>.
        /// </value>
        public bool GetPageContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [get page parameters]. Defaults to True
        /// </summary>
        /// <value>
        ///   <c>true</c> if [get page parameters]; otherwise, <c>false</c>.
        /// </value>
        public bool GetPageParameters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [get current person]. Defaults to True
        /// </summary>
        /// <value>
        ///   <c>true</c> if [get current person]; otherwise, <c>false</c>.
        /// </value>
        public bool GetCurrentPerson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [get campuses]. Defaults to True
        /// </summary>
        /// <value>
        ///   <c>true</c> if [get campuses]; otherwise, <c>false</c>.
        /// </value>
        public bool GetCampuses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [get device family]. Defaults to False
        /// </summary>
        /// <value>
        ///   <c>true</c> if [get device family]; otherwise, <c>false</c>.
        /// </value>
        public bool GetDeviceFamily { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [get os family]. Defaults to False
        /// </summary>
        /// <value>
        ///   <c>true</c> if [get os family]; otherwise, <c>false</c>.
        /// </value>
        public bool GetOSFamily { get; set; }

        /// <summary>
        /// If this is True (the default), get the GlobalAttribute merge fields when in LegacyMode
        /// Set to False to never get the Legacy Global Merges fields, even when in Legacy Mode
        /// </summary>
        /// <value>
        /// The get global attributes.
        /// </value>
        public bool GetLegacyGlobalMergeFields { get; set; }
    }

     
}
