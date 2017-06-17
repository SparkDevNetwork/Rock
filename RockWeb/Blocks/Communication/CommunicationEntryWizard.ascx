<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationEntryWizard.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationEntryWizard" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <style>
            // always hide thead image remove
            .propertypanel-image .imageupload-remove {
                display: none !important;
            }
        </style>


        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comment"></i> New Communication</h1>

                <div class="panel-labels">
                    <div class="label label-default"><a href="#">Use Legacy Editor</a></div>
                </div>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlRecipientSelection" runat="server" Visible="false">
                    <h4>Recipient Selection</h4>

                    <Rock:Toggle ID="tglRecipientSelection" runat="server" CssClass="btn-group-justified margin-b-lg" OnText="Select From List" OffText="Select Specific Individuals" Checked="true" OnCssClass="btn-primary" OffCssClass="btn-primary" />
                    
                    <asp:Panel ID="pnlRecipientSelectionList" runat="server">

                        <Rock:RockDropDownList ID="ddlList" runat="server" Label="List" CssClass="input-width-xxl" Required="true">
                            <asp:ListItem Text="All Members and Attendees" />
                        </Rock:RockDropDownList>

                        <label>Segments</label>
                        <p>Optionally, further refine your recipients by filtering by segment.</p>
                        <asp:CheckBoxList ID="cblSegments" runat="server" RepeatDirection="Horizontal" CssClass="margin-b-lg">
                            <asp:ListItem Text="Male" />
                            <asp:ListItem Text="Female" />
                            <asp:ListItem Text="Under 35" />
                            <asp:ListItem Text="35 and older" />
                            <asp:ListItem Text="Members" />
                        </asp:CheckBoxList>

                        <Rock:RockRadioButtonList ID="rblSegmentFilterType" runat="server" Label="Recipients Must Meet" RepeatDirection="Horizontal">
                            <asp:ListItem Text="All segment filters" Value="all" />
                            <asp:ListItem Text="Any segment filter" Value="any" />
                        </Rock:RockRadioButtonList>
                    </asp:Panel>                   

                    <div class="actions">
                        <asp:LinkButton ID="lbRecipientSelectionNext" runat="server" AccessKey="n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbRecipientSelectionNext_Click" />
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlEmailEditor" runat="server" CssClass="emaileditor-wrapper">
                    <section id="emaileditor">
			            <div id="emaileditor-designer">
				            <iframe id="ifEmailDesigner" name="emaileditor-iframe" class="emaileditor-iframe js-emaileditor-iframe" runat="server" src="javascript: window.frameElement.getAttribute('srcdoc');" frameborder="0" border="0" cellspacing="0"></iframe>
			            </div>
			            <div id="emaileditor-properties">
				
				            <div class="emaileditor-propertypanels js-propertypanels">
					            <!-- Text/Html Properties -->
                                <div class="propertypanel propertypanel-text" data-component="text" style="display: none;">
						            <h4 class="propertypanel-title">Text</h4>

                                    <div class="row">
							            <div class="col-md-6">
								            <div class="form-group">
									            <label for="component-text-backgroundcolor">Background Color</label>
									            <div id="component-text-backgroundcolor" class="input-group colorpicker-component">
										            <input type="text" value="" class="form-control" />
										            <span class="input-group-addon"><i></i></span>
									            </div>
								            </div>
                                            <Rock:RockDropDownList Id="ddlLineHeight" CssClass="js-component-text-lineheight" ClientIDMode="Static" runat="server" Label="Line Height">
                                                <asp:ListItem />
                                                <asp:ListItem Text="Normal" Value="100%" />
                                                <asp:ListItem Text="Slight" Value="125%" />
                                                <asp:ListItem Text="1 &frac12; spacing" Value="150%" />
                                                <asp:ListItem Text="Double space" Value="200%" />
                                                <asp:ListItem />
                                            </Rock:RockDropDownList>
							            </div>
							            <div class="col-md-6">
                                            <div class="row">
                                                <div class="col-md-6">
                                                    <div class="form-group">
									                    <label for="component-text-margin-top">Margin Top</label>
                                                        <div class="input-group input-width-md date">
								                            <input class="form-control" id="component-text-margin-top" type="number"><span class="input-group-addon">px</span>
							                            </div>
								                    </div>
                                                    <div class="form-group">
									                    <label for="component-text-margin-bottom">Margin Bottom</label>
									                    <div class="input-group input-width-md date">
								                            <input class="form-control" id="component-text-margin-bottom" type="number"><span class="input-group-addon">px</span>
							                            </div>
								                    </div>
                                                </div>
                                                <div class="col-md-6">
                                                    <div class="form-group">
									                    <label for="component-text-margin-left">Margin Left</label>
									                    <div class="input-group input-width-md date">
								                            <input class="form-control" id="component-text-margin-left" type="number"><span class="input-group-addon">px</span>
							                            </div>
								                    </div>
                                                    <div class="form-group">
									                    <label for="component-text-margin-right">Margin Right</label>
									                    <div class="input-group input-width-md date">
								                            <input class="form-control" id="component-text-margin-right" type="number"><span class="input-group-addon">px</span>
							                            </div>
								                    </div>
                                                </div>
                                            </div>
                                            
                                            
							            </div>
						            </div>

                                    <Rock:HtmlEditor ID="htmlEditor" CssClass="js-component-text-htmlEditor" runat="server" Height="350" CallbackOnChangeScript="updateTextComponent(this, contents);" />
					            </div>

                                <!-- Image Properties -->
                                <div class="propertypanel propertypanel-image" data-component="image" style="display: none;">
						            <h4 class="propertypanel-title">Image</h4>
						            <Rock:ImageUploader ID="componentImageUploader" ClientIDMode="Static" runat="server" Label="Image" UploadAsTemporary="false" DoneFunctionClientScript="handleImageUpdate(e, data)" />

                                    <div class="row">
							            <div class="col-md-6">
								            <div class="form-group">
									            <label for="component-image-imgcsswidth">Width</label>
									            <select id="component-image-imgcsswidth" class="form-control">
										            <option value="0">Image Width</option>
										            <option value="1">Full Width</option>
									            </select>
								            </div>

                                            <div class="form-group">
									            <label for="component-image-imagealign">Align</label>
									            <select id="component-image-imagealign" class="form-control">
										            <option value="left">Left</option>
										            <option value="center">Center</option>
										            <option value="right">Right</option>
									            </select>
								            </div>
                                            
                                            <div class="form-group">
									            <label for="component-image-resizemode">Resize Mode</label>
									            <select id="component-image-resizemode" class="form-control">
										            <option value="crop">Crop</option>
										            <option value="pad">Pad</option>
										            <option value="stretch">Stretch</option>
									            </select>
								            </div>
							            </div>
							            <div class="col-md-6">
                                            <div class="form-group">
									            <label for="component-image-imagewidth">Image Width</label>
									            <div class="input-group input-width-md date">
								                    <input class="form-control" id="component-image-imagewidth" type="number"><span class="input-group-addon">px</span>
							                    </div>
								            </div>
                                            <div class="form-group">
									            <label for="component-image-imageheight">Image Height</label>
									            <div class="input-group input-width-md date">
								                    <input class="form-control" id="component-image-imageheight" type="number"><span class="input-group-addon">px</span>
							                    </div>
								            </div>
							            </div>
						            </div>
					            </div>

                                <!-- Section Properties -->
                                <div class="propertypanel propertypanel-section" data-component="section" style="display: none;">
						            <h4 class="propertypanel-title">Section</h4>
                                    <pre>todo</pre>
					            </div>

                                <!-- Divider Properties -->
                                <div class="propertypanel propertypanel-divider" data-component="divider" style="display: none;">
						            <h4 class="propertypanel-title">Divider</h4>
                                    <pre>todo</pre>
					            </div>

                                <!-- Code Properties -->
                                <div class="propertypanel propertypanel-code" data-component="code" style="display: none;">
						            <h4 class="propertypanel-title">Code</h4>
                                    <pre>todo</pre>
					            </div>

					            <!-- Button Properties -->
                                <div class="propertypanel propertypanel-button" data-component="button" style="display: none;">
						            <h4 class="propertypanel-title">Button</h4>
						            <hr />
						            <div class="form-group">
							            <label for="component-button-buttontext">Button Text</label>
							            <input class="form-control" id="component-button-buttontext" placeholder="Press Me">
						            </div>

						            <div class="form-group">
							            <label for="component-button-buttonurl">Url</label>
							            <div class="input-group">
								            <span class="input-group-addon"><i class="fa fa-link"></i></span>
								            <input class="form-control" id="component-button-buttonurl" placeholder="http://yourlink.com">
							            </div>
						            </div>

						            <div class="row">
							            <div class="col-md-6">
								            <div class="form-group">
									            <label for="component-button-buttonbackgroundcolor">Background Color</label>
									            <div id="component-button-buttonbackgroundcolor" class="input-group colorpicker-component">
										            <input type="text" value="" class="form-control" />
										            <span class="input-group-addon"><i></i></span>
									            </div>
								            </div>
							            </div>
							            <div class="col-md-6">
								            <div class="form-group">
									            <label for="component-button-buttonfontcolor">Font Color</label>
									            <div id="component-button-buttonfontcolor" class="input-group colorpicker-component">
										            <input type="text" value="" class="form-control" />
										            <span class="input-group-addon"><i></i></span>
									            </div>
								            </div>
							            </div>
						            </div>

						            <div class="row">
							            <div class="col-md-6">
								            <div class="form-group">
									            <label for="component-button-buttonwidth">Width</label>
									            <select id="component-button-buttonwidth" class="form-control">
										            <option value="0">Fit To Text</option>
										            <option value="1">Full Width</option>
									            </select>
								            </div>
							            </div>
							            <div class="col-md-6">
								            <div class="form-group">
									            <label for="component-button-buttonalign">Align</label>
									            <select id="component-button-buttonalign" class="form-control">
										            <option value="left">Left</option>
										            <option value="center">Center</option>
										            <option value="right">Right</option>
									            </select>
								            </div>
							            </div>
						            </div>

						            <div class="form-group">
							            <label for="component-button-buttofont">Font</label>
							            <select id="component-button-buttonfont" class="form-control">
								            <option value=""></option>
								            <option value="Arial, Helvetica, sans-serif">Arial</option>
								            <option value='"Arial Black", Gadget, sans-serif'>Arial Black</option>
								            <option value='"Courier New", Courier, monospace'>Courier New</option>
								            <option value="Georgia, serif">Georgia</option>
								            <option value="Helvetica, Arial, sans-serif">Helvetica</option>
								            <option value="Impact, Charcoal, sans-serif">Impact</option>
								            <option value='"Lucida Sans Unicode", "Lucida Grande", sans-serif'>Lucida</option>
								            <option value='"Lucida Console", Monaco, monospace'>Lucida Console</option>
								            <option value="Tahoma, Geneva, sans-serif">Tahoma</option>
								            <option value='Times New Roman", Times, serif'>Times New Roman</option>
								            <option value='Trebuchet MS", Helvetica, sans-serif'>Trebuchet MS</option>
								            <option value="Verdana, Geneva, sans-serif">Verdana</option>
							            </select>
						            </div>

						            <div class="row">
							            <div class="col-md-6">
								            <div class="form-group">
									            <label for="component-button-buttonfontweight">Font Weight</label>
									            <select id="component-button-buttonfontweight" class="form-control">
										            <option value="normal">Normal</option>
										            <option value="bold">Bold</option>
										            <option value="bolder">Bolder</option>
										            <option value="lighter">Lighter</option>
									            </select>
								            </div>
							            </div>
							            <div class="col-md-6">
								            <div class="form-group">
									            <label for="component-button-buttonfontsize">Font Size</label>
									            <input class="form-control" id="component-button-buttonfontsize">
								            </div>
							            </div>
						            </div>

						            <div class="row">
							            <div class="col-md-6">
								            <div class="form-group">
									            <label for="component-button-buttonpadding">Button Padding</label>
									            <input class="form-control" id="component-button-buttonpadding">
								            </div>
							            </div>
							            <div class="col-md-6">
							            </div>
						            </div>
					            </div>

                                <div class="js-propertypanel-actions actions" style="display:none">
                                    <a href="#" class="btn btn-primary" onclick="clearPropertyPane(event)">Complete</a>
                                    <a href="#" class="btn btn-link" onclick="deleteCurrentComponent()">Delete</a>
                                </div>
				            </div>

			            </div>
		            </section>
			
		            <div id="editor-controls" style="display: none;">
			            <div id="editor-toolbar">
				            <div class="component component-text" data-content="<h1>Big News</h1><p> This is a text block. You can use it to add text to your template.</p>" data-state="template">
					            <i class="fa fa-align-justify"></i><br /> Text
				            </div>
				            <div class="component component-image" data-content="<img src='<%= VirtualPathUtility.ToAbsolute("~/Assets/Images/image-placeholder.jpg") %>' style='width: 100%;' data-width='full' />" data-state="template">
					            <i class="fa fa-picture-o"></i> <br /> Image
				            </div>
				            <div class="component component-section" data-content="<table class='component component-separator' width='100%'><tr><td width='50%'><div class='dropzone'></div></td><td width='50%'><div class='dropzone'></div></td></tr></table>" data-state="template">
					            <i class="fa fa-columns"></i> <br /> Section
				            </div>
				            <div class="component component-divider" data-content="<hr style='margin: 18px 0; border: 0; height: 2px; background: #aaa;' />" data-state="template">
					            <i class="fa fa-minus"></i> <br /> Divider
				            </div>
				            <div class="component component-code" data-content="Add your code here..." data-state="template">
					            <i class="fa fa-code"></i> <br /> Code
				            </div>
				            <div class="component component-button" data-content="<table class='button-outerwrap' border='0' cellpadding='0' cellspacing='0' width='100%' style='min-width:100%;'><tbody><tr><td style='padding-top:0; padding-right:0; padding-bottom:0; padding-left:0;' valign='top' align='center' class='button-innerwrap'><table border='0' cellpadding='0' cellspacing='0' class='button-shell' style='display: inline-table; border-collapse: separate !important; border-radius: 3px; background-color: rgb(43, 170, 223);'><tbody><tr><td align='center' valign='middle' class='button-content' style='font-family: Arial; font-size: 16px; padding: 15px;'><a class='button-link' title='Push Me' href='http://' target='_blank' style='font-weight: bold; letter-spacing: normal; line-height: 100%; text-align: center; text-decoration: none; color: rgb(255, 255, 255);'>Push Me</a></td></tr></tbody></table></td></tr></tbody></table>" data-state="template">
					            <i class="fa fa-square-o"></i> <br /> Button
				            </div>
			            </div>
		            </div>	
                </asp:Panel>
            </div>
        
        </asp:Panel>


        <script>
            var $currentComponent = $(false);
            var $currentTextComponent = $(false);
			
			// load in editor styles and scripts
			var cssLink = document.createElement("link") 
            cssLink.href = '<%=RockPage.ResolveRockUrl("~/Themes/Rock/Styles/email-editor.css", true ) %>';
			cssLink.rel = "stylesheet"; 
			cssLink.type = "text/css"; 

			var fontAwesomeLink = document.createElement("link")
			fontAwesomeLink.href = '<%=RockPage.ResolveRockUrl("~/Themes/Rock/Styles/font-awesome.css", true ) %>';
			fontAwesomeLink.rel = "stylesheet"; 
			fontAwesomeLink.type = "text/css";

            var jqueryLoaderScript = document.createElement("script");
            jqueryLoaderScript.type = "text/javascript";
            jqueryLoaderScript.src = '<%=RockPage.ResolveRockUrl("~/Scripts/jquery-1.12.4.min.js", true ) %>';

            var dragulaLoaderScript = document.createElement("script");
            dragulaLoaderScript.type = "text/javascript";
            dragulaLoaderScript.src = '<%=RockPage.ResolveRockUrl("~/Scripts/dragula.min.js", true ) %>';

            var $editorIframe = $('.js-emaileditor-iframe');
            
            var editorScript = document.createElement("script");
			editorScript.type = "text/javascript";
			editorScript.src = '<%=RockPage.ResolveRockUrl("~/Scripts/Rock/Controls/EmailEditor/email-editor.js", true ) %>';
            editorScript.onload = function ()
            {
                $editorIframe[0].contentWindow.Rock.controls.emailEditor.initialize({
                    id: 'editor-window',
                    componentSelected: loadPropertiesPage
                });
            };

            $editorIframe.load(function ()
			{
			    frames['emaileditor-iframe'].document.head.appendChild(jqueryLoaderScript);
			    frames['emaileditor-iframe'].document.head.appendChild(cssLink);
			    frames['emaileditor-iframe'].document.head.appendChild(fontAwesomeLink);
			    frames['emaileditor-iframe'].document.head.appendChild(dragulaLoaderScript);
			    frames['emaileditor-iframe'].document.head.appendChild(editorScript);

			    var $this = $(this);
			    var contents = $this.contents();

			    var editorMarkup = $('#editor-controls').contents();

			    $(contents).find('body').prepend(editorMarkup);
			});
			
			function loadPropertiesPage(componentType, $component)
			{
			    $currentComponent = $component;
			    var $currentPropertiesPanel = $('.js-propertypanels').find("[data-component='" + componentType + "']");

				// hide all property panels
				$('.propertypanel').hide();

				// temp - set text of summernote
				switch(componentType){
					case 'text':
					    Rock.controls.emailEditor.textComponentHelper.setProperties($currentComponent);
						break;
					case 'button':
					    Rock.controls.emailEditor.buttonComponentHelper.setProperties($currentComponent);
						break;
				    case 'image':
				        Rock.controls.emailEditor.imageComponentHelper.setProperties($currentComponent);
				        break;
				    case 'section':
				    case 'divider':
				    case 'code':
				        break;
					default:
						 clearPropertyPane(null);
				}

			    // show proper panel
				$currentPropertiesPanel.show();

			    // show panel actions
				$('.js-propertypanel-actions').show();
			}

			// function that components will call after they have processed their own save and close logic
			function clearPropertyPane(e){

			    // hide all property panes, hide panel actions and set current as not selected
			    $('.propertypanel').hide();
			    $('.js-propertypanel-actions').hide();
			    $currentComponent.removeClass('selected');
				
				if (e != null){
					e.preventDefault();
				}
			}

            // function that will remove the currently selected component from the email html
			function deleteCurrentComponent()
			{
			    $currentComponent.remove();
			    clearPropertyPane(null);
			}

		</script>
        
        <!-- Text Component -->
        <script src='<%=RockPage.ResolveRockUrl("~/Scripts/Rock/Controls/EmailEditor/textComponentHelper.js", true)%>' ></script>

        <!-- Button Component -->
        <script src='<%=RockPage.ResolveRockUrl("~/Scripts/Rock/Controls/EmailEditor/buttonComponentHelper.js", true)%>' ></script>

        <!-- Image Component -->
        <script src='<%=RockPage.ResolveRockUrl("~/Scripts/Rock/Controls/EmailEditor/imageComponentHelper.js", true)%>' ></script>

        <script>
            function updateTextComponent(el, contents)
            {
                Rock.controls.emailEditor.textComponentHelper.updateTextComponent(el, contents);
            }
            function handleImageUpdate(e, data)
            {
                Rock.controls.emailEditor.imageComponentHelper.handleImageUpdate(e, data);
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>