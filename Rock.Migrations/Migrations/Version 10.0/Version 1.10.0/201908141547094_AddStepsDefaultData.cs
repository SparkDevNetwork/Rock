// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Diagnostics;

namespace Rock.Migrations
{
    /// <summary>
    /// Add data for a sample Steps Program.
    /// </summary>
    public partial class AddStepsDefaultData : Rock.Migrations.RockMigration
    {
        private static class Constants
        {
            public static string StepsCategoryGuid = "{C47AD7D0-DDD0-4BE6-AE1D-D69D72B13842}";
            public static string StepProgramDiscipleshipGuid = "{A31AE259-885E-4ACE-B8B6-56000F58EA3B}";
            public static string StepStatusInProgressGuid = "{E2F2EA68-7D30-490D-8EA7-E1B0CC625B0F}";
            public static string StepStatusCompleteGuid = "{2BC1146E-4DBF-4915-814B-F0FED29DDD19}";
            public static string StepTypeBaptismGuid = "{801CC43C-0641-4271-939E-75E428F31D06}";
            public static string StepTypeStartingPointGuid = "{F3585131-3EBF-49C4-83A0-3F88271A9B9B}";
            public static string StepTypeServeGuid = "{71E66730-8F7D-4EEF-9C53-524C4BDE5E59}";
            public static string StepTypeSmallGroupGuid = "{EFA15A4F-5666-4153-B92F-AF3ECD73C504}";
            public static string StepPrerequisiteServeRequiresStartingPointGuid = "{85ADDB51-3C92-4A82-84B3-00B9A4E3AE29}";
            public static string ColorYellow = "#FFC870";
            public static string ColorGreen = "#16C98D";
            public static int StepsListCardView = 0;
            public static int StepsListGridView = 1;
        }

        private DateTime _CreatedDateTime = DateTime.Now;

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddStepsProgramsUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
        }

        private void AddStepsProgramsUp()
        {
            // Rock.Model.StepProgram doesn't exist on a new database build yet
            RockMigrationHelper.UpdateEntityType( "Rock.Model.StepProgram", SystemGuid.EntityType.STEP_PROGRAM, true, true );

            //
            // Add Step Program Category: "Discipleship"
            //
            AddCategory( Constants.StepsCategoryGuid, "Discipleship", "Rock.Model.StepProgram" );

            // Add Step Program.
            AddStepProgram( Constants.StepProgramDiscipleshipGuid,
                "Discipleship Path",
                "This program outlines the official steps necessary for becoming a 'Disciple'.",
                "fa fa-walking",
                Constants.StepsListCardView,
                Constants.StepsCategoryGuid,
                0 );

            // Add Step Statuses.
            AddStepStatus( Constants.StepStatusInProgressGuid,
                "In Progress",
                Constants.StepProgramDiscipleshipGuid,
                Constants.ColorYellow,
                false,
                1 );
            AddStepStatus( Constants.StepStatusCompleteGuid,
                "Complete",
                Constants.StepProgramDiscipleshipGuid,
                Constants.ColorGreen,
                true,
                2 );

            // Add Step Types
            AddStepType( Constants.StepTypeBaptismGuid,
                Constants.StepProgramDiscipleshipGuid,
                "Baptism",
                "An act of obedience symbolizing the believer's faith in a crucified, buried, and risen Savior, the believer's death to sin, the burial of the old life, and the resurrection to walk in newness of life in Christ Jesus. It is a testimony to the believer's faith in the final resurrection of the dead.",
                "fa fa-tint",
                "#709ac7",
                false,
                false,
                false,
                1 );

            AddStepType( Constants.StepTypeStartingPointGuid,
                Constants.StepProgramDiscipleshipGuid,
                "Starting Point Class",
                "Starting Point will give you a sneak peek into the basic beliefs and practices of our church, equip you with the essentials of the Christian faith, and show you opportunities where you can get involved. This is your first step in getting connected.",
                "fa fa-play",
                "#709ac7",
                true,
                true,
                false,
                2 );

            AddStepType( Constants.StepTypeServeGuid,
                Constants.StepProgramDiscipleshipGuid,
                "Serve",
                "This means leveraging gifts, talents and abilities that God has given each of us to serve, engage in our church and local community and have fun impacting others for God's kingdom.",
                "fa fa-concierge-bell",
                "#709ac7",
                true,
                true,
                true,
                3 );
            AddStepTypePrerequisite( Constants.StepPrerequisiteServeRequiresStartingPointGuid,
                Constants.StepTypeServeGuid,
                Constants.StepTypeStartingPointGuid );

            AddStepType( Constants.StepTypeSmallGroupGuid,
                Constants.StepProgramDiscipleshipGuid,
                "Small Group",
                "A Small Group is an intentional gathering, meeting regularly for the purpose of joining God's mission.",
                "fa fa-users",
                "#709ac7",
                true,
                true,
                false,
                4 );
        }

