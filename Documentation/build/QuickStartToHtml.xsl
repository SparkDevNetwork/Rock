<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:template match="article">
<xsl:text disable-output-escaping="yes">---
layout: header_only_layout
title: Rock-ChMS Developer
---
</xsl:text>
        <div class="sidebar" style="position: fixed; top: 60px;">
          <div class="well">
            <a name="toc"><h5>Contents</h5></a>
            <ul>
            <xsl:for-each select="section">
              <xsl:call-template name="sidebar">
                <xsl:with-param name="item" select="section"/>
              </xsl:call-template>
            </xsl:for-each>
            </ul>
          </div>
        </div>
        
		    <div class="content">
		      <xsl:apply-templates select="section"/>
		    </div>
</xsl:template>

<xsl:template name="sidebar">
	<xsl:param name="item"/>
	<li><a href="#{./title}">
		<xsl:value-of select="./title"/>
	</a></li>
</xsl:template>

<xsl:template match="section">
	<a style="padding-top:50px; display: block;">
			<xsl:attribute name="name">
				<xsl:value-of select="title"/>
			</xsl:attribute>
	<h1 class="page-header"><xsl:value-of select="title"/></h1>
	</a>
	<xsl:apply-templates select="para"/>
	<br/>
</xsl:template>

<xsl:template match="para">
	<p><xsl:value-of select="."/>
	<!-- <xsl:apply-templates select="emphasis"/> -->
	<xsl:apply-templates select="figure"/>
	</p>
</xsl:template>

<xsl:template match="emphasis">
	<em><xsl:value-of select="."/></em>
</xsl:template>

<xsl:template match="figure">
	<div class="well" style="background-color: white">					
		<img>
			<xsl:attribute name="src">
				<xsl:apply-templates select="screenshot"/>
				<xsl:apply-templates select="mediaobject"/>
				<xsl:value-of select="graphic/@fileref"/>
			</xsl:attribute>
		</img>
		<br/>
		<br/>
		<caption>
			<em styl="small">Figure: <xsl:value-of select="title"/></em>
		</caption>
	</div>	
</xsl:template>

<xsl:template match="screenshot">
				<xsl:value-of select="mediaobject/imageobject/imagedata/@fileref"/>
</xsl:template>

<xsl:template match="mediaobject">
				<xsl:value-of select="imageobject/imagedata/@fileref"/>
</xsl:template>
</xsl:stylesheet>