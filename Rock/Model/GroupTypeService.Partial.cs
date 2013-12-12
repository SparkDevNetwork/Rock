//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.GroupType"/> entity objects. This class extends <see cref="Rock.Data.Service"/>.
    /// </summary>
    public partial class GroupTypeService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.GroupType"/> entities by the Id of their <see cref="Rock.Model.GroupTypeRole"/>.
        /// </summary>
        /// <param name="defaultGroupRoleId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupTypeRole"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.GroupType">GroupTypes</see> that use the provided <see cref="Rock.Model.GroupTypeRole"/> as the 
        /// default GroupRole for their member Groups.</returns>
        public IEnumerable<GroupType> GetByDefaultGroupRoleId( int? defaultGroupRoleId )
        {
            return Repository.Find( t => ( t.DefaultGroupRoleId == defaultGroupRoleId || ( defaultGroupRoleId == null && t.DefaultGroupRoleId == null ) ) );
        }

        /// <summary>
        /// Gets the child group types.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public IQueryable<GroupType> GetChildGroupTypes(int groupTypeId)
        {
            return Repository.AsQueryable().Where( t => t.ParentGroupTypes.Select( p => p.Id ).Contains( groupTypeId ) );
        }

        /// <summary>
        /// Gets the parent group types.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public IQueryable<GroupType> GetParentGroupTypes( int groupTypeId )
        {
            return Repository.AsQueryable().Where( t => t.ChildGroupTypes.Select( p => p.Id ).Contains( groupTypeId ) );
        }

        /// <summary>
        /// Verifies if the specified <see cref="Rock.Model.GroupType"/> can be deleted, and if so deletes it.
        /// </summary>
        /// <param name="item">The <see cref="Rock.Model.GroupType"/> to delete.</param>
        /// <param name="personId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who is attempting to delete the
        /// <see cref="Rock.Model.GroupType"/>.</param>
        /// <returns>A <see cref="System.Boolean"/> value that is <c>true</c> if the <see cref="Rock.Model.GroupType"/> was able to be successfully deleted, otherwise <c>false</c>.</returns>
        public override bool Delete( GroupType item, int? personId )
        {
            string message;
            if ( !CanDelete( item, out message ) )
            {
                return false;
            }

            item.ChildGroupTypes.Clear();
            this.Save( item, personId );

            return base.Delete( item, personId );
        }

        /// <summary>
        /// Saves the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public override bool Save( GroupType item, int? personId )
        {
            // ensure that the BinaryFile.IsTemporary flag is set to false for any BinaryFiles that are associated with this record
            if ( item.IconLargeFileId.HasValue )
            {
                BinaryFileService binaryFileService = new BinaryFileService( this.RockContext );
                var binaryFile = binaryFileService.Get( item.IconLargeFileId.Value );
                if ( binaryFile != null && binaryFile.IsTemporary )
                {
                    binaryFile.IsTemporary = false;
                }
            }

            // ensure that the BinaryFile.IsTemporary flag is set to false for any BinaryFiles that are associated with this record
            if ( item.IconSmallFileId.HasValue )
            {
                BinaryFileService binaryFileService = new BinaryFileService( this.RockContext );
                var binaryFile = binaryFileService.Get( item.IconSmallFileId.Value );
                if ( binaryFile != null && binaryFile.IsTemporary )
                {
                    binaryFile.IsTemporary = false;
                }
            }
            
            return base.Save( item, personId );
        }
    }
}
