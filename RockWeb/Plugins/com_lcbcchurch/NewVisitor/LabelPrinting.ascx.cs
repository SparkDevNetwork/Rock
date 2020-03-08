// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.MergeTemplates;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_lcbcchurch.NewVisitor
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Label Printing" )]
    [Category( "LCBC > New Visitor" )]
    [Description( "Used for printing label using a pre-defined template." )]

    #region Block Attributes
    [DataViewField( "Data View",
        entityTypeName: "Rock.Model.Person",
        Description = " This will provide the list of people that would be shown.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKeys.DataView )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON,
        name: "Date Attribute",
        description: "The date person attribute that should be updated with the current date when marking as complete.",
        required: true,
        allowMultiple: false,
        order: 1,
        Key = AttributeKeys.DateAttribute )]
    [TextField( "Header Title",
        Description = "The title to put into the block.",
        IsRequired = true,
        Key = AttributeKeys.HeaderTitle,
        Order = 2 )]
    [TextField( "Header Icon Css Class",
        Description = "The icon to show in the panel header.",
        IsRequired = false,
        Key = AttributeKeys.HeaderIconCssClass,
        Order = 3 )]
    [BooleanField( "Enable Campus Filter",
        defaultValue: true,
        Description = "Determines if the campus filter should be shown / enabled.",
        Key = AttributeKeys.EnableCampusFilter,
        IsRequired = true,
        Order = 4
        )]
    [MergeTemplateField( "Merge Template",
        Description = "The merge template to use.",
        IsRequired = true,
        Key = AttributeKeys.MergeTemplate,
        Order = 5 )]
    [BooleanField( "Combine Family Members",
        defaultValue: false,
        Description = "Set this to combine family members into a single row. For example, Ted Decker and Cindy Decker would be combined into 'Ted & Cindy Decker'.",
        Key = AttributeKeys.CombineFamilyMembers,
        IsRequired = true,
        Order = 6
        )]
    #endregion Block Attributes

    public partial class LabelPrinting : RockBlock
    {

        #region Attribute Keys

        protected static class AttributeKeys
        {
            public const string DataView = "DataView";
            public const string DateAttribute = "DateAttribute";
            public const string MergeTemplate = "MergeTemplate";
            public const string EnableCampusFilter = "EnableCampusFilter";
            public const string HeaderTitle = "HeaderTitle";
            public const string HeaderIconCssClass = "HeaderIconCssClass";
            public const string CombineFamilyMembers = "CombineFamilyMembers";
        }

        #endregion Attribute Keys
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            SetPanel();

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                if ( IsBlockSettingValid() )
                {
                    ShowView();
                }
            }
            else
            {
                nbWarningMessage.Visible = false;
                nbSuccess.Visible = false;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetPanel();
            if ( IsBlockSettingValid() )
            {
                ShowView();
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbSelectAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void cbSelectAll_CheckedChanged( object sender, EventArgs e )
        {
            foreach ( ListItem item in cblPerson.Items )
            {
                item.Selected = cbSelectAll.Checked;
            }
        }

        /// <summary>
        /// Handles the selection index change event of the cpCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            var campusId = cpCampus.SelectedCampusId;
            hfCampusId.Value = campusId.ToStringSafe();
            cbSelectAll.Checked = false;
            BindPersonChechboxList( campusId );
        }

        /// <summary>
        /// Handles the Click event of the btnMerge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMerge_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var personIds = cblPerson.SelectedValuesAsInt;
            if ( personIds.Count <= 0 )
            {
                foreach ( ListItem item in cblPerson.Items )
                {
                    personIds.Add( item.Value.AsInteger() );
                }
            }

            if ( personIds.Count == 0 )
            {
                nbWarningMessage.Text = "No person found for label printing.";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }

            List<object> mergeObjectsList = GetMergeObjectList( rockContext, personIds );

            MergeTemplate mergeTemplate = new MergeTemplateService( rockContext ).Get( GetAttributeValue( AttributeKeys.MergeTemplate ).AsGuid() );
            if ( mergeTemplate == null )
            {
                nbWarningMessage.Text = "Unable to get merge template";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }

            MergeTemplateType mergeTemplateType = this.GetMergeTemplateType( rockContext, mergeTemplate );
            if ( mergeTemplateType == null )
            {
                nbWarningMessage.Text = "Unable to get merge template type";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }

            var campusId = hfCampusId.ValueAsInt();
            var campus = CampusCache.Get( campusId );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            if ( campus != null )
            {
                mergeFields.AddOrReplace( "SelectedCampus", campus );
            }
            BinaryFile outputBinaryFileDoc = null;

            try
            {
                outputBinaryFileDoc = mergeTemplateType.CreateDocument( mergeTemplate, mergeObjectsList, mergeFields );

                if ( mergeTemplateType.Exceptions != null && mergeTemplateType.Exceptions.Any() )
                {
                    if ( mergeTemplateType.Exceptions.Count == 1 )
                    {
                        this.LogException( mergeTemplateType.Exceptions[0] );
                    }
                    else if ( mergeTemplateType.Exceptions.Count > 50 )
                    {
                        this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions for top 50.", mergeTemplate.Name ), mergeTemplateType.Exceptions.Take( 50 ).ToList() ) );
                    }
                    else
                    {
                        this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions", mergeTemplate.Name ), mergeTemplateType.Exceptions.ToList() ) );
                    }
                }

                string getFileUrl = string.Format( "{0}?Guid={1}&attachment=true", ResolveRockUrl( "~/GetFile.ashx" ), outputBinaryFileDoc.Guid );
                Response.Redirect( getFileUrl, false );
                Context.ApplicationInstance.CompleteRequest();
            }
            catch ( Exception ex )
            {
                this.LogException( ex );
                if ( ex is System.FormatException )
                {
                    nbMergeError.Text = "Error loading the merge template. Please verify that the merge template file is valid.";
                }
                else
                {
                    nbMergeError.Text = "An error occurred while merging";
                }

                nbMergeError.Details = ex.Message;
                nbMergeError.Visible = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdate_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var personIds = cblPerson.SelectedValuesAsInt;
            if ( personIds.Count <= 0 )
            {
                foreach ( ListItem item in cblPerson.Items )
                {
                    personIds.Add( item.Value.AsInteger() );
                }
            }

            if ( personIds.Count == 0 )
            {
                nbWarningMessage.Text = "No person found for updating.";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }

            var attributeGuid = GetAttributeValue( AttributeKeys.DateAttribute ).AsGuid();
            var attribute = AttributeCache.Get( attributeGuid );

            if ( attribute == null )
            {
                return;
            }

            var currentDate = RockDateTime.Now;
            var persons = new PersonService( rockContext ).GetByIds( personIds );
            persons.LoadAttributes( rockContext );
            
            foreach ( var person in persons )
            {
                person.SetAttributeValue( attribute.Key, currentDate );
                person.SaveAttributeValues();
            }

            nbSuccess.Text = string.Format( "{0} people updated.", persons.Count() );
            nbSuccess.NotificationBoxType = NotificationBoxType.Success;
            nbSuccess.Visible = true;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Display the Panel
        /// </summary>
        private void ShowView()
        {
            int? campusId = PageParameter( "campusId" ).AsIntegerOrNull();
            if ( !campusId.HasValue )
            {
                campusId = CurrentPerson.GetCampusIds().FirstOrDefault();
            }
            hfCampusId.Value = campusId.ToStringSafe();
            cpCampus.SetValue( campusId );
            cbCombineFamilyMembers.Visible = !GetAttributeValue( AttributeKeys.CombineFamilyMembers ).AsBoolean();
            BindPersonChechboxList( campusId );
        }


        /// <summary>
        /// Gets the merge object list for the current EntitySet
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="fetchCount">The fetch count.</param>
        /// <returns></returns>
        private List<object> GetMergeObjectList( RockContext rockContext, List<int> personIds, int? fetchCount = null )
        {
            Dictionary<int, object> mergeObjectsDictionary = new Dictionary<int, object>();

            bool combineFamilyMembers = !cbCombineFamilyMembers.Visible || ( cbCombineFamilyMembers.Visible && cbCombineFamilyMembers.Checked );
            IQueryable<Person> qryPersons = new PersonService( rockContext ).GetByIds( personIds );
            if ( combineFamilyMembers )
            {
                Guid familyGroupType = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                var qryFamilyGroupMembers = new GroupMemberService( rockContext ).Queryable( "GroupRole,Person" ).AsNoTracking()
                     .Where( a => a.Group.GroupType.Guid == familyGroupType )
                     .Where( a => qryPersons.Any( aa => aa.Id == a.PersonId ) );

                var qryCombined = qryFamilyGroupMembers.Join(
                    qryPersons,
                    m => m.PersonId,
                    p => p.Id,
                    ( m, p ) => new { GroupMember = m, Person = p } )
                    .GroupBy( a => a.GroupMember.GroupId )
                    .Select( x => new
                    {
                        GroupId = x.Key,
                        // Order People to match ordering in the GroupMembers.ascx block.
                        Persons =
                                // Adult Male 
                                x.Where( xx => xx.GroupMember.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                                xx.GroupMember.Person.Gender == Gender.Male ).OrderByDescending( xx => xx.GroupMember.Person.BirthDate ).Select( xx => xx.Person )
                                // Adult Female
                                .Concat( x.Where( xx => xx.GroupMember.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) &&
                                xx.GroupMember.Person.Gender != Gender.Male ).OrderByDescending( xx => xx.GroupMember.Person.BirthDate ).Select( xx => xx.Person ) )
                                // non-adults
                                .Concat( x.Where( xx => !xx.GroupMember.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) )
                                .OrderByDescending( xx => xx.GroupMember.Person.BirthDate ).Select( xx => xx.Person ) )
                    } );

                foreach ( var combinedFamilyItem in qryCombined )
                {
                    object mergeObject;

                    string commaPersonIds = combinedFamilyItem.Persons.Select( a => a.Id ).Distinct().ToList().AsDelimited( "," );

                    var primaryGroupPerson = combinedFamilyItem.Persons.FirstOrDefault() as Person;

                    if ( mergeObjectsDictionary.ContainsKey( primaryGroupPerson.Id ) )
                    {
                        foreach ( var person in combinedFamilyItem.Persons )
                        {
                            if ( !mergeObjectsDictionary.ContainsKey( person.Id ) )
                            {
                                primaryGroupPerson = person as Person;
                                break;
                            }
                        }
                    }


                    if ( combinedFamilyItem.Persons.Count() > 1 )
                    {
                        var combinedPerson = primaryGroupPerson.ToJson().FromJsonOrNull<MergeTemplateCombinedPerson>();

                        var familyTitle = RockUdfHelper.ufnCrm_GetFamilyTitle( rockContext, null, combinedFamilyItem.GroupId, commaPersonIds, true );
                        combinedPerson.FullName = familyTitle;

                        var firstNameList = combinedFamilyItem.Persons.Select( a => ( a as Person ).FirstName ).ToList();
                        var nickNameList = combinedFamilyItem.Persons.Select( a => ( a as Person ).NickName ).ToList();

                        combinedPerson.FirstName = firstNameList.AsDelimited( ", ", " & " );
                        combinedPerson.NickName = nickNameList.AsDelimited( ", ", " & " );
                        combinedPerson.LastName = primaryGroupPerson.LastName;
                        combinedPerson.SuffixValueId = null;
                        combinedPerson.SuffixValue = null;
                        mergeObject = combinedPerson;
                    }
                    else
                    {
                        mergeObject = primaryGroupPerson;
                    }

                    mergeObjectsDictionary.AddOrIgnore( primaryGroupPerson.Id, mergeObject );
                }
            }
            else
            {
                foreach ( var person in qryPersons.AsNoTracking() )
                {
                    mergeObjectsDictionary.AddOrIgnore( person.Id, person );
                }
            }

            var result = mergeObjectsDictionary.Select( a => a.Value );
            if ( fetchCount.HasValue )
            {
                // make sure the result is limited to fetchCount (even though the above queries are also limited to fetch count)
                result = result.Take( fetchCount.Value );
            }

            return result.ToList();
        }

        /// <summary>
        /// Gets the type of the merge template.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="mergeTemplate">The merge template.</param>
        /// <returns></returns>
        private MergeTemplateType GetMergeTemplateType( RockContext rockContext, MergeTemplate mergeTemplate )
        {
            if ( mergeTemplate == null )
            {
                return null;
            }

            return mergeTemplate.GetMergeTemplateType();
        }

        /// <summary>
        /// Binds the person checkbox list control
        /// </summary>
        private void BindPersonChechboxList( int? campusId )
        {
            cblPerson.Items.Clear();
            var dataViewGuid = this.GetAttributeValue( AttributeKeys.DataView ).AsGuid();
            using ( var rockContext = new RockContext() )
            {
                var dataView = new DataViewService( rockContext ).Get( dataViewGuid );
                if ( dataView != null )
                {
                    var personService = new PersonService( rockContext );
                    // Filter people by dataview
                    var errorMessages = new List<string>();
                    var paramExpression = personService.ParameterExpression;
                    var whereExpression = dataView.GetExpression( personService, paramExpression, out errorMessages );
                    var persons = personService
                        .Queryable( false, false ).AsNoTracking()
                        .Where( paramExpression, whereExpression, null )
                        .ToList();

                    var personIds = persons.Select( a => a.Id ).ToList();


                    if ( campusId.HasValue )
                    {
                        foreach ( var personId in personIds )
                        {
                            var person = persons.First( a => a.Id == personId );
                            if ( !person.GetCampusIds().Contains( campusId.Value ) )
                            {
                                persons.Remove( person );
                            }
                        }
                    }

                    cblPerson.DataSource = persons;
                    cblPerson.DataTextField = "FullName";
                    cblPerson.DataValueField = "Id";
                    cblPerson.DataBind();

                    hlNumberOfIndividuals.Text = "Individuals: " + persons.Count;
                }
            }

            cbSelectAll.Visible = cblPerson.Items.Count > 0;
        }

        private bool IsBlockSettingValid()
        {
            bool isValid = false;
            pnlEntry.Visible = true;
            nbWarningMessage.Visible = false;

            var dataViewGuid = GetAttributeValue( AttributeKeys.DataView ).AsGuidOrNull();
            var mergeTemplateGuid = this.GetAttributeValue( AttributeKeys.MergeTemplate ).AsGuidOrNull();
            if ( !dataViewGuid.HasValue || !mergeTemplateGuid.HasValue )
            {
                nbWarningMessage.Title = "Error";
                nbWarningMessage.Text = "<p>Data View and Merge Template block settings are required.</p>";
                nbWarningMessage.Visible = true;
                pnlEntry.Visible = false;
                return isValid;
            }

            var attributeGuid = GetAttributeValue( AttributeKeys.DateAttribute ).AsGuid();
            var attribute = AttributeCache.Get( attributeGuid );
            if ( !( attribute != null && attribute.FieldTypeId == FieldTypeCache.Get( new Guid( Rock.SystemGuid.FieldType.DATE ) ).Id ) )
            {
                nbWarningMessage.Title = "Error";
                nbWarningMessage.Text = "<p>Date Attribute block setting is not correctly configured.</p>";
                nbWarningMessage.Visible = true;
                pnlEntry.Visible = false;
                return isValid;
            }

            return true;
        }

        /// <summary>
        /// Sets the panel title and icon.
        /// </summary>
        private void SetPanel()
        {
            string title = this.GetAttributeValue( AttributeKeys.HeaderTitle );
            if ( !string.IsNullOrEmpty( title ) )
            {
                lTitle.Text = title;
            }

            string panelIcon = this.GetAttributeValue( AttributeKeys.HeaderIconCssClass );
            if ( !string.IsNullOrEmpty( panelIcon ) )
            {
                iIcon.Attributes["class"] = panelIcon;
            }

            var mergeTemplateGuid = this.GetAttributeValue( AttributeKeys.MergeTemplate ).AsGuidOrNull();
            if ( mergeTemplateGuid.HasValue )
            {
                var mergeTemplate = new MergeTemplateService( new RockContext() ).Get( mergeTemplateGuid.Value );
                btnMerge.Text = "<i class='fa fa-th'></i>" + mergeTemplate.Name;
            }

            cpCampus.Visible = this.GetAttributeValue( AttributeKeys.EnableCampusFilter ).AsBoolean();
        }

        #endregion

        #region helper class
        /// <summary>
        /// Special class that overrides Person so that FullName can be set (vs readonly/derived)
        /// The class is specifically for MergeTemplates
        /// </summary>
        public class MergeTemplateCombinedPerson : Person
        {
            /// <summary>
            /// Override of FullName that should be set to whatever the FamilyTitle should be
            /// </summary>
            /// <value>
            /// A <see cref="System.String" /> representing the Family Title of a combined person
            /// </value>
            [DataMember]
            public new string FullName { get; set; }
        }
        #endregion helper class

    }
}