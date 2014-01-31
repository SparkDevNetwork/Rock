// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;
using System.Text.RegularExpressions;
using System.Data;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// Merges two or more person records into one.
    /// </summary>
    [DisplayName( "Person Merge" )]
    [Category( "CRM" )]
    [Description( "Merges two or more person records into one." )]

    public partial class PersonMerge : Rock.Web.UI.RockBlock
    {

        #region Fields

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the people.
        /// </summary>
        /// <value>
        /// The people.
        /// </value>
        private MergeData MergeData
        {
            get
            {
                var data = ViewState["MergeData"] as MergeData;
                if (data == null)
                {
                    data = new MergeData();
                    MergeData = data;
                }
                return data;
            }
            set
            {
                ViewState["MergeData"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if (!Page.IsPostBack)
            {
                var personIDs = PageParameter( "People" ).SplitDelimitedValues().Select( p => p.AsInteger().Value).ToList();
                var people = new PersonService().Queryable().Where( p => personIDs.Contains( p.Id ) );

                // Create the data structure used to build grid
                MergeData = new MergeData();
                foreach ( var person in people )
                {
                    MergeData.AddPerson(person);
                }
                MergeData.SetSelection( people.OrderBy( p => p.CreatedDateTime ).Select( p => p.Id ).FirstOrDefault() );
            }

            // Build the grid columns
            BuildColumns();
        }

        protected override void OnLoad( EventArgs e )
        {
            if (!Page.IsPostBack)
            {
                BindData();
            }
        }
       
        #endregion

        #region Events

        #endregion

        #region Methods


        private void BuildColumns()
        {
            gMerge.Columns.Clear();

            if ( MergeData.People.Any() )
            {
                var keyCol = new BoundField();
                keyCol.DataField = "Key";
                keyCol.Visible = false;
                gMerge.Columns.Add( keyCol );

                var labelCol = new BoundField();
                labelCol.DataField = "Label";
                labelCol.HeaderText = "Value";
                gMerge.Columns.Add( labelCol );

                foreach ( var person in MergeData.People )
                {
                    var personCol = new SelectField();
                    personCol.SelectionMode = SelectionMode.SingleColumn;
                    personCol.HeaderText = person.FullName;
                    personCol.DataTextField = string.Format( "property_{0}", person.Id );
                    personCol.DataSelectedField = string.Format( "property_{0}_selected", person.Id );
                    gMerge.Columns.Add( personCol );
                }
            }
        }

        private void BindData()
        {
            DataTable dt = MergeData.GetDataTable();
            gMerge.DataSource = dt;
            gMerge.DataBind();
        }

        #endregion

    }

    [Serializable]
    class MergeData
    {
        public List<Person> People { get; set; }
        public List<PersonProperty> Properties { get; set; }
        
        public MergeData()
        {
            People = new List<Person>();
            Properties = new List<PersonProperty>();
        }

        public void AddPerson(Person person)
        {
            People.Add(person);

            AddProperty( "Title", person.Id, person.TitleValue );
            AddProperty( "FirstName", person.Id, person.FirstName );
            AddProperty( "NickName", person.Id, person.NickName );
            AddProperty( "MiddleName", person.Id, person.MiddleName );
            AddProperty( "LastName", person.Id, person.LastName );
            AddProperty( "Suffix", person.Id, person.SuffixValue );
            AddProperty( "RecordType", person.Id, person.RecordTypeValue );
            AddProperty( "RecordStatus", person.Id, person.RecordStatusValue );
            AddProperty( "RecordStatusReason", person.Id, person.RecordStatusReasonValue );
            AddProperty( "ConnectionStatus", person.Id, person.ConnectionStatusValue );
            AddProperty( "Deceased", person.Id, person.IsDeceased );
            AddProperty( "Gender", person.Id, person.Gender );
            AddProperty( "MaritalStatus", person.Id, person.MaritalStatusValue );
            AddProperty( "AnniversaryDate", person.Id, person.AnniversaryDate );
            AddProperty( "GraduationDate", person.Id, person.GraduationDate );
            AddProperty( "Email", person.Id, person.Email );
            AddProperty( "IsEmailActive", person.Id, person.IsEmailActive );
            AddProperty( "EmailNote", person.Id, person.EmailNote );
            AddProperty( "DoNotEmail", person.Id, person.DoNotEmail );
            AddProperty( "SystemNote", person.Id, person.SystemNote );
        }

        public void AddProperty( string key, int personId, string value, bool selected = false )
        {
            AddProperty( key, personId, value, value, selected );
        }

        public void AddProperty(string key, int personId, string value, string formattedValue, bool selected = false)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var property = GetProperty(key);
                var propertyValue = property.Values.Where( v => v.PersonId == personId).FirstOrDefault();
                if (propertyValue == null)
                {
                    propertyValue = new PersonPropertyValue { PersonId = personId };
                    property.Values.Add(propertyValue);
                }
                propertyValue.Value = value;
                propertyValue.FormattedValue = formattedValue;
                propertyValue.Selected = selected;
            }
        }



        public void AddProperty( string key, int personId, DefinedValue value, bool selected = false )
        {
            AddProperty( key, personId, value != null ? value.Name : "", selected );
        }

        public void AddProperty( string key, int personId, bool? value, bool selected = false )
        {
            AddProperty( key, personId, ( value ?? false ).ToString(), selected );
        }

        public void AddProperty( string key, int personId, DateTime? value, bool selected = false )
        {
            AddProperty( key, personId, value.HasValue ? value.Value.ToShortDateString() : string.Empty, selected );
        }

        public void AddProperty( string key, int personId, Enum value, bool selected = false )
        {
            AddProperty( key, personId, value.ConvertToString(), selected );
        }

        public void SetSelection(int primaryPersonId)
        {
            foreach( var personProperty in Properties)
            {
                // Unselect all the values
                personProperty.Values.ForEach( v => v.Selected = false);

                // Find primary person's non-blank value
                var value = personProperty.Values.Where( v => v.PersonId == primaryPersonId && v.FormattedValue != "" ).FirstOrDefault();
                if (value == null)
                {
                    // Find first non-blank value
                    value = personProperty.Values.Where( v => v.FormattedValue != "").FirstOrDefault();                
                    if (value == null)
                    {
                        value = personProperty.Values.FirstOrDefault();
                    }
                }

                if (value != null)
                {
                    value.Selected = true;
                }
            }
        }

        public DataTable GetDataTable()
        {
            var tbl = new DataTable();

            tbl.Columns.Add( "Key" );
            tbl.Columns.Add( "Label" );

            foreach ( var person in People )
            {
                tbl.Columns.Add( string.Format("property_{0}", person.Id ) );
                tbl.Columns.Add( string.Format("property_{0}_selected", person.Id ), typeof(bool) );
            }

            foreach( var personProperty in Properties)
            {
                var rowValues = new List<object>();
                rowValues.Add(personProperty.Key);
                rowValues.Add(personProperty.Label);

                var values = new List<string>();
                foreach(var person in People )
                {
                    var value = personProperty.Values.Where( v => v.PersonId == person.Id).FirstOrDefault();
                    string formattedValue = value != null ? value.FormattedValue : string.Empty;
                    values.Add(formattedValue);
                    rowValues.Add( formattedValue );
                    rowValues.Add( value != null ? value.Selected : false );
                }

                // Only add rows where people have different values
                if ( values.Distinct().Count() > 1 )
                {
                    tbl.Rows.Add( rowValues.ToArray() );
                }
            }

            return tbl;
        }

        private PersonProperty GetProperty(string key)
        {
            var property = Properties.Where( p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (property == null)
            {
                property = new PersonProperty(key);
                Properties.Add( property );
            }
            return property;
        }
    }

    [Serializable]
    class PersonProperty
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public List<PersonPropertyValue> Values { get; set; }

        public PersonProperty()
        {
            Values = new List<PersonPropertyValue>();
        }

        public PersonProperty( string key ) : this()
        {
            Key = key;
            Label = key.SplitCase();
        }
    }

    [Serializable]
    class PersonPropertyValue
    {
        public int PersonId { get; set; }
        public bool Selected { get; set; }
        public string Value { get; set; }
        public string FormattedValue { get; set; }
    }
 
}