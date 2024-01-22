document.addEventListener("DOMContentLoaded", function () {
  // Counters
  new PureCounter({
    selector: ".purecounter",
    duration: 2,
    delay: 10,
    once: true,
    repeat: false,
    legacy: true,
    currency: false,
    separator: true,
  });

  // Give Controls
  const giveInput = $(".js-give-input"),
    giveLinks = $(".js-give-link"),
    giveButton = $(".js-give-btn"),
    totalGiven = $(".js-total-given"),
    totalGivenAmount = parseInt(totalGiven.data("purecounter-end")),
    debtRemaining = $(".js-debt-remaining"),
    debtRemainingAmount = parseInt(debtRemaining.data("purecounter-end")),
    progressBar = $(".js-progress-bar"),
    percentGiven = progressBar[0].getAttribute("aria-valuenow"),
    progressBarLabel = $(".js-progress-bar-label");

  $(giveInput).keyup(function (e) {
    let giveAmount = giveInput.val(),
      updatedTotalGivenAmount = totalGivenAmount + parseInt(giveAmount),
      updatedDebtRemainingAmount = debtRemainingAmount - parseInt(giveAmount),
      updatedPercentGiven = Math.floor(
        (updatedTotalGivenAmount / debtRemainingAmount) * 100
      );

    // don't let the debt remaining go negative
    if (updatedDebtRemainingAmount < 0) {
      updatedDebtRemainingAmount = 0;
    }

    // don't let the updatedPercentGiven go over 100
    if (updatedPercentGiven > 100) {
      updatedPercentGiven = 100;
    }

    if (giveAmount >= 1) {
      // If there's an amount, update the values to count to for purecounter
      $(totalGiven).attr("data-purecounter-end", updatedTotalGivenAmount);
      $(debtRemaining).attr("data-purecounter-end", updatedDebtRemainingAmount);
      $(giveButton).removeAttr("disabled");

      // Update progress bar width
      progressBar.css("width", updatedPercentGiven + "%");
      progressBarLabel.text(updatedPercentGiven + "%");
    } else {
      // If there isn't an amount, update the values to count back to their original state
      $(totalGiven).attr("data-purecounter-end", totalGivenAmount);
      $(debtRemaining).attr("data-purecounter-end", debtRemainingAmount);
      $(giveButton).attr("disabled", "true");

      // Update progress bar width
      progressBar.css("width", percentGiven + "%");
      progressBarLabel.text(percentGiven + "%");
    }

    // make 'em count
    new PureCounter({
      selector: ".purecounter",
      duration: 20,
      delay: 10,
      once: true,
      repeat: false,
      legacy: true,
      // filesizing: false,
      currency: false,
      separator: true,
    });
  });

  // When give links are clicked, focus on input (this only works on desktop)
  giveLinks.on("click", function (e) {
    e.preventDefault();

    setTimeout(function () {
      $(giveInput).focus();
    }, 500);
  });

  // give button click handler
  $(giveButton).on("click", function (e) {
    e.preventDefault();
    let giveAmount = giveInput.val();
    let newLocation =
      "https://newspring.cc/give/now?AccountIds=307^" +
      giveAmount +
      "&utm_source=Overflow%202022%20Page&utm_medium=Hero%20CTA%20-%20Give%20Now&utm_campaign=Overflow%202022";

    // send browser to give page with amount populated
    window.location = newLocation;
  });

  // Watch bar graph and animate when in view
  var observer = new IntersectionObserver(
    function (entries) {
      if (entries[0].isIntersecting === true) {
        anime({
          targets: ".bar",
          duration: 800,
          height: function (el, i, l) {
            return el.dataset.amount + "%";
          },
          delay: anime.stagger(100), // increase delay by 100ms for each elements.
        });
      }
    },
    { threshold: [1] }
  );

  observer.observe(document.querySelector("#chart"));

  // AOS
  AOS.init({
    once: false,
    duration: 500,
    easing: "ease-in-out-cubic",
    // This disables AOS on mobile chrome because it's dumb
    disable: navigator.userAgent.includes("CriOS"),
  });
});
