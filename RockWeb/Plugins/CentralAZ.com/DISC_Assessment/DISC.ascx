<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DISC.ascx.cs" Inherits="Plugins.CentralAZ.DISC" %>
<div id="tabs">
	<ul>
		<li><a href="#instructions">Instructions</a></li>
		<li><a href="#questions">Questions</a></li>
		<li><a href="#results">Results</a></li>
	</ul>
	<div id="instructions" class="tabContent">
		<h3>
			Welcome!</h3>
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
	<div id="questions" class="tabContent">
		<asp:Table ID="tblQuestions" runat="server">
		</asp:Table>
		<asp:Button ID="btnScoreTest" Text="Score Test" runat="server" OnClick="btnScoreTest_Click" />
		<p />
	</div>
	<div id="results" class="tabContent">
		<table border="1" cellpadding="5" cellspacing="3" class="center">
			<tr>
				<td>
					<asp:Label ID="Label1" Text="Adaptive Behavior" runat="server" />
					<br />
					<asp:Label ID="lblABd" Text="" runat="server" />
					<br />
					<asp:Label ID="lblABi" Text="" runat="server" />
					<br />
					<asp:Label ID="lblABs" Text="" runat="server" />
					<br />
					<asp:Label ID="lblABc" Text="" runat="server" />
				</td>
				<td>
					<asp:Label ID="Label2" Text="Natural Behavior" runat="server" />
					<br />
					<asp:Label ID="lblNBd" Text="" runat="server" />
					<br />
					<asp:Label ID="lblNBi" Text="" runat="server" />
					<br />
					<asp:Label ID="lblNBs" Text="" runat="server" />
					<br />
					<asp:Label ID="lblNBc" Text="" runat="server" />
				</td>
			</tr>
		</table>
	</div>
</div>
