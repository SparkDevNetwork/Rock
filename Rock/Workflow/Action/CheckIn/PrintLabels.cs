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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Data;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Prints next-gen check-in labels for an existing attendance record.
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Prints next-gen check-in labels for an existing attendance record." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Print Labels" )]

    [BooleanField( "Enable Saving Label Data", "Select 'Yes' if the label data should be temporarily saved on the attendance record. Select 'No' to disable saving label data.", true )]

    [Rock.SystemGuid.EntityTypeGuid( "5978a2d6-7f0f-465e-b469-7d883742c853" )]
    public class PrintLabels : CheckInActionComponent
    {
        /// <inheritdoc/>
        public override bool Execute( RockContext rockContext, Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = null;

            return false;
        }
    }
}