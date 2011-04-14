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

        public SelectSingle( Dictionary<string, string> qualifierValues )
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
                        "Comma-Separated list of values for user to select from",
                        new Text() ) );

                    Dictionary<string, string> options = new Dictionary<string, string>();
                    options.Add( "Values", "Radio Buttons, Drop Down List" );
                    options.Add( "Field Type", "Drop Down List" );

                    qualifiers.Add( new FieldQualifier(
                        "Field Type",
                        "",
                        new SelectSingle(options) ) );
                }

                return qualifiers;
            }
        }

        public override bool IsValid( string value, out string message )
        {
            if (!this.QualifierValues["Values"].Split( ',' ).Contains(value))
            {
                message = "'" + value + "' is not a valid value";
                return false;
            }

            return base.IsValid( value, out message );
        }

        public override Control CreateControl( string value )
        {
            ListControl listControl;

            if ( this.QualifierValues["Field Type"] == "Radio Buttons" )
            {
                RadioButtonList rbl = new RadioButtonList();
                rbl.RepeatLayout = RepeatLayout.Flow;
                rbl.RepeatDirection = RepeatDirection.Horizontal;
                listControl = rbl;
            }
            else
                listControl = new DropDownList();

            foreach ( string option in this.QualifierValues["Values"].Split( ',' ) )
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