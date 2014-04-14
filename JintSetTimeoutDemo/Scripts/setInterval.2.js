function f1() {
    f1.counter++;
    print('Hi 1 - '+f1.counter);
    if (f1.counter == 4)
        clearInterval(f1.timeout);
}
f1.counter = 0;
f1.timeout = setInterval(f1, 500);

function f2() {
    f2.counter++;
    print('Hi 2 - '+f2.counter);
    if (f2.counter == 4)
        clearInterval(f2.timeout);
}
f2.counter = 0;
f2.timeout = setInterval(f2, 1000);

