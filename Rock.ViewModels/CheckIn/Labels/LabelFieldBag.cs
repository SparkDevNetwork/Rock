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

using Rock.Enums.CheckIn.Labels;
using Rock.ViewModels.Reporting;

namespace Rock.ViewModels.CheckIn.Labels
{
    /// <summary>
    /// Represents a single field on a check-in label. This contains all the
    /// information required to edit and render this field on the label. All
    /// details regarding the conditions required to display the field are
    /// also included.
    /// </summary>
    public class LabelFieldBag
    {
        /// <summary>
        /// The unique identifier of this field, this is generated automatically
        /// when adding a field to a label.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The type of field. This indicates what process is used to render
        /// this specific field.
        /// </summary>
        public LabelFieldType FieldType { get; set; }

        /// <summary>
        /// <para>
        /// The field sub type. This is specific to each label field type.
        /// </para>
        /// <para>
        /// For example a text field uses this to identify which field data
        /// source objects are available to be selected in the editor.
        /// </para>
        /// </summary>
        public int FieldSubType { get; set; }

        /// <summary>
        /// An optional filter that controls the visibility of this field on
        /// the label.
        /// </summary>
        public FieldFilterGroupBag ConditionalVisibility { get; set; }

        /// <summary>
        /// A value indicating whether the item is included on the generated
        /// preview image that will be stored with the label.
        /// </summary>
        public bool IsIncludedOnPreview { get; set; }

        /// <summary>
        /// The position in inches of the top left corner of the field.
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// The position in inches of the top left corner of the field.
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// The width of the field in inches. This represents the unrotated
        /// width.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// The height of the field in inches. This represents the unrotated
        /// height.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Gets or sets the rotation angle between 0 and 360 degrees.
        /// </summary>
        public double RotationAngle { get; set; }

        /// <summary>
        /// The custom data value used by the field source when rendering
        /// the field.
        /// </summary>
        public Dictionary<string, string> CustomData { get; set; }

        /// <summary>
        /// The dictionary of configuration values. This represents the actual
        /// settings of the field type, such as color or border width.
        /// </summary>
        public Dictionary<string, string> ConfigurationValues { get; set; }
    }
}
