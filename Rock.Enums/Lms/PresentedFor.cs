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
namespace Rock.Enums.Lms
{
    /// <summary>
    /// Determines the reason a component will be presented and who it will
    /// be presented to.
    /// </summary>
    public enum PresentedFor
    {
        /// <summary>
        /// The component is presented to display configuration information.
        /// This implies a trusted level of access.
        /// </summary>
        Configuration = 0,

        /// <summary>
        /// The component is presented to display non-configuration screens
        /// to one of the facilitators. This implies a trusted level of access.
        /// </summary>
        Facilitator = 1,

        /// <summary>
        /// The component is presented to display non-configuration screens to
        /// the student. Sensitive data should not be included.
        /// </summary>
        Student = 2
    }
}
