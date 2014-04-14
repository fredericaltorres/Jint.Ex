function f1() {
    print('Hi 1 '+(new Date()));
}
var timeout1 = setInterval(f1, 1000);
print(timeout1);

function f2() {
    print('Hi 2 '+(new Date()));
}
var timeout2 = setInterval(f2, 3000);
print(timeout2);
