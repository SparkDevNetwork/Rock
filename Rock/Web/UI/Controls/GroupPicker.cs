//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupPicker : ItemPicker
    {
        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="group">The group.</param>
        public void SetValue( Rock.Model.Group group )
        {
            if ( group != null )
            {
                ItemId = group.Id.ToString();
                
                string parentGroupIds = string.Empty;
                var parentGroup = group.ParentGroup;
                while ( parentGroup != null )
                {
                    parentGroupIds = parentGroup.Id + "," + parentGroupIds;
                    parentGroup = parentGroup.ParentGroup;
                }

                InitialItemParentIds = parentGroupIds.TrimEnd( new char[] { ',' } );
                ItemName = group.Name;
            }
            else
            {
                ItemId = Rock.Constants.None.IdValue;
                ItemName = Rock.Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected override void SetValueOnSelect()
        {
            var group = new GroupService().Get( int.Parse( ItemId ) );
            SetValue( group );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/groups/getchildren/"; }
        }

        /// <summary>
        /// Gets the item rest URL extra params.
        /// </summary>
        /// <value>
        /// The item rest URL extra params.
        /// </value>
        public override string ItemRestUrlExtraParams
        {
            get { return "/0/false/0"; }
        }

        
    }
}