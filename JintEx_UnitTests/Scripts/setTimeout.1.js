var counter = 0;

function f1() {
    print('Hi 1');
    counter++;
}
var timeout1 = setTimeout(f1, 100);
