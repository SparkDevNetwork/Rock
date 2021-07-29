var personalLinks = function () {
  
    var _returnItemArray = new Array();

    function addQuickReturn(type, typeOrder, itemName) {
        getLocalStorage();
        var today = new Date();
        var returnItem = {
            type: type,
            typeOrder: typeOrder,
            createdDateTime: today,
            itemName: itemName,
            url: window.location.href
        };

        const found = _returnItemArray.some(el => el.url.toLowerCase() === window.location.href.toLowerCase());
        if (found)
        {
            _returnItemArray = _returnItemArray.filter(function (el) {
                return !(el.url.toLowerCase() === window.location.href.toLowerCase() && el.type.toLowerCase() === type.toLowerCase());
            });
        }

        _returnItemArray.push(returnItem);

        _returnItemArray.sort(function (a, b) {
            // Sort by count
            var dtypeOrder = a.typeOrder - b.typeOrder;
            if (dtypeOrder) return dtypeOrder;

            // If there is a tie, sort by year
            var dCreatedDateTime = new Date(b.createdDateTime) - new Date(a.createdDateTime);
            return dCreatedDateTime;
        });

        var arrLength = _returnItemArray.length;
        if (arrLength > 20) {
            _returnItemArray.splice(20, arrLength - 20);
        }

        setLocalStorage();
    }

    function getQuickReturns() {
        getLocalStorage();
        var types = {};
        for (var i = 0; i < _returnItemArray.length; i++) {
            var type = _returnItemArray[i].type;
            if (!types[type]) {
                types[type] = [];
            }
            types[type].push(_returnItemArray[i]);
        }
        itemsByType = [];
        for (var itemType in types) {
            itemsByType.push({ type: itemType, items: types[itemType] });
        }
        return itemsByType;
    }

    function setLocalStorage() {
        localStorage.setItem("quickReturn", JSON.stringify(_returnItemArray));
    }

    function getLocalStorage() {
        _returnItemArray = JSON.parse(localStorage.getItem("quickReturn"));
        if (_returnItemArray === null) {
            _returnItemArray = new Array();
        }
    }

    return {
        addQuickReturn: addQuickReturn,
        getQuickReturns: getQuickReturns
    };

}();
