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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:LiquidField runat=server></{0}:LiquidField>" )]
    [Obsolete( "Use LavaField instead" )]
    public class LiquidField : LavaField
    {
        /// <summary>
        /// Gets or sets the liquid template.
        /// </summary>
        /// <value>
        /// The liquid template.
        /// </value>
        public string LiquidTemplate
        {
            get
            {
                return this.LavaTemplate;
            }

            set
            {
                this.LavaTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets the liquid key, for example: Person
        /// </summary>
        /// <value>
        /// The liquid key.
        /// </value>
        public string LiquidKey
        {
            get
            {
                return this.LavaKey;
            }

            set
            {
                this.LavaKey = value;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Obsolete( "Use LavaFieldTemplate instead" )]
    public class LiquidFieldTemplate : LavaFieldTemplate
    {

    }
}