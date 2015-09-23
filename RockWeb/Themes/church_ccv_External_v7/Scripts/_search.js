window.CCV = window.CCV || {}

CCV.receiveSearchKeypress = function(e) {
  var keyCode = e.keyCode ? e.keyCode : e.which;
  if (keyCode == 13) {
    var query = $('#search-input').val()
    // var query = e.srcElement.value
    this.searchFor(query)
    e.preventDefault ? e.preventDefault() : e.returnValue = false;
  }
}

CCV.receiveSearchEnter = function() {
  var query = $('#search-input').val()
  this.searchFor(query)
  e.preventDefault ? e.preventDefault() : e.returnValue = false;
}

CCV.searchFor = function(query) {
  document.location.href = '/search?q=' + query
}

CCV.getParameterByName = function(name) {
  name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
  var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
      results = regex.exec(location.search);
  return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}
