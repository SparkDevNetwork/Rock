{% group where:'Guid == "{{PageParameter.Group}}"'%}
    {% assign groupId = group.Id %}
{% endgroup %}

select count(*), p.email
from Person p

    {% if groupId %}
    join [GroupMember] GM on GM.PersonId = p.id
    join [Group] g on g.id = gm.groupid and g.id = {{ groupId }}
    {% endif%}
    
where p.RecordStatusValueId = 3
and p.email is not null
and p.email != ''
group by p.Email
having count(*) > 1
order by count(*) desc
