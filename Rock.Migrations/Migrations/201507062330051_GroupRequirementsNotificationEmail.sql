DECLARE @CategoryId int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'B31064D2-F2EF-43AA-8BEA-14DF257CBC59')
        IF @CategoryId IS NOT NULL
        BEGIN     
            INSERT INTO [SystemEmail]
	        ([IsSystem], [Title], [Subject], [Body], [Guid], [CategoryId])
            VALUES
	            (0, 'Group Requirements Notification', 'Group Requirements Report | {{ ''Global'' | Attribute:''OrganizationName'' }}', '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>
    {{ Person.NickName }}:
</p>

<p>
    Below are groups that have members with requirements that are either not met or are in a warning state.
</p>


{% for group in GroupsMissingRequirements %}
    {% assign leaderCount = group.Leaders | Size %}
    
    <table style="border: 1px solid #c4c4c4; border-collapse:collapse; mso-table-lspace:0pt; mso-table-rspace:0pt; width: 100%; margin-bottom: 24px;" cellspacing="0" cellpadding="4">
        <tr style="border: 1px solid #c4c4c4;">
            <td colspan="2" bgcolor="#a6a5a5" align="left" style="color: #ffffff; padding: 4px 8px; border: 1px solid #c4c4c4;">
                <h4 style="color: #ffffff; line-height: 1.2em;"><a style="color: #ffffff;" href="{{ ''Global'' | Attribute:''InternalApplicationRoot'' }}Group/{{ group.Id }}">{{ group.Name }}</a> <small>({{ group.GroupTypeName }})</small></h4>
                <small>{{ group.AncestorPathName }}</small> <br />
                <small>{{ ''Leader'' | PluralizeForQuantity:leaderCount }}:</strong> {{ group.Leaders | Map:''FullName'' | Join:'', '' | ReplaceLast:'','','' and'' }}</small> <br />
            </td>
        </tr>
        
        {% for groupMember in group.GroupMembersMissingRequirements %}
            <tr style="border: 1px solid #c4c4c4;">
                <td style="border: 1px solid #c4c4c4; padding: 6px; width: 50%;">
                    {{ groupMember.FullName }} <small>( {{ groupMember.GroupMemberRole }} )</small><br />
                </td>
                <td style="border: 1px solid #c4c4c4; width: 50%;">
                    <table style="width: 100%; margin: 0; border-collapse:collapse; mso-table-lspace:0pt; mso-table-rspace:0pt;" cellspacing="0" cellpadding="4">{% for missingRequirement in groupMember.MissingRequirements -%}
                        {% case missingRequirement.Status -%}
                            {% when ''NotMet'' -%}
                                {% assign messageColor = "#d9534f" -%}
                            {% when ''MeetsWithWarning'' -%}
                                {% assign messageColor = "#f0ad4e" -%}
                        {% endcase -%}<tr><td style="border-bottom: 1px solid #ffffff; background-color: {{ messageColor }}; padding: 6px; color: #ffffff;"><small>{{ missingRequirement.Message }} as of {{ missingRequirement.OccurrenceDate | Date:''M/d/yyyy'' }}</small></td></tr>
                    {% endfor -%}</table>
                </td>
            </tr>
        {% endfor %}
    </table>
    &nbsp;

{% endfor %}

<p>&nbsp;</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}', '91EA23C3-2E16-2597-4EAF-27C40D3A66D8', @CategoryId )

                
END

