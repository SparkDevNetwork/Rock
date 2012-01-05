//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

using Rock.Web.UI.Controls;

namespace RockWeb.Blocks
{
    [Rock.Attribute.Property( 0, "Script", "IronPython script to run.", true )]
    public partial class ScriptHost : Rock.Web.UI.Block
    {

        protected override void OnLoad( EventArgs e )
        {
            ScriptEngine scriptEngine = Python.CreateEngine();
            ScriptScope scriptScope = scriptEngine.CreateScope();
            ScriptSource source = scriptEngine.CreateScriptSourceFromString( AttributeValue( "Script" ) );
            source.Execute( scriptScope );
        }
    }
}