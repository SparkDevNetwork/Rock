<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">

	<!-- A Recursive XSLT for displaying a page tree in UL/LI form -->
	<xsl:output method="html" indent="yes"/>
	<xsl:template match="/">
		<xsl:if test="page/@display-child-pages = 'true' and page/pages[count(page) > 0]">
			<ul>
				<xsl:for-each select="page/pages/page">
					<xsl:call-template name="otherLevels">
						<xsl:with-param name="page" select="."/>
					</xsl:call-template>
				</xsl:for-each>
			</ul>
		</xsl:if>
	</xsl:template>

	<xsl:template name="otherLevels">
		<xsl:param name="page"/>
		<li>
			<a>
				<xsl:attribute name="href">
					<xsl:value-of select="@id"/>
				</xsl:attribute>
				<xsl:value-of select="@title"/>
			</a>
			<xsl:if test="@display-child-pages = 'true' and pages[count(page) > 0]">
				<!-- recursive children -->
				<ul>
					<xsl:for-each select="./pages/page">
						<xsl:call-template name="otherLevels">
							<xsl:with-param name="page" select="."/>
						</xsl:call-template>
					</xsl:for-each>
				</ul>
			</xsl:if>
		</li>
	</xsl:template>
</xsl:stylesheet>
