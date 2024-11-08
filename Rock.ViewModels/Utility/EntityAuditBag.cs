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

namespace Rock.ViewModels.Utility
{
    /// <summary>
    /// Contains the information required to show the audit panel details for a model.
    /// </summary>
    public class EntityAuditBag
    {
        /// <summary>
        /// Gets or sets the identifier of the model.
        /// </summary>
        /// <value>The identifier of the model.</value>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier key of the model.
        /// </summary>
        /// <value>The identifier key of the model.</value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the model.
        /// </summary>
        /// <value>The unique identifier of the model.</value>
        public Guid? Guid { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the person that created the model.
        /// </summary>
        /// <value>The identifier of the person that created the model.</value>
        public int? CreatedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the name of the person that created the model.
        /// </summary>
        /// <value>The name of the person that created the model.</value>
        public string CreatedByName { get; set; }

        /// <summary>
        /// Gets or sets the time the model was created relative to now.
        /// </summary>
        /// <value>The time the model was created relative to now.</value>
        public string CreatedRelativeTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the model was created.
        /// </summary>
        /// <value>The date and time the model was created.</value>
        public DateTimeOffset? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the person that modified the model.
        /// </summary>
        /// <value>The identifier of the person that modified the model.</value>
        public int? ModifiedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the name of the person that modified the model.
        /// </summary>
        /// <value>The name of the person that modified the model.</value>
        public string ModifiedByName { get; set; }

        /// <summary>
        /// Gets or sets the time the model was modified relative to now.
        /// </summary>
        /// <value>The time the model was modified relative to now.</value>
        public string ModifiedRelativeTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the model was modified.
        /// </summary>
        /// <value>The date and time the model was modified.</value>
        public DateTimeOffset? ModifiedDateTime { get; set; }
    }
}
