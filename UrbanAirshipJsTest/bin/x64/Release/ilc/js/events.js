var ev;

function events1() {
    ev = new RuntimeComponentTest.Eventful();
    ev.addEventListener("testtest", function (e) {
        document.getElementById('output').innerHTML = e.value1;
        document.getElementById('output').innerHTML += "<br/>" + e.value2;
    });
    ev.onTest("Number of feet in a mile", 5280);
}
var events1Button = document.getElementById("events1Button");
events1Button.addEventListener("click", events1, false);