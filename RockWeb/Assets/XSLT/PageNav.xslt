<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">

	<xsl:output method="html" indent="yes"/>

	<xsl:template match="/">
		<nav>
			<ul>
				<xsl:for-each select="page/pages/page">
					<!-- top level child pages of the root page -->
					<li>
						<a href="#">
							<xsl:attribute name="href">
								<xsl:value-of select="@id"/>
							</xsl:attribute>
							<xsl:value-of select="@title"/>
						</a>
						<!-- only display the children if true -->
						<xsl:if test="@display-child-pages = 'true' and pages[count(page) > 0]">
							<div class="nav-menu group">
								<div class="width-1column">
									<!-- second level children -->
									<xsl:for-each select="pages/page">
										<xsl:call-template name="secondLevel">
											<xsl:with-param name="page" select="."/>
										</xsl:call-template>
									</xsl:for-each>
								</div>
							</div>
						</xsl:if>
					</li>
				</xsl:for-each>
			</ul>
		</nav>
	</xsl:template>

	<xsl:template name="secondLevel">
		<xsl:param name="page"/>
		<section>
			<header>
				<strong>
					<xsl:value-of select="@title"/>
				</strong>
				<xsl:if test="@display-icon = 'true'">
					<img>
						<xsl:attribute name="src">
							<xsl:value-of disable-output-escaping="yes" select="icon-url"/>
						</xsl:attribute>
					</img>
				</xsl:if>
				<xsl:if test="@display-description = 'true'">
					<xsl:value-of disable-output-escaping="yes" select="description"/>
				</xsl:if>
			</header>
			<!-- only display the children if true and there are some to display -->
			<xsl:if test="@display-child-pages = 'true'">
				<ul class="group">
					<!-- third level children -->
					<xsl:for-each select="./pages/page">
						<li>
							<a>
								<xsl:attribute name="href">
									<xsl:value-of select="@id"/>
								</xsl:attribute>
								<xsl:value-of select="@title"/>
							</a>
						</li>
					</xsl:for-each>
				</ul>
			</xsl:if>
		</section>
	</xsl:template>
</xsl:stylesheet>
