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
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// Select a field of the entity and format through Lava.
    /// </summary>
    [Description( "Select a field of the entity and format through Lava" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Person Formatted Field" )]
    public class PersonFormattedFieldSelect : FormattedFieldSelect<Rock.Model.Person>
    {
    }
}