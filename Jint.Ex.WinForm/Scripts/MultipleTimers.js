var secondCounter       = 0;
var halfSecondCounter   = 0;

function MessageEverySecond() {

    secondCounter++;
    setUserMessage("Every second " + secondCounter, false);
}
setInterval(MessageEverySecond, 1000);

function MessageHalfSecond() {

    halfSecondCounter++;
    setUserMessage("Every half second " + halfSecondCounter, false);
}
setInterval(MessageHalfSecond, 500);
