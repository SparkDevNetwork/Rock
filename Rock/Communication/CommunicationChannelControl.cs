//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.Communication
{
    /// <summary>
    /// abstract class for controls used to render a communication channel
    /// </summary>
    public abstract class CommunicationChannelControl : System.Web.UI.UserControl
    {
        public abstract void SetControlProperties(Rock.Model.Communication communication);
        public abstract void GetControlProperties(Rock.Model.Communication communication);
    }
}