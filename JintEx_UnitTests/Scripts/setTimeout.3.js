var counter = 0;

function f3() {
    print('Hi 3');
    counter++;
}
var timeout3 = setTimeout(f3, 300);
print(timeout3);

function f2() {
    print('Hi 2');
    counter++;
}
var timeout2 = setTimeout(f2, 200);
print(timeout2);

function f1() {
    print('Hi 1');
    counter++;
}
var timeout1 = setTimeout(f1, 100);
print(timeout1);

clearTimeout(timeout3);
print('timeout3 cleared');