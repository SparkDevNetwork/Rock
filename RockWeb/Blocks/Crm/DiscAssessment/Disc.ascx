<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Disc.ascx.cs" Inherits="Rockweb.Blocks.Crm.Disc" ViewStateMode="Enabled" EnableViewState="true" %>

<div id="tabs">
    <ul id="navTabs" class="nav nav-tabs">
        <li class="active"><a data-toggle="tab" href="#instructions">Instructions</a></li>
        <li><a data-toggle="tab" href="#questions">Questions</a></li>
        <li><a data-toggle="tab" href="#results">Results</a></li>
    </ul>
    <div class="tab-content">
        <div id="instructions" class="tab-pane active">
            <h3>Welcome!</h3>
            <p>
                In this assessment you are given a series of questions, each containing four phrases.
            Select one phrase from each question that MOST describes you and one phrase that
            LEAST describes you.
            </p>
            <p>
                This assessment is environmentally sensitive, which means that you may score differently
            in different situations. In other words, you may act differently at home than you
            do on the job. So, as you complete the assessment you should focus on one environment
            for which you are seeking to understand yourself. For instance, if you are trying
            to understand yourself in marriage, you should only think of your responses to situations
            in the context of your marriage. On the other hand, if you want to know your behavioral
            needs on the job, then only think of how you would respond in the job context.
            </p>
            <p>
                One final thought as you give your responses. On these kinds of assessments, it
            is often best and easiest if you respond quickly and do not deliberate too long
            on each question. Your response on one question will not unduly influence your scores,
            so simply answer as quickly as possible and enjoy the process. Don't get too hung
            up, if none of the phrases describe you or if there are some phrases that seem too
            similar, just go with your instinct.
            </p>
            <p>
                When you are ready, click the 'Questions' tab above to proceed.
            </p>
        </div>
        <div id="questions" class="tab-pane">

            <div class="container">
                <asp:Repeater ID="rQuestions" runat="server" OnItemDataBound="rQuestions_ItemDataBound">
                    <ItemTemplate>
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Question <%# Container.ItemIndex + 1 %>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-md-1">
                                        MORE
                                    </div>
                                    <div class="col-md-4">
                                        LESS
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-1">
                                        <Rock:RockRadioButtonList ID="rblMore" runat="server" />
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:RockRadioButtonList ID="rblLess" runat="server" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <asp:Button ID="btnScoreTest" Text="Score Test" runat="server" CssClass="btn btn-primary" OnClick="btnScoreTest_Click" />
        </div>
        <div id="results" class="tab-pane">
            <div class="row">
                <div class="col-md-3">
                    <table class="table table-bordered table-condensed table-striped">
                        <thead>
                            <tr class="info">
                                <th colspan="2">Adaptive Behavior
                                </th>
                                <th colspan="2">Natural Behavior
                                </th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>D:
                                </td>
                                <td>
                                    <asp:Label ID="lblABd" Text="" runat="server" />
                                </td>
                                <td>D:
                                </td>
                                <td>
                                    <asp:Label ID="lblNBd" Text="" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td>I:
                                </td>
                                <td>
                                    <asp:Label ID="lblABi" Text="" runat="server" />
                                </td>
                                <td>I:
                                </td>
                                <td>
                                    <asp:Label ID="lblNBi" Text="" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td>S:
                                </td>
                                <td>
                                    <asp:Label ID="lblABs" Text="" runat="server" />
                                </td>
                                <td>S:
                                </td>
                                <td>
                                    <asp:Label ID="lblNBs" Text="" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td>C:
                                </td>
                                <td>
                                    <asp:Label ID="lblABc" Text="" runat="server" />
                                </td>
                                <td>C:
                                </td>
                                <td>
                                    <asp:Label ID="lblNBc" Text="" runat="server" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
            <asp:Button ID="btnSaveResults" Text="Save Results" runat="server" CssClass="btn btn-primary" OnClick="btnSaveResults_Click" />
            <hr />
            <h2>Your Saved DISC Assessment Scores
            </h2>
            <div>
                Saved on:
                <asp:Label ID="lblLastAssessmentDate" Text="" runat="server" />
            </div>
            <div class="row">
                <div class="col-md-3">
                    <table class="table table-bordered table-condensed table-striped">
                        <thead>
                            <tr class="info">
                                <th colspan="2">Adaptive Behavior
                                </th>
                                <th colspan="2">Natural Behavior
                                </th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>D:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevABd" Text="" runat="server" />
                                </td>
                                <td>D:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevNBd" Text="" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td>I:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevABi" Text="" runat="server" />
                                </td>
                                <td>I:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevNBi" Text="" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td>S:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevABs" Text="" runat="server" />
                                </td>
                                <td>S:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevNBs" Text="" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td>C:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevABc" Text="" runat="server" />
                                </td>
                                <td>C:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevNBc" Text="" runat="server" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>
