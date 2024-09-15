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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select the Photo of the Person" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person's Photo" )]
    [Rock.SystemGuid.EntityTypeGuid( "C2EE52FA-044F-47DE-A398-18F8E3D9311D" )]
    public class PhotoSelect : DataSelectComponent
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
                return typeof( Rock.Model.Person ).FullName;
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
                return "Photo";
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
            get { return typeof( string ); }
        }

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override DataControlField GetGridField( Type entityType, string selection )
        {
            BoundField result = new BoundField();
            result.HtmlEncode = false;

            return result;
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
                return "Photo";
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
            return "Photo";
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
            string[] selectionValues = selection.Split( '|' );
            int? width = 50;
            int? height = 50;
            if ( selectionValues.Length == 2 )
            {
                width = selectionValues[0].AsIntegerOrNull() ?? width;
                height = selectionValues[1].AsIntegerOrNull() ?? height;
            }

            // Construct the widthHeightHtmlParams strings
            string baseUrl = VirtualPathUtility.ToAbsolute( "~/" );
            string widthHeightUrlParams = string.Format( "&width={0}&height={1}", width, height );
            string widthHeightHtmlParams = string.Format( " width='{0}' height='{1}' ", width, height );
            string nophotoAdultFemaleHtml = "<img src='" + baseUrl + "Assets/Images/person-no-photo-female.svg'" + widthHeightHtmlParams + " />";
            string nophotoAdultMaleHtml = "<img src='" + baseUrl + "Assets/Images/person-no-photo-male.svg'" + widthHeightHtmlParams + " />";
            string nophotoChildFemaleHtml = "<img src='" + baseUrl + "Assets/Images/person-no-photo-child-female.svg'" + widthHeightHtmlParams + " />";
            string nophotoChildMaleHtml = "<img src='" + baseUrl + "Assets/Images/person-no-photo-child-male.svg'" + widthHeightHtmlParams + " />";

            DateTime childBirthdateCutoff = RockDateTime.Now.Date.AddYears( -18 );

            var securitySettings = new SecuritySettingsService().SecuritySettings;
            var disablePredictableIds = securitySettings.DisablePredictableIds;

            var personPhotoQuery = new PersonService( context ).Queryable()
                .Select( p => p.Photo != null
                    ? "<img src='" + baseUrl + "GetImage.ashx?" +
                      ( disablePredictableIds
                          ? "guid=" + p.Photo.Guid.ToString()
                          : "id=" + SqlFunctions.StringConvert( ( double? ) p.PhotoId ) ) +
                      widthHeightUrlParams + "' " + widthHeightHtmlParams + " />"
                    : ( p.BirthDate.HasValue && p.BirthDate > childBirthdateCutoff )
                        ? ( p.Gender == Gender.Female ? nophotoChildFemaleHtml : nophotoChildMaleHtml )
                        : ( p.Gender == Gender.Female ? nophotoAdultFemaleHtml : nophotoAdultMaleHtml ) );

            var selectPhotoExpression = SelectExpressionExtractor.Extract( personPhotoQuery, entityIdProperty, "p" );

            return selectPhotoExpression;
        }
        
        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            NumberBox widthBox = new NumberBox();
            widthBox.NumberType = ValidationDataType.Integer;
            widthBox.MaxLength = 3;
            widthBox.MinimumValue = "0";
            widthBox.Label = "Image Width";
            widthBox.ID = parentControl.ID + "_widthBox";
            parentControl.Controls.Add( widthBox );

            NumberBox heightBox = new NumberBox();
            heightBox.NumberType = ValidationDataType.Integer;
            heightBox.MaxLength = 3;
            heightBox.MinimumValue = "0";
            heightBox.Label = "Image Height";
            heightBox.ID = parentControl.ID + "_heightBox";
            parentControl.Controls.Add( heightBox );

            return new System.Web.UI.Control[] { widthBox, heightBox };
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
            if ( controls.Count() == 2 )
            {
                NumberBox widthBox = controls[0] as NumberBox;
                NumberBox heightBox = controls[1] as NumberBox;
                return string.Format( "{0}|{1}", widthBox.Text, heightBox.Text );
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
            if ( controls.Count() == 2 )
            {
                string[] selectionValues = selection.Split( '|' );
                if ( selectionValues.Length >= 2 )
                {
                    NumberBox widthBox = controls[0] as NumberBox;
                    NumberBox heightBox = controls[1] as NumberBox;
                    widthBox.Text = selectionValues[0].AsIntegerOrNull().ToString();
                    heightBox.Text = selectionValues[1].AsIntegerOrNull().ToString();
                }
            }
        }

        #endregion
    }
}
