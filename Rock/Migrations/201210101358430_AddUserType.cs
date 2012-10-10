namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserType : RockMigration
    {
        public override void Up()
        {
            AddColumn("dbo.cmsUser", "ServiceType", c => c.Int(nullable: false));
            AddColumn("dbo.cmsUser", "ServiceName", c => c.String(nullable: false, maxLength: 200));

            Sql( @"
                UPDATE [cmsUser] SET 
                    [ServiceType] = CASE [AuthenticationType] 
                        WHEN 2 THEN 1
                        ELSE 0 END,
                    [ServiceName] = CASE [AuthenticationType]
                        WHEN 1 THEN 'Rock.Security.Authentication.Database'
                        WHEN 2 THEN 'Rock.Security.ExternalAuthentication.Facebook'
                        WHEN 3 THEN 'Rock.Security.Authentication.ActiveDirectory'
                        END
" );

            DropColumn("dbo.cmsUser", "AuthenticationType");

            AlterColumn( "dbo.cmsUser", "Password", c => c.String( maxLength: 128 ) );

            AddPage("0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Authentication Services", "List of services used to authenticate user", "CE2170A9-2C8E-40B1-A42E-DFA73762D01D" );
            AddBlock( "CE2170A9-2C8E-40B1-A42E-DFA73762D01D", "21F5F466-59BC-40B2-8D73-7314D936C3CB", "Internal", "Content", "10D2886B-40F6-47EE-B137-23595FAC224D", 0 );
            AddBlock( "CE2170A9-2C8E-40B1-A42E-DFA73762D01D", "21F5F466-59BC-40B2-8D73-7314D936C3CB", "External", "Content", "FA273FE7-C278-4A41-967B-C7ED85C48B3B", 1 );

            Sql( @"
                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [coreAttribute] WHERE [Guid] = '259AF14D-0214-4BE4-A7BF-40423EA07C99')

                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '10D2886B-40F6-47EE-B137-23595FAC224D')
                INSERT INTO [coreAttributeValue] ( [IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid]) VALUES ( 1,@AttributeId,@BlockId,0,'Rock.Security.AuthenticationContainer, Rock','16C4F4C8-9B6F-4300-B1BF-E39513D0D35E')

                SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = 'FA273FE7-C278-4A41-967B-C7ED85C48B3B')
                INSERT INTO [coreAttributeValue] ( [IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid]) VALUES ( 1,@AttributeId,@BlockId,0,'Rock.Security.ExternalAuthenticationContainer, Rock','A578FB11-4F4F-467D-BDD8-A15D39F4AFB0')

                INSERT INTO [coreAttribute] ( [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid]) VALUES ( 1, 3,'Rock.Security.Authentication.ActiveDirectory','','','Active','Active','','Should Service be used?',0,0,'False',0,0,'0B1A8FA0-BD26-45CD-9A2B-D2879FB15C45')
                INSERT INTO [coreAttribute] ( [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid]) VALUES ( 1, 1,'Rock.Security.Authentication.ActiveDirectory','','','Order','Order','','The order that this service should be used (priority)',0,0,'0',0,0,'733A5D67-A809-46FB-B932-A95904FCECB7')
                INSERT INTO [coreAttribute] ( [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid]) VALUES ( 1, 1,'Rock.Security.Authentication.ActiveDirectory','','','Server','Server','Server','The Active Directory server name',1,0,'',0,1,'A09421A8-B37C-42AA-86FE-700195C9E410')
                INSERT INTO [coreAttribute] ( [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid]) VALUES ( 1, 1,'Rock.Security.Authentication.ActiveDirectory','','','Domain','Domain','Server','The network domain that users belongs to',2,0,'',0,1,'8AF7DE76-A294-4946-8921-09CDBD180021')
                INSERT INTO [coreAttribute] ( [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid]) VALUES ( 1, 3,'Rock.Security.Authentication.Database','','','Active','Active','','Should Service be used?',0,0,'False',0,0,'0FB80CB1-D065-462B-A5A0-7A6AF712025D')
                INSERT INTO [coreAttribute] ( [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid]) VALUES ( 1, 1,'Rock.Security.Authentication.Database','','','Order','Order','','The order that this service should be used (priority)',0,0,'0',0,0,'D3C9C482-DE0B-491F-A18A-6C64E78EA6DE')
                INSERT INTO [coreAttribute] ( [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid]) VALUES ( 1, 3,'Rock.Security.ExternalAuthentication.Facebook','','','Active','Active','','Should Service be used?',0,0,'False',0,0,'BAE112EE-40D4-4F86-AED8-81C3942FF87D')
                INSERT INTO [coreAttribute] ( [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid]) VALUES ( 1, 1,'Rock.Security.ExternalAuthentication.Facebook','','','Order','Order','','The order that this service should be used (priority)',0,0,'0',0,0,'AD8F8ED6-698B-47E7-950A-7CADCED70226')
                INSERT INTO [coreAttribute] ( [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid]) VALUES ( 1, 1,'Rock.Security.ExternalAuthentication.Facebook','','','AppID','App ID','Facebook','The Facebook App ID',1,0,'',0,1,'73D53921-4AF9-4EBF-B84B-107D2A40D073')
                INSERT INTO [coreAttribute] ( [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid]) VALUES ( 1, 1,'Rock.Security.ExternalAuthentication.Facebook','','','AppSecret','App Secret','Faceboook','The Facebook App Secret',2,0,'',0,1,'12211DBC-A51D-4FD8-B89A-A45189A94C6F')

                SET @AttributeId = (SELECT [Id] FROM [coreAttribute] WHERE [Guid] = '733A5D67-A809-46FB-B932-A95904FCECB7')
                INSERT INTO [coreAttributeValue] ( [IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid]) VALUES ( 1,@AttributeId,0,0,'1','04D349B5-FCAD-45F3-8275-859A46B825DA')

                SET @AttributeId = (SELECT [Id] FROM [coreAttribute] WHERE [Guid] = '0FB80CB1-D065-462B-A5A0-7A6AF712025D')
                INSERT INTO [coreAttributeValue] ( [IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid]) VALUES ( 1,@AttributeId,0,0,'True','3A1FFA35-4CDD-4EF3-9C88-7C7D054F98FB')

                -- Delete all previous authorizations
                DELETE [cmsAuth]

                -- Globally grant every user view access, and admins access to edit/config
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Global',0,0,'View','A',1,NULL,NULL,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Global',0,0,'Edit','A',0,NULL,2,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Global',0,1,'Edit','D',1,NULL,NULL,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Global',0,0,'Configure','A',0,NULL,2,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Global',0,1,'Configure','D',1,NULL,NULL,NEWID())

                -- Give logged in users access to view site, and admins access to edit/config
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Site',1,0,'View','A',2,NULL,NULL,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Site',1,1,'View','D',1,NULL,NULL,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Site',1,0,'Edit','A',0,NULL,2,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Site',1,1,'Edit','D',1,NULL,NULL,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Site',1,0,'Configure','A',0,NULL,2,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Site',1,1,'Configure','D',1,NULL,NULL,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Site',1,0,'Approve','A',0,NULL,2,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Site',1,1,'Approve','D',1,NULL,NULL,NEWID())

                -- Give everyone access to view security pages
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Page',3,0,'View','A',1,NULL,NULL,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Page',4,0,'View','A',1,NULL,NULL,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Page',54,0,'View','A',1,NULL,NULL,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Page',56,0,'View','A',1,NULL,NULL,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Page',55,0,'View','A',2,NULL,NULL,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Page',57,0,'View','A',2,NULL,NULL,NEWID())

                -- Give only admins access to Financials page
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Page',43,0,'View','A',0,NULL,2,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Page',43,1,'View','D',1,NULL,NULL,NEWID())

                -- Give only admins access to Administration page
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Page',44,0,'View','A',0,NULL,2,NEWID())
                INSERT INTO [cmsAuth] ( [EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])    VALUES ('Cms.Page',44,1,'View','D',1,NULL,NULL,NEWID())
