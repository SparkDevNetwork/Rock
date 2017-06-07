using Rock.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

/// <summary>
/// Each class in here defines a wrapper function for all of the "Actions" a person at CCV can take.
/// There are Adult and Student versions of each.
/// See the Database UserDefined Functions to manage their actual implementation
/// </summary>
namespace church.ccv.Actions
{
    public static class Actions_Adult
    {
        /// <summary>
        /// Baptised
        /// </summary>
        public static class Baptised
        {
            class ResultTable
            {
                public int PersonId { get; set; }
                public bool IsBaptised { get; set; }
                public DateTime? BaptisedDate { get; set; }
            }

            public static bool IsBaptised( int personId, out DateTime? baptismDate )
            {
                // default to null
                baptismDate = null;

                RockContext rockContext = new RockContext( );
                var result = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Adult_IsBaptised(@PersonId)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                // if they're performing the action, get the baptism date
                if( result.IsBaptised == true )
                {
                    baptismDate = result.BaptisedDate;
                }
            
                return result.IsBaptised;
            }
        }

        /// <summary>
        /// ERA
        /// </summary>
        public static class ERA
        {
            class ResultTable
            {
                public int PersonId { get; set; }
                public bool IsERA { get; set; }
            }

            public static bool IsERA( int personId )
            {
                RockContext rockContext = new RockContext( );
                var result = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Adult_IsERA(@PersonId)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );
                        
                return result.IsERA;
            }
        }

        /// <summary>
        /// Giving
        /// </summary>
        public static class Give
        {
            class ResultTable
            {
                public int PersonId { get; set; }
                public bool IsGiving { get; set; }
            }

            public static bool IsGiving( int personId )
            {
                RockContext rockContext = new RockContext( );
                var result = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Adult_IsGiving(@PersonId)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );
                        
