<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">

	<xsl:output method="html" indent="yes"/>

	<xsl:template match="/">

			<ul class="nav nav-pills">
				<xsl:for-each select="page/pages/page">
					<!-- top level child pages of the root page -->
                    <li>
                        <xsl:if test="@current = 'True'">
                            <xsl:attribute name="class">
                              active
                            </xsl:attribute>
                        </xsl:if>
                        <a href="#">
                            <xsl:attribute name="href">
                                <xsl:value-of select="@url"/>
                            </xsl:attribute>
                            <xsl:value-of select="@title"/>
                        </a>
                   </li>
				</xsl:for-each>
			</ul>

	</xsl:template>

</xsl:stylesheet>
