/**
 * range.spec.js
 * (c) 2013~ Alan Hong
 * summernote may be freely distributed under the MIT license./
 */
define([
  'chai',
  'chaidom',
  'jquery',
  'summernote/base/core/dom',
  'summernote/base/core/range'
], function (chai, chaidom, $, dom, range) {
  'use strict';

  var expect = chai.expect;
  chai.use(chaidom);

  describe('base:core.range', function () {
    describe('nodes', function () {
      describe('1 depth', function () {
        var $para;
        before(function () {
          var $cont = $('<div class="note-editable"><p>para1</p><p>para2</p></div>');
          $para = $cont.find('p');
        });

        it('should return array of two paragraphs', function () {
          var rng = range.create($para[0].firstChild, 0, $para[1].firstChild, 1);
          expect(rng.nodes(dom.isPara, {includeAncestor: true})).to.have.length(2);
        });

        it('should return array of a paragraph', function () {
          var rng = range.create($para[0].firstChild, 0, $para[0].firstChild, 0);
          expect(rng.nodes(dom.isPara, { includeAncestor: true })).to.have.length(1);
        });
      });

      describe('multi depth', function () {
        it('should return array of a paragraph', function () {
          var $cont = $('<div class="note-editable"><p>p<b>ar</b>a1</p><p>para2</p></div>');
          var $b = $cont.find('b');
          var rng = range.create($b[0].firstChild, 0, $b[0].firstChild, 0);

          expect(rng.nodes(dom.isPara, { includeAncestor: true })).to.have.length(1);
        });
      });

      describe('on list, on heading', function () {
        it('should return array of list paragraphs', function () {
          var $cont = $('<div class="note-editable"><ul><li>para1</li><li>para2</li></ul></div>');
          var $li = $cont.find('li');
          var rng = range.create($li[0].firstChild, 0, $li[1].firstChild, 1);

          expect(rng.nodes(dom.isPara, { includeAncestor: true })).to.have.length(2);
        });

        it('should return array of list paragraphs', function () {
          var $cont = $('<div class="note-editable"><h1>heading1</h1><h2>heading2</h2></div>');
          var $h1 = $cont.find('h1');
          var $h2 = $cont.find('h2');
          var rng = range.create($h1[0].firstChild, 0, $h2[0].firstChild, 1);

          expect(rng.nodes(dom.isPara, { includeAncestor: true })).to.have.length(2);
        });
      });
    });

    describe('commonAncestor', function () {
      var $cont;
      before(function () {
        $cont = $('<div><span><b>b</b><u>u</u></span></div>');
      });

      it('should return <span> for <b>|b</b> and <u>u|</u>', function () {
        var $span = $cont.find('span');
        var $b = $cont.find('b');
        var $u = $cont.find('u');

        var rng = range.create($b[0].firstChild, 0, $u[0].firstChild, 1);
        expect(rng.commonAncestor()).to.deep.equal($span[0]);
      });

      it('should return b(#textNode) for <b>|b|</b>', function () {
        var $b = $cont.find('b');

        var rng = range.create($b[0].firstChild, 0, $b[0].firstChild, 1);
        expect(rng.commonAncestor()).to.deep.equal($b[0].firstChild);
      });
    });

    describe('expand', function () {
      it('should return <b>|b</b> ~ <u>u|</u> for <b>|b</b> with isAnchor', function () {
        var $cont = $('<div><a><b>b</b><u>u</u></a></div>');
        var $anchor = $cont.find('a');
        var $b = $cont.find('b');

        var rng = range.create($b[0].firstChild, 0, $b[0].firstChild, 0).expand(dom.isAnchor);
        expect(rng.sc).to.deep.equal($anchor[0]);
        expect(rng.so).to.equal(0);
        expect(rng.ec).to.deep.equal($anchor[0]);
        expect(rng.eo).to.equal(2);
      });
    });

    describe('collapse', function () {
      it('should return <u>u|</u> for <b>|b</b> ~ <u>u|</u>', function () {
        var $cont = $('<div><b>b</b><u>u</u></div>');
        var $b = $cont.find('b');
        var $u = $cont.find('u');

        var rng = range.create($b[0].firstChild, 0, $u[0].firstChild, 1).collapse();
        expect(rng.sc).to.deep.equal($u[0].firstChild);
        expect(rng.so).to.equal(1);
        expect(rng.ec).to.deep.equal($u[0].firstChild);
        expect(rng.eo).to.equal(1);
      });
    });

    describe('normalize', function () {
      var $cont;
      before(function () {
        $cont = $('<div><p><b>b</b><u>u</u><s>s</s></p></div>');
      });

      it('should return <b>|b</b> ~ <u>u|</u> for |<b>b</b> ~ <u>u</u>|', function () {
        var $p = $cont.find('p');
        var $b = $cont.find('b');
        var $u = $cont.find('u');

        var rng = range.create($p[0], 0,  $p[0], 2).normalize();
        expect(rng.sc).to.deep.equal($b[0].firstChild);
        expect(rng.so).to.equal(0);
        expect(rng.ec).to.deep.equal($u[0].firstChild);
        expect(rng.eo).to.equal(1);
      });

      it('should return <b>b|</b><u>u</u> for <b>b</b>|<u>u</u>', function () {
        var $p = $cont.find('p');
        var $b = $cont.find('b');

        var rng = range.create($p[0], 1,  $p[0], 1).normalize();
        expect(rng.sc).to.deep.equal($b[0].firstChild);
        expect(rng.so).to.equal(1);
        expect(rng.ec).to.deep.equal($b[0].firstChild);
        expect(rng.eo).to.equal(1);
      });

      it('should return <b>b</b><u>|u|</u><s>s</s> for <b>b|</b><u>u</u><s>|s</s>', function () {
        var $b = $cont.find('b');
        var $u = $cont.find('u');
        var $s = $cont.find('s');

        var rng = range.create($b[0].firstChild, 1,  $s[0].firstChild, 0).normalize();
        expect(rng.sc).to.deep.equal($u[0].firstChild);
        expect(rng.so).to.equal(0);
        expect(rng.ec).to.deep.equal($u[0].firstChild);
        expect(rng.eo).to.equal(1);
      });

      it('should return <b>b|</b><u>u</u><s>s</s> for <b>b|</b><u>u</u><s>s</s>', function () {
        var $b = $cont.find('b');

        var rng = range.create($b[0].firstChild, 1,  $b[0].firstChild, 1).normalize();
        expect(rng.sc).to.deep.equal($b[0].firstChild);
        expect(rng.so).to.equal(1);
        expect(rng.ec).to.deep.equal($b[0].firstChild);
        expect(rng.eo).to.equal(1);
      });
    });

    describe('normalize (block mode)', function () {
      it('should return <p>text</p><p>|<br></p> for <p>text</p><p>|<br></p>', function () {
        var $cont = $('<div><p>text</p><p><br></p></div>');
        var $p = $cont.find('p');

        var rng = range.create($p[1], 0,  $p[1], 0).normalize();
        expect(rng.sc).to.deep.equal($p[1]);
        expect(rng.so).to.equal(0);
        expect(rng.ec).to.deep.equal($p[1]);
        expect(rng.eo).to.equal(0);
      });

      it('should return <p>text</p><p>|text</b></p> for <p>text</p><p>|text</p>', function () {
        var $cont = $('<div><p>text</p><p>text</p></div>');
        var $p = $cont.find('p');

        var rng = range.create($p[1], 0,  $p[1], 0).normalize();
        expect(rng.sc).to.deep.equal($p[1].firstChild);
        expect(rng.so).to.equal(0);
        expect(rng.ec).to.deep.equal($p[1].firstChild);
        expect(rng.eo).to.equal(0);
      });

      it('should return <p>|text</p><p>text|</b></p> for |<p>text</p><p>text</p>|', function () {
        var $cont = $('<div class="note-editable"><p>text</p><p>text</p></div>');
        var $p = $cont.find('p');

        var rng = range.create($cont[0], 0,  $cont[0], 2).normalize();
        expect(rng.sc).to.deep.equal($p[0].firstChild);
        expect(rng.so).to.equal(0);
        expect(rng.ec).to.deep.equal($p[1].firstChild);
        expect(rng.eo).to.equal(4);
      });
    });

    describe('normalize (void element)', function () {
      it('should return <p><img>|<b>bold</b></p> for <p><img>|<b>bold</b></p>', function () {
        var $cont = $('<div><p><img><b>bold</b></p></div>');
        var $p = $cont.find('p');
        var $b = $cont.find('b');

        var rng = range.create($p[0], 1,  $p[0], 1).normalize();
        expect(rng.sc).to.deep.equal($b[0].firstChild);
        expect(rng.so).to.equal(0);
        expect(rng.ec).to.deep.equal($b[0].firstChild);
        expect(rng.eo).to.equal(0);
      });
    });

    describe('insertNode', function () {
      it('should split paragraph when inserting a block element', function () {
        var $cont = $('<div class="note-editable"><p><b>bold</b></p></div>');
        var $b = $cont.find('b');
        var $p2 = $('<p>p</p>');

        var rng = range.create($b[0].firstChild, 2, $b[0].firstChild, 2);
        rng.insertNode($p2[0]);

        expect($cont.html()).to.equalsIgnoreCase('<p><b>bo</b></p><p>p</p><p><b>ld</b></p>');
      });

      it('should not split paragraph when inserting an inline element', function () {
        var $cont = $('<div class="note-editable"><p>text</p></div>');
        var $p = $cont.find('p');
        var $u = $('<u>u</u>');

        var rng = range.create($p[0].firstChild, 2, $p[0].firstChild, 2);
        rng.insertNode($u[0]);
        expect($cont.html()).to.equalsIgnoreCase('<p>te<u>u</u>xt</p>');
      });

      it('should not split paragraph when inserting an inline element case 2', function () {
        var $cont = $('<div class="note-editable"><p><b>bold</b></p></div>');
        var $b = $cont.find('b');
        var $u = $('<u>u</u>');

        var rng = range.create($b[0].firstChild, 2, $b[0].firstChild, 2);
        rng.insertNode($u[0]);
        expect($cont.html()).to.equalsIgnoreCase('<p><b>bo</b><u>u</u><b>ld</b></p>');
      });
    });

    describe('pasteHTML', function () {
      it('should not split a block element when inserting inline elements into it', function () {
        var $cont = $('<div class="note-editable"><p>text</p></div>');
        var $p = $cont.find('p');
        var markup = '<span>span</span><i>italic</i>';

        var rng = range.create($p[0].firstChild, 2);
        rng.pasteHTML(markup);

        expect($cont.html()).to.equalsIgnoreCase('<p>te<span>span</span><i>italic</i>xt</p>');
      });

      it('should split an inline element when pasting inline elements into it', function () {
        var $cont = $('<div class="note-editable"><p><b>bold</b></p></div>');
        var $b = $cont.find('b');
        var markup = '<span>span</span><i>italic</i>';

        var rng = range.create($b[0].firstChild, 2);
        rng.pasteHTML(markup);

        expect($cont.html()).to.equalsIgnoreCase('<p><b>bo</b><span>span</span><i>italic</i><b>ld</b></p>');
      });

      it('should split inline node when pasting an inline node and a block node into it', function () {
        var $cont = $('<div class="note-editable"><p><b>bold</b></p></div>');
        var $b = $cont.find('b');
        var markup = '<span>span</span><p><i>italic</i></p>';

        var rng = range.create($b[0].firstChild, 2);
        rng.pasteHTML(markup);

        expect($cont.html()).to.equalsIgnoreCase('<p><b>bo</b><span>span</span></p><p><i>italic</i></p><p><b>ld</b></p>');
      });
    });

    describe('deleteContents', function () {
      var $cont, $b;
      beforeEach(function () {
        $cont = $('<div class="note-editable"><p><b>bold</b><u>u</u></p></div>');
        $b = $cont.find('b');
      });

      it('should remove text only for partial text', function () {
        var rng = range.create($b[0].firstChild, 1, $b[0].firstChild, 3);
        rng.deleteContents();

        expect($cont.html()).to.equalsIgnoreCase('<p><b>bd</b><u>u</u></p>');
      });

      it('should remove text for entire text', function () {
        var rng = range.create($b[0].firstChild, 0, $b[0].firstChild, 4);
        rng.deleteContents();

        expect($cont.html()).to.equalsIgnoreCase('<p><b></b><u>u</u></p>');
      });
    });

    describe('wrapBodyInlineWithPara', function () {
      it('should insert an empty paragraph when there is no contents', function () {
        var $cont = $('<div class="note-editable"></div>');

        var rng = range.create($cont[0], 0);
        rng.wrapBodyInlineWithPara();

        expect($cont.html()).to.equalsIgnoreCase('<p><br></p>');
      });

      it('should wrap text with paragraph for text', function () {
        var $cont = $('<div class="note-editable">text</div>');

        var rng = range.create($cont[0].firstChild, 2);
        rng.wrapBodyInlineWithPara();

        expect($cont.html()).to.equalsIgnoreCase('<p>text</p>');
      });

      it('should wrap an inline node with paragraph when selecting text in the inline node', function () {
        var $cont = $('<div class="note-editable"><b>bold</b></div>');
        var $b = $cont.find('b');

        var rng = range.create($b[0].firstChild, 2);
        rng.wrapBodyInlineWithPara();

        expect($cont.html()).to.equalsIgnoreCase('<p><b>bold</b></p>');
      });

      it('should wrap inline nodes with paragraph when selecting text in the inline nodes', function () {
        var $cont = $('<div class="note-editable"><b>b</b><i>i</i></div>');

        var rng = range.create($cont[0], 0);
        rng.wrapBodyInlineWithPara();

        expect($cont.html()).to.equalsIgnoreCase('<p><b>b</b><i>i</i></p>');
      });

      it('should wrap inline nodes with paragraph when selection some of text in the inline nodes #1', function () {
        var $cont = $('<div class="note-editable"><b>b</b><i>i</i></div>');

        var rng = range.create($cont[0], 1);
        rng.wrapBodyInlineWithPara();

        expect($cont.html()).to.equalsIgnoreCase('<p><b>b</b><i>i</i></p>');
      });

      it('should wrap inline nodes with paragraph when selection some of text in the inline nodes #2', function () {
        var $cont = $('<div class="note-editable"><b>b</b><i>i</i></div>');

        var rng = range.create($cont[0], 2);
        rng.wrapBodyInlineWithPara();

        expect($cont.html()).to.equalsIgnoreCase('<p><b>b</b><i>i</i></p>');
      });
    });

    describe('getWordRange', function () {
      var $cont;
      before(function () {
        $cont = $('<div class="note-editable">super simple wysiwyg editor</div>');
      });

      it('should return the range itself when there is no word before cursor', function () {
        var rng = range.create($cont[0].firstChild, 0).getWordRange();

        expect(rng.sc).to.deep.equal($cont[0].firstChild);
        expect(rng.so).to.equal(0);
        expect(rng.ec).to.deep.equal($cont[0].firstChild);
        expect(rng.eo).to.equal(0);
      });

      it('should return expanded range when there is a word before cursor', function () {
        var rng = range.create($cont[0].firstChild, 5).getWordRange();

        expect(rng.sc).to.deep.equal($cont[0].firstChild);
        expect(rng.so).to.equal(0);
        expect(rng.ec).to.deep.equal($cont[0].firstChild);
        expect(rng.eo).to.equal(5);
      });

      it('should return expanded range when there is a half word before cursor', function () {
        var rng = range.create($cont[0].firstChild, 3).getWordRange();

        expect(rng.sc).to.deep.equal($cont[0].firstChild);
        expect(rng.so).to.equal(0);
        expect(rng.ec).to.deep.equal($cont[0].firstChild);
        expect(rng.eo).to.equal(3);
      });

      it('should return expanded range when there are words before cursor', function () {
        var rng = range.create($cont[0].firstChild, 12).getWordRange();

        expect(rng.sc).to.deep.equal($cont[0].firstChild);
        expect(rng.so).to.equal(6);
        expect(rng.ec).to.deep.equal($cont[0].firstChild);
        expect(rng.eo).to.equal(12);
      });
    });
  });
});
