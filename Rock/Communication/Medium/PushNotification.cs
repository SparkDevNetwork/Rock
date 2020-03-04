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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Model;
using Rock.Web.UI.Controls.Communication;

namespace Rock.Communication.Medium
{

    /// <summary>
    /// A push notification communication
    /// </summary>
    [Description( "A push notification communication" )]
    [Export( typeof( MediumComponent ))]
    [ExportMetadata( "ComponentName", "Push Notification")]
    class PushNotification : MediumComponent
    {

        public override CommunicationType CommunicationType { get { return CommunicationType.PushNotification; } }

        /// <summary>
        /// Gets the control.
        /// </summary>
        /// <param name="useSimpleMode">if set to <c>true</c> [use simple mode].</param>
        /// <returns></returns>
        public override MediumControl GetControl( bool useSimpleMode )
        {
            return new Web.UI.Controls.Communication.PushNotification();
        }
    }
}
