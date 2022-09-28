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

using System;

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Specifies how an e-mail used by the FormBuilder system will be generated.
    /// </summary>
    public class FormEmailSourceViewModel
    {
        /// <summary>
        /// The source type that will be used to generate the contents of the
        /// e-mail.
        /// </summary>
        public FormEmailSourceType Type { get; set; }

        /// <summary>
        /// The template unique identifier that should be used to generate the
        /// e-mail contents.
        /// </summary>
        /// <remarks>
        /// This property is only used if <see cref="Type"/> contains the value
        /// <see cref="FormEmailSourceType.UseTemplate"/>.
        /// </remarks>
        public Guid? Template { get; set; }

        /// <summary>
        /// The plain text to use for the custom subject of the e-mail.
        /// </summary>
        /// <remarks>
        /// This property is only used if <see cref="Type"/> contains the value
        /// <see cref="FormEmailSourceType.Custom"/>.
        /// </remarks>
        public string Subject { get; set; }

        /// <summary>
        /// The e-mail address to be used as the reply-to address for the
        /// custom e-mail.
        /// </summary>
        /// <remarks>
        /// This property is only used if <see cref="Type"/> contains the value
        /// <see cref="FormEmailSourceType.Custom"/>.
        /// </remarks>
        public string ReplyTo { get; set; }

        /// <summary>
        /// The HTML content to use for the custom e-mail body.
        /// </summary>
        /// <remarks>
        /// This property is only used if <see cref="Type"/> contains the value
        /// <see cref="FormEmailSourceType.Custom"/>.
        /// </remarks>
        public string Body { get; set; }

        /// <summary>
        /// Determines if the standard organization header and footer should
        /// be prepended and appended to the custom <see cref="Body"/>.
        /// </summary>
        /// <remarks>
        /// This property is only used if <see cref="Type"/> contains the value
        /// <see cref="FormEmailSourceType.Custom"/>.
        /// </remarks>
        public bool AppendOrgHeaderAndFooter { get; set; }
    }
}
