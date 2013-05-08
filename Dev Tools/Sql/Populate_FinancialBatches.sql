USE [RockChMS_dev01]
GO

INSERT INTO [dbo].[FinancialBatch] ([Name] ,[BatchDate] ,[CampusId] ,[Guid] ,[Status] ,[AccountingSystemCode] ,[ControlAmount], [CreatedByPersonId])
 VALUES
 ('Batch 1' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 100.50, (select top 1 id from person order by NEWID())),
 ('Batch 2' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 200.50, (select top 1 id from person order by NEWID())),
 ('Batch 3' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 300.50, (select top 1 id from person order by NEWID())),
 ('Batch 4' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 400.50, (select top 1 id from person order by NEWID())),
 ('Batch 5' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 500.50, (select top 1 id from person order by NEWID())),
 ('Batch 6' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 600.50, (select top 1 id from person order by NEWID())),
 ('Batch a' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 444400.50, (select top 1 id from person order by NEWID())),
 ('Batch b' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 1.50, (select top 1 id from person order by NEWID())),
 ('Batch c' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 987100.50, (select top 1 id from person order by NEWID())),
 ('Batch d' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 200.50, (select top 1 id from person order by NEWID())),
 ('Batch e' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 200.50, (select top 1 id from person order by NEWID())),
 ('Batch f' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 200.50, (select top 1 id from person order by NEWID())),
 ('Batch j' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 300.50, (select top 1 id from person order by NEWID())),
 ('Batch k' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 400.50, (select top 1 id from person order by NEWID())),
 ('Batch l' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 500.50, (select top 1 id from person order by NEWID())),
 ('Batch 1234' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 500.50, (select top 1 id from person order by NEWID())),
 ('Batch 4321' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 6100.50, (select top 1 id from person order by NEWID())),
 ('Batch 7' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 10.50, (select top 1 id from person order by NEWID())),
 ('Batch 8' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 1.50, (select top 1 id from person order by NEWID())),
 ('Batch 9' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 0.50, (select top 1 id from person order by NEWID())),
 ('Batch 10' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 0.40, (select top 1 id from person order by NEWID())),
 ('Batch 11' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 100.60, (select top 1 id from person order by NEWID())),
 ('Batch 12' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 100.70, (select top 1 id from person order by NEWID())),
 ('Batch 13' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 100.51, (select top 1 id from person order by NEWID())),
 ('Batch 14' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 100.52, (select top 1 id from person order by NEWID())),
 ('Batch 15' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 100.53, (select top 1 id from person order by NEWID())),
 ('Batch 16' ,SYSDATETIME() , null ,NEWID() , 0 ,'' , 100.54, (select top 1 id from person order by NEWID()))