" );

        }
        
        public override void Down()
        {
            Sql( @"
                DELETE [coreAttribute] WHERE [Guid] = '0B1A8FA0-BD26-45CD-9A2B-D2879FB15C45'
                DELETE [coreAttribute] WHERE [Guid] = '733A5D67-A809-46FB-B932-A95904FCECB7'
                DELETE [coreAttribute] WHERE [Guid] = 'A09421A8-B37C-42AA-86FE-700195C9E410'
                DELETE [coreAttribute] WHERE [Guid] = '8AF7DE76-A294-4946-8921-09CDBD180021'
                DELETE [coreAttribute] WHERE [Guid] = '0FB80CB1-D065-462B-A5A0-7A6AF712025D'
                DELETE [coreAttribute] WHERE [Guid] = 'D3C9C482-DE0B-491F-A18A-6C64E78EA6DE'
                DELETE [coreAttribute] WHERE [Guid] = 'BAE112EE-40D4-4F86-AED8-81C3942FF87D'
                DELETE [coreAttribute] WHERE [Guid] = 'AD8F8ED6-698B-47E7-950A-7CADCED70226'
                DELETE [coreAttribute] WHERE [Guid] = '73D53921-4AF9-4EBF-B84B-107D2A40D073'
                DELETE [coreAttribute] WHERE [Guid] = '12211DBC-A51D-4FD8-B89A-A45189A94C6F'

                DELETE [coreAttributeValue] WHERE [Guid] = '16C4F4C8-9B6F-4300-B1BF-E39513D0D35E'
                DELETE [coreAttributeValue] WHERE [Guid] = 'A578FB11-4F4F-467D-BDD8-A15D39F4AFB0'
" );

            DeleteBlock( "FA273FE7-C278-4A41-967B-C7ED85C48B3B" );
            DeleteBlock( "10D2886B-40F6-47EE-B137-23595FAC224D" );
            DeletePage( "CE2170A9-2C8E-40B1-A42E-DFA73762D01D" );

            Sql( "UPDATE [cmsUser] SET [Password] = '' WHERE [Password] IS NULL" );
            AlterColumn( "dbo.cmsUser", "Password", c => c.String( nullable: false, maxLength: 128 ) );

            AddColumn( "dbo.cmsUser", "AuthenticationType", c => c.Int( nullable: false ) );

            Sql( @"
                UPDATE [cmsUser] SET 
                    [AuthenticationType] = CASE [ServiceName]
                        WHEN 'Rock.Security.ExternalAuthentication.Facebook' THEN 2
                        WHEN 'Rock.Security.Authentication.ActiveDirectory' THEN 3
                        ELSE 1 END
" );
            DropColumn( "dbo.cmsUser", "ServiceName" );
            DropColumn( "dbo.cmsUser", "ServiceType" );

            
        }
    }
}
