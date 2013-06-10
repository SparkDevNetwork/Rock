//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a bootstrap badge
    /// </summary>
    public class NewFamilyAttributesRow : CompositeControl
    {
        List<Control> attributeControls;

        /// <summary>
        /// Gets or sets the attribute ids.
        /// </summary>
        /// <value>
        /// The attribute ids.
        /// </value>
        public List<AttributeCache> Attributes
        {
            get
            {
                if ( _attributes == null )
                {
                    _attributes = ViewState["Attributes"] as List<AttributeCache>;
                    if ( _attributes == null )
                    {
                        _attributes = new List<AttributeCache>();
                    }
                }
                return _attributes;
            }
            set
            {
                _attributes = value;
                ViewState["Attributes"] = _attributes;
                RecreateChildControls();
            }
        }
        private List<AttributeCache> _attributes = null;

        public NewFamilyAttributesRow()
            : base()
        {
            attributeControls = new List<Control>();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            attributeControls.Clear();

            int i = 0;
            foreach ( var attribute in Attributes )
            {
                Control control = attribute.CreateControl();
                control.ID = string.Format( "{0}_{1}", this.ID, i++ );
                attributeControls.Add( control );
                Controls.Add( control );
            }
        }

        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.RenderBeginTag( HtmlTextWriterTag.Tr );

            foreach ( Control control in attributeControls )
            {
                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                control.RenderControl( writer );
                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }

    }

}