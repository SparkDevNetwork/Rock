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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Connection Status Badge
    /// </summary>
    [Description( "Connection Status Badge" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata("ComponentName", "Connection Status")]
    public class ConnectionStatus : TextBadge
    {
        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override System.Collections.Generic.Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = base.AttributeValueDefaults;
                defaults["Order"] = "0";
                return defaults;
            }
        }

        /// <summary>
        /// Gets the badge label
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override HighlightLabel GetLabel(Person person)
        {
            if ( Person != null )
            {
                var label = new HighlightLabel();
                label.LabelType = LabelType.Success;
                label.Text = Person.ConnectionStatusValueId.DefinedValue();
                return label;
            }

            return null;
        } 
    }
}
