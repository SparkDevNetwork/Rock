using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;

namespace com.bemaservices.PastoralCare.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class CareTypeService : Service<CareType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CareTypeService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public CareTypeService( RockContext context ) : base( context ) { }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( CareType item, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( item.CareItemCount > 0 )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", CareType.FriendlyTypeName, CareItem.FriendlyTypeName );
                return false;
            }
            return true;
        }

        private void InitModel<T>( ref T model ) where T : Rock.Data.IModel
        {
            model.CreatedByPersonAlias = null;
            model.CreatedByPersonAliasId = null;
            model.CreatedDateTime = RockDateTime.Now;
            model.ModifiedByPersonAlias = null;
            model.ModifiedByPersonAliasId = null;
            model.ModifiedDateTime = RockDateTime.Now;
            model.Id = 0;
            model.Guid = Guid.NewGuid();
        }

        public int Copy( int careTypeId )
        {
            var careType = this.Get( careTypeId );
            RockContext rockContext = ( RockContext ) Context;
            int newCareTypeId = 0;
            AttributeService attributeService = new AttributeService( rockContext );
            var authService = new AuthService( rockContext );

            // Get current Opportunity attributes 
            var careItemAttributes = attributeService
                .GetByEntityTypeId( new CareItem().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "CareTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( careType.Id.ToString() ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            CareType newCareType = new CareType();
            rockContext.WrapTransaction( () =>
            {

                newCareType.CopyPropertiesFrom( careType );
                InitModel( ref newCareType );
                newCareType.Name = careType.Name + " - Copy";
                this.Add( newCareType );
                rockContext.SaveChanges();
                newCareTypeId = newCareType.Id;

                rockContext.SaveChanges();

                // Clone the Opportunity attributes
                List<Rock.Model.Attribute> newAttributesState = new List<Rock.Model.Attribute>();
                foreach ( var attribute in careItemAttributes )
                {
                    var newAttribute = attribute.Clone( false );
                    InitModel( ref newAttribute );
                    newAttribute.IsSystem = false;
                    newAttributesState.Add( newAttribute );

                    foreach ( var qualifier in attribute.AttributeQualifiers )
                    {
                        var newQualifier = qualifier.Clone( false );
                        newQualifier.Id = 0;
                        newQualifier.Guid = Guid.NewGuid();
                        newQualifier.IsSystem = false;
                        newAttribute.AttributeQualifiers.Add( qualifier );
                    }
                }

                // Save Attributes
                string qualifierValue = newCareType.Id.ToString();
                Rock.Attribute.Helper.SaveAttributeEdits( newAttributesState, new ConnectionOpportunity().TypeId, "CareTypeId", qualifierValue, rockContext );

                // Copy Security
                Rock.Security.Authorization.CopyAuthorization( careType, newCareType, rockContext );
            } );

            return newCareTypeId;
        }

    }

    public static partial class ConnectionTypeExtensionMethods
    {
        public static CareType Clone( this CareType source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as CareType;
            }
            else
            {
                var target = new CareType();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from another CareType object to this CareType object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this CareType target, CareType source )
        {
            target.Id = source.Id;
            target.Description = source.Description;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.IsActive = source.IsActive;
            target.Name = source.Name;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }
    }
}