        /// <summary>
        /// Add a Category to the database.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="entityTypeName"></param>
        /// <param name="order"></param>
        private void AddCategory( string guid, string name, string entityTypeName, int order = 0 )
        {
            var sql = @"
            DECLARE @entityTypeId INT

            SET @entityTypeId = ( SELECT [Id] FROM [EntityType] WHERE [Name] = @entityTypeName )

            IF NOT EXISTS ( SELECT * FROM Category WHERE Guid = @guid )
            BEGIN
                INSERT INTO [Category] (
	                [IsSystem]
	                ,[EntityTypeId]
	                ,[Name]
	                ,[Guid]
	                ,[Order]
                    ,CreatedDateTime
	                )
                VALUES (
	                0
	                ,@entityTypeId
	                ,@name
	                ,@guid
	                ,@order
                    ,@createdDateTime
	                )
            END";

            sql = ReplaceSqlTextParameter( sql, "@guid", guid );
            sql = ReplaceSqlTextParameter( sql, "@name", name );
            sql = ReplaceSqlTextParameter( sql, "@entityTypeName", entityTypeName );
            sql = ReplaceSqlParameter( sql, "@order", order );

            ExecuteSql( sql );
        }

        /// <summary>
        /// Add a Step Program to the database.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="iconCssClass"></param>
        /// <param name="defaultListView"></param>
        /// <param name="categoryGuid"></param>
        /// <param name="order"></param>
        private void AddStepProgram( string guid, string name, string description, string iconCssClass, int defaultListView, string categoryGuid, int order = 0 )
        {
            var sql = @"
                IF NOT EXISTS ( SELECT * FROM StepProgram WHERE Guid = @guid )
                BEGIN
                    INSERT INTO StepProgram (
                        Guid
                        ,Name
                        ,Description
                        ,IconCssClass
                        ,IsActive
                        ,[Order]
                        ,CategoryId
                        ,DefaultListView
                        ,CreatedDateTime
                    ) VALUES (
                        @guid
                        ,@name
                        ,@description
                        ,@iconCssClass
                        ,1
                        ,@order
                        ,( SELECT Id FROM Category WHERE Guid = @categoryGuid )
                        ,@defaultListView
                        ,@createdDateTime
                    );
                END";

            sql = ReplaceSqlTextParameter( sql, "@guid", guid );
            sql = ReplaceSqlTextParameter( sql, "@name", name );
            sql = ReplaceSqlTextParameter( sql, "@description", description );
            sql = ReplaceSqlTextParameter( sql, "@iconCssClass", iconCssClass );
            sql = ReplaceSqlTextParameter( sql, "@categoryGuid", categoryGuid );
            sql = ReplaceSqlParameter( sql, "@order", order );
            sql = ReplaceSqlParameter( sql, "@defaultListView", defaultListView );

            ExecuteSql( sql );
        }

        /// <summary>
        /// Add a Step Status to the database.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="stepProgramGuid"></param>
        /// <param name="color"></param>
        /// <param name="isCompletedStatus"></param>
        /// <param name="order"></param>
        private void AddStepStatus( string guid, string name, string stepProgramGuid, string color, bool isCompletedStatus, int order = 0 )
        {
            var sql = @"
                IF NOT EXISTS ( SELECT * FROM StepStatus WHERE Guid = @guid )
                BEGIN
                    INSERT INTO StepStatus (
                        StepProgramId
                        ,Name
                        ,IsCompleteStatus
                        ,StatusColor
                        ,IsActive
                        ,[Order]
                        ,Guid
                        ,CreatedDateTime
                    ) VALUES (
                        ( SELECT Id FROM StepProgram WHERE Guid = @stepProgramGuid )
                        ,@name
                        ,@isCompletedStatus
                        ,@color
                        ,1
                        ,@order
                        ,@guid
                        ,@createdDateTime
                    );
                END";

            sql = ReplaceSqlTextParameter( sql, "@guid", guid );
            sql = ReplaceSqlTextParameter( sql, "@name", name );
            sql = ReplaceSqlTextParameter( sql, "@stepProgramGuid", stepProgramGuid );
            sql = ReplaceSqlTextParameter( sql, "@color", color );
            sql = ReplaceSqlBooleanParameter( sql, "@isCompletedStatus", isCompletedStatus );
            sql = ReplaceSqlParameter( sql, "@order", order );

            ExecuteSql( sql );
        }

