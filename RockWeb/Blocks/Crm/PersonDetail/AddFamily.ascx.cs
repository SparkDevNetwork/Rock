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
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    [BooleanField("Require Gender", "Should Gender be required")]
    [BooleanField("Require Grade", "Should Grade by required")]
    [AttributeCategoryField("Category", "The Attribute Categories to display attributes from", true, "Rock.Model.Person")]
    [DefinedValueField(Rock.SystemGuid.DefinedType.LOCATION_LOCATION_TYPE, "Location Type", "The type of location that address should use")]
    public partial class AddFamily : Rock.Web.UI.RockBlock
    {
        private bool _requireGender = false;
        private bool _requireGrade = false;
        private int _childRoleId = 0;

        
        /// <summary>
        /// Gets or sets the index of the current category.
        /// </summary>
        /// <value>
        /// The index of the current category.
        /// </value>
        protected int CurrentCategoryIndex
        {
            get { return ViewState["CurrentCategoryIndex"] as int? ?? 0; }
            set { ViewState["CurrentCategoryIndex"] = value; }
        }
        
        private List<NewFamilyAttributes> attributeControls = new List<NewFamilyAttributes>();

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var campusi = new CampusService().Queryable().OrderBy( a => a.Name ).ToList();
            cpCampus.Campuses = campusi;
            cpCampus.Visible = campusi.Any();

            var childRole = new GroupRoleService().Get(new Guid(Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD));
            if ( childRole != null )
            {
                _childRoleId = childRole.Id;
            }

            bool.TryParse( GetAttributeValue( "RequireGender" ), out _requireGender );
            bool.TryParse( GetAttributeValue( "RequireGrade" ), out _requireGrade );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                CreateControls( new List<GroupMember>(), false );
                AddFamilyMember();
            }
            else
            {
                // Update the name on attribute panels
                if ( CurrentCategoryIndex == 0 )
                {
                    foreach(var familyMemberRow in nfmMembers.FamilyMemberRows)
                    {
                        foreach(var attributeControl in attributeControls)
                        {
                            var attributeRow = attributeControl.AttributesRows.FirstOrDefault( r => r.PersonGuid == familyMemberRow.PersonGuid );
                            if ( attributeRow != null )
                            {
                                attributeRow.PersonName = string.Format("{0} {1}", familyMemberRow.FirstName, familyMemberRow.LastName);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var familyMembers = new List<GroupMember> ();
            List<string> jsonStrings = ViewState["FamilyMembers"] as List<string>;
            jsonStrings.ForEach( j => familyMembers.Add( GroupMember.FromJson( j ) ) );
            CreateControls( familyMembers, false );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var groupMembers = new List<string>();
            GetControlData().ForEach( m => groupMembers.Add( m.ToJson() ) );

            ViewState["FamilyMembers"] = groupMembers;
            return base.SaveViewState();
        }

        /// <summary>
        /// Handles the AddFamilyMemberClick event of the nfmMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void nfmMembers_AddFamilyMemberClick( object sender, EventArgs e )
        {
            AddFamilyMember();
        }

        /// <summary>
        /// Handles the RoleUpdated event of the familyMemberRow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void familyMemberRow_RoleUpdated( object sender, EventArgs e )
        {
            NewFamilyMembersRow row = sender as NewFamilyMembersRow;
            row.ShowGrade = row.RoleId == _childRoleId;
        }

        /// <summary>
        /// Handles the DeleteClick event of the familyMemberRow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void familyMemberRow_DeleteClick( object sender, EventArgs e )
        {
            NewFamilyMembersRow row = sender as NewFamilyMembersRow;

            foreach ( var attributeControl in attributeControls )
            {
                var attributeRow = attributeControl.AttributesRows.FirstOrDefault( r => r.PersonGuid == row.PersonGuid );
                if ( attributeRow != null )
                {
                    attributeControl.Controls.Remove( attributeRow );
                }
            }

            nfmMembers.Controls.Remove( row );
        }

        private void CreateControls( List<GroupMember> familyMembers, bool setSelection )
        {
            // Load all the attribute controls
            attributeControls.Clear();
            pnlAttributes.Controls.Clear();

            foreach ( string categoryGuid in GetAttributeValue( "Category" ).SplitDelimitedValues( false ) )
            {
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( categoryGuid, out guid ) )
                {
                    var category = CategoryCache.Read( guid );
                    if ( category != null )
                    {
                        var attributeControl = new NewFamilyAttributes();
                        attributeControl.ClearRows();
                        pnlAttributes.Controls.Add( attributeControl );
                        attributeControls.Add( attributeControl );
                        attributeControl.ID = "familyAttributes_" + category.Id.ToString();
                        attributeControl.CategoryId = category.Id;

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

            nfmMembers.ClearRows();

            int count = 0;
            foreach ( var familyMember in familyMembers )
            {
                var familyMemberRow = new NewFamilyMembersRow();
                nfmMembers.Controls.Add( familyMemberRow );
                familyMemberRow.ID = string.Format( "row_{0}", familyMember.Person.Guid.ToString().Replace("-", "_") );
                familyMemberRow.RoleUpdated += familyMemberRow_RoleUpdated;
                familyMemberRow.DeleteClick += familyMemberRow_DeleteClick;
                familyMemberRow.PersonGuid = familyMember.Person.Guid;
                familyMemberRow.RequireGender = _requireGender;
                familyMemberRow.RequireGrade = _requireGrade;
                familyMemberRow.RoleId = familyMember.GroupRoleId;
                familyMemberRow.ShowGrade = familyMember.GroupRoleId == _childRoleId;

                if ( setSelection )
                {
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

                foreach(var attributeControl in attributeControls)
                {
                    var attributeRow = new NewFamilyAttributesRow();
                    attributeControl.Controls.Add(attributeRow);
                    attributeRow.ID = string.Format( "{0}_{1}", attributeControl.ID, familyMember.Person.Guid );
                    attributeRow.AttributeList = attributeControl.AttributeList;
                    attributeRow.PersonGuid = familyMember.Person.Guid;

                    if (setSelection)
                    {
                        attributeRow.SetEditValues(familyMember.Person);
                    }
                }

                count++;
            }

            ShowAttributeCategory( CurrentCategoryIndex );
        }

        private List<GroupMember> GetControlData()
        {
            var familyMembers = new List<GroupMember>();

            foreach ( NewFamilyMembersRow row in nfmMembers.FamilyMemberRows )
            {
                var groupMember = new GroupMember();
                groupMember.Person = new Person();
                groupMember.Person.Guid = row.PersonGuid.Value;

                if ( row.RoleId.HasValue )
                {
                    groupMember.GroupRoleId = row.RoleId.Value;
                }
                groupMember.Person.TitleValueId = row.TitleValueId;
                groupMember.Person.GivenName = row.FirstName;
                groupMember.Person.NickName = row.NickName;
                groupMember.Person.LastName = row.LastName;
                groupMember.Person.Gender = row.Gender;
                groupMember.Person.BirthDate = row.BirthDate;
                groupMember.Person.PersonStatusValueId = row.StatusValueId;
                //groupMember.Person.GraduationDate == 

                groupMember.Person.Attributes = new Dictionary<string, AttributeCache>();
                groupMember.Person.AttributeValues = new Dictionary<string, List<AttributeValue>>();

                foreach ( var attributeControl in attributeControls )
                {
                    attributeControl.AttributeList.ForEach( a => {
                        groupMember.Person.Attributes.Add( a.Key, a);
                        groupMember.Person.AttributeValues.Add( a.Key, new List<AttributeValue>() );
                        groupMember.Person.AttributeValues[a.Key].Add( new AttributeValue { AttributeId = a.Id } );
                    });

                    NewFamilyAttributesRow attributeRow = attributeControl.AttributesRows.FirstOrDefault( r => r.PersonGuid == row.PersonGuid );
                    if (attributeRow != null)
                    {
                        attributeRow.GetEditValues( groupMember.Person );
                    }
                }

                familyMembers.Add( groupMember );
            }

            return familyMembers;
        }

        private void AddFamilyMember()
        {
            var rows = nfmMembers.FamilyMemberRows;
            var familyMemberGuid = Guid.NewGuid();

            var familyMemberRow = new NewFamilyMembersRow();
            nfmMembers.Controls.Add( familyMemberRow );
            familyMemberRow.ID = string.Format( "row_{0}", familyMemberGuid.ToString().Replace("-", "_") );
            familyMemberRow.RoleUpdated += familyMemberRow_RoleUpdated;
            familyMemberRow.DeleteClick += familyMemberRow_DeleteClick;
            familyMemberRow.PersonGuid = familyMemberGuid;
            familyMemberRow.Gender = Gender.Unknown;
            familyMemberRow.RequireGender = _requireGender;
            familyMemberRow.RequireGrade = _requireGrade;


            var familyGroupType = new GroupTypeService().Get( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ) );
            if ( familyGroupType != null && familyGroupType.DefaultGroupRoleId.HasValue )
            {
                familyMemberRow.RoleId = familyGroupType.DefaultGroupRoleId;
                familyMemberRow.ShowGrade = familyGroupType.DefaultGroupRoleId == _childRoleId;
            }
            else
            {
                familyMemberRow.ShowGrade = false;
            }

            if ( rows.Count > 0 )
            {
                familyMemberRow.LastName = rows[0].LastName;
            }

            foreach ( var attributeControl in attributeControls )
            {
                var attributeRow = new NewFamilyAttributesRow();
                attributeControl.Controls.Add( attributeRow );
                attributeRow.ID = string.Format( "{0}_{1}", attributeControl.ID, familyMemberGuid );
                attributeRow.AttributeList = attributeControl.AttributeList;
                attributeRow.PersonGuid = familyMemberGuid;
            }
        }

        protected void btnPrevious_Click( object sender, EventArgs e )
        {
            if ( CurrentCategoryIndex > 0 )
            {
                CurrentCategoryIndex--;
                ShowAttributeCategory( CurrentCategoryIndex );
            }
        }

        protected void btnNext_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                if ( CurrentCategoryIndex < attributeControls.Count )
                {
                    CurrentCategoryIndex++;
                    ShowAttributeCategory( CurrentCategoryIndex );
                }
                else
                {
                    var familyMembers = GetControlData();
                    if ( familyMembers.Any() )
                    {

                        RockTransactionScope.WrapTransaction( () =>
                        {
                            using ( new UnitOfWorkScope() )
                            {
                                var groupTypeService = new GroupTypeService();
                                var familyGroupType = groupTypeService.Get( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ) );

                                if ( familyGroupType != null )
                                {
                                    var groupService = new GroupService();
                                    var familyGroup = new Group();
                                    familyGroup.Name = tbFamilyName.Text;
                                    familyGroup.GroupTypeId = familyGroupType.Id;
                                    familyGroup.CampusId = cpCampus.SelectedValueAsInt();
                                    familyMembers.ForEach( m => familyGroup.Members.Add( m ) );

                                    var groupLocation = new GroupLocation();
                                    var location = new LocationService().Get(
                                        tbStreet1.Text, tbStreet2.Text, tbCity.Text, ddlState.SelectedValue, tbZip.Text );
                                    groupLocation.Location = location;

                                    Guid locationTypeGuid = Guid.Empty;
                                    if ( Guid.TryParse( GetAttributeValue( "LocationType" ), out locationTypeGuid ) )
                                    {
                                        var locationType = Rock.Web.Cache.DefinedValueCache.Read( locationTypeGuid );
                                        if (locationType != null)
                                        {
                                            groupLocation.GroupLocationTypeValueId = locationType.Id;
                                        }
                                    }

                                    familyGroup.GroupLocations.Add( groupLocation );

                                    groupService.Add( familyGroup, CurrentPersonId );
                                    groupService.Save( familyGroup, CurrentPersonId );

                                    var groupMemberService = new GroupMemberService();
                                    foreach ( var person in familyMembers.Select( m => m.Person ) )
                                    {
                                        foreach ( var attributeControl in attributeControls )
                                        {
                                            foreach ( var attribute in attributeControl.AttributeList )
                                            {
                                                Rock.Attribute.Helper.SaveAttributeValue( person, attribute, person.GetAttributeValue( attribute.Key ), CurrentPersonId );
                                            }
                                        }
                                    }
                                }
                            }
                        } );

                        Response.Redirect( string.Format( "~/Person/{0}", familyMembers[0].Person.Id ), false );
                    }

                }
            }

        }

        private void ShowAttributeCategory(int index)
        {
            pnlFamilyData.Visible = (index == 0);

            attributeControls.ForEach( c => c.Visible = false);
            if (index > 0 && attributeControls.Count >= index)
            {
                attributeControls[index-1].Visible = true;
            }

            btnPrevious.Visible = index > 0;
            btnNext.Text = index < attributeControls.Count ? "Next" : "Finish";
        }
    }
}