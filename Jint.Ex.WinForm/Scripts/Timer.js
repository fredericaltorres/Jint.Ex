function addUserMessage() {

    setUserMessage("" + (new Date()).toLocaleTimeString(), true);
}
setInterval(addUserMessage, 1000);
