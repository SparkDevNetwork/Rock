UPDATE [SystemCommunication] SET [Body]=N'{% assign peopleReminders = Reminders | Where:''IsPersonReminder'',true %}
{% assign otherReminders = Reminders | Where:''IsPersonReminder'',false %}
{% assign currentDate = ''Now'' | Date:''MMMM d, yyyy'' %}
{{ ''Global'' | Attribute:''EmailHeader'' }}
<style>table[class="body"] table.columns td.reminder-img-today{width:58px !important;}table[class="body"] table.columns td.reminder-img{width:50px !important;}</style>
<h1 style="margin:0;">Your Reminders</h1>

<p>
    Below are {{ MaxRemindersPerEntityType }} of the most recent reminders for each feature as of {{ currentDate }}. <a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}reminders" style="font-weight:700;text-decoration:underline;">View all of your reminders</a>
</p>

{% if peopleReminders != empty %}
    {% assign entityTypeId = ''0'' %}
    <h2>People</h2>
    <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%">
        {% for personReminder in peopleReminders %}
        {% assign entityTypeId = personReminder.EntityTypeId %}
        {% assign reminderDate = personReminder.ReminderDate | Date:''MMMM d, yyyy'' %}
        {% assign reminderDateIsToday = false %}
        {% if currentDate == reminderDate %}
            {% assign reminderDateIsToday = true %}
        {% endif %}
        {% assign entityUrl = personReminder.EntityUrl %}
        {% if entityUrl == '''' %}
            {% assign entityUrl = '''' %}
        {% endif %}
        <tr>
            <td class="{% if reminderDateIsToday %}reminder-img-today{% else %}reminder-img{% endif %}" valign="top" style="vertical-align:top;{% if reminderDateIsToday %}background: #f3f4f6;padding-top:8px;padding-left:8px;width:58px !important;{% else %}width:50px !important;{% endif %}" width="50">
                {% unless personReminder.PersonProfilePictureUrl contains ''.svg'' %}
                    <img src="{{ personReminder.PersonProfilePictureUrl }}&w=50" width="50" height="50" alt="" style="display:block;width:50px !important;border-radius:6px;">
                {% endunless %}
            </td>
            <td style="vertical-align:top;padding-left:12px;{% if reminderDateIsToday %}background: #f3f4f6;padding-top:8px;padding-right:8px;padding-bottom:12px;{% endif %}">
                <a href="{{ entityUrl }}" style="font-weight:700">{{ personReminder.EntityDescription }}</a><br>
                <span style="font-size:12px;"><span style="color:{{ personReminder.HighlightColor }}">&#11044;</span> {{ personReminder.ReminderTypeName }} &middot; {{ reminderDate |  Date:''MMMM d, yyyy'' }}</span>

                <p>{{ personReminder.Note | NewlineToBr }}</p>
            </td>
        </tr>
        <tr><td colspan="2" style="padding-bottom:24px;"></td></tr>
        {% endfor %}
        <tr>
            <td colspan="2" style="padding-bottom:12px;">
                <p style="text-align:left;"><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}reminders/{{ entityTypeId }}" style="font-weight:700;font-size:12px;">View All Reminders</a></p>
            </td>
        </tr>
    </table>
{% endif %}


{% assign lastEntityType = '''' %}
{% assign entityTypeId = ''0'' %}
{% if otherReminders != empty %}
    {% if peopleReminders != empty %}
    <hr />
    {% endif %}

    {% for reminder in otherReminders %}
        {% assign reminderDate = reminder.ReminderDate | Date:''MMMM d, yyyy'' %}
        {% assign reminderDateIsToday = false %}
        {% if currentDate == reminderDate %}
            {% assign reminderDateIsToday = true %}
        {% endif %}
        {% assign entityUrl = reminder.EntityUrl %}
        {% if entityUrl == '''' %}
            {% assign entityUrl = ''#'' %}
        {% endif %}

        {% if lastEntityType != reminder.EntityTypeName %}
            {% if lastEntityType != '''' %}
                    <tr>
                        <td colspan="2" style="padding-bottom:12px;">
                            <p style="text-align:left;"><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}reminders/{{ entityTypeId }}" style="font-weight:700;font-size:12px;">View All Reminders</a></p>
                        </td>
                    </tr>
                </table>
                <hr/>
            {% endif %}
            <h2>{{ reminder.EntityTypeName | Pluralize }}</h2>
            <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%">
            {% assign lastEntityType = reminder.EntityTypeName %}
            {% assign entityTypeId = reminder.EntityTypeId %}
        {% endif %}

        <tr>
            <td style="{% if reminderDateIsToday %}background: #f3f4f6;padding-top:12px;padding-right:8px;padding-left:8px;{% endif %}">
                <a href="{{ entityUrl }}" style="font-weight:700">{{ reminder.EntityDescription }}</a><br>
                <span style="font-size:12px;"><span style="color:{{ reminder.ReminderType.HighlightColor }}">&#11044;</span> {{ reminder.ReminderTypeName }} &middot; {{ reminder.ReminderDate | Date:''MMMM d, yyyy'' }}</span>

                <p>{{ reminder.Note | NewlineToBr }}</p>
            </td>
        </tr>
        <tr><td colspan="2" style="padding-bottom:24px;"></td></tr>

    {% endfor %}
        <tr>
            <td colspan="2" style="padding-bottom:12px;">
                <p style="text-align:left;"><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}reminders/{{ entityTypeId }}" style="font-weight:700;font-size:12px;">View All Reminders</a></p>
            </td>
        </tr>
    </table>
{% endif %}

{{ ''Global'' | Attribute:''EmailFooter'' }}' WHERE ([Guid]='7899958C-BC2F-499E-A5CC-11DE1EF8DF20')