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
        private Dictionary<int, List<PersonProperty>> PeopleValues
        {
            get
            {
                var peopleValues = ViewState["PeopleValues"] as Dictionary<int, List<PersonProperty>>;
                if (peopleValues == null)
                {
                    peopleValues = new Dictionary<int, List<PersonProperty>>();
                    PeopleValues = peopleValues;
                }
                return peopleValues;
            }
            set
            {
                ViewState["PeopleValues"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if (!Page.IsPostBack)
            {
                PeopleValues = new Dictionary<int, List<PersonProperty>>();
                var personIDs = new List<int>();
                PageParameter( "People" ).SplitDelimitedValues().ToList().ForEach(p => personIDs.Add(p.AsInteger().Value));
                foreach ( var person in new PersonService().Queryable().Where( p => personIDs.Contains( p.Id ) ) )
                {
                    PeopleValues.Add( person.Id, GetPersonValues( person ) );
                }
            }

            BuildDisplay( !Page.IsPostBack );
        }

        protected override void OnLoad( EventArgs e )
        {
        }
       
        #endregion

        #region Events

        #endregion

        #region Methods

        private List<PersonProperty> GetPersonValues( Person person )
        {
            var personValues = new List<PersonProperty>();

            personValues.Add( new PersonProperty( "Name", person.TitleValue ) );
            personValues.Add( new PersonProperty( "Title", person.TitleValue ) );
            personValues.Add( new PersonProperty( "FirstName", person.FirstName ) );
            personValues.Add( new PersonProperty( "NickName", person.NickName ) );
            personValues.Add( new PersonProperty( "MiddleName", person.MiddleName ) );
            personValues.Add( new PersonProperty( "LastName", person.LastName ) );
            personValues.Add( new PersonProperty( "Suffix", person.SuffixValue ) );
            personValues.Add( new PersonProperty( "RecordType", person.RecordTypeValue ) );
            personValues.Add( new PersonProperty( "RecordStatus", person.RecordStatusValue ) );
            personValues.Add( new PersonProperty( "RecordStatusReason",person.RecordStatusReasonValue ) );
            personValues.Add( new PersonProperty( "ConnectionStatus", person.ConnectionStatusValue ) );
            personValues.Add( new PersonProperty( "Deceased", person.IsDeceased ) );
            personValues.Add( new PersonProperty( "Gender", person.Gender ) );
            personValues.Add( new PersonProperty( "MaritalStatus", person.MaritalStatusValue ) );
            personValues.Add( new PersonProperty( "AnniversaryDate", person.AnniversaryDate ) );
            personValues.Add( new PersonProperty( "GraduationDate", person.GraduationDate ) );
            personValues.Add( new PersonProperty( "Email", person.Email ) );
            personValues.Add( new PersonProperty( "IsEmailActive", person.IsEmailActive ) );
            personValues.Add( new PersonProperty( "EmailNote", person.EmailNote ) );
            personValues.Add( new PersonProperty( "DoNotEmail", person.DoNotEmail ) );
            personValues.Add( new PersonProperty( "SystemNote", person.SystemNote ) );
            // TODO: Giving Group

            return personValues;
        }

        private void BuildDisplay( bool SetSelection )
        {
            gMerge.Columns.Clear();

            if ( PeopleValues.Any() )
            {
                // Create Columns
                var tbl = new DataTable();
                tbl.Columns.Add( "Key" );
                tbl.Columns.Add( "Label" );

                var labelCol = new BoundField();
                labelCol.DataField = "Label";
                labelCol.HeaderText = "Value";
                gMerge.Columns.Add( labelCol );

                foreach ( var person in PeopleValues )
                {
                    tbl.Columns.Add( person.Key.ToString() );

                    var personCol = new BoundField();
                    personCol.DataField = person.Key.ToString();
                    personCol.HeaderText = person.Key.ToString();
                    gMerge.Columns.Add( personCol );
                }

                var propertyList = PeopleValues.FirstOrDefault().Value;
                foreach(PersonProperty property in propertyList)
                {
                    var rowValues = new List<object>();
                    rowValues.Add(property.Key);
                    rowValues.Add(property.Label);
                    bool display = false;

                    foreach(var valueList in PeopleValues.Select( p => p.Value))
                    {
                        string value = valueList.Where( v => v.Key == property.Key).Select( v => v.FormattedValue).FirstOrDefault() ?? string.Empty;
                        rowValues.Add(value);
                        if ( value != ( property.FormattedValue ?? string.Empty ) )
                        {
                            display = true;
                        }
                    }

                    if ( display )
                    {
                        tbl.Rows.Add( rowValues.ToArray() );
                    }
                }

                gMerge.DataSource = tbl;
                gMerge.DataBind();

            }
        }

        #endregion

    }

    [Serializable]
    class PersonProperty
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string FormattedValue { get; set; }
        public object Value { get; set; }
        public bool Selected { get; set; }

        public PersonProperty()
        {
            Selected = false;
        }

        public PersonProperty( string key ) : this()
        {
            Key = key;
            Label = key.SplitCase();
        }

        public PersonProperty( string key, string value )
            : this(key)
        {
            FormattedValue = value;
            Value = value;
        }

        public PersonProperty( string key, DefinedValue value )
            : this(key)
        {
            if ( value != null )
            {
                FormattedValue = value.Name;
                Value = value.Id;
            }
            else
            {
                FormattedValue = string.Empty;
                Value = null;
            }
        }

        public PersonProperty( string key, bool? value )
            : this( key )
        {
            FormattedValue = (value ?? false).ToString();
            Value = value;
        }

        public PersonProperty( string key, DateTime? value )
            : this( key )
        {
            FormattedValue = value.HasValue ? value.Value.ToShortDateString() : string.Empty;
            Value = value;
        }

        public PersonProperty( string key, Enum value )
            : this( key )
        {
            FormattedValue = value.ConvertToString();
            Value = value;
        }

        public PersonProperty( string key, string formattedValue, object value )
            : this( key )
        {
            FormattedValue = formattedValue;
            Value = value;
        }
    }
 
}