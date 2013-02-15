//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.CheckIn;
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Finds families based on a given search critieria (i.e. phone, barcode, etc)
    /// </summary>
    [Description("Finds families based on a given search critieria (i.e. phone, barcode, etc)")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Find Families" )]
    public class FindFamilies : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( Model.WorkflowAction action, Data.IEntity entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( action, out errorMessages );
            if (checkInState != null)
            {
                if ( checkInState.CheckIn.SearchType.Guid.Equals( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER ) )
                {
                    using ( new Rock.Data.UnitOfWorkScope() )
                    {
                        var personService = new PersonService();
                        var memberService = new GroupMemberService();

                        foreach ( var person in personService.GetByPhonePartial( checkInState.CheckIn.SearchValue ) )
                        {
                            foreach ( var group in person.Members.Where( m => m.Group.GroupType.Guid == SystemGuid.GroupType.GROUPTYPE_FAMILY ).Select( m => m.Group ) )
                            {
                                var family = checkInState.CheckIn.Families.Where( f => f.Group.Id == group.Id ).FirstOrDefault();
                                if ( family == null )
                                {
                                    family = new CheckInFamily();
                                    family.Group = group.Clone( false );
                                    family.Group.LoadAttributes();
                                    family.Caption = group.ToString();
                                    family.SubCaption = memberService.GetFirstNames( group.Id ).ToList().AsDelimited( "," );
                                    checkInState.CheckIn.Families.Add( family );
                                }
                            }
                        }
                    }

                    if ( checkInState.CheckIn.Families.Count > 0 )
                    {
                        SetCheckInState( action, checkInState );
                        return true;
                    }
                    else
                    {
                        errorMessages.Add( "There are not any families with the selected phone number" );
                    }

                }
                else
                {
                    errorMessages.Add( "Invalid Search Type" );
                }
            }

            return false;
        }
    }
}