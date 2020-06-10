select 
s1.ContentChannelItemId [Item 1 Id]
, '<a href="{{'Global' | Attribute:'InternalApplicationRoot'}}page/342?contentItemId=' + convert(varchar(10), s1.ContentChannelItemId) + '">' + i1.Title + '</a>' [Item 1 Name]
,  s2.ContentChannelItemId [Item 2 Id]
, '<a href="{{'Global' | Attribute:'InternalApplicationRoot'}}page/342?contentItemId=' + convert(varchar(10), s2.ContentChannelItemId) + '">' + i2.Title + '</a>' [Item 2 Name]
, s1.Slug as [Duplicate Slug]
from ContentChannelItemSlug s1
join ContentChannelItemSlug s2 on s2.Slug = s1.Slug and s2.id != s1.id
join ContentChannelItem i1 on i1.id = s1.ContentChannelItemId
join ContentChannelItem i2 on i2.id = s2.ContentChannelItemId