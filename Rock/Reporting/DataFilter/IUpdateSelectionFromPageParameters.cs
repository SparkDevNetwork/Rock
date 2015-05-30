﻿// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

namespace Rock.Reporting.DataFilter
{
    /// <summary>
    /// A DataFilter that implements a function that can update the selection from rockblock page parameters or context
    /// </summary>
    public interface IUpdateSelectionFromPageParameters
    {
        /// <summary>
        /// Updates the selection from page parameters.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <param name="rockBlock">The rock block.</param>
        /// <returns></returns>
        string UpdateSelectionFromPageParameters( string selection, Rock.Web.UI.RockBlock rockBlock );
    }
}
