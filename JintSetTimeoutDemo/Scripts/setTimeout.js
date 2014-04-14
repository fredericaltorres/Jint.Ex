function f3() {
    print('Hi 3');
}
var timeout3 = setTimeout(f3, 3000);
print(timeout3);

function f2() {
    print('Hi 2');
}
var timeout2 = setTimeout(f2, 2000);
print(timeout2);

function f1() {
    print('Hi 1');
}
var timeout1 = setTimeout(f1, 1000);
print(timeout1);

clearTimeout(timeout3);
print('timeout3 cleared');