        /// <summary>
        /// Add a Step Type to the database.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="stepProgramGuid"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="iconCssClass"></param>
        /// <param name="color"></param>
        /// <param name="allowMultiple"></param>
        /// <param name="spansTime"></param>
        /// <param name="showCountOnBadge"></param>
        /// <param name="order"></param>
        private void AddStepType( string guid, string stepProgramGuid, string name, string description, string iconCssClass, string color, bool allowMultiple, bool spansTime, bool showCountOnBadge, int order = 0 )
        {
            var sql = @"
                IF NOT EXISTS ( SELECT * FROM StepType WHERE Guid = @guid )
                BEGIN
                    INSERT INTO StepType (
                        StepProgramId
                        ,Name
                        ,Description
                        ,IconCssClass
                        ,HighlightColor
                        ,AllowMultiple
                        ,HasEndDate
                        ,IsActive
                        ,[Order]
                        ,Guid
                        ,AllowManualEditing
                        ,ShowCountOnBadge
                        ,CreatedDateTime
                    ) VALUES (
                        ( SELECT Id FROM StepProgram WHERE Guid = @stepProgramGuid )
                        ,@name
                        ,@description
                        ,@iconCssClass
                        ,@color
                        ,@allowMultiple
                        ,@spansTime
                        ,1
                        ,@order
                        ,@guid
                        ,1
                        ,@showCountOnBadge
                        ,@createdDateTime
                    );
                END";

            sql = ReplaceSqlTextParameter( sql, "@guid", guid );
            sql = ReplaceSqlTextParameter( sql, "@name", name );
            sql = ReplaceSqlTextParameter( sql, "@description", description );
            sql = ReplaceSqlTextParameter( sql, "@stepProgramGuid", stepProgramGuid );
            sql = ReplaceSqlTextParameter( sql, "@iconCssClass", iconCssClass );
            sql = ReplaceSqlTextParameter( sql, "@color", color );
            sql = ReplaceSqlBooleanParameter( sql, "@allowMultiple", allowMultiple );
            sql = ReplaceSqlBooleanParameter( sql, "@spansTime", spansTime );
            sql = ReplaceSqlBooleanParameter( sql, "@showCountOnBadge", showCountOnBadge );
            sql = ReplaceSqlParameter( sql, "@order", order );

            ExecuteSql( sql );
        }

        /// <summary>
        /// Add a Step Type Prerequisite to the database.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="stepTypeGuid"></param>
        /// <param name="prerequisiteStepTypeGuid"></param>
        /// <param name="order"></param>
        private void AddStepTypePrerequisite( string guid, string stepTypeGuid, string prerequisiteStepTypeGuid, int order = 0 )
        {
            var sql = @"
                IF NOT EXISTS ( SELECT * FROM StepTypePrerequisite WHERE Guid = @guid )
                BEGIN
                    INSERT INTO StepTypePrerequisite (
                        StepTypeId
                        ,PrerequisiteStepTypeId
                        ,Guid
                        ,[Order]
                        ,CreatedDateTime
                    ) VALUES (
                        ( SELECT Id FROM StepType WHERE Guid = @stepTypeGuid )
                        ,( SELECT Id FROM StepType WHERE Guid = @prerequisiteStepTypeGuid )
                        ,@guid
                        ,@order
                        ,@createdDateTime
                    )
                END";

            sql = ReplaceSqlTextParameter( sql, "@guid", guid );
            sql = ReplaceSqlTextParameter( sql, "@stepTypeGuid", stepTypeGuid );
            sql = ReplaceSqlTextParameter( sql, "@prerequisiteStepTypeGuid", prerequisiteStepTypeGuid );
            sql = ReplaceSqlParameter( sql, "@order", order );

            ExecuteSql( sql );
        }

        /// <summary>
        /// Pre-process and execute an arbitrary SQL statement.
        /// </summary>
        /// <param name="sql"></param>
        private void ExecuteSql( string sql )
        {
            sql = ReplaceSqlTextParameter( sql, "@createdDateTime", _CreatedDateTime.ToString( "yyyy-MM-dd'T'HH:mm:ss" ) );

            try
            {
                Sql( sql );
            }
            catch ( Exception ex )
            {
                // For debugging purposes.
                Debug.Print( ex.ToString() );
            }
        }

        /// <summary>
        /// Replace and format a text parameter in a SQL statement.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ReplaceSqlTextParameter( string sql, string parameterName, string value )
        {
            // Escape any single-quotes in the parameter value.
            value = value.Replace( "'", "''" );

            return sql.Replace( parameterName, "'" + value + "'" );
        }

        /// <summary>
        /// Replace and format a boolean parameter in a SQL statement.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ReplaceSqlBooleanParameter( string sql, string parameterName, bool value )
        {
            return sql.Replace( parameterName, value.Bit().ToString() );
        }

        /// <summary>
        /// Replace and format a parameter in a SQL statement.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ReplaceSqlParameter( string sql, string parameterName, object value )
        {
            // Escape any single-quotes in the parameter value.
            value = value ?? string.Empty;

            return sql.Replace( parameterName, value.ToString() );
        }
    }
}
