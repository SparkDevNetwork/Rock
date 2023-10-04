<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TemplateDetail.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.MinePass.TemplateDetail" %>
<%@ Import namespace="com.minecartstudio.MinePass.Common.Enums" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-id-badge"></i> <asp:Literal ID="lTitle" runat="server" /></h1>

                 <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblPassType" runat="server" LabelType="Info" />
                    <Rock:HighlightLabel ID="hlblIsActive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                 <fieldset id="fieldsetViewSummary" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lTemplateName" runat="server" Label="Template Name" />

                            <Rock:RockLiteral ID="lDescription" runat="server" Label="Description" />
                        </div>
                        <div class="col-md-6">
                            <asp:Literal ID="lPassType" runat="server" />
                        </div>
                    </div>

                     <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnEdit_Click" />
                    </div>
                </fieldset>
                
                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="com.minecartstudio.MinePass.Client.Model.MinePassTemplate, com.minecartstudio.MinePass.Client" PropertyName="Name" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" Checked="true" />
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="com.minecartstudio.MinePass.Client.Model.MinePassTemplate, com.minecartstudio.MinePass.Client" PropertyName="Description" Rows="3" TextMode="MultiLine" />

                    <hr />

                    <h4>Apple Pass Information</h4>

                    <ul class="nav nav-tabs nav-justified margin-b-lg">
                        <li id="tabGeneral" runat="server" role="presentation" class="active"><asp:LinkButton ID="btnTabGeneral" runat="server" Text="General" OnClick="btnTab_Click" /></li>
                        <li id="tabFields" runat="server" role="presentation"><asp:LinkButton ID="btnTabFields" runat="server" Text="Fields" OnClick="btnTab_Click" /></li>
                        <li id="tabImages" runat="server" role="presentation"><asp:LinkButton ID="btnTabImages" runat="server" Text="Additional Images" OnClick="btnTab_Click" /></li>
                        <li id="tabBarcode" runat="server" role="presentation"><asp:LinkButton ID="btnTabBarcode" runat="server" Text="Barcode" OnClick="btnTab_Click" /></li>
                        <li id="tabRelevance" runat="server" role="presentation"><asp:LinkButton ID="btnTabRelevance" runat="server" Text="Relevance Fields" OnClick="btnTab_Click" /></li>
                    </ul>

                    <asp:Panel ID="pnlTabGeneral" runat="server">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockRadioButtonList ID="rblPassType" runat="server" Label="Pass Type" RepeatDirection="Horizontal" Required="true" OnSelectedIndexChanged="rblPassType_SelectedIndexChanged" AutoPostBack="true" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockRadioButtonList ID="rblTransitType" runat="server" Label="Transit Type" RepeatDirection="Horizontal" Required="true" Visible="false" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:ColorPicker ID="cpBackgroundColor" runat="server" Label="Background Color" />

                                <Rock:ColorPicker ID="cpLabelColor" runat="server" Label="Label Color" />

                                <Rock:ColorPicker ID="cpForegroundColor" runat="server" Label="Foreground Color" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="txtOrganizationName" runat="server" Label="Organization Name" Help="Display name of the organization that originated the pass." />

                                <Rock:RockTextBox ID="txtLogoText" runat="server" Label="Logo Text" Help="Text displayed next to the logo on the pass." />

                                <Rock:RockTextBox ID="txtPassDescription" runat="server" Label="Pass Description" TextMode="MultiLine" Rows="3" Help="Used by the iOS accessibility technologies. Don’t try to include all of the data on the pass in its description, just include enough detail to distinguish passes of the same type." />
                            </div>
                        </div>

                        <h5>Icon</h5>
                        <p>
                            The required icon is displayed when a pass is shown on the lock screen and by apps such as 
                            Mail when showing a pass attached to an email.
                            <br />
                            <small>Standard Size: 29 x 29 pxs</small>
                            <br />
                            <small>Retina Size: 58 x 58 pxs</small>
                        </p>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupIcon" runat="server" Label="Standard Size" Required="true" />
                            </div>
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupIconRetina" runat="server" Label="Retina" Required="true" />
                            </div>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlTabImages" runat="server" Visible="false">
                        
                        <p>
                            For the most up to date guide on images for Apple Passes see the 
                            <a href="https://developer.apple.com/library/archive/documentation/UserExperience/Conceptual/PassKit_PG/index.html#//apple_ref/doc/uid/TP40012195-CH1-SW1">
                                Wallet Developer Guide</a>.
                        </p>

                        <hr />

                        <h5>Logo</h5>
                        <p>
                            <span class="label label-default"><i class="<%= MinePassAppleStyle.Generic.GetIconCssClass() %>"></i> Generic</span>
                            <span class="label label-default"><i class="<%= MinePassAppleStyle.BoardingPass.GetIconCssClass() %>"></i> Boarding Pass</span>
                            <span class="label label-default"><i class="<%= MinePassAppleStyle.Coupon.GetIconCssClass() %>"></i> Coupon</span>
                            <span class="label label-default"><i class="<%= MinePassAppleStyle.EventTicket.GetIconCssClass() %>"></i> Event Ticket</span>
                            <span class="label label-default"><i class="<%= MinePassAppleStyle.StoreCard.GetIconCssClass() %>"></i> Store Card</span>
                        </p>
                        <p>
                            The logo image is displayed in the top left corner of the pass, next to 
                            the logo text. The allotted space is shown below; in most cases 
                            it should be narrower.
                            <br />
                            <small>Standard Size: 160 x 50 pxs</small>
                            <br />
                            <small>Retina Size: 320 x 100 pxs</small>
                        </p>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupLogo" runat="server" Label="Standard Size" />
                            </div>
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupLogoRetina" runat="server" Label="Retina" />
                            </div>
                        </div>

                        <hr />

                        <h5>Strip</h5>
                        <p>
                            <span class="label label-default"><i class="<%= MinePassAppleStyle.Coupon.GetIconCssClass() %>"></i> Coupon</span>
                            <span class="label label-default"><i class="<%= MinePassAppleStyle.EventTicket.GetIconCssClass() %>"></i> Event Ticket</span>
                            <span class="label label-default"><i class="<%= MinePassAppleStyle.StoreCard.GetIconCssClass() %>"></i> Store Card</span>
                        </p>
                        <p>
                            The strip image is displayed behind the primary fields. If you specify a strip image, do not specify a background image or a thumbnail.
                            <br />
                            <small>Standard Size: 375 x 98 pxs for event tickets, 375 x 144 px for gift cards and coupons</small>
                            <br />
                            <small>Retina Size: 750 x 196 pxs for event tickets, 750 x 288 px for gift cards and coupons</small>
                        </p>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupStrip" runat="server" Label="Standard Size" />
                            </div>
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupStripRetina" runat="server" Label="Retina" />
                            </div>
                        </div>

                        <hr />

                        <h5>Footer</h5>
                        <p>
                            <span class="label label-default"><i class="<%= MinePassAppleStyle.BoardingPass.GetIconCssClass() %>"></i> Boarding Pass</span>
                        </p>
                        <p>
                            The footer image is displayed near the barcode.
                            <br />
                            <small>Standard Size: 286 x 15 pxs</small>
                            <br />
                            <small>Retina Size: 572 x 30 pxs</small>
                        </p>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupFooter" runat="server" Label="Standard Size" />
                            </div>
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupFooterRetina" runat="server" Label="Retina" />
                            </div>
                        </div>

                        <hr />

                        <h5>Background</h5>
                        <p>
                            <span class="label label-default"><i class="<%= MinePassAppleStyle.Generic.GetIconCssClass() %>"></i> Generic</span>
                        </p>
                        <p>
                            The background image is displayed behind the entire front of the pass. The image is 
                            cropped slightly on all 
                            sides and blurred. Depending on the image, you can often provide an image at a smaller 
                            size and let it be scaled up, because the blur effect hides details. This lets you 
                            reduce the file size without a noticeable difference in the pass.
                            <br />
                            <small>Standard Size: 180 x 220 pxs</small>
                            <br />
                            <small>Retina Size: 360 x 440 pxs</small>
                        </p>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupBackground" runat="server" Label="Standard Size" />
                            </div>
                            <div class="col-md-6">
                                <Rock:ImageUploader ID="imgupBackgroundRetina" runat="server" Label="Retina" />
                            </div>
                        </div>

                        <hr />

                        <h5>Thumbnail</h5>
                        <p>
                            <span class="label label-default"><i class="<%= MinePassAppleStyle.Generic.GetIconCssClass() %>"></i> Generic</span>
                            <span class="label label-default"><i class="<%= MinePassAppleStyle.EventTicket.GetIconCssClass() %>"></i> Event Ticket</span>
                        </p>
                        <p>
                            The thumbnail image is
                            displayed next to the fields on the front of the pass. 
                            The aspect ratio should be in the range of 2:3 to 3:2, otherwise the 
                            image is cropped.
                        <p>
                            Since this image is typically customized for each pass we have provided you with space to
                            provide a Lava template.
                        </p>
                            <br />
                            <small>Standard Size: 90 x 90 pxs</small>
                            <br />
                            <small>Retina Size: 180 x 180 pxs</small>
                        </p>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:CodeEditor ID="ceThumbnailImageUrl" runat="server" EditorMode="Lava" Label="Thumbnail Image URL" Help="Lava template to create a URL for the thumbnail image." />
                            </div>
                            <div class="col-md-6">
                                <Rock:CodeEditor ID="ceThumbnailRetinaImageUrl" runat="server" EditorMode="Lava" Label="Thumbnail Retina Image URL" Help="Lava template to create a URL for the thumbnail retina image." />
                            </div>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlTabBarcode" runat="server" Visible="false">
                        <Rock:RockCheckBox ID="cbShowBarcode" runat="server" Label="Show Barcode" />
                        
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:CodeEditor ID="ceBarcodeValue" runat="server" EditorMode="Lava" Label="Barcode Value" Help="Lava template to format the barcode." />
                                <Rock:RockRadioButtonList ID="rblBarcodeType" runat="server" Label="Barcode Type" RepeatDirection="Horizontal" Required="true" />
                            </div>
                            <div class="col-md-6">
                                <Rock:CodeEditor ID="ceBarcodeAlternateText" runat="server" EditorMode="Lava" Label="Barcode Alternate Text" Help="Lava template for the text displayed near the barcode. For example, a human-readable version of the barcode data in case the barcode doesn’t scan." />
                                <Rock:RockTextBox ID="txtBarcodeEncoding" runat="server" Label="Barcode Encoding" />
                            </div>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlTabFields" runat="server" Visible="false">
                        
                        <p>
                            The pass style determines the maximum number of fields that can appear on 
                            the front of a pass:
                        </p>
                        <ul>
                            <li>
                                In general, a pass can have up to three header fields, a single primary field, up to 
                                four secondary fields, and up to four auxiliary fields.
                            </li>
                            <li>
                                Boarding passes can have up to two primary fields and up to five auxiliary fields.
                            </li>
                            <li>
                                Coupons, store cards, and generic passes with a square barcode can have a total of 
                                up to four secondary and auxiliary fields, combined.
                            </li>
                        </ul>
                        
                        <p>
                            The number of fields displayed on the pass also depends on the length of the text in 
                            each field. If there is too much text, some fields may not be displayed.
                        </p>

                        <h5>Header Fields</h5>
                        <p>
                            Header fields contain highly salient information, and they are the only field that 
                            is visible when the passes are stacked up in Wallet, so use them sparingly.
                        </p>
                        <Rock:Grid ID="gHeaderFields" runat="server" DisplayType="Light" OnRowSelected="gHeaderFields_Edit">
                            <Columns>
                                <Rock:ReorderField/>
                                <Rock:RockBoundField DataField="Label" HeaderText="Label"  />
                                <Rock:RockBoundField DataField="PassFieldType" HeaderText="Type" />
                                <Rock:RockBoundField DataField="Alignment" HeaderText="Alignment" />
                                <Rock:RockBoundField DataField="Value" HeaderText="Value" />
                                <Rock:DeleteField OnClick="gHeaderFields_Delete"/>
                            </Columns>
                        </Rock:Grid>

                        <hr />
                        <h5>Primary Fields</h5>
                        <p>
                            The primary fields contain the most important information and are shown prominently on the pass.
                        </p>
                        <Rock:Grid ID="gPrimaryFields" runat="server" DisplayType="Light" OnRowSelected="gPrimaryFields_Edit">
                            <Columns>
                                <Rock:ReorderField/>
                                <Rock:RockBoundField DataField="Label" HeaderText="Label"  />
                                <Rock:RockBoundField DataField="PassFieldType" HeaderText="Type" />
                                <Rock:RockBoundField DataField="Alignment" HeaderText="Alignment" />
                                <Rock:RockBoundField DataField="Value" HeaderText="Value" />
                                <Rock:DeleteField OnClick="gPrimaryFields_Delete"/>
                            </Columns>
                        </Rock:Grid>

                        <hr />
                        <h5>Secondary Fields</h5>
                        <p>
                            Secondary fields are less important and less prominent. 
                        </p>
                        <Rock:Grid ID="gSecondaryFields" runat="server" DisplayType="Light" OnRowSelected="gSecondaryFields_Edit">
                            <Columns>
                                <Rock:ReorderField/>
                                <Rock:RockBoundField DataField="Label" HeaderText="Label"  />
                                <Rock:RockBoundField DataField="PassFieldType" HeaderText="Type" />
                                <Rock:RockBoundField DataField="Alignment" HeaderText="Alignment" />
                                <Rock:RockBoundField DataField="Value" HeaderText="Value" />
                                <Rock:DeleteField OnClick="gSecondaryFields_Delete"/>
                            </Columns>
                        </Rock:Grid>

                        <hr />
                        <h5>Auxiliary Fields</h5>
                        <p>
                            Auxiliary fields are the least important of the fields.
                        </p>
                        <Rock:Grid ID="gAuxiliaryFields" runat="server" DisplayType="Light" OnRowSelected="gAuxiliaryFields_Edit">
                            <Columns>
                                <Rock:ReorderField/>
                                <Rock:RockBoundField DataField="Label" HeaderText="Label"  />
                                <Rock:RockBoundField DataField="PassFieldType" HeaderText="Type" />
                                <Rock:RockBoundField DataField="Alignment" HeaderText="Alignment" />
                                <Rock:RockBoundField DataField="Value" HeaderText="Value" />
                                <Rock:DeleteField OnClick="gAuxiliaryFields_Delete"/>
                            </Columns>
                        </Rock:Grid>

                        <hr />
                        <h5>Back Fields</h5>
                        <p>
                            Space on the front of the pass is at a premium, so the number of fields is limited, 
                            and the contents of fields must be brief. In contrast, the back of the pass can have 
                            as many fields as needed and the contents of fields can be much longer. If there is 
                            too much content fit on the screen at once, let the users scroll through it on the back. 
                            As the contents of each field on the back gets longer, the field’s font size is reduced.
                        </p>
                        <p>
                            The back of the pass gives you a place to put information that is too long to fit on 
                            the front of the pass, including extra information that isn’t as important and general 
                            information that may be helpful for your users. Examples of back-field information includes 
                            terms and conditions, the full address of a venue, your customer service phone number, and 
                            the URL for your website.
                        </p>
                        <Rock:Grid ID="gBackFields" runat="server" DisplayType="Light" OnRowSelected="gBackFields_Edit">
                            <Columns>
                                <Rock:ReorderField/>
                                <Rock:RockBoundField DataField="Label" HeaderText="Label"  />
                                <Rock:RockBoundField DataField="PassFieldType" HeaderText="Type" />
                                <Rock:RockBoundField DataField="Alignment" HeaderText="Alignment" />
                                <Rock:RockBoundField DataField="Value" HeaderText="Value" />
                                <Rock:DeleteField OnClick="gBackFields_Delete"/>
                            </Columns>
                        </Rock:Grid>
                    </asp:Panel>

                    <asp:Panel ID="pnlTabRelevance" runat="server" Visible="false">
                        
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DatePicker ID="dpRelevantDate" runat="server" Label="Relevant Date" Help="Date and time when the pass becomes relevant. For example, the start time of a movie. The value must be a complete date with hours and minutes." />
                            </div>
                            <div class="col-md-6">
                                <Rock:DatePicker ID="dpExpirationDate" runat="server" Label="Expiration Date" Help="Date and time when the pass expires. The value must be a complete date with hours and minutes." />
                            </div>
                        </div>

                        <h4>Relevant Points in Space</h4>
                        <p>
                            Passes let your users take some action in the real world, so accessing them needs to 
                            be easy and fast. Wallet makes relevant passes immediately accessible by integrating 
                            them with the lock screen. To take advantage of this feature, add information about 
                            where and when your pass is relevant. A pass’s relevancy can be based on the time 
                            and/or the place in which the user can use the pass. For example, a gym membership 
                            is relevant at the gym, while a boarding pass is relevant at the time the flight 
                            begins boarding.
                        </p>
                        
                        <h5>Locations</h5>
                        <p>
                            A pass can have up to ten relevant locations. Only the top ten locations below will be used.
                        </p>
                        <Rock:Grid ID="gLocations" runat="server" DisplayType="Light" OnRowSelected="gLocations_Edit">
                            <Columns>
                                <Rock:ReorderField/>
                                <Rock:RockBoundField DataField="Latitude" HeaderText="Latitude"  />
                                <Rock:RockBoundField DataField="Longitude" HeaderText="Longitude" />
                                <Rock:RockBoundField DataField="Message" HeaderText="Message" />
                                <Rock:DeleteField OnClick="gLocations_Delete"/>
                            </Columns>
                        </Rock:Grid>

                        <h5>Beacons</h5>
                        <p>
                            You can also provide relevancy information using iBeacons. A pass is relevant when 
                            the device is brought into proximity of an iBeacon with a matching UUID. You can 
                            add up to ten unique beacon UUIDs to the pass. Beacons can optionally include relevancy 
                            text, as well as the major and minor Bluetooth identifiers. The system uses the relevancy 
                            text from the most-specific matching beacon (the beacon matching the most of the UUID, 
                            minor, and major identifiers). A pass can have up to ten relevant beacons. Only the top ten beacons below will be used.
                        </p>
                        <Rock:Grid ID="gBeacons" runat="server" DisplayType="Light" OnRowSelected="gBeacons_Edit">
                            <Columns>
                                <Rock:ReorderField/>
                                <Rock:RockBoundField DataField="ProximityUuid" HeaderText="ProximityUuid"  />
                                <Rock:RockBoundField DataField="Major" HeaderText="Major" />
                                <Rock:RockBoundField DataField="Minor" HeaderText="Minor" />
                                <Rock:RockBoundField DataField="Message" HeaderText="Message" />
                                <Rock:DeleteField OnClick="gBeacons_Delete"/>
                            </Columns>
                        </Rock:Grid>

                        <div class="well">
                            <h5>Additional Details</h5>
                            <table class="table table-striped">
                                <thead>
                                    <tr>
                                        <th>Pass style</th>
                                        <th>Relevant date</th>
                                        <th>Relevant locations</th>
                                        <th>Relevance</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>Boarding pass</td>
                                        <td>Recommended: Include the relevant date unless the pass is relevant for an extended period of time, such as for a month-long bus pass.</td>
                                        <td>Optional: Interpreted with a large radius.</td>
                                        <td>Relevant if the date <em>and</em> any location matches. Without a relevant date, pass is relevant if the location matches.</td>
                                    </tr>
                                    <tr>
                                        <td>Coupon</td>
                                        <td>Not supported.</td>
                                        <td>Required if relevance information is provided. Interpreted with a small radius.</td>
                                        <td>Relevant if any location matches.</td>
                                    </tr>
                                    <tr>
                                        <td>Event ticket</td>
                                        <td>Recommended. Include the relevant date unless the pass is relevant for an extended period of time, such as for a multi-day event. </td>
                                        <td>Optional. Interpreted with a large radius.</td>
                                        <td>Relevant if the date <em>and</em> any location matches. Without a relevant location, relevant if the date matches.</td>
                                    </tr>
                                    <tr>
                                        <td>Generic</td>
                                        <td>Optional.</td>
                                        <td>Required if relevance information is provided. Interpreted with a small radius.</td>
                                        <td>Relevant if the date <em>and</em> any location matches. Without a relevant date, relevant if any location matches.</td>
                                    </tr>
                                    <tr>
                                        <td>Store card</td>
                                        <td>Not supported.</td>
                                        <td>Required if relevance information is provided. Interpreted with a small radius.</td>
                                        <td>Relevant if any location matches.</td>
                                    </tr>
                                </tbody>
                              </table>
                            <p>
                                <small>When the location is interpreted with a small radius, the current location must be on the order of a hundred meters or closer; with a large radius, on the order of a thousand meters or closer. In both cases, the exact relevance radius is an implementation detail and may change.</small>
                            </p>
                        </div>
                    </asp:Panel>
                    
                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>

            </div>
        
        </asp:Panel>

        <Rock:ModalDialog ID="mdFieldDetail" runat="server" Title="Field Detail" OnSaveClick="mdFieldDetail_SaveClick" ValidationGroup="FieldDetail">
            <Content>
                <asp:ValidationSummary ID="valSummaryField" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="FieldDetail" />
                <asp:HiddenField ID="hfFieldKey" runat="server" />
                <asp:HiddenField ID="hfFieldType" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbLabel" runat="server" Label="Label" Required="true"  ValidationGroup="FieldDetail" />
                        <Rock:RockRadioButtonList ID="rdlFieldAlignment" runat="server" Label="Field Text Alignment" RepeatDirection="Horizontal" Required="true" ValidationGroup="FieldDetail" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockRadioButtonList ID="rdlFieldType" runat="server" Label="Field Type" RepeatDirection="Horizontal" Required="true" ValidationGroup="FieldDetail" />
                    </div>
                </div>

                <Rock:CodeEditor ID="ceFieldValue" runat="server" EditorMode="Lava" Label="Value" Help="Lava template to specify the label value." Required="true" ValidationGroup="FieldDetail" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdLocationDetail" runat="server" Title="Location Detail" OnSaveClick="mdLocationDetail_SaveClick" ValidationGroup="LocationDetail">
            <Content>
                <asp:ValidationSummary ID="valSummaryLocation" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="BeaconDetail" />
                <asp:HiddenField ID="hfLocationKey" runat="server" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextbox ID="tbLatitude" runat="server" Label="Latitude" Required="true"  ValidationGroup="LocationDetail" Help="Latitude, in degrees, of the location." />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextbox ID="tbLongitude" runat="server" Label="Longitude" Required="true"  ValidationGroup="LocationDetail" Help="Longitude, in degrees, of the location." />
                    </div>
                </div>

                <Rock:CodeEditor ID="ceLocationMessage" runat="server" EditorMode="Lava" Label="Message"  Help="Lava message displayed on the lock screen when the pass is currently relevant. For example, a description of the nearby location such as 'Store nearby on 1st and Main.'" Required="true" ValidationGroup="LocationDetail" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdBeaconDetail" runat="server" Title="Beacon Detail" OnSaveClick="mdBeaconDetail_SaveClick" ValidationGroup="BeaconDetail">
            <Content>
                <asp:ValidationSummary ID="valSummaryBeacon" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="BeaconDetail" />
                <asp:HiddenField ID="hfBeaconKey" runat="server" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="txtProximityUuid" runat="server" Label="Proximity UUID" Required="true"  ValidationGroup="LocationDetail" Help="Unique identifier of a Bluetooth Low Energy location beacon." />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:NumberBox NumberType="Integer" ID="nbMajor" runat="server" Label="Major" ValidationGroup="BeaconDetail" Help="Major identifier of a Bluetooth Low Energy location beacon." />
                    </div>
                    <div class="col-md-6">
                        <Rock:NumberBox NumberType="Integer" ID="nbMinor" runat="server" Label="Minor" ValidationGroup="BeaconDetail" Help="Minor identifier of a Bluetooth Low Energy location beacon." />
                    </div>
                </div>

                <Rock:CodeEditor ID="ceBeaconMessage" runat="server" EditorMode="Lava" Label="Message"  Help="Lava message displayed on the lock screen when the pass is currently relevant. For example, a description of the nearby location such as 'You are standing in front of the shoe display.'" Required="true" ValidationGroup="BeaconDetail" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>