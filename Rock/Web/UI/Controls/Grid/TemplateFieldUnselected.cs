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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A Template field with the INotRowSelectedField interface to prevent clicks on this field from selecting row
    /// </summary>
    [Obsolete("Use RockTemplateFieldUnselected instead.")]
    [ToolboxData( "<{0}:TemplateFieldUnselected runat=server></{0}:TemplateFieldUnselected>" )]
    public class TemplateFieldUnselected : RockTemplateFieldUnselected
    {
    }
}