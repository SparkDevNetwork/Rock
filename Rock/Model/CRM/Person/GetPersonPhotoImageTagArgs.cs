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
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Method arguments for <see cref="Person.GetPersonPhotoImageTag(int?, GetPersonPhotoImageTagArgs)"/>
    /// </summary>
    public sealed class GetPersonPhotoImageTagArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetPersonPhotoImageTagArgs"/> class.
        /// </summary>
        public GetPersonPhotoImageTagArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPersonPhotoImageTagArgs"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        public GetPersonPhotoImageTagArgs( Person person )
        {
            PhotoId = person?.PhotoId;
            Age = person?.Age;
            Gender = person?.Gender ?? Gender.Unknown;
            if ( person?.RecordStatusValueId != null )
            {
                RecordTypeValueGuid = DefinedValueCache.Get( person.RecordTypeValueId.Value )?.Guid;
            }
        }

        /// <summary>
        /// Gets or sets the photo identifier.
        /// </summary>
        /// <value>
        /// The photo identifier.
        /// </value>
        public int? PhotoId { get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value>
        /// The gender.
        /// </value>
        public Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets the record type value unique identifier.
        /// </summary>
        /// <value>
        /// The record type value unique identifier.
        /// </value>
        public Guid? RecordTypeValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the age classification.
        /// </summary>
        /// <value>
        /// The age classification.
        /// </value>
        public AgeClassification? AgeClassification { get; set; }

        /// <summary>
        /// Gets or sets the maximum width.
        /// </summary>
        /// <value>
        /// The maximum width.
        /// </value>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the maximum height.
        /// </summary>
        /// <value>
        /// The maximum height.
        /// </value>
        public int? MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets the alt text.
        /// </summary>
        /// <value>
        /// The alt text.
        /// </value>
        public string AltText { get; set; }

        /// <summary>
        /// Gets or sets the name of the class.
        /// </summary>
        /// <value>
        /// The name of the class.
        /// </value>
        public string ClassName { get; set; }
    }
}
