using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Models.Core;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Interface that a custom field type must implement
    /// </summary>
    public interface IFieldType
    {
        List<FieldQualifier> Qualifiers { get; }
        Dictionary<string, KeyValuePair<string, string>> QualifierValues { get; set;  }
        string FormatValue( string value, bool condensed );
        Control CreateControl( string value );
        string ReadValue( Control control );
    }
}
