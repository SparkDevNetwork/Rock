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

namespace Rock.ViewModels.Blocks.Security.ForgotUserName
{
    /// <summary>
    /// A bag containing the required information to display various block captions.
    /// </summary>
    public class ForgotUserNameCaptionsBag
    {
        /// <summary>
        /// The caption to display in the block heading (HTML).
        /// </summary>
        public string HeadingCaption { get; set; }

        /// <summary>
        /// The caption to display when an invalid email is used to get reset instructions (HTML).
        /// </summary>
        public string InvalidEmailCaption { get; set; }

        /// <summary>
        /// The caption to display when instructions are sent successfully (HTML).
        /// </summary>
        public string SuccessCaption { get; set; }
    }
}
