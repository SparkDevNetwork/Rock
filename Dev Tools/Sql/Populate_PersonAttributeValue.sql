set nocount on

IF CURSOR_STATUS('global','attributeCursor')>=-1
BEGIN
    DEALLOCATE attributeCursor;
END

declare attributeCursor cursor LOCAL FAST_FORWARD for select Id, [Name], FieldTypeId from Attribute where EntityTypeId = 15;
open attributeCursor;


IF CURSOR_STATUS('global','personCursor')>=-1
BEGIN
    DEALLOCATE personCursor;
END

declare personCursor cursor LOCAL FAST_FORWARD for select top 1 Id from Person order by newid()
open personCursor;


declare @insertCounter int = 0;
declare @attributeCount int = (select count(*) from Attribute where EntityTypeId = 15)
declare @attributeId int;
declare @attributeName nvarchar(1000);
declare @fieldTypeId int;
declare 
    @attributeValueAsText nvarchar(max),
    @attributeValueAsDateTime datetime,
    @attributeValueAsNumeric int,
    @attributeValueAsGuid uniqueidentifier


declare @personId int,
  @personFetchStatus int = 0;

select count(*) [Before] from AttributeValue

while (1=1)
begin
    fetch next from personCursor into @personId;
    if (@@FETCH_STATUS != 0) begin
       close personCursor
       break;
    end

    while (1=1)
    begin
      fetch next from attributeCursor into @attributeId, @attributeName, @fieldTypeId;
      if (@@FETCH_STATUS != 0) begin
        close attributeCursor;
        open attributeCursor;
        fetch next from attributeCursor into @attributeId, @attributeName, @fieldTypeId;
        break;
      end

      set @attributeValueAsDateTime = null;
      set @attributeValueAsNumeric = null;
  
      if (@fieldTypeId = 1) begin
         set @attributeValueAsText = concat('Random Text ', @attributeName, NEWID());
      end else if (@fieldTypeId = 3)
      begin
         set @attributeValueAsText = 'true'
      end else if (@fieldTypeId = 7 or @fieldTypeId = 14)
      begin
        set @attributeValueAsNumeric = round(rand()*1000, 0);
        set @attributeValueAsText = convert(nvarchar(max), @attributeValueAsNumeric);
      end else if (@fieldTypeId = 11) begin
        set @attributeValueAsDateTime = DATEADD(day, rand()*1000, getdate())
        set @attributeValueAsText = convert(nvarchar(max), @attributeValueAsDateTime);
      end else if (@fieldTypeId = 16) begin
        set @attributeValueAsGuid = (select top 1 dv.Guid from DefinedValue dv order by newid())
        set @attributeValueAsText = convert(nvarchar(max), @attributeValueAsGuid);
      end else begin
        set @attributeValueAsText = concat('Something else ', @attributeName, NEWID());
      end

      delete from AttributeValue where AttributeId = @attributeId and EntityId = @personId;

      set @insertCounter = @insertCounter+1
      if (@insertCounter%1000 = 0) begin
         print @insertCounter
      end

      INSERT INTO [dbo].[AttributeValue]
           ([IsSystem]
           ,[AttributeId]
           ,[EntityId]
           ,[Value]
           ,[ValueAsDateTime]
           ,[ValueAsNumeric]
           ,[Guid])
           values (0, @attributeId, @personId, @attributeValueAsText, @attributeValueAsDateTime, @attributeValueAsNumeric, newid() )
    end
end

select count(*) [After] from AttributeValue