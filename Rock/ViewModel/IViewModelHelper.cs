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

using Rock.Model;

namespace Rock.ViewModel
{
    /// <summary>
    /// A helper class that will convert a model into a view model.
    /// </summary>
    public interface IViewModelHelper
    {
        /// <summary>
        /// Creates the view model for the given model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> then attributes should be loaded.</param>
        /// <returns></returns>
        IViewModel CreateViewModel( object model, Person currentPerson, bool loadAttributes );
    }
}
