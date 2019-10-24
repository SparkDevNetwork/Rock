-- generates an ASCII treeview of the Pages
-- either copy and paste the grid results to a text editor, or do Right-Click | Results To | Text

with CTE as (
select *, '~' + RIGHT('000' + cast([Order] as nvarchar(max)), 3) + '/' + RIGHT('000000' + cast([Id] as nvarchar(max)), 6) [Sequence], 1 [Level] from [Page] where [ParentPageId] is null
union all
select [a].*, pcte.Sequence + '~' + RIGHT('000' + cast(a.[Order] as nvarchar(max)), 3) + '/' + RIGHT('000000' + cast(a.[Id] as nvarchar(max)), 6) [Sequence], pcte.Level + 1 [Level] from [Page] [a]
inner join CTE pcte on pcte.Id = [a].[ParentPageId]
)

select 
  REPLICATE('| ', [Level]-1) + [InternalName] 
  -- + ' - ' + [Sequence]
  -- + ' - ' + cast([Order] as varchar(max))
  -- + ' - ' + cast([Guid] as varchar(max))
  --, [Sequence]
from CTE
-- where Level < 5
order by [Sequence]
