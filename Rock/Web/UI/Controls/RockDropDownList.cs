using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Custom DropDownList control that retains options groups after post-back
    /// </summary>
    public class RockDropDownList : DropDownList
    {
        /// <summary>
        /// Saves the current view state of the <see cref="T:System.Web.UI.WebControls.ListControl" /> -derived control and the items it contains.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object" /> that contains the saved state of the <see cref="T:System.Web.UI.WebControls.ListControl" /> control.
        /// </returns>
        protected override object SaveViewState()
        {
            object[] allStates = new object[this.Items.Count + 1];
            allStates[0] = base.SaveViewState();

            int i = 1;
            foreach ( ListItem li in this.Items )
            {
                allStates[i++] = li.Attributes["optiongroup"];
            }

            return allStates;
        }

        /// <summary>
        /// Loads the previously saved view state of the <see cref="T:System.Web.UI.WebControls.DetailsView" /> control.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the state of the <see cref="T:System.Web.UI.WebControls.ListControl" /> -derived control.</param>
        protected override void LoadViewState( object savedState )
        {
            if ( savedState != null )
            {
                object[] allStates = (object[])savedState;
                if ( allStates[0] != null )
                {
                    base.LoadViewState( allStates[0] );
                }

                int i = 1;
                foreach ( ListItem li in this.Items )
                {
                    li.Attributes["optiongroup"] = (string)allStates[i++];
                }
            }
        }
    }
}
