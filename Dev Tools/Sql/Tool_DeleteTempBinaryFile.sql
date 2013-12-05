begin
    declare @fileId int
    declare crBinaryFile cursor for
    select [Id] from [BinaryFile] where [IsTemporary] = 1;

    open crBinaryFile
    fetch next from crBinaryFile into @fileId

    while @@FETCH_STATUS = 0
    begin
    begin try
        delete from BinaryFile where [Id] = @fileId
    end try
    begin catch
        print 'unable to delete ' 
    end catch
    fetch next from crBinaryFile into @fileId
    end

    close crBinaryFile
    deallocate crBinaryFile
end

select * from BinaryFile