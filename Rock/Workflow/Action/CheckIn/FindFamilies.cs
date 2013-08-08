//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
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
    [BooleanField( "Allow None Found", "If true, show an error if nothing is returned by the search. Otherwise ignore the error and continue.", false, key: "AllowNoneFound" )]
    // ************************  WE SHOULD DEFAULT THE ALLOWNONEFOUND TO TRUE WHEN WE FIGURE OUT HOW TO USE THESE ATTRIBUTES *************************** //
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
            bool allowNoneFound = GetAttributeValue( "AllowNoneFound" ).AsBoolean();

            var checkInState = GetCheckInState( action, out errorMessages );
            if (checkInState != null)
            {
                var phoneNumberSearch = false;
                var nameSearch = false;
                var personService = new PersonService();
                var memberService = new GroupMemberService();
                IEnumerable<Person> person;
                var errorMessage = "";
                using ( new Rock.Data.UnitOfWorkScope() )
                {
                    switch ( checkInState.CheckIn.SearchType.Guid.ToString().ToUpper() )
                    {
                        case SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER:
                            {
                                phoneNumberSearch = true;
                                person = personService.GetByPhonePartial( checkInState.CheckIn.SearchValue );
                                errorMessage = "There are not any families with the selected phone number";
                                break;
                            }
                        case SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME:
                            {
                                nameSearch = true;
                                person = personService.GetByFullName( checkInState.CheckIn.SearchValue );
                                errorMessage = "There are not any families with the selected name";
                                break;
                            }
                        default:
                            {
                                person = null;
                                errorMessages.Add( "Invalid Search Type" );
                                break;
                            }
                    }
                }
                if ( phoneNumberSearch || nameSearch )
                {
                    foreach ( var p in person )
                    {
                        foreach ( var group in p.Members.Where( m => m.Group.GroupType.Guid == new Guid( SystemGuid.GroupType.GROUPTYPE_FAMILY ) ).Select( m => m.Group ) )
                        {
                            var family = checkInState.CheckIn.Families.Where( f => f.Group.Id == group.Id ).FirstOrDefault();
                            if ( family == null )
                            {
                                family = new CheckInFamily();
                                family.Group = group.Clone( false );
                                family.Group.LoadAttributes();
                                family.Caption = group.ToString();
                                family.SubCaption = memberService.GetFirstNames( group.Id ).ToList().AsDelimited( ", " );
                                checkInState.CheckIn.Families.Add( family );
                            }
                        }
                    }
                    if ( allowNoneFound )
                    {
                        if ( checkInState.CheckIn.Families.Count > 0 )
                        {
                            SetCheckInState( action, checkInState );
                            return true;
                        }
                        else
                        {
                            errorMessages.Add( errorMessage );
                        }
                    }
                    else
                    {
                        SetCheckInState( action, checkInState );
                        return true;
                    }
                }
            }
            return false;
        }
    }
}