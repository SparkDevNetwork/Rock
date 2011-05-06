using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Field Type used to display a dropdown list of System.Drawing.Color options
    /// </summary>
    public class SelectSingle : Field
    {
        public SelectSingle()
            : base()
        {
        }

        public SelectSingle( Dictionary<string, KeyValuePair<string, string>> qualifierValues )
            : base( qualifierValues )
        {
        }

        private List<FieldQualifier> qualifiers = null;
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

        public override bool IsValid( string value, out string message )
        {
            if (!this.QualifierValues["Values"].Value.Split( ',' ).Contains(value))
            {
                message = string.Format( "'{0}' {1}", value, "is not a valid value" );
                return false;
            }

            return base.IsValid( value, out message );
        }

        public override Control CreateControl( string value )
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
                li.Selected = li.Value == value;
                listControl.Items.Add( li );
            }

            return listControl;
        }

        public override string ReadValue( Control control )
        {
            if ( control != null && control is ListControl )
                return ( ( ListControl )control ).SelectedValue;
            return null;
        }
    }
}