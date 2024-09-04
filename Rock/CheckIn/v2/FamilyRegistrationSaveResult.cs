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
using System.Collections.Generic;

using Rock.Model;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// The result of the save operation. This will determine if the save was
    /// successful as well as any error message if it was not.
    /// </summary>
    public class FamilyRegistrationSaveResult
    {
        /// <summary>
        /// Can be used check if the save operation was successful. If <c>false</c>
        /// then the <see cref="ErrorMessage"/> will contain a message that
        /// describes the error.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// If <see cref="IsSuccess"/> is <c>false</c> then this will contain
        /// an error message describing what happened.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The primary family that was either created or modified.
        /// </summary>
        public Group PrimaryFamily { get; set; }

        /// <summary>
        /// The list of new families that were created by the save operation.
        /// </summary>
        public List<Group> NewFamilyList { get; } = new List<Group>();

        /// <summary>
        /// The list of new people that were created by the save operation.
        /// </summary>
        public List<Person> NewPersonList { get; private set; } = new List<Person>();
    }

}
