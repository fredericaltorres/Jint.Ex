var counter = 0;

function f1() {

    counter++;

    function f2() {        

        counter++;
        function f3() {        

            counter++;
        }
        setTimeout(f3, 10);
    }
    setTimeout(f2, 10);
}
setTimeout(f1, 10);
