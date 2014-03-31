var counter = 0;

function f1() {
    counter++;
    f1.counter++;
    print('Hi 1 - '+f1.counter);
    if (f1.counter == 4)
        clearInterval(f1.timeout);
}
f1.counter = 0;
f1.timeout = setInterval(f1, 50);
clearInterval(f1.timeout);

