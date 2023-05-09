var categoryList = [];
var topicParameter = null;
var categoryParameter = null;

// on document load vanilla js
document.addEventListener('DOMContentLoaded', () => {

  // for rockData filter to unique CategoryIds
  const uniqueCategoryIds = [...new Set(rockData.map(item => item.CategoryId))];
  // for unique CategoryIds output to html with CategoryName and CategoryGuid
  categoryList = uniqueCategoryIds.map(categoryId => {
    const category = rockData.find(item => item.CategoryId === categoryId);
    return {
      CategoryName: category.CategoryName,
      CategoryGuid: category.Category,
      TopicIds: rockData.filter(item => item.CategoryId === categoryId).map(item => item.TopicId)
    };
  });

});


document.addEventListener('content-collection-view-full-search', () => {
  // console.log('content-collection-view-full-search');
  var urlParams = new URLSearchParams(window.location.search);
  var selectedCategory = urlParams.get('category');
  var selectedTopicIds = urlParams.get('new topic');

  console.log("Category: " + categoryParameter + " : " + selectedCategory);
  console.log("Topics: " + topicParameter + " : " + selectedTopicIds);

  if ((!topicParameter && selectedTopicIds) || topicParameter !== selectedTopicIds) {
    console.log("Topic Primary Change");
    topicChange(selectedTopicIds, categoryList);
    topicParameter = selectedTopicIds;
  }

  if ((!categoryParameter && selectedCategory) || categoryParameter !== selectedCategory) {
    console.log("Category Primary Change");
    categoryChange(selectedCategory, categoryList);
    categoryParameter = selectedCategory;
  }
});



function categoryChanges(selectedCategory, categoryList) {
  console.log("categoryChange");
  // get categoryList index of selectedCategory
  const categoryIndex = categoryList.findIndex(category => category.CategoryName === selectedCategory);
  // if categoryIndex is not -1 then set selectedTopicIds to categoryList[categoryIndex].TopicIds
  if (categoryIndex !== -1) {
    selectedTopicIds = categoryList[categoryIndex].TopicIds;
    var filterText = categoryList[categoryIndex].CategoryName;

    // add class .inactive to all .checkbox-inline elements inside .filter-new-topic where the value of the input is not in selectedTopicIds
    const topicIds = document.querySelectorAll('.filter-new-topic .radio-inline input');
    topicIds.forEach(topicId => {
      if (!selectedTopicIds.includes(parseInt(topicId.value)) && topicId.value !== '') {
        topicId.parentElement.classList.add('hidden');
      } else {
        topicId.parentElement.classList.remove('hidden');
      }
    });
  } else {
    const topicIds = document.querySelectorAll('.filter .radio-inline');
    topicIds.forEach(topicId => {
      topicId.classList.remove('hidden');
    });
  }

  //updatePageTitle(filterText);
  //clearTopicFilters();
  categoryParameter = selectedCategory;
}

function topicChange(selectedTopicId, categoryList) {
  console.log("topicChange");
  // get rockData index of selectedCategory
  const rockDataIndex = rockData.findIndex(rock => rock.TopicId === parseInt(selectedTopicId));
  // if rockDataIndex is not -1 then set selectedRock to rockData[rockDataIndex]
  if (rockDataIndex !== -1) {
    selectedRock = rockData[rockDataIndex];

    //categoryParameter = selectedRock.CategoryName;
    const categoryInputs = document.querySelectorAll('.filter-category .radio-inline input');
    categoryInputs.forEach(categoryId => {
      if (categoryId.value !== selectedRock.CategoryName && categoryId.value !== '') {
        categoryId.parentElement.classList.add('hidden');
      } else {
        categoryId.parentElement.classList.remove('hidden');
        if (categoryId.checked === false) {
          categoryId.checked = true;
          updatePageTitle(selectedRock.CategoryName);
        }
      }
    });

    if (true) {
      const categoryIndex = categoryList.findIndex(category => category.CategoryName === selectedRock.CategoryName);
      if (categoryIndex !== -1) {
        var topicList = categoryList[categoryIndex].TopicIds;
        console.log("topicList: " + topicList);

        var topicInputs = document.querySelectorAll('.filter-new-topic .radio-inline input');
        topicInputs.forEach(topicInput => {
          console.log(topicInput.value + " " + topicList.includes(parseInt(topicInput.value)));
          if (topicList.includes(parseInt(topicInput.value)) && topicInput.value !== '') {
            topicInput.parentElement.classList.add('hidden');
          } else {
            topicInput.parentElement.classList.remove('hidden');
          }
        });

      } 
    }
  }
}

function clearTopicFilters() {
  const firstTopicId = document.querySelector('.filter-new-topic .radio-inline:not(.hidden) input');
  firstTopicId.checked = true;
  firstTopicId.dispatchEvent(new Event('change'));
}

function updatePageTitle(filterText) {
  var pageTitle = document.querySelector('#page-title');
  var pageOverview = document.querySelector('.pageoverview-description');

  pageTitle.innerText = filterText ? `Discover ${filterText}` : 'Discover';
  pageOverview.innerText = filterText ? `Browse content and topics around ${filterText}.` : 'Browse content and topics.';
}


// use push date to add or update parameter on the current url
function updateUrlParameter(key, value) {
  // get current url
  var url = window.location.href;
  // get current url parameters
  var urlParams = new URLSearchParams(window.location.search);
  // set parameter key to value
  urlParams.set(key, value);
  // update url with new parameters
  window.history.pushState({}, '', `${window.location.pathname}?${urlParams.toString()}`);
}