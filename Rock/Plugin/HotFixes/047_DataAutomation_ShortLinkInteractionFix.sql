DECLARE @ShortLinkMediumValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '371066D5-C5F9-4783-88C8-D9AC8DC67468')

IF NOT EXISTS (SELECT * FROM [InteractionChannel]  WHERE [ChannelTypeMediumValueId] = @ShortLinkMediumValueId )
	BEGIN
		DECLARE @ShortlinkEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PageShortLink')
		
		INSERT INTO [InteractionChannel]
			([Name], [ChannelTypeMediumValueId], [ComponentEntityTypeId], [Guid])
			VALUES
			('Short Links', @ShortLinkMediumValueId, @ShortlinkEntityTypeId, 'AEFF9B52-AE61-8EBB-4F43-37C152342076')
	END
ELSE
	BEGIN
		UPDATE [InteractionChannel]
			SET [Name] = 'Short Links'
				, [Guid] = 'AEFF9B52-AE61-8EBB-4F43-37C152342076'
			WHERE [ChannelTypeMediumValueId] = @ShortLinkMediumValueId
	END


-- Upate component list
UPDATE [InteractionChannel]	
	SET
		[ComponentListTemplate] = '	<div class=''panel panel-block''>
        <div class=''panel-heading''>
			<h1 class=''panel-title''>
                <i class=''fa fa-th''></i>
                Components
            </h1>
        </div>
		<div class=''panel-body''>
			{% for component in InteractionComponents %}
			
				 {% if ComponentDetailPage != null and ComponentDetailPage != ''''  %}
                    <a href = ''{{ ComponentDetailPage }}?ComponentId={{ component.Id }}''>
                {% endif %}
                
				 <div class=''panel panel-widget''>
                    <div class=''panel-heading clearfix''>
                        {% if component.Name != '''' %}<h1 class=''panel-title pull-left''>{{ component.Name }}</h1>{% endif %}
                        <div class=''pull-right'' style="padding-top: 2px;">
                            
                            <i class=''fa fa-chevron-right'' style=''float: right; display: block;''></i>
                            
                            {% assign interactionCount = InteractionCounts | Where:''ComponentId'', component.Id | Select:''Count'' %}

                            {% if interactionCount != '''' %}
                                <span class=''label label-default'' style=''float: right; display: block; margin-right: 6px; margin-top: -1px;''>{{ interactionCount }}</span>
                            {% else %}
                                <span class=''label label-default'' style=''float: right; display: block; margin-right: 6px; margin-top: -1px;''>0</span>
                            {% endif %}

                        </div>
                    </div>
                </div>
                {% if ComponentDetailPage != null and ComponentDetailPage != ''''  %}
                    </a>
                {% endif %}
				
			{% endfor %}	
            <div class =''nav-paging''>
            {% if PreviousPageNavigateUrl != null and PreviousPageNavigateUrl != ''''  %}
                <a Id =''lPrev'' class = ''btn btn-primary btn-prev'' href=''{{ PreviousPageNavigateUrl }}''><i class=''fa fa-chevron-left''></i>Prev<a/>
            {% endif %}
            {% if NextPageNavigateUrl != null and NextPageNavigateUrl != ''''  %}
                <a Id =''hlNext'' class = ''btn btn-primary btn-next'' href=''{{ NextPageNavigateUrl }}''> Next <i class=''fa fa-chevron-right''></i><a/>
            {% endif %}
            </div>
		</div>
	</div>'
	WHERE [ChannelTypeMediumValueId] = @ShortLinkMediumValueId

	-- Upate interaction list
	UPDATE [InteractionChannel]	
	SET
		[InteractionListTemplate] = '{% for interaction in Interactions %}
		        {% if InteractionDetailPage != null and InteractionDetailPage != ''''  %}
                    <a href = ''{{ InteractionDetailPage }}?interactionId={{ interaction.Id }}''>
                {% endif %}
		        
		         <div class=''panel panel-widget''>
                    <div class=''panel-heading''>
                        
                        <div class=''row''>
                            <div class=''col-md-12''>
                                <span class=''label label-info pull-left margin-r-md''>{{ interaction.Operation }}</span>
                            
                                {% if interaction.PersonAliasId and interaction.PersonAliasId != '''' and interaction.PersonAliasId != 0 %}
                                    {% assign interactionPerson = interaction.PersonAliasId | PersonByAliasId %}
                                    <h1 class=''panel-title pull-left''>{{ interactionPerson.FullName }} - {{ interaction.InteractionDateTime }}</h1>
                                {% else %}
                                    <h1 class=''panel-title pull-left''>Anonymous - {{ interaction.InteractionDateTime }}</h1>
                                {% endif %}

                                <div class=''pull-right''><i class=''fa fa-chevron-right''></i></div>
                            </div>
                        </div>
                        
                        <dl>
                            <dt>URL</dt>
                            <dd>{{ interaction.InteractionData }}</dd>
                        </dl>
                        
                        <div class=''row margin-t-md''>

                            <div class=''col-md-6''>
                                <dl>
                                    <dt>IP Address</dt>
                                    <dd>{{ interaction.InteractionSession.IpAddress }}</dd>
                                    
                                </dl>
                            </div>

                            
                            
                            <div class=''col-md-6''>
                                <dl>
                                    <dt>Client Type</dt>
                                    <dd>{{ interaction.InteractionSession.DeviceType.ClientType }}</dd>
                                    
                                    <dt>Operating System</dt>
                                    <dd>{{ interaction.InteractionSession.DeviceType.OperatingSystem }}</dd>
                                    
                                    <dt>Browser</dt>
                                    <dd>{{ interaction.InteractionSession.DeviceType.Application }}</dd>
                                </dl>
                            </div>

                        </div>
                    </div>
                </div>
		        
		        {% if InteractionDetailPage != null and InteractionDetailPage != ''''  %}
    		        </a>
		        {% endif %}
	        {% endfor %}	'
	WHERE [ChannelTypeMediumValueId] = @ShortLinkMediumValueId


	-- Upate interaction detail
	UPDATE [InteractionChannel]	
	SET
		[InteractionDetailTemplate] = '    <div class=''panel panel-block''>
        <div class=''panel-heading''>
	        <h1 class=''panel-title''>
                <i class=''fa fa-user''></i>
                Interaction Detail
            </h1>
        </div>
        <div class=''panel-body''>
            <div class=''row''>
                <div class=''col-md-6''>
                    <dl>
                        <dt>Channel</dt><dd>{{ InteractionChannel.Name }}<dd/>
                        <dt>Date / Time</dt><dd>{{ Interaction.InteractionDateTime }}<dd/>
                        <dt>Operation</dt><dd>{{ Interaction.Operation }}<dd/>
                        
                        {% if InteractionEntityName != '''' %}
                            <dt>Related Entity</dt><dd>{{ InteractionEntityName }}<dd/>
                        {% endif %}
                    </dl>
                </div>
                <div class=''col-md-6''>
                    <dl>
                        <dt> Component</dt><dd>{{ InteractionComponent.Name }}<dd/>
                        {% if Interaction.PersonAlias.Person.FullName != '''' %}
                            <dt>Person</dt><dd>{{ Interaction.PersonAlias.Person.FullName }}<dd/>
                        {% endif %}
                        
                        
                    </dl>
                </div>
            </div>
            
            <dl>
                <dt>URL</dt>
                <dd>{{ Interaction.InteractionData }}</dd>
            </dl>
            
            <div class=''row margin-t-md''>
    
                <div class=''col-md-6''>
                    <dl>
                        <dt>IP Address</dt>
                        <dd>{{ Interaction.InteractionSession.IpAddress }}</dd>
                    </dl>
                </div>
    
                
                
                <div class=''col-md-6''>
                    <dl>
                        <dt>Client Type</dt>
                        <dd>{{ Interaction.InteractionSession.DeviceType.ClientType }}</dd>
                        
                        <dt>Operating System</dt>
                        <dd>{{ Interaction.InteractionSession.DeviceType.OperatingSystem }}</dd>
                        
                        <dt>Browser</dt>
                        <dd>{{ Interaction.InteractionSession.DeviceType.Application }}</dd>
                    </dl>
                </div>
    
            </div>
            
        </div>
    </div>'
	WHERE [ChannelTypeMediumValueId] = @ShortLinkMediumValueId