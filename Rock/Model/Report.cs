using System;
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Reporting;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Report (based off of a <see cref="Rock.Model.DataView"/> in RockChMS.
    /// </summary>
    [Table( "Report" )]
    [DataContract]
    public partial class Report : Model<Report>, ICategorized
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Report is part of the RockChMS core system/framework. This property is required.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> that is <c>true</c> if the Report is part of the RockChMS core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Report. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Name of the Report.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Report's Description.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Report's Description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId of the <see cref="Rock.Model.Category"/> that the Report belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CateogryId of the <see cref="Rock.Model.Category"/> that the report belongs to. If the Report does not belong to
        /// a <see cref="Rock.Model.Category"/> this value will be null.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that is being reported on.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that is being reported on.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or the DataViewId of the root <see cref="Rock.Model.DataView"/> that this Report is based on.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DataViewId of the root <see cref="Rock.Model.DataView"/> that this Report is based on.
        /// </value>
        [DataMember]
        public int? DataViewId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Category"/> that this Report belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Category"/> that this Report belongs to. If the Report does not belong to a <see cref="Rock.Model.Category"/> this value will be null.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> that is being reported on. 
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> that is being reported on.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the base/root <see cref="Rock.Model.DataView"/> that this Report is based on.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.DataView"/> that this Report is based on.
        /// </value>
        [DataMember]
        public virtual DataView DataView { get; set; }

        /// <summary>
        /// Gets or sets the report fields.
        /// </summary>
        /// <value>
        /// The report fields.
        /// </value>
        [DataMember]
        public virtual ICollection<ReportField> ReportFields
        {
            get
            {
                return _reportFields ?? ( _reportFields = new Collection<ReportField>() );
            }
            set
            {
                _reportFields = value;
            }
        }
        private ICollection<ReportField> _reportFields;

        #endregion

        #region Methods

        public DataTable GetDataTable( RockContext context, Type entityType, List<EntityField> entityFields, List<AttributeCache> attributes, List<ReportField> selectComponents, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            if ( entityType != null )
            {
                Type[] modelType = { entityType };
                Type genericServiceType = typeof( Rock.Data.Service<> );
                Type modelServiceType = genericServiceType.MakeGenericType( modelType );

                object serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { context } );

                if ( serviceInstance != null )
                {
                    ParameterExpression paramExpression = serviceInstance.GetType().GetProperty( "ParameterExpression" ).GetValue( serviceInstance ) as ParameterExpression;
                    MemberExpression idExpression = Expression.Property( paramExpression, "Id" );

                    // Get AttributeValue queryable and parameter
                    var attributeValues = context.Set<AttributeValue>();
                    ParameterExpression attributeValueParameter = Expression.Parameter( typeof( AttributeValue ), "v" );

                    // Create the dynamic type
                    var dynamicFields = new Dictionary<string, Type>();
                    entityFields.ForEach( f => dynamicFields.Add( string.Format( "Entity_{0}", f.Name ), f.PropertyType ) );
                    attributes.ForEach( a => dynamicFields.Add( string.Format( "Attribute_{0}", a.Id ), typeof( string ) ) );
                    foreach( var reportField in selectComponents)
                    {
                        DataSelectComponent selectComponent = DataSelectContainer.GetComponent( reportField.DataSelectComponentEntityType.Name );
                        if (selectComponent != null)
                        {
                            dynamicFields.Add( string.Format( "Data_{0}", selectComponent.ColumnPropertyName ), selectComponent.ColumnFieldType );
                        }
                    }
                    Type dynamicType = LinqRuntimeTypeBuilder.GetDynamicType( dynamicFields );
                    ConstructorInfo methodFromHandle = dynamicType.GetConstructor( Type.EmptyTypes );

                    // Bind the dynamic fields to their expressions
                    var bindings = new List<MemberBinding>();
                    entityFields.ForEach( f => bindings.Add( Expression.Bind( dynamicType.GetField( string.Format( "Entity_{0}", f.Name ) ), Expression.Property( paramExpression, f.Name ) ) ) );
                    attributes.ForEach( a => bindings.Add( Expression.Bind( dynamicType.GetField( string.Format( "Attribute_{0}", a.Id ) ), GetAttributeValueExpression( attributeValues, attributeValueParameter, idExpression, a.Id ) ) ) );
                    foreach( var reportField in selectComponents)
                    {
                        DataSelectComponent selectComponent = DataSelectContainer.GetComponent( reportField.DataSelectComponentEntityType.Name );
                        if (selectComponent != null)
                        {
                            bindings.Add( Expression.Bind( dynamicType.GetField( string.Format( "Data_{0}", selectComponent.ColumnPropertyName ) ), selectComponent.GetExpression( context, idExpression, reportField.Selection ) ) );
                        }
                    }

                    Expression selector = Expression.Lambda( Expression.MemberInit( Expression.New( dynamicType.GetConstructor( Type.EmptyTypes ) ), bindings ), paramExpression );

                    Expression whereExpression = this.DataView.GetExpression( serviceInstance, paramExpression, out errorMessages );

                    MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ) } );
                    if ( getMethod != null )
                    {
                        var getResult = getMethod.Invoke( serviceInstance, new object[] { paramExpression, whereExpression } );
                        var qry = getResult as IQueryable<IEntity>;

                        var selectExpression = Expression.Call( typeof( Queryable ), "Select", new Type[] { qry.ElementType, dynamicType }, Expression.Constant( qry ), selector );
                        var query = qry.Provider.CreateQuery( selectExpression );

                        DataTable dt = new Service().GetDataTable( query.ToString(), CommandType.Text, null );

                        // The select does not return fields with the same name as the generic type's field names (not sure why), so need to rename columns
                        foreach ( var field in entityFields )
                        {
                            RenameColumn( dt, field.Name, "Entity_" + field.Name );
                        }

                        int colNum = 1;
                        foreach ( var attribute in attributes )
                        {
                            RenameColumn( dt, string.Format( "C{0}", colNum ), string.Format( "Attribute_{0}", attribute.Id ) );
                            colNum++;
                        }
                        foreach ( var reportField in selectComponents )
                        {
                            DataSelectComponent selectComponent = DataSelectContainer.GetComponent( reportField.DataSelectComponentEntityType.Name );
                            if ( selectComponent != null )
                            {
                                RenameColumn( dt, string.Format( "C{0}", colNum ), string.Format( "Data_{0}", selectComponent.ColumnPropertyName ) );
                                colNum++;
                            }
                        }

                        return dt;
                    }
                }
            }

            return null;
        }

        private Expression GetAttributeValueExpression(IQueryable<AttributeValue> attributeValues, ParameterExpression attributeValueParameter, Expression parentIdProperty, int attributeId)
        {
            MemberExpression attributeIdProperty = Expression.Property(attributeValueParameter, "AttributeId");
            MemberExpression entityIdProperty = Expression.Property( attributeValueParameter, "EntityId" );
            Expression attributeIdConstant = Expression.Constant(attributeId);

            Expression attributeIdCompare = Expression.Equal(attributeIdProperty, attributeIdConstant);
            Expression entityIdCompre = Expression.Equal( entityIdProperty, Expression.Convert( parentIdProperty, typeof( int? ) ) );
            Expression andExpression = Expression.And( attributeIdCompare, entityIdCompre );

            var match = new Expression[] {
                Expression.Constant(attributeValues),
                Expression.Lambda<Func<AttributeValue, bool>>( andExpression, new ParameterExpression[] { attributeValueParameter })
            };
            Expression whereExpression = Expression.Call( typeof( Queryable ), "Where", new Type[] { typeof( AttributeValue ) }, match );

            MemberExpression valueProperty = Expression.Property( attributeValueParameter, "Value" );
            Expression valueLambda = Expression.Lambda( valueProperty, new ParameterExpression[] { attributeValueParameter } );

            Expression selectValue = Expression.Call( typeof( Queryable ), "Select", new Type[] { typeof( AttributeValue ), typeof( string ) }, whereExpression, valueLambda );

            Expression firstOrDefault = Expression.Call( typeof( Queryable ), "FirstOrDefault", new Type[] { typeof( string ) }, selectValue );

            return firstOrDefault;
        }

        private void RenameColumn(DataTable dt, string colName, string newName)
        {
            var col = dt.Columns[colName];
            if (col != null)
            {
                col.ColumnName = newName;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Campus Configuration class.
    /// </summary>
    public partial class ReportConfiguration : EntityTypeConfiguration<Report>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfiguration"/> class.
        /// </summary>
        public ReportConfiguration()
        {
            this.HasOptional( r => r.Category ).WithMany().HasForeignKey( r => r.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.DataView ).WithMany().HasForeignKey( r => r.DataViewId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
