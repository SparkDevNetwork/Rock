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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.CheckIn.Manager.PersonRight
{
    /// <summary>
    /// Input to the PrintNextGenLabels block action.
    /// </summary>
    public class PersonRightPrintNextGenLabelsRequestBag
    {
        /// <summary>
        /// Gets or sets the selected next-gen label-type values.
        /// </summary>
        public List<string> LabelTypeValues { get; set; }

        /// <summary>
        /// Gets or sets the guid of the printer the user selected.
        /// </summary>
        public Guid? PrinterGuid { get; set; }
    }
}
