-- generates an ASCII treeview of the Pages
-- either copy and paste the grid results to a text editor, or do Right-Click | Results To | Text
-- NOTE: This does not sort pages by order, but the parent-child relation is displayed correctly

with CTE as (
select *, RIGHT('000000' + cast([Id] as nvarchar(max)), 6) [Sequence], 1 [Level] from [Page] where [ParentPageId] is null
union all
select [a].*, pcte.Sequence + '/' + RIGHT('000000' + cast(a.[Id] as nvarchar(max)), 6) [Sequence], pcte.Level + 1 [Level] from [Page] [a]
inner join CTE pcte on pcte.Id = [a].[ParentPageId]
)

select 
  REPLICATE('| ', [Level]-1) + [Name], [Order], [Level], [Guid], [Sequence]
from CTE
order by [Sequence], [Order], [Name]
