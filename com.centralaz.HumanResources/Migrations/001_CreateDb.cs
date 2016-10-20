using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
namespace com.centralaz.HumanResources.Migrations
{
    [MigrationNumber( 1, "1.5.1" )]
    public class CreateDb : Migration
    {
        public override void Up()
        {
            Sql( @"
                CREATE TABLE [dbo].[_com_centralaz_HumanResources_Salary](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [PersonAliasId] [int] NOT NULL,
	                [Amount] [float] NOT NULL,
	                [IsSalariedEmployee] [bit] NOT NULL,
	                [HousingAllowance] [float] NULL,
	                [FuelAllowance] [float] NULL,
	                [PhoneAllowance] [float] NULL,
	                [EffectiveDate] [datetime] NOT NULL,
	                [ReviewedDate] [datetime] NULL,
                    [IsActive] [bit] NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_centralaz_HumanResources_Salary] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_Salary]  WITH CHECK ADD CONSTRAINT [FK__com_centralaz_HumanResources_Salary_PersonAliasId] FOREIGN KEY([PersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_Salary] CHECK CONSTRAINT [FK__com_centralaz_HumanResources_Salary_PersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_Salary]  WITH CHECK ADD CONSTRAINT [FK__com_centralaz_HumanResources_Salary_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_Salary] CHECK CONSTRAINT [FK__com_centralaz_HumanResources_Salary_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_Salary]  WITH CHECK ADD CONSTRAINT [FK__com_centralaz_HumanResources_Salary_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_Salary] CHECK CONSTRAINT [FK__com_centralaz_HumanResources_Salary_ModifiedByPersonAliasId]

" );
            RockMigrationHelper.UpdateEntityType( "com.centralaz.HumanResources.Model.Salary", "9850875E-DBB7-4CF0-A7C7-80FE25F58109", true, true );

            Sql( @"
                CREATE TABLE [dbo].[_com_centralaz_HumanResources_RetirementFund](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [PersonAliasId] [int] NOT NULL,
                    [FundValueId] [int] NOT NULL,
                    [IsFixedAmount] [bit] NOT NULL,
                    [EmployeeAmount] [float] NOT NULL,
                    [EmployerAmount] [float] NULL,
                    [ActiveDate] [datetime] NOT NULL,
                    [InactiveDate] [datetime] NULL,
                    [IsActive] [bit] NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_centralaz_HumanResources_RetirementFund] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_RetirementFund]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_HumanResources_RetirementFund_FundValueId] FOREIGN KEY([FundValueId])
                REFERENCES [dbo].[DefinedValue] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_RetirementFund] CHECK CONSTRAINT [FK__com_centralaz_HumanResources_RetirementFund_FundValueId]

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_RetirementFund]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_HumanResources_RetirementFund_PersonAliasId] FOREIGN KEY([PersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_RetirementFund] CHECK CONSTRAINT [FK__com_centralaz_HumanResources_RetirementFund_PersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_RetirementFund]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_HumanResources_RetirementFund_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_RetirementFund] CHECK CONSTRAINT [FK__com_centralaz_HumanResources_RetirementFund_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_RetirementFund]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_HumanResources_RetirementFund_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_RetirementFund] CHECK CONSTRAINT [FK__com_centralaz_HumanResources_RetirementFund_ModifiedByPersonAliasId]

" );
            RockMigrationHelper.UpdateEntityType( "com.centralaz.HumanResources.Model.RetirementFund", "1DB7B91B-186B-445B-B7AD-0755227ED80C", true, true );

            Sql( @"
                CREATE TABLE [dbo].[_com_centralaz_HumanResources_ContributionElection](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [PersonAliasId] [int] NOT NULL,
                    [FinancialAccountId] [int] NOT NULL,
                    [IsFixedAmount] [bit] NOT NULL,
                    [Amount] [float] NOT NULL,
                    [ActiveDate] [datetime] NOT NULL,
                    [InactiveDate] [datetime] NULL,
                    [IsActive] [bit] NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_centralaz_HumanResources_ContributionElection] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_ContributionElection]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_HumanResources_ContributionElection_FinancialAccountId] FOREIGN KEY([FinancialAccountId])
                REFERENCES [dbo].[FinancialAccount] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_ContributionElection] CHECK CONSTRAINT [FK__com_centralaz_HumanResources_ContributionElection_FinancialAccountId]

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_ContributionElection]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_HumanResources_ContributionElection_PersonAliasId] FOREIGN KEY([PersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_ContributionElection] CHECK CONSTRAINT [FK__com_centralaz_HumanResources_ContributionElection_PersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_ContributionElection]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_HumanResources_ContributionElection_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_ContributionElection] CHECK CONSTRAINT [FK__com_centralaz_HumanResources_ContributionElection_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_ContributionElection]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_HumanResources_ContributionElection_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_ContributionElection] CHECK CONSTRAINT [FK__com_centralaz_HumanResources_ContributionElection_ModifiedByPersonAliasId]

" );
            RockMigrationHelper.UpdateEntityType( "com.centralaz.HumanResources.Model.ContributionElection", "4576029F-7E86-457C-A1F0-611E58E3332C", true, true );

        }
        public override void Down()
        {
            RockMigrationHelper.DeleteEntityType( "4576029F-7E86-457C-A1F0-611E58E3332C" );
            RockMigrationHelper.DeleteEntityType( "1DB7B91B-186B-445B-B7AD-0755227ED80C" );
            RockMigrationHelper.DeleteEntityType( "9850875E-DBB7-4CF0-A7C7-80FE25F58109" );

            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_HumanResources_ContributionElection] DROP CONSTRAINT [FK__com_centralaz_HumanResources_ContributionElection_PersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_HumanResources_ContributionElection] DROP CONSTRAINT [FK__com_centralaz_HumanResources_ContributionElection_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_HumanResources_ContributionElection] DROP CONSTRAINT [FK__com_centralaz_HumanResources_ContributionElection_CreatedByPersonAliasId]
                DROP TABLE [dbo].[_com_centralaz_HumanResources_ContributionElection]

                ALTER TABLE [dbo].[_com_centralaz_HumanResources_RetirementFund] DROP CONSTRAINT [FK__com_centralaz_HumanResources_RetirementFund_PersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_HumanResources_RetirementFund] DROP CONSTRAINT [FK__com_centralaz_HumanResources_RetirementFund_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_HumanResources_RetirementFund] DROP CONSTRAINT [FK__com_centralaz_HumanResources_RetirementFund_CreatedByPersonAliasId]
                DROP TABLE [dbo].[_com_centralaz_HumanResources_RetirementFund]
                
                ALTER TABLE [dbo].[_com_centralaz_HumanResources_Salary] DROP CONSTRAINT [FK__com_centralaz_HumanResources_Salary_PersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_HumanResources_Salary] DROP CONSTRAINT [FK__com_centralaz_HumanResources_Salary_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_HumanResources_Salary] DROP CONSTRAINT [FK__com_centralaz_HumanResources_Salary_CreatedByPersonAliasId]
                DROP TABLE [dbo].[_com_centralaz_HumanResources_Salary]
" );
        }
    }
}
