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
        /// Share Story
        /// </summary>
        public static class ShareStory
        {
            class ResultTable
            {
                public int PersonId { get; set; }
                public bool SharedStory { get; set; }
                public string StoryIds { get; set; }
            }

            public static bool SharedStory( int personId, out List<int> storyIds )
            {
                // assume null
                storyIds = null;

                RockContext rockContext = new RockContext( );
                var result = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Adult_SharedStory(@PersonId)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                if ( result.SharedStory == true )
                {
                    storyIds = result.StoryIds != null ? result.StoryIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( );
                }
                        
                return result.SharedStory;
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

                public bool IsPeerLearning { get; set; }
                public string GroupIds { get; set; }
            }

            public class Result
            {
                public bool IsPeerLearning { get; set; }
	            public List<int> GroupIds { get; set; }
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
                    IsPeerLearning = sqlResultTable.IsPeerLearning,
                    GroupIds = sqlResultTable.GroupIds != null ? sqlResultTable.GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( )
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

                public bool IsMentored { get; set; }
                public string GroupIds { get; set; }
            }

            public class Result
            {
                public bool IsMentored { get; set; }
                public List<int> GroupIds { get; set; }
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
                    IsMentored = sqlResultTable.IsMentored,
                    GroupIds = sqlResultTable.GroupIds != null ? sqlResultTable.GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( )
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
                    GroupIds = sqlResultTable.GroupIds != null ? sqlResultTable.GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( )
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
            
                public bool IsTeaching  { get; set; }
                public string GroupIds  { get; set; }
            }

            public class Result
            {
                public bool IsTeaching  { get; set; }
                public List<int> GroupIds  { get; set; }
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
                    IsTeaching = sqlResultTable.IsTeaching,
                    GroupIds   = sqlResultTable.GroupIds != null ? sqlResultTable.GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( )
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
        /// Share Story
        /// </summary>
        public static class ShareStory
        {
            class ResultTable
            {
                public int PersonId { get; set; }
                public bool SharedStory { get; set; }
                public string StoryIds { get; set; }
            }

            public static bool SharedStory( int personId, out List<int> storyIds )
            {
                // assume null
                storyIds = null;

                RockContext rockContext = new RockContext( );
                var result = rockContext.Database.SqlQuery<ResultTable>
                    (
                        "SELECT TOP 1 * FROM dbo._church_ccv_ufnActions_Student_SharedStory(@PersonId)",
                        new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                    ).ToList( ).SingleOrDefault( );

                if ( result.SharedStory == true )
                {
                    storyIds = result.StoryIds != null ? result.StoryIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( );
                }
                        
                return result.SharedStory;
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
            	
                public bool IsPeerLearning { get; set; }
	            public string GroupIds { get; set; }
            }

            public class Result
            {
                public bool IsPeerLearning { get; set; }
	            public List<int> GroupIds { get; set; }
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
                    IsPeerLearning = sqlResultTable.IsPeerLearning,
                    GroupIds = sqlResultTable.GroupIds != null ? sqlResultTable.GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( )
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

                public bool IsMentored { get; set; }
                public string GroupIds { get; set; }
            }

            public class Result
            {
                public bool IsMentored { get; set; }
                public List<int> GroupIds { get; set; }
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
                    IsMentored = sqlResultTable.IsMentored,
                    GroupIds = sqlResultTable.GroupIds != null ? sqlResultTable.GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( )
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
            
                public bool IsTeaching  { get; set; }
                public string GroupIds  { get; set; }
            }

            public class Result
            {
                public bool IsTeaching  { get; set; }
                public List<int> GroupIds  { get; set; }
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
                    IsTeaching = sqlResultTable.IsTeaching,
                    GroupIds   = sqlResultTable.GroupIds != null ? sqlResultTable.GroupIds.Split( ',' ).Select( Int32.Parse ).ToList( ) : new List<int>( )
                };
            }
        }
    }
}
