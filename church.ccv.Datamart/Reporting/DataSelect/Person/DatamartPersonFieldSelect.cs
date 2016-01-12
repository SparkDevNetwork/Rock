using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using church.ccv.Datamart.Model;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace church.ccv.Datamart.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select a field for the person using the value from Datamart Person " )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select a field value from Datamart Person" )]
    public class DatamartPersonFieldSelect : DataSelectComponent
    {
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
        /// Gets the section that this will appear in in the Field Selector
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get
            {
                return "Datamart";
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
                return "DatamartPersonField";
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
            get { return typeof( object ); }
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
                return this.ColumnPropertyName;
            }
        }

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
            return this.ColumnHeaderText;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            RockDropDownList ddlField = new RockDropDownList();
            ddlField.Label = "Field Name";
            ddlField.ID = parentControl.ID + "_ddlField";
            var datamartFields = EntityHelper.GetEntityFields( typeof( DatamartPerson ) ).Where( a => a.FieldKind == FieldKind.Property ).OrderBy( a => a.Name ).ToList();
            var supportedTypes = new Type[] { typeof( string ), typeof( bool? ), typeof( DateTime? ), typeof( decimal? ) };
            foreach ( var field in datamartFields )
            {
                if ( supportedTypes.Any( a => a.IsAssignableFrom( field.PropertyType ) ) )
                {
                    ddlField.Items.Add( field.Name );
                }
            }

            parentControl.Controls.Add( ddlField );

            return new System.Web.UI.Control[] { ddlField };
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
                RockDropDownList ddlField = controls[0] as RockDropDownList;
                return string.Format( "{0}", ddlField.SelectedValue );
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
                RockDropDownList ddlField = controls[0] as RockDropDownList;
                ddlField.SelectedValue = selection;
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Linq.Expressions.Expression GetExpression( Rock.Data.RockContext context, System.Linq.Expressions.MemberExpression entityIdProperty, string selection )
        {
            var datamartPersonService = new Service<DatamartPerson>( context );
            var personService = new PersonService( context );
            var qryDatamartPerson = datamartPersonService.Queryable();
            var qryPerson = personService.Queryable();

            string fieldName = selection;
            if ( string.IsNullOrWhiteSpace( fieldName ) )
            {
                return null;
            }

            IQueryable qryResult = null;

            var datamartParameterExpression = Expression.Parameter( typeof( DatamartPerson ), "d" );
            MemberExpression datamartMember = Expression.Property( datamartParameterExpression, fieldName );
            if ( datamartMember.Type == typeof( bool ) )
            {
                var projectionLamba = Expression.Lambda<Func<DatamartPerson, bool>>( datamartMember, new ParameterExpression[] { datamartParameterExpression } );
                qryResult = qryPerson
                    .Select( p => qryDatamartPerson.Where( d => d.PersonId == p.Id )
                      .Select( projectionLamba ).FirstOrDefault() );
            }
            else if ( datamartMember.Type == typeof( bool? ) )
            {
                var projectionLamba = Expression.Lambda<Func<DatamartPerson, bool?>>( datamartMember, new ParameterExpression[] { datamartParameterExpression } );
                qryResult = qryPerson
                    .Select( p => qryDatamartPerson.Where( d => d.PersonId == p.Id )
                      .Select( projectionLamba ).FirstOrDefault() );
            }
            else if ( datamartMember.Type == typeof( DateTime? ) )
            {
                var projectionLamba = Expression.Lambda<Func<DatamartPerson, DateTime?>>( datamartMember, new ParameterExpression[] { datamartParameterExpression } );
                qryResult = qryPerson
                    .Select( p => qryDatamartPerson.Where( d => d.PersonId == p.Id )
                      .Select( projectionLamba ).FirstOrDefault() );
            }
            else if ( datamartMember.Type == typeof( decimal? ) )
            {
                var projectionLamba = Expression.Lambda<Func<DatamartPerson, decimal?>>( datamartMember, new ParameterExpression[] { datamartParameterExpression } );
                qryResult = qryPerson
                    .Select( p => qryDatamartPerson.Where( d => d.PersonId == p.Id )
                      .Select( projectionLamba ).FirstOrDefault() );
            }
            else
            {
                var projectionLamba = Expression.Lambda<Func<DatamartPerson, string>>( datamartMember, new ParameterExpression[] { datamartParameterExpression } );
                qryResult = qryPerson
                    .Select( p => qryDatamartPerson.Where( d => d.PersonId == p.Id )
                      .Select( projectionLamba ).FirstOrDefault() );
            }

            return SelectExpressionExtractor.Extract( qryResult, entityIdProperty, "p" );
        }
    }
}
