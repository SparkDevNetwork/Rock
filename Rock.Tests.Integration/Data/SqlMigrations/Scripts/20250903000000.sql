CREATE TABLE [dbo].[BulkRow] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(100) NULL,
    [Code] CHAR(1) NULL,
    [Amount] DECIMAL(18,2) NULL,
    [IsActive] BIT NULL,
    [Created] DATETIME2(7) NULL,
    [Data] VARBINARY(MAX) NULL,
    [Reference] UNIQUEIDENTIFIER NULL,
    [Shape] GEOGRAPHY NULL,
    [Guid] UNIQUEIDENTIFIER NOT NULL,
    [Doubled] AS ( [Id] * 2 ),
    CONSTRAINT [PK_dbo.BulkRow] PRIMARY KEY ( [Id] ),
    CONSTRAINT [CK_BulkRow_Amount] CHECK ( [Amount] >= 0 )
);
