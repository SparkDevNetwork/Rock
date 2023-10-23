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

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// Birthday Picker View Model
    /// </summary>
    public sealed class BirthdayPickerBag
    {
        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the month.
        /// </summary>
        /// <value>
        /// The month.
        /// </value>
        public int Month { get; set; }

        /// <summary>
        /// Gets or sets the day.
        /// </summary>
        /// <value>
        /// The day.
        /// </value>
        public int Day { get; set; }
    }

    /// <summary>
    /// Birthday Picker View Model Extensions
    /// </summary>
    public static class BirthdayPickerViewModelExtensions
    {
        /// <summary>
        /// Converts to datetime.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <returns></returns>
        public static DateTime? ToDateTime( this BirthdayPickerBag viewModel )
        {
            if ( viewModel == null )
            {
                return null;
            }

            try
            {
                return new DateTime( viewModel.Year, viewModel.Month, viewModel.Day );
            }
            catch
            {
                return null;
            }
        }
    }
}
