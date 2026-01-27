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

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormList
{
    /// <summary>
    /// 
    /// </summary>
	public class FormListItemBag
	{
		/// <summary>
		/// Gets or sets the workflow type identifier.
		/// </summary>
		public int Id { get; set; }

        /// <summary>
        /// Gets or sets the form name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the workflow type Guid.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the category Guid.
        /// </summary>
        public Guid? CategoryGuid { get; set; }

		/// <summary>
		/// Gets or sets the number of submissions (workflows) for this type.
		/// </summary>
		public int SubmissionCount { get; set; }

        /// <summary>
		/// Gets or sets the slug of the workflow type (used for sharing routes).
		/// </summary>
		public string Slug { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person can edit this form.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
		/// Gets or sets when the form (workflow type) was created.
		/// </summary>
		public DateTime? CreatedDateTime { get; set; }

		/// <summary>
		/// Gets or sets the name of who created the form.
		/// </summary>
		public string CreatedByPersonName { get; set; }
	}
}






