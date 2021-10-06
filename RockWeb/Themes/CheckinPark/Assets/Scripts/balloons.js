function randomBalloon(num) {
    return Math.floor(Math.random() * (num - 1) + 1)
}

function getRandomStyles() {
    var c = randomBalloon(6);
    var mt = randomBalloon(200);
    var ml = randomBalloon(50);
    var dur = randomBalloon(4) + 4;
    return `
background-color: var(--celebration-${c});
color: var(--celebration-${c});
margin: ${mt}px 0 0 ${ml}px;
animation: float ${dur}s ease-in
`
}

function createBalloons(num) {
    document.body.innerHTML += '<div id="balloon-container"></div>';
    var balloonContainer = document.getElementById("balloon-container")
    for (var i = num; i > 0; i--) {
        var balloon = document.createElement("div");
        balloon.className = "balloon";
        balloon.style.cssText = getRandomStyles();
        balloonContainer.append(balloon);
    }
}
