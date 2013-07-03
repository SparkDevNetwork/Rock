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
        /// Gets or sets the person GUID.
        /// </summary>
        /// <value>
        /// The person GUID.
        /// </value>
        public Guid? PersonGuid
        {
            get
            {
                if ( ViewState["PersonGuid"] != null )
                {
                    return (Guid)ViewState["PersonGuid"];
                }
                else
                {
                    return Guid.Empty;
                }
            }
            set { ViewState["PersonGuid"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonName
        {
            get { return ViewState["PersonName"] as string ?? string.Empty; }
            set { ViewState["PersonName"] = value; }
        }

        /// <summary>
        /// Gets or sets the attribute ids.
        /// </summary>
        /// <value>
        /// The attribute ids.
        /// </value>
        public List<AttributeCache> AttributeList
        {
            get
            {
                if ( _attributeList == null )
                {
                    _attributeList = ViewState["AttributeList"] as List<AttributeCache>;
                    if ( _attributeList == null )
                    {
                        _attributeList = new List<AttributeCache>();
                    }
                }
                return _attributeList;
            }
            set
            {
                _attributeList = value;
                ViewState["AttributeList"] = _attributeList;
                RecreateChildControls();
            }
        }
        private List<AttributeCache> _attributeList = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewFamilyAttributesRow" /> class.
        /// </summary>
        public NewFamilyAttributesRow()
            : base()
        {
            attributeControls = new List<Control>();
        }

        /// <summary>
        /// Sets the edit values.
        /// </summary>
        /// <param name="person">The person.</param>
        public void SetEditValues(Person person)
        {
            EnsureChildControls();

            int i = 0;
            foreach ( var attribute in AttributeList )
            {
                attribute.FieldType.Field.SetEditValue(attributeControls[i], attribute.QualifierValues, person.GetAttributeValue(attribute.Key));
                i++;
            }
        }

        /// <summary>
        /// Gets the edit values.
        /// </summary>
        /// <param name="person">The person.</param>
        public void GetEditValues( Person person )
        {
            EnsureChildControls();

            int i = 0;
            foreach ( var attribute in AttributeList )
            {
                person.SetAttributeValue( attribute.Key, attribute.FieldType.Field.GetEditValue( attributeControls[i], attribute.QualifierValues ) );
                i++;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            attributeControls.Clear();

            int i = 0;
            foreach ( var attribute in AttributeList )
            {
                Control control = attribute.CreateControl();
                control.ID = string.Format( "{0}_{1}", this.ID, i++ );

                if ( control is Rock.Web.UI.Controls.IRequiredControl )
                {
                    var requiredControl = control as Rock.Web.UI.Controls.IRequiredControl;
                    requiredControl.Required = attribute.IsRequired;
                    requiredControl.RequiredErrorMessage = attribute.Name + " is required.";
                }

                attributeControls.Add( control );
                Controls.Add( control );
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.AddAttribute( "rowid", ID );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.Write( PersonName );
                writer.RenderEndTag();

                foreach ( Control control in attributeControls )
                {
                    string errorCss = string.Empty;
                    if ( control is Rock.Web.UI.Controls.IRequiredControl )
                    {
                        var requiredControl = control as Rock.Web.UI.Controls.IRequiredControl;
                        errorCss = requiredControl.IsValid ? "" : " error";
                    }

                    writer.RenderBeginTag( HtmlTextWriterTag.Td );
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-group" + errorCss );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    control.RenderControl( writer );
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }

                writer.RenderEndTag();
            }
        }

    }

}