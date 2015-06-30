using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Use DynamicControlsPanel as a container for controls that will be changed at runtime 
    /// and it is difficult or not possible to create the controls early enough in the page lifecycle to avoid a viewstate issue
    /// Note: there is a small performance hit 
    /// From the Help: 
    ///     The ViewStateModeByIdAttribute class is used to specify a control that requires view-state loading by ID. 
    ///     The default view-state loading behavior is for ASP.NET to load the view-state information for a control by 
    ///     its index in the control tree of the page. There is a performance cost for loading view-state information
    ///     by ID because the page control tree must be searched for the control specifically before loading its view-state information.
    /// </summary>
    [ViewStateModeById]
    public class DynamicControlsPanel : Panel
    {
    }
}
