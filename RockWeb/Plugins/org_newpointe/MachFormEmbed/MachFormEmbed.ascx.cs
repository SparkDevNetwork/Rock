using System;
using System.ComponentModel;
using System.Linq;
using Rock;
using Rock.Model;
using Rock.Attribute;

namespace RockWeb.Plugins.org_newpointe.MachFormEmbed
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Embed MachForm" )]
    [Category( "NewPointe.org Web Blocks" )]
    [Description( "Embed a MachForm." )]
    [TextField( "Machform Server Root", "Address of the MachForm Server", true, "https://forms.newpointe.org", "", 1 )]
    [IntegerField( "Form ID", "The ID of the form.", true, 18093, "", 2 )]
    [IntegerField( "Height", "The height of the form.", true, 595, "", 3 )]
    [IntegerField( "Name Element Id", "Element that contains the person's name.", false, 1, "", 4 )]
    [IntegerField( "Email Element Id", "Element that contains the person's email address.", false, 1, "", 5 )]
    [KeyValueListField( "Passed Paramaters", "The Element ID is the ID of the form element on this MachForm.  The URL Paramater is the Paramater to look for the the Query String.", false, "", "ElementId", "URL Paramater", "", "", "", 6 )]
    public partial class MachFormEmbed : Rock.Web.UI.RockBlock
    {
        public string MFParameters;
        public string FormScript;

        protected void Page_Load( object sender, EventArgs e )
        {
            // Set URL Paramaters
            MFParameters += string.Join( "", GetAttributeValue( "PassedParamaters" ).ToKeyValuePairList().Select( pair => "&element_" + pair.Key + "=" + PageParameter( pair.Value.ToString() ) ) );

            // Insert the logged in person's name if they are logged in and a name element is defined.
            if ( CurrentPerson != null )
            {
                string NameElement = GetAttributeValue( "NameElementId" );
                string EmailElement = GetAttributeValue( "EmailElementId" );
                if ( !string.IsNullOrWhiteSpace( NameElement ) )
                {
                    MFParameters += "&element_" + NameElement + "_1=" + CurrentPerson.NickName;
                    MFParameters += "&element_" + NameElement + "_2=" + CurrentPerson.LastName;
                }
                if ( !string.IsNullOrWhiteSpace( EmailElement ) )
                {
                    MFParameters += "&element_" + EmailElement + "=" + CurrentPerson.Email;
                }

            }

            //Generate Form Javascript
            FormScript = String.Format( @"<script type='text/javascript'>
            var __machform_url = '{0}/embed.php?id={1}{2}';
            var __machform_height = {3};
            </script>
            <div id='mf_placeholder'></div>
            <script type='text/javascript' src='https://forms.newpointe.org/js/jquery.ba-postmessage.min.js'></script>
            <script type='text/javascript' src='https://forms.newpointe.org/js/machform_loader.js'></script>", GetAttributeValue( "MachformServerRoot" ), GetAttributeValue( "FormID" ), MFParameters, GetAttributeValue( "Height" ) );
        }
    }
}