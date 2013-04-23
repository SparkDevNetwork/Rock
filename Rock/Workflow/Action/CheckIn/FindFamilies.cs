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
                var phoneNumberSearch = false;
                var nameSearch = false;
                switch ( checkInState.CheckIn.SearchType.Guid.ToString().ToUpper() )
                {
                    case SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER:
                        {
                            phoneNumberSearch = true;
                            break;
                        }
                    case SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME:
                        {
                            nameSearch = true;
                            break;
                        }
                }
                //if ( checkInState.CheckIn.SearchType.Guid.Equals( new Guid( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER ) ) ) 
                //{ 
                //    phoneNumberSearch = true; 
                //}
                //else if ( checkInState.CheckIn.SearchType.Guid.Equals( new Guid( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME ) ) )
                //{
                //    nameSearch = true;
                //}
                //if ( checkInState.CheckIn.SearchType.Guid.Equals( new Guid( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER ) ) )
                if ( phoneNumberSearch || nameSearch )
                {
                    using ( new Rock.Data.UnitOfWorkScope() )
                    {
                        var personService = new PersonService();
                        var memberService = new GroupMemberService();

                        if ( phoneNumberSearch )
                        {
                            foreach ( var person in personService.GetByPhonePartial( checkInState.CheckIn.SearchValue ) )
                            {
                                foreach ( var group in person.Members.Where( m => m.Group.GroupType.Guid == new Guid( SystemGuid.GroupType.GROUPTYPE_FAMILY ) ).Select( m => m.Group ) )
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
                        } else if ( nameSearch )
                        {
                            foreach ( var person in personService.GetByPartialName( checkInState.CheckIn.SearchValue ) )
                            {
                                foreach ( var group in person.Members.Where( m => m.Group.GroupType.Guid == new Guid( SystemGuid.GroupType.GROUPTYPE_FAMILY ) ).Select( m => m.Group ) )
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
                        }
                    }

                    if ( checkInState.CheckIn.Families.Count > 0 )
                    {
                        SetCheckInState( action, checkInState );
                        return true;
                    }
                    else
                    {
                        if ( phoneNumberSearch )
                        {
                            errorMessages.Add( "There are not any families with the selected phone number" );
                        }
                        else if ( nameSearch )
                        {
                            errorMessages.Add( "There are not any families with the selected name" );
                        }
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