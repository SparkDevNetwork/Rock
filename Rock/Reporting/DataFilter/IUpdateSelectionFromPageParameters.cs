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

namespace Rock.Reporting.DataFilter
{
    /// <summary>
    /// A DataFilter that implements a function that can update the selection from rockblock page parameters or context
    /// </summary>
    [RockObsolete( "18.0" )]
    [Obsolete( "This interface is obsolete and will be removed in a future version of Rock because filters are moving to Obsidian, which cannot use RockBlock. Use IUpdateSelectionFromRockRequestContext instead." )]
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
