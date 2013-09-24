<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicData.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DynamicData" %>

<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>

        <div class="dynamic-report">
            
            <div class="dynamic-report-details" >
                <asp:PlaceHolder ID="phEdit" runat="server">
                    <p><a id="edit-report" class="report-add btn" onclick="return toggleDetails();" ><i class="icon-edit"></i></a></p>
                </asp:PlaceHolder>
            </div>

            <asp:PlaceHolder ID="phDetails" runat="server">
                
                <div class="dynamic-report-details row-fluid well" style="display: none;">
                    
                    <div class="span6">
                        <fieldset>
                            <Rock:RockTextBox ID="tbName" runat="server" Label="Page Name" CssClass="input-large" Help="The current page's title" />
                            <Rock:RockTextBox ID="tbDesc" runat="server" Label="Page Description" TextMode="MultiLine" Rows="2" CssClass="input-xlarge" Help="The current page's description" />
                            <Rock:RockTextBox ID="tbQuery" runat="server" Label="Query" TextMode="MultiLine" Rows="5" CssClass="input-xlarge" 
                                Help="The SQL query or stored procedure name to execute.  If parameters are included below, this should be the name of a stored procedure, otherwise it can be any SQL text.
By default, a grid will be displayed showing all the rows and columns returned by the query.  However, if the query returns one row and one column with a name of 'Html,' the html will be rendered instead.  Likewise, if it 
returns one row and one column with a name of 'Xml', the escaped xml will be displayed, or the xml can optionally be transformed to html by specifying an 'Xslt File' value." />
                            <Rock:RockTextBox ID="tbParams" runat="server" Label="Parameters" TextMode="MultiLine" Rows="2" CssClass="input-xlarge" 
                                Help="The parameters that the stored procedure expects in the format of 'param1=value;param2=value'.  Any parameter with the same name as a page parameter (i.e. querystring, 
form, or page route) will have it's value replaced with the page's current value.  A parameter with the name of 'CurrentPersonId' will have it's value replaced with the currently logged in person's id."/>
                        </fieldset>
                    </div>

                    <div class="span6">
                        <fieldset>
                            <Rock:RockTextBox ID="tbUrlMask" runat="server" Label="Selection Url" CssClass="input-large"
                                Help="The Url to redirect user to when they click on a row in the grid.  Any column's value can be used in the url by including it in braces.  For example if the grid includes an 'Id' column that contains Person Ids, you can link to the Person view, by specifying a value here of '~/Person/{Id}" />
                            <div class="control-group">
                                <div class="control-label">
                                    <asp:DropDownList ID="ddlHideShow" runat="server" CssClass="input-small">
                                        <asp:ListItem Text="Hide" Value="False"></asp:ListItem>
                                        <asp:ListItem Text="Show" Value="True"></asp:ListItem>
                                    </asp:DropDownList>
                                    Columns<Rock:HelpBlock ID="helpHideShow" runat="server" Text="If using 'Show Columns,' only the columns specified will be displayed.  If using 'Hide Columns,' all columns except the columns specified will be displayed." />
                                </div>
		                        <div class="controls">
                                    <asp:TextBox ID="tbColumns" runat="server" TextMode="MultiLine" Rows="2" CssClass="input-xlarge" />
                                </div>
                            </div>

                            <Rock:RockTextBox ID="tbXslt" runat="server" Label="Xslt File" CssClass="input-large" 
                                Help="The Xslt file to use for transformation when outputing Xml.  The filename is relative to the current theme's Assets/Xslt folder.  If left blank, the escaped Xml will be displayed." />
                            <Rock:RockCheckBox ID="cbPersonReport" runat="server" Label="Person Report" Text="Yes"
                                Help="Does this query return a list of people? If it does, then additional options will be available from the result grid.  (i.e. Communicate, etc).  Note: A column named 'Id' that contains the person's Id is required for a person report." />
                            <Rock:RockTextBox ID="tbMergeFields" runat="server" Label="Communication Merge Fields" TextMode="MultiLine" Rows="2" CssClass="input-xlarge" 
                                Help="When creating a new communication from a person report, additional fields from the report can be used as merge fields on the communication.  Enter any column names that you'd like to be available for the communication."/>

                        </fieldset>
                    </div>

                    <div class="actions span12">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <a onclick="return toggleDetails();" class="btn">Cancel</a>
                    </div>

                </div>

            </asp:PlaceHolder>

            <Rock:NotificationBox ID="nbError" runat="server" Heading="Error" Title="Query Error!" NotificationBoxType="Error" Visible="false" />

            <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" />
            <asp:PlaceHolder ID="phHtml" runat="server" Visible="false" />
            <asp:Xml ID="xmlContent" runat="server" Visible="false" />

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
