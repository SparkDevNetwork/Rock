//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    [AttributeCategoryField("Category", "The Attribute Category to display attributes from", "Rock.Model.Person")]
    public partial class AddFamily : Rock.Web.UI.RockBlock
    {
        private List<NewFamilyAttributes> attributeControls = new List<NewFamilyAttributes>();

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            cpCampus.Campuses = new CampusService().Queryable().OrderBy( a => a.Name ).ToList();
            cpCampus.Visible = cpCampus.Items.Count > 0;

            pnlAttributes.Controls.Clear();

            foreach ( string categoryGuid in GetAttributeValue( "Category" ).SplitDelimitedValues(false) )
            {
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( categoryGuid, out guid ) )
                {
                    var category = CategoryCache.Read( guid );
                    if ( category != null )
                    {
                        var attributeControl = new NewFamilyAttributes();
                        pnlAttributes.Controls.Add( attributeControl );
                        attributeControls.Add( attributeControl );
                        attributeControl.ID = "familyAttributes_" + category.Id.ToString();

                        foreach ( var attribute in new AttributeService().GetByCategoryId( category.Id ) )
                        {
                            if ( attribute.IsAuthorized( "Edit", CurrentPerson ) )
                            {
                                attributeControl.AttributeList.Add( AttributeCache.Read( attribute ) );
                            }
                        }
                    }
                }
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                AddFamilyMember();
            }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            CreateControls( ViewState["FamilyMembers"] as List<string>, false );
        }

        protected override object SaveViewState()
        {
            ViewState["FamilyMembers"] = GetControlData();
            return base.SaveViewState();
        }

        protected void nfmMembers_AddFamilyMemberClick( object sender, EventArgs e )
        {
            AddFamilyMember();
        }

        void familyMemberRow_DeleteClick( object sender, EventArgs e )
        {
            NewFamilyMembersRow row = sender as NewFamilyMembersRow;
            row.Parent.Controls.Remove( row );
        }

        private void CreateControls( List<string> familyMembers, bool setSelection )
        {
            nfmMembers.ClearRows();

            int count = 1;
            foreach ( string json in familyMembers )
            {
                NewFamilyMembersRow familyMemberRow = new NewFamilyMembersRow();
                nfmMembers.Controls.Add( familyMemberRow );
                familyMemberRow.ID = string.Format( "nfmMembers_row_{1}", nfmMembers.ID, count );

                familyMemberRow.DeleteClick += familyMemberRow_DeleteClick;
                if ( setSelection )
                {
                    var familyMember = GroupMember.FromJson( json );
                    familyMemberRow.RoleId = familyMember.GroupRoleId;

                    if ( familyMember.Person != null )
                    {
                        familyMemberRow.TitleValueId = familyMember.Person.TitleValueId;
                        familyMemberRow.FirstName = familyMember.Person.GivenName;
                        familyMemberRow.NickName = familyMember.Person.NickName;
                        familyMemberRow.LastName = familyMember.Person.LastName;
                        familyMemberRow.Gender = familyMember.Person.Gender;
                        familyMemberRow.BirthDate = familyMember.Person.BirthDate;
                        familyMemberRow.StatusValueId = familyMember.Person.PersonStatusValueId;
                    }
                }

                count++;
            }
        }

        private List<string> GetControlData()
        {
            var familyMembers = new List<string>();

            foreach ( Control control in nfmMembers.Controls )
            {
                NewFamilyMembersRow familyMemberRow = control as NewFamilyMembersRow;
                if ( familyMemberRow != null )
                {
                    var groupMember = new GroupMember();
                    groupMember.Person = new Person();

                    if ( familyMemberRow.RoleId.HasValue )
                    {
                        groupMember.GroupRoleId = familyMemberRow.RoleId.Value;
                    }
                    groupMember.Person.TitleValueId = familyMemberRow.TitleValueId;
                    groupMember.Person.GivenName = familyMemberRow.FirstName;
                    groupMember.Person.NickName = familyMemberRow.NickName;
                    groupMember.Person.LastName = familyMemberRow.LastName;
                    groupMember.Person.Gender = familyMemberRow.Gender;
                    groupMember.Person.BirthDate = familyMemberRow.BirthDate;
                    groupMember.Person.PersonStatusValueId = familyMemberRow.StatusValueId;

                    familyMembers.Add( groupMember.ToJson() );
                }
            }

            return familyMembers;
        }

        private void AddFamilyMember()
        {
            var rows = nfmMembers.FamilyMemberRows;

            var familyMemberRow = new NewFamilyMembersRow();
            nfmMembers.Controls.Add( familyMemberRow );
            familyMemberRow.ID = string.Format( "nfmMembers_row_{1}", nfmMembers.ID, rows.Count + 1 );

            if ( rows.Count > 0 )
            {
                familyMemberRow.LastName = rows[0].LastName;
            }

            foreach ( var attributeControl in attributeControls )
            {
                var attributeRow = new NewFamilyAttributesRow();
                attributeControl.Controls.Add( attributeRow );
                attributeRow.AttributeList = attributeControl.AttributeList;
                attributeRow.ID = string.Format( "attribute_row_{0}_{1}", attributeControl.ID, rows.Count + 1 );
            }
        }
    }
}