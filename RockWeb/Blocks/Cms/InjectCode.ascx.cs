//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using Rock.Attribute;
using CSScriptLibrary;

namespace RockWeb.Blocks.Cms
{
    [CodeEditorField("Code", "Source code to compile and put results on page.", true, "using%20System%3B%0Ausing%20Rock%3B%0Ausing%20Rock.Data%3B%0Ausing%20Rock.Model%3B%0Ausing%20Rock.Web%3B%0Ausing%20Rock.Web.UI%3B%0Ausing%20Rock.Web.UI.Controls%3B%0A%0Apublic%20class%20Script%0A%7B%20%0A%20%20%20%20%2F%2F%20the%20execute%20method%20method%20will%20be%20called%20by%20the%20block%0A%20%20%20%20%2F%2F%20the%20string%20returned%20will%20be%20placed%20on%20the%20page%0A%20%20%20%20static%20string%20Execute()%0A%20%20%20%20%7B%0A%20%20%20%20%20%20%20%20%2F%2F%20your%20code%20here%0A%0A%20%20%20%20%20%20%20%20return%20%22%22%3B%20%0A%20%20%20%20%7D%0A%7D")]
    public partial class InjectCode : Rock.Web.UI.RockBlock
    {

        protected override void OnInit(EventArgs e)
        {
            this.EnableViewState = false;

            base.OnInit(e);

            this.AttributesUpdated += InjectCode_AttributesUpdated;
        }
        
        protected override void OnLoad( EventArgs e )
        {
            ExecuteScript();
        }

        /// <summary>
        /// Handles the AttributesUpdated event of the Inject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void InjectCode_AttributesUpdated(object sender, EventArgs e)
        {
            ExecuteScript();
        }

        protected void ExecuteScript()
        {
            // todo only show errors if user has edit rights to this block

            // put code results on the page
            string scriptSource = GetAttributeValue("Code");
            string scriptOutput = string.Empty;

            try
            {
                dynamic script = CSScript.Evaluator.LoadCode(scriptSource);
                scriptOutput = script.Execute();
            }
            catch (Exception ex)
            {
                scriptOutput = "<div class='alert alert-warning'><h4>Script Error</h4><pre>" + ex.Message + "</pre></div>";
            }

            lOutput.Text += scriptOutput;
        }
    }
}