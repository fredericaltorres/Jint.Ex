
print("Starting Script2.js");

function addUserMessage() {

    setUserMessage("string async " + (new Date()));
}

setInterval(addUserMessage, 1000);