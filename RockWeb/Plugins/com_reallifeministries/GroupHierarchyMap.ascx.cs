using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;

using Rock;
using Rock.Model;
using Rock.Data;
using Rock.Attribute;

namespace com.reallifeministries
{
    [GroupTypesField( "Group Types", "What Group Types to show",true )]
    public partial class GroupHierarchyMap : Rock.Web.UI.RockBlock
    {
        #region Private Variables

        private Group _group = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
    
            var groupId = PageParameter( "GroupId" ).AsInteger();

            if (!(groupId == 0))
            {
                string key = string.Format( "Group:{0}", groupId );
                _group = RockPage.GetSharedItem( key ) as Group;
                if (_group == null)
                {
                    _group = new GroupService( new RockContext() ).Queryable( "GroupType.Roles" )
                        .Where( g => g.Id == groupId )
                        .FirstOrDefault();
                    RockPage.SaveSharedItem( key, _group );
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var pul = new HtmlGenericControl( "ul" );
            pnlGroups.Controls.Add( pul );

            AddChildGroup( _group, pul );  
        }

        #endregion

        private void AddChildGroup(Group childGroup, HtmlGenericControl ul)

        {
            if (childGroup != null)
            {
                var li = new HtmlGenericControl( "li" );
                li.InnerText = childGroup.Name;
                ul.Controls.Add( li );

                var children = childGroup.Groups.ToList();
                if( children.Count > 0 )
                {
                    var sul = new HtmlGenericControl( "ul" );
                    li.Controls.Add( sul );
                    foreach (Group child in children)
                    {
                        AddChildGroup( child, sul );
                    }
                }
            }
        }
        
    }
}