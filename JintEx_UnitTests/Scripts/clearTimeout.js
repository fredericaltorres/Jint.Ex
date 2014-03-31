var counter = 0;

function f3() {
    print('Hi 3');
    counter++;
}
var timeout3 = setTimeout(f3, 30);
clearTimeout(timeout3);
