var counter = 0;

function f1() {

    counter++;

    function f2() {        

        counter++;
    }
    setTimeout(f2, 20);    
}
setTimeout(f1, 10);
