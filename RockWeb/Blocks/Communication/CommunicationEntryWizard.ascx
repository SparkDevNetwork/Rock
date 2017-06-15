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
						            <div id="text-editor">Hello Summernote</div>

						            <a href="#" class="btn btn-primary" onclick="completeTextComponent(event);">Complete</a>
					            </div>

                                <!-- Image Properties -->
                                <div class="propertypanel propertypanel-image" data-component="image" style="display: none;">
						            <h4 class="propertypanel-title">Image</h4>
						            <Rock:ImageUploader ID="imgupImage" ClientIDMode="Static" runat="server" Label="Image" UploadAsTemporary="false" DoneFunctionClientScript="completeImageComponent(e, data)" />

                                    <div class="row">
							            <div class="col-md-6">
								            <div class="form-group">
									            <label for="component-image-imagewidth">Width</label>
									            <select id="component-image-imagewidth" class="form-control">
										            <option value="0">Image Width</option>
										            <option value="1">Full Width</option>
									            </select>
								            </div>
							            </div>
							            <div class="col-md-6">
								            <div class="form-group">
									            <label for="component-image-imagealign">Align</label>
									            <select id="component-image-imagealign" class="form-control">
										            <option value="left">Left</option>
										            <option value="center">Center</option>
										            <option value="right">Right</option>
									            </select>
								            </div>
							            </div>
						            </div>

						            <a href="#" class="btn btn-primary" onclick="completeImageComponent(event);">Complete</a>
					            </div>

                                <!-- Section Properties -->
                                <div class="propertypanel propertypanel-section" data-component="section" style="display: none;">
						            <h4 class="propertypanel-title">Section</h4>
                                    <pre>todo</pre>

						            <a href="#" class="btn btn-primary" onclick="completeSectionComponent(event);">Complete</a>
					            </div>

                                <!-- Divider Properties -->
                                <div class="propertypanel propertypanel-divider" data-component="divider" style="display: none;">
						            <h4 class="propertypanel-title">Divider</h4>
                                    <pre>todo</pre>

						            <a href="#" class="btn btn-primary" onclick="completeDividerComponent(event);">Complete</a>
					            </div>

                                <!-- Code Properties -->
                                <div class="propertypanel propertypanel-code" data-component="code" style="display: none;">
						            <h4 class="propertypanel-title">Code</h4>
                                    <pre>todo</pre>

						            <a href="#" class="btn btn-primary" onclick="completeCodeComponent(event);">Complete</a>
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

						            <a href="#" class="btn btn-primary" onclick="completeButtonComponent(event);">Complete</a>
					            </div>

                                
				            </div>

			            </div>
		            </section>
			
		            <div id="editor-controls" style="display: none;">
			            <div id="editor-toolbar">
				            <div class="component component-text" data-content="<h1>Yo MTV Raps</h1>" data-state="template">
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
			cssLink.href = "<%=BaseUrl %>/Themes/Rock/Styles/email-editor.css"; 
			cssLink.rel = "stylesheet"; 
			cssLink.type = "text/css"; 

			var fontAwesomeLink = document.createElement("link") 
			fontAwesomeLink.href = "<%=BaseUrl %>Themes/Rock/Styles/font-awesome.css"; 
			fontAwesomeLink.rel = "stylesheet"; 
			fontAwesomeLink.type = "text/css";

            var jqueryLoaderScript = document.createElement("script");
            jqueryLoaderScript.type = "text/javascript";
            jqueryLoaderScript.src = "<%=BaseUrl %>Scripts/jquery-1.12.4.min.js";

            var dragulaLoaderScript = document.createElement("script");
            dragulaLoaderScript.type = "text/javascript";
            dragulaLoaderScript.src = "<%=BaseUrl %>Scripts/dragula.min.js";

			var editorScript = document.createElement("script");
			editorScript.type = "text/javascript";
			editorScript.src = "<%=BaseUrl %>Scripts/email-editor.js";

            var editorIframe = document.getElementsByClassName('js-emaileditor-iframe')[0];
            editorIframe.addEventListener("load", function ()
            {
                frames['emaileditor-iframe'].document.head.appendChild(jqueryLoaderScript);
                frames['emaileditor-iframe'].document.head.appendChild(cssLink);
                frames['emaileditor-iframe'].document.head.appendChild(fontAwesomeLink);
                frames['emaileditor-iframe'].document.head.appendChild(dragulaLoaderScript);
                frames['emaileditor-iframe'].document.head.appendChild(editorScript);
			});
			

			$(".js-emaileditor-iframe").load(function () {
					var $this = $(this);
					var contents = $this.contents();

					var editorMarkup = $('#editor-controls').contents();

					$(contents).find('body').prepend(editorMarkup);
			});	
			
			function loadPropertiesPage(componentType, $component)
			{
			    $currentComponent = $component;
			    $currentTextComponent = $currentComponent.hasClass('component-text') ? $currentComponent : $(false);

				// hide all panels
				$('.propertypanel').hide();

				// get proper panel
				var propertyPanel = $('.js-propertypanels').find("[data-component='" + componentType + "']");

				// temp - set text of summernote
				switch(componentType){
					case 'text':
					    setPropertiesTextComponent($currentTextComponent);
						break;
					case 'button':
					    setPropertiesButtonComponent($currentComponent);
						break;
				    case 'image':
				        setPropertiesImageComponent($currentComponent);
				        break;
				    case 'section':
				    case 'divider':
				    case 'code':
				        break;
					default:
						 clearPropertyPane(null);
				}
				

				propertyPanel.show();
			}

			// function that components will call after they have processed their own save and close logic
			function clearPropertyPane(e){
				$('.propertypanel').hide();
				$currentComponent.removeClass('selected');
				
				if (e != null){
					e.preventDefault();
				}
			}

		</script>

        <!-- Button Component -->
		<script>
			// script logic for the button component
			
			$('#component-button-buttonbackgroundcolor').colorpicker().on('changeColor', function() {
				buttonSetButtonBackgroundColor();
			});

			$('#component-button-buttonfontcolor').colorpicker().on('changeColor', function() {
				buttonSetButtonFontColor();
			});

			$('#component-button-buttontext').on('input',function(e){
				buttonSetButtonText();
			});

			$('#component-button-buttonurl').on('input',function(e){
				buttonSetButtonUrl();
			});

			$('#component-button-buttonwidth').on('change',function(e){
				buttonSetButtonWidth();
			});

			$('#component-button-buttonalign').on('change',function(e){
				buttonSetButtonAlign();
			});

			$('#component-button-buttonfont').on('change',function(e){
				buttonSetButtonFont();
			});

			$('#component-button-buttonfontweight').on('change',function(e){
				buttonSetButtonFontWeight()
			});

			$('#component-button-buttonfontsize').on('input',function(e){
				buttonSetButtonFontSize();
			});

			$('#component-button-buttonpadding').on('input',function(e){
				buttonSetButtonPadding();
			});

			function completeButtonComponent(e){
				buttonSetButtonText();
				buttonSetButtonUrl();
				buttonSetButtonBackgroundColor();
				buttonSetButtonFontColor();
				buttonSetButtonWidth();
				buttonSetButtonAlign();
				buttonSetButtonFont();
				buttonSetButtonFontWeight()
				buttonSetButtonFontSize();
				buttonSetButtonPadding();
				clearPropertyPane(e);
			}

			function setPropertiesButtonComponent($buttonComponent){
				var buttonText = $buttonComponent.find('.button-link').text();
				var buttonUrl = $buttonComponent.find('.button-link').attr('href');
				var buttonBackgroundColor = $buttonComponent.find('.button-shell').css('backgroundColor');
				var buttonFontColor = $buttonComponent.find('.button-link').css('color');
				var buttonWidth = $buttonComponent.find('.button-shell').attr('width');
				var buttonAlign = $buttonComponent.find('.button-innerwrap').attr('align');
				var buttonFont = $buttonComponent.find('.button-link').css("font-family");
				var buttonFontWeight = $buttonComponent.find('.button-link').css("font-weight");
				var buttonFontSize = $buttonComponent.find('.button-link').css("font-size");
				var buttonPadding = $buttonComponent.find('.button-content').css("padding");

				$('#component-button-buttontext').val(buttonText);
				$('#component-button-buttonurl').val(buttonUrl);
				$('#component-button-buttonbackgroundcolor').colorpicker('setValue', buttonBackgroundColor);
				$('#component-button-buttonfontcolor').colorpicker('setValue', buttonFontColor);

				if (buttonWidth == '100%'){
					$('#component-button-buttonwidth').val(1);
				}
				else {
					$('#component-button-buttonwidth').val(0);
				}

				$('#component-button-buttonalign').val(buttonAlign);

				$('#component-button-buttonfont').val(buttonFont);
				$('#component-button-buttonfontweight').val(buttonFontWeight);
				$('#component-button-buttonfontsize').val(buttonFontSize);
				$('#component-button-buttonpadding').val(buttonPadding);
			}

			function buttonSetButtonText(){
				var text = $('#component-button-buttontext').val()

				var component = $currentComponent;
				$(component).find('.button-link').text(text);
				$(component).find('.button-link').attr('title', text);
			}

			function buttonSetButtonUrl(){
				var text = $('#component-button-buttonurl').val()

				var component = $currentComponent;
				$(component).find('.button-link').attr('href', text);
			}

			function buttonSetButtonBackgroundColor(){
				var color = $('#component-button-buttonbackgroundcolor').colorpicker('getValue');

				var component = $currentComponent;
				$(component).find('.button-shell').css('backgroundColor', color);
			}

			function buttonSetButtonFontColor(){
				var color = $('#component-button-buttonfontcolor').colorpicker('getValue');

				var component = $currentComponent;
				$(component).find('.button-link').css('color', color);
			}

			function buttonSetButtonWidth(){
				var selectValue = $('#component-button-buttonwidth').val();

				if (selectValue == 0){
				    var component = $currentComponent;
					$(component).find('.button-shell').removeAttr('width');
				}
				else 
				{
				    var component = $currentComponent;
					$(component).find('.button-shell').attr('width', '100%');
				}
			}

			function buttonSetButtonAlign() {
				var selectValue = $('#component-button-buttonalign').val();

				var component = $currentComponent;
				$(component).find('.button-innerwrap').attr('align', selectValue);
				$(component).find('.button-innerwrap').css('text-align', selectValue);
			}

			function buttonSetButtonFont() {
				var selectValue = $('#component-button-buttonfont').val();

				var component = $currentComponent;
				$(component).find('.button-link').css('font-family', selectValue);
			}

			function buttonSetButtonFontWeight() {
				var selectValue = $('#component-button-buttonfontweight').val();

				var component = $currentComponent;
				$(component).find('.button-link').css('font-weight', selectValue);
			}

			function buttonSetButtonFontSize(){
				var text = $('#component-button-buttonfontsize').val()

				var component = $currentComponent;
				$(component).find('.button-link').css('font-size', text);
			}

			function buttonSetButtonPadding(){
				var text = $('#component-button-buttonpadding').val()

				var component = $currentComponent;
				$(component).find('.button-content').css('padding', text);
			}
		</script>

        <!-- Text Component -->
		<script>
			// script logic for the text component

			// setup summernote
			$(document).ready(function() {
				$('#text-editor').summernote({
					height: 350,
					callbacks: {
					    onChange: function (contents, $editable)
					    {
                            updateTextComponent(this, contents);
						}
					}
				});
			});

			function setPropertiesTextComponent($textComponent){
			    $('#text-editor').summernote('code', $textComponent.html());
			}

			function completeTextComponent(e)
			{
				clearPropertyPane(e);
			}

			function updateTextComponent(el, contents)
			{
			    if ($currentTextComponent)
			    {
			        $currentTextComponent.html(contents);
			    }
			}
		</script>

        <!-- Image Component -->
        <script>
            function setPropertiesImageComponent($imageComponent) {
                var imageUrl = $imageComponent.find('img').attr('src');
                var imageWidth = $imageComponent.find('img').attr('data-width');
                var imageAlign = $imageComponent.css('text-align');

                $('#imgupImage').find('.imageupload-thumbnail-image').css('background-image', 'url("' + imageUrl + '")');

                if (imageWidth == 'full') {
                    $('#component-image-imagewidth').val(1);
                }
                else {
                    $('#component-image-imagewidth').val(0);
                }

                $('#component-image-imagealign').val(imageAlign);
            }

            function completeImageComponent(e, data) {

                if (data != null) {
                    imageSetImage(data);
                }

                imageSetImageWidth();
                imageSetImageAlign();

                clearPropertyPane(e);
            }


            $('#component-image-imagewidth').on('change', function (e) {
                imageSetImageWidth();
            });

            $('#component-image-imagealign').on('change', function (e) {
                imageSetImageAlign();
            });

            function imageSetImageAlign(){
                var selectValue = $('#component-image-imagealign').val();

                var component = $currentComponent;
                $(component).css('text-align', selectValue);
            }

            function imageSetImageWidth(){
                var selectValue = $('#component-image-imagewidth').val();

                var component = $currentComponent;

                if (selectValue == 0) {
                    $(component).find('img').css('width', 'auto');
                    $(component).find('img').attr('data-width', 'image');
                }
                else {
                    $(component).find('img').css('width', '100%');
                    $(component).find('img').css('data-width', 'full');
                }
            }

            function imageSetImage(data) {

                var imageUrl = Rock.settings.get('baseUrl')
                        + 'GetImage.ashx?'
                        + 'isBinaryFile=T' 
                        + '&id=' + data.response().result.Id
                        + '&fileName=' + data.response().result.FileName
                        + '&width=500';

                var component = $currentComponent;
                $(component).find('img').attr('src', imageUrl);
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>