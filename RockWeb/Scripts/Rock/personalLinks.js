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

        // Check if the return items already has this extact entry, if so remove it. This allows the new item to be at the top
        removeDuplicates(returnItem);

        // Add new item
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

    function removeDuplicates(newReturnItem) {

        for (var i = 0; i < _returnItemArray.length; i++) {
            if (returnItemIsEqual(_returnItemArray[i], newReturnItem)) {
                _returnItemArray.splice(i, 1);
            }
        }
    }

    // Determines if the two return items are the same item
    function returnItemIsEqual(returnItemA, returnItemB) {
        var isEqual = true;

        if (returnItemA.type != returnItemB.type) {
            isEqual = false;
        }

        if (returnItemA.typeOrder != returnItemB.typeOrder) {
            isEqual = false;
        }

        if (returnItemA.itemName != returnItemB.itemName) {
            isEqual = false;
        }

        if (returnItemA.url != returnItemB.url) {
            isEqual = false;
        }
        return isEqual;
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
