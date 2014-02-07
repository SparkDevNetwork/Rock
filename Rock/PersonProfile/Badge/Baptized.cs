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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;

using Rock.Model;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Baptized Badge
    /// </summary>
    [Description( "Baptized Badge" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Baptized" )]
    public class Baptized : IconBadge
    {

        /// <summary>
        /// Gets the tool tip text.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override string GetToolTipText( Person person )
        {
            var attributeValue = person.GetAttributeValue("BaptismDate");
            if (!string.IsNullOrWhiteSpace(attributeValue))
            {
                var date = DateTime.MinValue;
                if (DateTime.TryParse(attributeValue, out date) && date > DateTime.MinValue)
                {
                    return "Baptized on " + date.ToShortDateString();
                }
            }

            return "No Baptism Date";
        }

        /// <summary>
        /// Gets the icon path.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override string GetIconPath( Person person )
        {
            var attributeValue = person.GetAttributeValue( "BaptismDate" );
            if ( !string.IsNullOrWhiteSpace( attributeValue ) )
            {
                var date = DateTime.MinValue;
                if ( DateTime.TryParse( attributeValue, out date ) && date > DateTime.MinValue )
                {
                    return Path.Combine( System.Web.VirtualPathUtility.ToAbsolute( "~" ), "Assets/Images/bap-e.png" );
                }
            }

            return Path.Combine( System.Web.VirtualPathUtility.ToAbsolute( "~" ), "Assets/Images/bap-d.png" );
        }
    }
}
