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
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Reporting
{
    /// <summary>
    /// Simplifies the construction of a Rock RMS Report object that serves as a template for a tabular report.
    /// </summary>
    public class ReportTemplateBuilder
    {
        #region Private Fields

        private Report _Report;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ReportBuilderTemplate class.
        /// </summary>
        /// <param name="entityType"></param>
        public ReportTemplateBuilder( Type entityType )
        {
            _Report = new Report();

            _Report.EntityTypeId = EntityTypeCache.GetId( entityType );
        }

        /// <summary>
        /// Initializes a new instance of the ReportBuilderTemplate class.
        /// </summary>
        /// <param name="report"></param>
        public ReportTemplateBuilder( Report report )
        {
            _Report = report;

            if ( _Report == null )
            {
                _Report = new Report();
            }
        }

        #endregion

        #region Properties and Methods

        /// <summary>
        /// The target report.
        /// </summary>
        public Report Report
        {
            get
            {
                return _Report;
            }
        }

        /// <summary>
        /// Add a DataSelect field to the report.
        /// </summary>
        /// <param name="componentName">The fully-qualified name of the DataSelect component that provides the field content.</param>
        /// <param name="settings">Delimited string or JSON data that represents custom settings used by the DataSelect component to generate the field content.</param>
        /// <param name="columnName">The display name for the column.</param>
        /// <returns></returns>
        public ReportField AddDataSelectField( string componentName, object settings, string columnName = null )
        {
            var reportField = new ReportField();

            // Add property field
            reportField.ReportFieldType = ReportFieldType.DataSelectComponent;

            // TODO: Resolve field name if unqualified.

            reportField.DataSelectComponentEntityTypeId = EntityTypeCache.GetId( componentName );

            if ( settings != null )
            {
                reportField.Selection = settings.ToJson();
            }

            reportField.ShowInGrid = true;

            if ( columnName != null )
            {
                reportField.ColumnHeaderText = columnName;
            }

            _Report.ReportFields.Add( reportField );

            return reportField;
        }

        /// <summary>
        /// Add a Property of the reporting entity as a report field.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="columnName">The display name for the column.</param>
        /// <returns></returns>
        public ReportField AddPropertyField( string propertyName, string columnName = null )
        {
            var reportField = new ReportField();

            reportField.ReportFieldType = ReportFieldType.Property;
            reportField.Selection = propertyName;
            reportField.ShowInGrid = true;

            if ( columnName != null )
            {
                reportField.ColumnHeaderText = columnName;
            }

            _Report.ReportFields.Add( reportField );

            return reportField;
        }

        /// <summary>
        /// Add a custom Attribute of the reporting entity as a report field.
        /// </summary>
        /// <param name="attributeGuid"></param>
        /// <param name="columnName">The display name for the column.</param>
        /// <returns></returns>
        public ReportField AddAttributeField( string attributeGuid, string columnName = null )
        {
            var reportField = new ReportField();

            reportField.ReportFieldType = ReportFieldType.Attribute;
            reportField.Selection = attributeGuid;
            reportField.ShowInGrid = true;

            if ( columnName != null )
            {
                reportField.ColumnHeaderText = columnName;
            }

            _Report.ReportFields.Add( reportField );

            return reportField;
        }

        #endregion
    }
}
