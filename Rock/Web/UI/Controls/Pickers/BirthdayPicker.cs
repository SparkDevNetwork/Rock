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

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select individual parts (month, day, year) of a birthday
    /// <para>Birthdates cannot be in the future</para>
    /// </summary>
    public class BirthdayPicker : DatePartsPicker, IRockChangeHandlerControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BirthdayPicker"/> class.
        /// </summary>
        public BirthdayPicker()
        {
            this.AllowFutureDates = false;
            this.RequireYear = false;
            this.FutureDatesErrorMessage = "Birthdates cannot be in the future";
        }

        /// <summary>
        /// Occurs when [selected birthday changed].
        /// </summary>
        public event EventHandler SelectedBirthdayChanged
        {
            add
            {
                this.SelectedDatePartsChanged += value;
            }

            remove
            {
                this.SelectedDatePartsChanged -= value;
            }
        }
    }
}