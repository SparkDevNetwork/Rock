//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Field Type used to display a dropdown list or radio list of custom values
    /// </summary>
    public class SelectSingle : Field
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectSingle"/> class.
        /// </summary>
        public SelectSingle()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectSingle"/> class.
        /// </summary>
        /// <param name="qualifierValues">The qualifier values.</param>
        public SelectSingle( Dictionary<string, KeyValuePair<string, string>> qualifierValues )
            : base( qualifierValues )
        {
        }

        private List<FieldQualifier> qualifiers = null;
        /// <summary>
        /// Gets the qualifiers.
        /// </summary>
        public override List<FieldQualifier> Qualifiers
        {
            get
            {
                if ( qualifiers == null )
                {
                    qualifiers = new List<FieldQualifier>();

                    qualifiers.Add( new FieldQualifier(
                        "Values",
                        "Values",
                        "Comma-Separated list of values for user to select from",
                        new Text() ) );

                    Dictionary<string, KeyValuePair<string, string>> options = new Dictionary<string, KeyValuePair<string, string>>();
                    options.Add( "Values", new KeyValuePair<string, string>( "Values", string.Format( "{0}, {1}",
                        "Radio Buttons", "Drop Down List" ) ) );
                    options.Add( "FieldType", new KeyValuePair<string, string>( "Field Type", "Drop Down List" ) );

                    qualifiers.Add( new FieldQualifier(
                        "FieldType",
                        "Field Type",
                        "",
                        new SelectSingle(options) ) );
                }

                return qualifiers;
            }
        }

        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool IsValid( string value, bool required, out string message )
        {
            if (!this.QualifierValues["Values"].Value.Split( ',' ).Contains(value))
            {
                message = string.Format( "'{0}' {1}", value, "is not a valid value" );
                return false;
            }

            return base.IsValid( value, required, out message );
        }

        /// <summary>
        /// Renders the controls neccessary for prompting user for a new value and adds them to the parentControl
        /// </summary>
        /// <param name="value"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public override Control CreateControl(string value, bool required, bool setValue)
        {
            ListControl listControl;

            if ( this.QualifierValues["FieldType"].Value == "Radio Buttons" )
            {
                RadioButtonList rbl = new RadioButtonList();
                rbl.RepeatLayout = RepeatLayout.Flow;
                rbl.RepeatDirection = RepeatDirection.Horizontal;
                listControl = rbl;
            }
            else
                listControl = new DropDownList();

            foreach ( string option in this.QualifierValues["Values"].Value.Split( ',' ) )
            {
                ListItem li = new ListItem( option, option );
                if (setValue)
                    li.Selected = li.Value == value;
                listControl.Items.Add( li );
            }

            return listControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public override string ReadValue( Control control )
        {
            if ( control != null && control is ListControl )
                return ( ( ListControl )control ).SelectedValue;
            return null;
        }
    }
}