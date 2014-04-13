function addUserMessage() {

    setUserMessage("" + (new Date()).toLocaleTimeString());
}
setInterval(addUserMessage, 1000);
