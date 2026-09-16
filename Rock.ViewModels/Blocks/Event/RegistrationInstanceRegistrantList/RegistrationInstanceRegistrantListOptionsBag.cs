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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceRegistrantList
{
    /// <summary>
    /// The additional configuration options for the Registration Instance - Registrant List block.
    /// </summary>
    public class RegistrationInstanceRegistrantListOptionsBag
    {
        /// <summary>
        /// Gets or sets the title to use when exporting the grid.
        /// </summary>
        public string ExportTitle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the registration template
        /// the registration instance belongs to. Used to scope person
        /// preference keys so filter state does not leak between templates.
        /// </summary>
        public Guid? RegistrationTemplateGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the signed document column
        /// should be shown. True when the registration template requires a
        /// signature document.
        /// </summary>
        public bool IsSignedDocumentColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Communicate to
        /// Registrars" grid action should be shown. True when the current
        /// person is authorized to view the site's communication page.
        /// </summary>
        public bool IsRegistrarCommunicationVisible { get; set; }

        /// <summary>
        /// Gets or sets the person field types that the registration template
        /// has configured to show on the grid. Each entry causes the matching
        /// column to be rendered.
        /// </summary>
        public List<RegistrationPersonFieldType> VisiblePersonFieldTypes { get; set; }

        /// <summary>
        /// Gets or sets the placements configured on the registration
        /// template. Each entry renders a placement button in the Placements
        /// column.
        /// </summary>
        public List<RegistrantPlacementConfigBag> Placements { get; set; }

        /// <summary>
        /// Gets or sets the title of the mobile phone column, taken from the
        /// phone type's defined value so renamed phone types are reflected.
        /// </summary>
        public string MobilePhoneLabel { get; set; }

        /// <summary>
        /// Gets or sets the title of the home phone column, taken from the
        /// phone type's defined value so renamed phone types are reflected.
        /// </summary>
        public string HomePhoneLabel { get; set; }

        /// <summary>
        /// Gets or sets the title of the work phone column, taken from the
        /// phone type's defined value so renamed phone types are reflected.
        /// </summary>
        public string WorkPhoneLabel { get; set; }
    }
}