                return result.IsGiving;
            }
        }

        /// <summary>
        /// Member
        /// </summary>
        public static class Member
        {
            class ResultTable
            {
                public int PersonId { get; set; }
                public bool IsMember { get; set; }
                public DateTime? MembershipDate { get; set; }
            }

            public static bool IsMember( int personId, out DateTime? membershipDate )
            {
                // assume null
                membershipDate = null;

                RockContext rockContext = new RockContext( );
                var result = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Adult_IsMember(@PersonId)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                if ( result.IsMember == true )
                {
                    membershipDate = result.MembershipDate;
                }
                        
                return result.IsMember;
            }
        }

        /// <summary>
        /// Starting Point
        /// </summary>
        public static class StartingPoint
        {
            class ResultTable
            {
                public int PersonId { get; set; }
                public bool TakenStartingPoint { get; set; }
                public DateTime? StartingPointDate { get; set; }
            }

            public static bool TakenStartingPoint( int personId, out DateTime? startingPointDate )
            {
                // assume null
                startingPointDate = null;

                RockContext rockContext = new RockContext( );
                var result = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Adult_TakenStartingPoint(@PersonId)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                if ( result.TakenStartingPoint == true )
                {
                    startingPointDate = result.StartingPointDate;
                }
                        
                return result.TakenStartingPoint;
            }
        }

        /// <summary>
        /// Peer Learning
        /// </summary>
        public static class PeerLearning
        {
            class ResultTable
            {
                public int PersonId { get; set; }
            	
                public bool Neighborhood_IsPeerLearning { get; set; }
	            public string Neighborhood_GroupIds { get; set; }

                public bool YoungAdult_IsPeerLearning { get; set; }
                public string YoungAdult_GroupIds { get; set; }
            }

            public class Result
            {
                public bool Neighborhood_IsPeerLearning { get; set; }
	            public List<int> Neighborhood_GroupIds { get; set; }

                public bool YoungAdult_IsPeerLearning { get; set; }
                public List<int> YoungAdult_GroupIds { get; set; }

                public bool IsPeerLearning( )
                {
                    return  // if any of the above learning flags are true, then yes, they're peer learning
                            (Neighborhood_IsPeerLearning == true ||
                             YoungAdult_IsPeerLearning == true )
                         
                             ? true : false;
                }

                public List<int> GetPeerLearningGroups( )
                {
                    // this combines all group ids, because it's likely the caller maybe just need
                    // to know the group ids, and doesn't care about which group is which type.

                    List<int> groupIds = new List<int>( );

                    groupIds.AddRange( Neighborhood_GroupIds );
                    groupIds.AddRange( YoungAdult_GroupIds );

                    return groupIds;
                }
            }

            public static void IsPeerLearning( int personId, out Result returnResult )
            {
                // call the function
                RockContext rockContext = new RockContext( );
                var sqlResultTable = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Adult_IsPeerLearning(@PersonId, 0)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                // convert the resultTable into a result for the caller
                returnResult = new Result( )
                {
                    Neighborhood_IsPeerLearning = sqlResultTable.Neighborhood_IsPeerLearning,
                    Neighborhood_GroupIds = sqlResultTable.Neighborhood_GroupIds != null ? sqlResultTable.Neighborhood_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),

                    YoungAdult_IsPeerLearning = sqlResultTable.YoungAdult_IsPeerLearning,
                    YoungAdult_GroupIds = sqlResultTable.YoungAdult_GroupIds != null ? sqlResultTable.YoungAdult_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( )
                };
            }
        }

        /// <summary>
        /// Mentored
        /// </summary>
        public static class Mentored
        {
            class ResultTable
            {
                public int PersonId { get; set; }

                public bool Neighborhood_IsMentored { get; set; }
                public string Neighborhood_GroupIds { get; set; }
    
                public bool YoungAdult_IsMentored { get; set; }
                public string YoungAdult_GroupIds { get; set; }

                public bool NextSteps_IsMentored { get; set; }
                public string NextSteps_GroupIds { get; set; }
            }

            public class Result
            {
                public bool Neighborhood_IsMentored { get; set; }
                public List<int> Neighborhood_GroupIds { get; set; }
    
                public bool YoungAdult_IsMentored { get; set; }
                public List<int> YoungAdult_GroupIds { get; set; }

                public bool NextSteps_IsMentored { get; set; }
                public List<int> NextSteps_GroupIds { get; set; }

                public bool IsMentored( )
                {
                    return  // if any of the above mentored flags are true, then yes, they're being mentored.
                            (Neighborhood_IsMentored == true ||
                             YoungAdult_IsMentored == true ||
                             NextSteps_IsMentored == true) 
                         
                             ? true : false;
                }

                public List<int> GetCombinedMentorGroups( )
                {
                    // this combines all group ids, because it's likely the caller maybe just need
                    // to know the group ids, and doesn't care about which group is which type.

                    List<int> groupIds = new List<int>( );

                    groupIds.AddRange( Neighborhood_GroupIds );
                    groupIds.AddRange( YoungAdult_GroupIds );
                    groupIds.AddRange( NextSteps_GroupIds );

                    return groupIds;
                }
            }

            public static void IsMentored( int personId, out Result returnResult )
            {            
                // call the function
                RockContext rockContext = new RockContext( );
                var sqlResultTable = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Adult_IsMentored(@PersonId, 0)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                // convert the resultTable into a result for the caller
                returnResult = new Result( )
                {
                    Neighborhood_IsMentored = sqlResultTable.Neighborhood_IsMentored,
                    Neighborhood_GroupIds = sqlResultTable.Neighborhood_GroupIds != null ? sqlResultTable.Neighborhood_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),
    
                    YoungAdult_IsMentored = sqlResultTable.YoungAdult_IsMentored,
                    YoungAdult_GroupIds = sqlResultTable.YoungAdult_GroupIds != null ? sqlResultTable.YoungAdult_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),

                    NextSteps_IsMentored = sqlResultTable.NextSteps_IsMentored,
                    NextSteps_GroupIds = sqlResultTable.NextSteps_GroupIds != null ? sqlResultTable.NextSteps_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),
                };
            }
        }

        /// <summary>
        /// Serving
        /// </summary>
        public static class Serving
        {
            class ResultTable
            {
                public int PersonId { get; set; }

                public bool IsServing { get; set; }
                public string GroupIds { get; set; }
            }

            public class Result
            {
                public bool IsServing { get; set; }
                public List<int> GroupIds { get; set; }

                // note: We don't need accessors for this, because there's only one group type.
            }

            public static void IsServing( int personId, out Result returnResult )
            {            
                // call the function
                RockContext rockContext = new RockContext( );
                var sqlResultTable = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Adult_IsServing(@PersonId, 0)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                // convert the resultTable into a result for the caller
                returnResult = new Result( )
                {
                    IsServing = sqlResultTable.IsServing,
                    GroupIds = sqlResultTable.GroupIds != null ? sqlResultTable.GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),
                };
            }
        }

        /// <summary>
        /// Teaching
        /// </summary>
        public static class Teaching
        {
            class ResultTable
            {
                public int PersonId { get; set; }
            
                public bool Neighborhood_SubSection_IsTeaching  { get; set; }
                public string Neighborhood_SubSection_GroupIds  { get; set; }

                public bool Neighborhood_IsTeaching  { get; set; }
                public string Neighborhood_GroupIds { get; set; }
    
                public bool YoungAdult_Section_IsTeaching { get; set; }
                public string YoungAdult_Section_GroupIds { get; set; }

                public bool YoungAdult_IsTeaching { get; set; }
                public string YoungAdult_GroupIds { get; set; }
    
                public bool NextSteps_SubSection_IsTeaching { get; set; }
                public string NextSteps_SubSection_GroupIds { get; set; }

                public bool NextSteps_IsTeaching { get; set; }
                public string NextSteps_GroupIds { get; set; }
	
                public bool NextGen_Section_IsTeaching { get; set; }
                public string NextGen_Section_GroupIds { get; set; }

                public bool NextGen_IsTeaching { get; set; }
                public string NextGen_GroupIds { get; set; }

                public bool LifeSteps_IsTeaching { get; set; }
                public string LifeSteps_GroupIds { get; set; }
            }

            public class Result
            {
                public bool Neighborhood_SubSection_IsTeaching  { get; set; }
                public List<int> Neighborhood_SubSection_GroupIds  { get; set; }

                public bool Neighborhood_IsTeaching  { get; set; }
                public List<int> Neighborhood_GroupIds { get; set; }
    
                public bool YoungAdult_Section_IsTeaching { get; set; }
                public List<int> YoungAdult_Section_GroupIds { get; set; }

                public bool YoungAdult_IsTeaching { get; set; }
                public List<int> YoungAdult_GroupIds { get; set; }
    
                public bool NextSteps_SubSection_IsTeaching { get; set; }
                public List<int> NextSteps_SubSection_GroupIds { get; set; }

                public bool NextSteps_IsTeaching { get; set; }
                public List<int> NextSteps_GroupIds { get; set; }
	
                public bool NextGen_Section_IsTeaching { get; set; }
                public List<int> NextGen_Section_GroupIds { get; set; }

                public bool NextGen_IsTeaching { get; set; }
                public List<int> NextGen_GroupIds { get; set; }

                public bool LifeSteps_IsTeaching { get; set; }
                public List<int> LifeSteps_GroupIds { get; set; }

                public bool IsTeaching( )
                {
                    return // if any of the above teaching flags are true, then yes, they're teaching.
                           (Neighborhood_SubSection_IsTeaching == true ||
                            Neighborhood_IsTeaching == true ||
                            YoungAdult_Section_IsTeaching == true ||
                            YoungAdult_IsTeaching == true ||
                            NextSteps_SubSection_IsTeaching == true ||
                            NextSteps_IsTeaching == true ||
                            NextGen_Section_IsTeaching == true ||
                            NextGen_IsTeaching == true ||
                            LifeSteps_IsTeaching == true) 
                    
                            ? true : false;
                }

                public List<int> GetCombinedTeachingGroups( )
                {
                    // this combines all group ids, because it's likely the caller maybe just need
                    // to know the group ids, and doesn't care about which group is which type.
                    List<int> groupIds = new List<int>( );

                    groupIds.AddRange( Neighborhood_SubSection_GroupIds );
                    groupIds.AddRange( Neighborhood_GroupIds );

                    groupIds.AddRange( YoungAdult_Section_GroupIds );
                    groupIds.AddRange( YoungAdult_GroupIds );

                    groupIds.AddRange( NextSteps_SubSection_GroupIds );
                    groupIds.AddRange( NextSteps_GroupIds );

                    groupIds.AddRange( NextGen_Section_GroupIds );
                    groupIds.AddRange( NextGen_GroupIds );

                    groupIds.AddRange( LifeSteps_GroupIds );

                    return groupIds;
                }
            }

            public static void IsTeaching( int personId, out Result returnResult )
            {            
                // call the function
                RockContext rockContext = new RockContext( );
                var sqlResultTable = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Adult_IsTeaching(@PersonId, 0)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                // convert the resultTable into a result for the caller
                returnResult = new Result( )
                {
                    Neighborhood_SubSection_IsTeaching = sqlResultTable.Neighborhood_SubSection_IsTeaching,
                    Neighborhood_SubSection_GroupIds   = sqlResultTable.Neighborhood_SubSection_GroupIds != null ? sqlResultTable.Neighborhood_SubSection_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),

                    Neighborhood_IsTeaching            = sqlResultTable.Neighborhood_IsTeaching,
                    Neighborhood_GroupIds              = sqlResultTable.Neighborhood_GroupIds != null ? sqlResultTable.Neighborhood_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),
    
                    YoungAdult_Section_IsTeaching      = sqlResultTable.YoungAdult_Section_IsTeaching,
                    YoungAdult_Section_GroupIds        = sqlResultTable.YoungAdult_Section_GroupIds != null ? sqlResultTable.YoungAdult_Section_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),

                    YoungAdult_IsTeaching              = sqlResultTable.YoungAdult_IsTeaching,
                    YoungAdult_GroupIds                = sqlResultTable.YoungAdult_GroupIds != null ? sqlResultTable.YoungAdult_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),
    
                    NextSteps_SubSection_IsTeaching    = sqlResultTable.NextSteps_SubSection_IsTeaching,
                    NextSteps_SubSection_GroupIds      = sqlResultTable.NextSteps_SubSection_GroupIds != null ? sqlResultTable.NextSteps_SubSection_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),

                    NextSteps_IsTeaching               = sqlResultTable.NextSteps_IsTeaching,
                    NextSteps_GroupIds                 = sqlResultTable.NextSteps_GroupIds != null ? sqlResultTable.NextSteps_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),
	
                    NextGen_Section_IsTeaching         = sqlResultTable.NextGen_Section_IsTeaching,
                    NextGen_Section_GroupIds           = sqlResultTable.NextGen_Section_GroupIds != null ? sqlResultTable.NextGen_Section_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),

                    NextGen_IsTeaching                 = sqlResultTable.NextGen_IsTeaching,
                    NextGen_GroupIds                   = sqlResultTable.NextGen_GroupIds != null ? sqlResultTable.NextGen_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),

                    LifeSteps_IsTeaching               = sqlResultTable.LifeSteps_IsTeaching,
                    LifeSteps_GroupIds                 = sqlResultTable.LifeSteps_GroupIds != null ? sqlResultTable.LifeSteps_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),
                };
            }
        }
    }

    public static class Actions_Student
    {
        /// <summary>
        /// Baptised
        /// </summary>
        public static class Baptised
        {
            class ResultTable
            {
                public int PersonId { get; set; }
                public bool IsBaptised { get; set; }
                public DateTime? BaptisedDate { get; set; }
            }

            public static bool IsBaptised( int personId, out DateTime? baptismDate )
            {
                // default to null
                baptismDate = null;

                RockContext rockContext = new RockContext( );
                var result = rockContext.Database.SqlQuery<ResultTable>
                (
                    "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Student_IsBaptised(@PersonId)",
                    new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                ).ToList( ).SingleOrDefault( );

                // if they're performing the action, get the baptism date
                if ( result.IsBaptised == true )
                {
                    baptismDate = result.BaptisedDate;
                }

                return result.IsBaptised;
            }
        }

        /// <summary>
        /// ERA
        /// </summary>
        public static class ERA
        {
            class ResultTable
            {
                public int PersonId { get; set; }
                public bool IsERA { get; set; }
            }

            public static bool IsERA( int personId )
            {
                RockContext rockContext = new RockContext( );
                var result = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Student_IsERA(@PersonId)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );
                        
                return result.IsERA;
            }
        }

        /// <summary>
        /// Giving
        /// </summary>
        public static class Give
        {
            class ResultTable
            {
                public int PersonId { get; set; }
                public bool IsGiving { get; set; }
            }

            public static bool IsGiving( int personId )
            {
                RockContext rockContext = new RockContext( );
                var result = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Student_IsGiving(@PersonId)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );
                        
                return result.IsGiving;
            }
        }

        /// <summary>
        /// Member
        /// </summary>
        public static class Member
        {
            class ResultTable
            {
                public int PersonId { get; set; }
                public bool IsMember { get; set; }
                public DateTime? MembershipDate { get; set; }
            }
            
            public static bool IsMember( int personId, out DateTime? membershipDate )
            {
                // assume null
                membershipDate = null;

                RockContext rockContext = new RockContext( );
                var result = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Student_IsMember(@PersonId)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                if ( result.IsMember == true )
                {
                    membershipDate = result.MembershipDate;
                }

                return result.IsMember;
            }
        }

        /// <summary>
        /// Starting Point
        /// </summary>
        public static class StartingPoint
        {
            class ResultTable
            {
                public int PersonId { get; set; }
                public bool TakenStartingPoint { get; set; }
                public DateTime? StartingPointDate { get; set; }
            }

            public static bool TakenStartingPoint( int personId, out DateTime? startingPointDate )
            {
                // assume null
                startingPointDate = null;

                RockContext rockContext = new RockContext( );
                var result = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Student_TakenStartingPoint(@PersonId)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                if ( result.TakenStartingPoint == true )
                {
                    startingPointDate = result.StartingPointDate;
                }
                        
                return result.TakenStartingPoint;
            }
        }

        /// <summary>
        /// Peer Learning
        /// </summary>
        public static class PeerLearning
        {
            class ResultTable
            {
                public int PersonId { get; set; }
            	
                public bool NextGen_IsPeerLearning { get; set; }
	            public string NextGen_GroupIds { get; set; }
            }

            public class Result
            {
                public bool NextGen_IsPeerLearning { get; set; }
	            public List<int> NextGen_GroupIds { get; set; }

                public bool IsPeerLearning( )
                {
                    return  // if any of the above learning flags are true, then yes, they're peer learning
                            (NextGen_IsPeerLearning == true)
                         
                             ? true : false;
                }

                public List<int> GetPeerLearningGroups( )
                {
                    // this combines all group ids, because it's likely the caller maybe just need
                    // to know the group ids, and doesn't care about which group is which type.

                    List<int> groupIds = new List<int>( );

                    groupIds.AddRange( NextGen_GroupIds );

                    return groupIds;
                }
            }

            public static void IsPeerLearning( int personId, out Result returnResult )
            {
                // call the function
                RockContext rockContext = new RockContext( );
                var sqlResultTable = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Student_IsPeerLearning(@PersonId, 0)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                // convert the resultTable into a result for the caller
                returnResult = new Result( )
                {
                    NextGen_IsPeerLearning = sqlResultTable.NextGen_IsPeerLearning,
                    NextGen_GroupIds = sqlResultTable.NextGen_GroupIds != null ? sqlResultTable.NextGen_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( )
                };
            }
        }

        /// <summary>
        /// Mentored
        /// </summary>
        public static class Mentored
        {
            class ResultTable
            {
                public int PersonId { get; set; }

                public bool NextGen_IsMentored { get; set; }
                public string NextGen_GroupIds { get; set; }
            }

            public class Result
            {
                public bool NextGen_IsMentored { get; set; }
                public List<int> NextGen_GroupIds { get; set; }

                public bool IsMentored( )
                {
                    return  // if any of the above mentored flags are true, then yes, they're being mentored.
                            (NextGen_IsMentored == true) 
                         
                             ? true : false;
                }

                public List<int> GetCombinedMentorGroups( )
                {
                    // this combines all group ids, because it's likely the caller maybe just need
                    // to know the group ids, and doesn't care about which group is which type.

                    List<int> groupIds = new List<int>( );

                    groupIds.AddRange( NextGen_GroupIds );

                    return groupIds;
                }
            }

            public static void IsMentored( int personId, out Result returnResult )
            {            
                // call the function
                RockContext rockContext = new RockContext( );
                var sqlResultTable = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Student_IsMentored(@PersonId, 0)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                // convert the resultTable into a result for the caller
                returnResult = new Result( )
                {
                    NextGen_IsMentored = sqlResultTable.NextGen_IsMentored,
                    NextGen_GroupIds = sqlResultTable.NextGen_GroupIds != null ? sqlResultTable.NextGen_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( )
                };
            }
        }

        /// <summary>
        /// Serving
        /// </summary>
        public static class Serving
        {
            class ResultTable
            {
                public int PersonId { get; set; }

                public bool IsServing { get; set; }
                public string GroupIds { get; set; }
            }

            public class Result
            {
                public bool IsServing { get; set; }
                public List<int> GroupIds { get; set; }

                // note: We don't need accessors for this, because there's only one group type.
            }

            public static void IsServing( int personId, out Result returnResult )
            {            
                // call the function
                RockContext rockContext = new RockContext( );
                var sqlResultTable = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Student_IsServing(@PersonId, 0)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                // convert the resultTable into a result for the caller
                returnResult = new Result( )
                {
                    IsServing = sqlResultTable.IsServing,
                    GroupIds = sqlResultTable.GroupIds != null ? sqlResultTable.GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( ),
                };
            }
        }

        /// <summary>
        /// Teaching
        /// </summary>
        public static class Teaching
        {
            class ResultTable
            {
                public int PersonId { get; set; }
            
                public bool Undefined_IsTeaching  { get; set; }
                public string Undefined_GroupIds  { get; set; }
            }

            public class Result
            {
                public bool Undefined_IsTeaching  { get; set; }
                public List<int> Undefined_GroupIds  { get; set; }

                public bool IsTeaching( )
                {
                    return // if any of the above teaching flags are true, then yes, they're teaching.
                           (Undefined_IsTeaching == true) 
                    
                            ? true : false;
                }

                public List<int> GetCombinedTeachingGroups( )
                {
                    // this combines all group ids, because it's likely the caller maybe just need
                    // to know the group ids, and doesn't care about which group is which type.
                    List<int> groupIds = new List<int>( );

                    groupIds.AddRange( Undefined_GroupIds );

                    return groupIds;
                }
            }

            public static void IsTeaching( int personId, out Result returnResult )
            {            
                // call the function
                RockContext rockContext = new RockContext( );
                var sqlResultTable = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Student_IsTeaching(@PersonId, 0)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                // convert the resultTable into a result for the caller
                returnResult = new Result( )
                {
                    Undefined_IsTeaching = sqlResultTable.Undefined_IsTeaching,
                    Undefined_GroupIds   = sqlResultTable.Undefined_GroupIds != null ? sqlResultTable.Undefined_GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( )
                };
            }
        }
    }
}
