// <copyright>
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
using System;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// By default all columns in the grid will fire the OnRowSelected event when a user clicks on a cell in that column. A Grid Field can implement
    /// this interface to prevent the OnRowSelected event from being fired when this field (column) is clicked
    /// </summary>
    public interface INotRowSelectedField
    {
    }
}
