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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Workflow Action Form Row Editor
    /// </summary>
    public class WorkflowFormAttributeRow : CompositeControl
    {
        private HiddenField _hfOrder;
        private CheckBox _cbVisible;
        private CheckBox _cbEditable;
        private CheckBox _cbRequired;

        /// <summary>
        /// Gets or sets the attribute unique identifier.
        /// </summary>
        /// <value>
        /// The attribute unique identifier.
        /// </value>
        public Guid AttributeGuid
        {
            get { return ViewState["AttributeGuid"] as Guid? ?? Guid.Empty; }
            set { ViewState["AttributeGuid"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        /// <value>
        /// The name of the attribute.
        /// </value>
        public string AttributeName
        {
            get { return ViewState["AttributeName"] as string ?? string.Empty; }
            set { ViewState["AttributeName"] = value; }
        }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid
        {
            get { return ViewState["Guid"] as Guid? ?? Guid.Empty; }
            set { ViewState["Guid"] = value; }
        }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order
        {
            get
            {
                EnsureChildControls();
                return _hfOrder.Value.AsInteger();
            }
            set
            {
                EnsureChildControls();
                _hfOrder.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is visible]; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible
        {
            get
            {
                EnsureChildControls();
                return _cbVisible.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbVisible.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is editable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is editable]; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditable
        {
            get
            {
                EnsureChildControls();
                return _cbEditable.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbEditable.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is required]; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired
        {
            get
            {
                EnsureChildControls();
                return _cbRequired.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbRequired.Checked = value;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _hfOrder = new HiddenField();
            _hfOrder.ID = this.ID + "_hfOrder";
            Controls.Add( _hfOrder );

            _cbVisible = new CheckBox();
            _cbVisible.ID = this.ID + "_cbVisible";
            Controls.Add( _cbVisible );

            _cbEditable = new CheckBox();
            _cbEditable.ID = this.ID + "_cbEditable";
            Controls.Add( _cbEditable );

            _cbRequired = new CheckBox();
            _cbRequired.ID = this.ID + "_cbRequired";
            Controls.Add( _cbRequired );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if (this.Visible)
            {
                writer.AddAttribute( "data-key", Guid.ToString() );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "grid-columncommand" );
                writer.AddAttribute( HtmlTextWriterAttribute.Align, "center" );
                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                _hfOrder.RenderControl( writer );
                writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "minimal workflow-formfield-reorder" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-bars" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();      // I
                writer.RenderEndTag();      // A
                writer.RenderEndTag();      // Td

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.Write( AttributeName );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "grid-select-field" );
                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                _cbVisible.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "grid-select-field" );
                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                _cbEditable.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "grid-select-field" );
                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                _cbRequired.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }
    }
}
