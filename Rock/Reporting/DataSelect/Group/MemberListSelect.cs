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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Group
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select a comma-delimited list of members of the group" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Group Member List" )]
    public class MemberListSelect : DataSelectComponent
    {
        #region Properties

        /// <summary>
        /// Gets the name of the entity type. Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public override string AppliesToEntityType
        {
            get
            {
                return typeof( Rock.Model.Group ).FullName;
            }
        }

        /// <summary>
        /// Gets the section that this will appear in in the Field Selector
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get
            {
                return base.Section;
            }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "MemberList";
            }
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get { return typeof( IEnumerable<MemberInfo> ); }
        }

        /// <summary>
        /// little class so that we only need to fetch the columns that we need from Person
        /// </summary>
        private class MemberInfo
        {
            public string NickName { get; set; }

            public string LastName { get; set; }

            public int? SuffixValueId { get; set; }

            public int PersonId { get; set; }

            public int GroupMemberId { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        private enum ShowAsLinkType
        {
            NameOnly = 0,
            PersonLink = 1,
            GroupMemberLink = 2
        }

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Web.UI.WebControls.DataControlField GetGridField( Type entityType, string selection )
        {
            var callbackField = new CallbackField();
            string basePersonUrl = System.Web.VirtualPathUtility.ToAbsolute( "~/Person/" );
            string baseGroupMemberUrl = System.Web.VirtualPathUtility.ToAbsolute( "~/GroupMember/" );
            var selectionParts = selection.Split( '|' );
            ShowAsLinkType showAsLinkType = selectionParts.Length > 0 ? selectionParts[0].ConvertToEnum<ShowAsLinkType>( ShowAsLinkType.NameOnly ) : ShowAsLinkType.NameOnly;
            callbackField.OnFormatDataValue += ( sender, e ) =>
            {
                var groupMemberList = e.DataValue as IEnumerable<MemberInfo>;
                if ( groupMemberList != null )
                {
                    var formattedList = new List<string>();
                    foreach ( var groupMember in groupMemberList )
                    {
                        var formattedPersonFullName = Rock.Model.Person.FormatFullName( groupMember.NickName, groupMember.LastName, groupMember.SuffixValueId );
                        string formattedValue;
                        if ( showAsLinkType == ShowAsLinkType.PersonLink )
                        {
                            formattedValue = "<a href='" + basePersonUrl + groupMember.PersonId.ToString() + "'>" + formattedPersonFullName + "</a>";
                        }
                        else if ( showAsLinkType == ShowAsLinkType.GroupMemberLink )
                        {
                            formattedValue = "<a href='" + baseGroupMemberUrl + groupMember.GroupMemberId.ToString() + "'>" + formattedPersonFullName + "</a>";
                        }
                        else
                        {
                            formattedValue = formattedPersonFullName;
                        }

                        formattedList.Add( formattedValue );
                    }

                    e.FormattedValue = formattedList.AsDelimited( ", " );
                }
                else
                {
                    e.FormattedValue = string.Empty;
                }
            };

            return callbackField;
        }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public override string ColumnHeaderText
        {
            get
            {
                return "Member List";
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Member List";
        }

        /// <summary>
        /// Comma-delimited list of the Entity properties that should be used for Sorting. Normally, you should leave this as null which will make it sort on the returned field
        /// To disable sorting for this field, return string.Empty;
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        /// <value>
        /// The sort expression.
        /// </value>
        public override string SortProperties( string selection )
        {
            // disable sorting on this column since it is an IEnumerable
            return string.Empty;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            var qryGroupService = new GroupService( context ).Queryable();

            var memberListQuery = qryGroupService.Select( p => p.Members
                .Select( m => new MemberInfo
                {
                    NickName = m.Person.NickName,
                    LastName = m.Person.LastName,
                    SuffixValueId = m.Person.SuffixValueId,
                    PersonId = m.PersonId,
                    GroupMemberId = m.Id
                } ).OrderBy( a => a.LastName ).ThenBy( a => a.NickName ) );

            var selectChildrenExpression = SelectExpressionExtractor.Extract<Rock.Model.Group>( memberListQuery, entityIdProperty, "p" );

            return selectChildrenExpression;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            RockRadioButtonList rblShowAsLinkType = new RockRadioButtonList();
            rblShowAsLinkType.ID = parentControl.ID + "_rblShowAsLinkType";
            rblShowAsLinkType.Items.Add( new ListItem( "Show Name Only", ShowAsLinkType.NameOnly.ConvertToInt().ToString() ) );
            rblShowAsLinkType.Items.Add( new ListItem( "Show as Person Link", ShowAsLinkType.PersonLink.ConvertToInt().ToString() ) );
            //rblShowAsLinkType.Items.Add( new ListItem( "Show as Group Member Link", ShowAsLinkType.GroupMemberLink.ConvertToInt().ToString() ) );
            parentControl.Controls.Add( rblShowAsLinkType );

            return new System.Web.UI.Control[] { rblShowAsLinkType };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            if ( controls.Count() == 1 )
            {
                RockRadioButtonList rblShowAsLinkType = controls[0] as RockRadioButtonList;
                if ( rblShowAsLinkType != null )
                {
                    return string.Format( "{0}", rblShowAsLinkType.SelectedValueAsEnum<ShowAsLinkType>().ConvertToInt() );
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            if ( controls.Count() == 1 )
            {
                string[] selectionValues = selection.Split( '|' );
                if ( selectionValues.Length >= 1 )
                {
                    RockRadioButtonList rblShowAsLinkType = controls[0] as RockRadioButtonList;
                    rblShowAsLinkType.SelectedValue = selectionValues[0].ConvertToEnum<ShowAsLinkType>( ShowAsLinkType.NameOnly ).ConvertToInt().ToString();
                }
            }
        }

        #endregion
    }
}
