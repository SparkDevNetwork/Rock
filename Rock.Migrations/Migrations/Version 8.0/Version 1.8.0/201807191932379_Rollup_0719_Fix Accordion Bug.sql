UPDATE [LavaShortCode] 
SET [Markup] = '{%- assign wrapperId = uniqueid -%}
{%- assign firstopen = firstopen | AsBoolean -%}
<div class="panel-group" id="accordion-{{ wrapperId }}" role="tablist" aria-multiselectable="true">
  {%- for item in items -%}
      {%- assign isopen = '''' -%}
      {%- if item.isopen and item.isopen !='''' -%}
        {%- assign isopen = item.isopen | AsBoolean -%}
      {%- else -%}
        {%- if forloop.index == 1 and firstopen -%}
            {%- assign isopen = true -%}
        {%- else -%}
            {%- assign isopen = false -%}
        {%- endif -%}
      {%- endif -%}
      
      <div class="panel panel-{{ paneltype }}">
        <div class="panel-heading" role="tab" id="heading{{ forloop.index }}-{{ wrapperId }}">
          <h4 class="panel-title">
            <a role="button" data-toggle="collapse" data-parent="#accordion-{{ wrapperId }}" href="#collapse{{ forloop.index }}-{{ wrapperId }}" aria-expanded="{% if isopen %}true{% else %}false{% endif %}" aria-controls="collapse{{ forloop.index }}">
              {{ item.title }}
            </a>
          </h4>
        </div>
        <div id="collapse{{ forloop.index }}-{{ wrapperId }}" class="panel-collapse collapse{% if isopen %} in{% endif %}" role="tabpanel" aria-labelledby="heading{{ forloop.index }}-{{ wrapperId }}">
          <div class="panel-body">
            {{ item.content }}
          </div>
        </div>
      </div>
  {%- endfor -%}
</div>' WHERE [Guid] = '18F87671-A848-4509-8058-C95682E7BAD4'