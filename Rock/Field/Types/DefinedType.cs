//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Field.Types
    
    /// <summary>
    /// Field Type used to display a dropdown list of Defined Types
    /// </summary>
    [Serializable]
    public class DefinedType : SelectSingle
        
        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues )
            
            ListControl editControl = new DropDownList();

            Rock.Core.DefinedTypeService definedTypeService = new Core.DefinedTypeService();
            foreach ( var definedType in definedTypeService.Queryable().OrderBy( d => d.Order ) )
                editControl.Items.Add( new ListItem( definedType.Name, definedType.Id.ToString() ) );

            return editControl;
        }
    }
}