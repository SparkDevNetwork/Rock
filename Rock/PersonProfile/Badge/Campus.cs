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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Campus Badge
    /// </summary>
    [Description("Campus Badge")]
    [Export(typeof(BadgeComponent))]
    [ExportMetadata("ComponentName", "Campus")]
    public class Campus : HighlightLabelBadge
    {

        /// <summary>
        /// Gets the badge label
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override HighlightLabel GetLabel(Person person)
        {
            if (ParentPersonBlock != null)
            {
                // Campus is associated with the family group(s) person belongs to.
                var families = ParentPersonBlock.PersonGroups(Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY);
                if (families != null)
                {
                    var label = new HighlightLabel();
                    label.LabelType = LabelType.Campus;

                    var campusNames = new List<string>();
                    foreach (int campusId in families
                        .Where(g => g.CampusId.HasValue)
                        .Select(g => g.CampusId)
                        .Distinct()
                        .ToList())
                        campusNames.Add(Rock.Web.Cache.CampusCache.Read(campusId).Name);

                    label.Text = campusNames.OrderBy(n => n).ToList().AsDelimited(", ");

                    return label;
                }
            }

            return null;

        }

    }
}
