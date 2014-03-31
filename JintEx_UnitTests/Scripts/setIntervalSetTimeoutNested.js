var counter = 0;

function f1() {

    counter++;
    f1.counter++;
    if(f1.counter == 4)
        clearInterval(f1.callBackId);

    function f2() {
        counter++;
    }
    var timeout2 = setTimeout(f2, 10);
}
f1.counter = 0;
f1.callBackId = setInterval(f1, 20);
