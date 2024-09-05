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
namespace Rock.ViewModels.Blocks.CheckIn.CheckInKiosk
{
    /// <summary>
    /// The response object for the SaveFamily action.
    /// </summary>
    public class SaveFamilyResponseBag
    {
        /// <summary>
        /// The encrypted identifier of the family that was either updated
        /// or newly created.
        /// </summary>
        public string FamilyId { get; set; }

        /// <summary>
        /// Determines if check-in after a successful registration should
        /// be allowed.
        /// </summary>
        public bool IsCheckInAllowed { get; set; }

        /// <summary>
        /// Determines if the save operation was successful or not.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// If the save operation was not successful then this will contain
        /// an error message describing what went wrong.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
