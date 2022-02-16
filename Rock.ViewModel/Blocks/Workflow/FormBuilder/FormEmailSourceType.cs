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

namespace Rock.ViewModel.Blocks.Workflow.FormBuilder
{
    /// <summary>
    /// The possible sources that can be used when generating an e-mail in
    /// the FormBuilder system.
    /// </summary>
    public enum FormEmailSourceType
    {
        /// <summary>
        /// A template will be used that contains all the information required
        /// to generate the e-mail contents.
        /// </summary>
        UseTemplate = 0,

        /// <summary>
        /// Custom properties will be used to generate the e-mail contents.
        /// </summary>
        Custom = 1
    }
}
