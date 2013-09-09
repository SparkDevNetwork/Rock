<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">

	<xsl:output method="html" indent="yes"/>

	<xsl:template match="/">

			<ul class="nav navbar-nav">
				<xsl:for-each select="page/pages/page">
					<!-- top level child pages of the root page -->
					<li class="dropdown pagenav-item">
						<a href="#" class="dropdown-toggle" data-toggle="dropdown">
							<xsl:attribute name="href">#</xsl:attribute>

              <i>
                <xsl:attribute name="class">
                  <xsl:value-of select="@icon-css-class"/>
                </xsl:attribute>
              </i>
							<xsl:value-of select="@title"/>
              
              <b class="caret"></b>
            </a>
              <!-- only display the children if true -->
              <xsl:if test="@display-child-pages = 'true' and pages[count(page) > 0]">
                <ul class="dropdown-menu" role="menu">
                  <!-- second level children -->
                  <xsl:for-each select="pages/page">
                    <xsl:call-template name="secondLevel">
                      <xsl:with-param name="page" select="."/>
                    </xsl:call-template>
                  </xsl:for-each>
                </ul>
              </xsl:if>
              
						
					</li>
				</xsl:for-each>
			</ul>

	</xsl:template>

	<xsl:template name="secondLevel">
		<xsl:param name="page"/>
		
				<li class="dropdown-header">
					<xsl:value-of select="@title"/>
				</li>
				
				
			<!-- only display the children if true and there are some to display -->
			<xsl:if test="@display-child-pages = 'true'">
				<!-- third level children -->
				<xsl:for-each select="./pages/page">
					<li role="presentation">
						<a role="menu-item">
							<xsl:attribute name="href">
								<xsl:value-of select="@url"/>
							</xsl:attribute>
							<xsl:value-of select="@title"/>
						</a>
					</li>
				</xsl:for-each>
        <li class="divider"></li>
			</xsl:if>
	</xsl:template>
</xsl:stylesheet>
