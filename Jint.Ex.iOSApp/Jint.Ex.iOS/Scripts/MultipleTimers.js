var secondCounter       = 0;
var halfSecondCounter   = 0;

function MessageEverySecond() {

    secondCounter++;
    setUserMessage("Every second " + secondCounter);
}
setInterval(MessageEverySecond, 1000);

function MessageHalfSecond() {

    halfSecondCounter++;
    setUserMessage("Every half second " + halfSecondCounter);
}
setInterval(MessageHalfSecond, 500);
