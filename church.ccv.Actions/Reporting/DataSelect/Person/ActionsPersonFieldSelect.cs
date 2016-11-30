using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;
using church.ccv.Actions.Data;
using church.ccv.Actions.Models;

namespace church.ccv.Actions
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select a field for the person using the value from Actions History Adult Person " )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select a field value from Actions History Adult Person" )]
    public class ActionsHistoryAdultPersonFieldSelect : DataSelectComponent
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
                return "CCV Actions";
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
                return "Actions History Adult Person";
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
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Web.UI.WebControls.DataControlField GetGridField( Type entityType, string selection )
        {
            var ahAdultPersonParameterExpression = Expression.Parameter( typeof( ActionsHistory_Adult_Person ), "a" );
            MemberExpression ahAdultPersonMember = Expression.Property( ahAdultPersonParameterExpression, selection );
            if ( ahAdultPersonMember != null )
            {
                return Grid.GetGridField( ahAdultPersonMember.Type );
            }
            else
            {
                return base.GetGridField( entityType, selection );
            }
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
            var actionHistoryFields = EntityHelper.GetEntityFields( typeof( ActionsHistory_Adult_Person ) ).Where( a => a.FieldKind == FieldKind.Property ).OrderBy( a => a.Name ).ToList();
            var supportedTypes = new Type[] { typeof( DateTime? ), typeof( bool ) };
            foreach ( var field in actionHistoryFields )
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
            var ahAdultPersonService = new Service<ActionsHistory_Adult_Person>( context );
            var qryAhAdultPersonQuery = ahAdultPersonService.Queryable();

            var personAliasService = new PersonAliasService( context );
            var qryPersonAlias = personAliasService.Queryable();

            var personService = new PersonService( context );
            var qryPerson = personService.Queryable();

            var qryHistory = qryAhAdultPersonQuery.Join( qryPersonAlias, ah => ah.PersonAliasId, pa => pa.Id, ( ah, pa ) => new { AH = ah, PersonAlias = pa } ).AsQueryable( );
            
            string fieldName = selection;
            if ( string.IsNullOrWhiteSpace( fieldName ) )
            {
                return null;
            }

            IQueryable qryResult = null;

            // ok, first create an argument param that's basically "ActionsHistory_Adult_Person d.AH"
            var ahAdultPersonParameterExpression = Expression.Parameter( typeof( ActionsHistory_Adult_Person ), "d.AH" );

            // now, create an expression that will access the desired Property (Like Coaching, Serving, Baptised, etc.)
            // Ex: "d.AH.Coaching"
            MemberExpression ahAdultPersonMember = Expression.Property( ahAdultPersonParameterExpression, fieldName );
            
            // now build functions that basically looks like this
            // bool func( ActionsHistory_Adult_Person d.AH ) { return d.AH.Coaching; }
            // where the return type differs based on the branch below
            if ( ahAdultPersonMember.Type == typeof( bool ) )
            {
                var projectionLamba = Expression.Lambda<Func<ActionsHistory_Adult_Person, bool>>( ahAdultPersonMember, new ParameterExpression[] { ahAdultPersonParameterExpression } );
                
                qryResult = qryPerson
                    .Select( p => qryHistory.Where( ah => ah.PersonAlias.PersonId == p.Id ).Select( ah => ah.AH )
                      .Select( projectionLamba ).FirstOrDefault() );
            }
            else if ( ahAdultPersonMember.Type == typeof( DateTime? ) )
            {
                var projectionLamba = Expression.Lambda<Func<ActionsHistory_Adult_Person, DateTime?>>( ahAdultPersonMember, new ParameterExpression[] { ahAdultPersonParameterExpression } );

                qryResult = qryPerson
                    .Select( p => qryHistory.Where( ah => ah.PersonAlias.PersonId == p.Id ).Select( ah => ah.AH )
                      .Select( projectionLamba ).FirstOrDefault() );
            }

            return SelectExpressionExtractor.Extract( qryResult, entityIdProperty, "p" );
        }
    }
}
