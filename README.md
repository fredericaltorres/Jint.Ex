# Jint.Ex - Alpha

## Overview

Jint.Ex is a C# library adding new features to the JavaScript runtime ***[Jint](https://github.com/sebastienros/jint)***.
The main goal is to be able to integrate Jint with UI SDK like 

1. .NET WinForm in Windows
2. UIKit SDK on iOS with the Xamarin stack 
3. Android too with Xamarin stack
4. WinPhone

Integrating with these UI SDKs (1 and 2 are my primary goals) requires the following:

* Building non blocking UI
    * Script are executed in a background thread with ability to access UI (forms and controls)
* Abililty to build true C# asynchronous API interacting with JavaScript code
* Ability to call setTimeout() and setInterval()

Jint.Ex implement the concept of Event Loop, to execute scripts and callback functions.
Only JavaScript script or callback is executed at a specific moment.

## Features

### setTimeout and clearTimeOut
The methods setTimeout() and clearTimeOut() are not part of the JavaScript language,
but part of the DOM. Nevertheless they can be usefull and sometime necessary
to run some JavaScript libraries written for the browser.
These methods are also present in NodeJs.

#### Javascript

```javascript
function f3() {
    print('Hi 3');
}
var timeout3 = setTimeout(f3, 3000);
    
function f1() {
    print('Hi 1');
}
var timeout1 = setTimeout(f1, 1000);

clearTimeout(timeout3);
print('timeout3 cleared');
```

#### CSharp

```csharp
static void SetIntervalDemo()
{
    Console.WriteLine("Jint setInterval() demo");
    
    AsyncronousEngine.EmbedScriptAssembly = Assembly.GetExecutingAssembly();
    AsyncronousEngine.RequestExecution("setIntervalSetTimeout.js");   
    AsyncronousEngine.Wait(); // Wait util all events are processed
    AsyncronousEngine.Stop(); // Stop the event loop

    Console.WriteLine("*** Done ***");
    Console.ReadKey();
}
```

### setInterval and clearInterval

#### Javascript

```javascript
function f1() {
    print('Hi 1 '+(new Date()));
}
var timeout1 = setInterval(f1, 1000);

function f2() {
    print('Hi 2 '+(new Date()));
}
var timeout2 = setInterval(f2, 3000);
```

### Implementing Custom Asynchronous Api

Jint allows to expose C# methods, class and singleton object to the JavaScript world. With Jint.Ex
you can implement true asynchronous method, like the method read() of the singleton object storage in sample below.

```javascript
var s = null;
storage.read(function(data) {
    s = data;
});
```

The method read() starts a background thread and returns right away. The thread will execute the read
operation and then request the execution of the call back function using the Jint.Ex event loop.
when the Jint.Ex event loop will reach the event it will execute the call back function.

For more information see blog: xxxxxxxxxxxxxxxxx.

### Jint.Ex Api

#### The AsyncronousEngine class.
The class AsyncronousEngine allows to run JavaScript script in a background thread
and supports interaction with UI and the execution of the asynchronous events.

```csharp

public static AsyncronousEngine {

    /// <summary>
    /// The instance of Jint
    /// </summary>
    public static Jint.Engine Engine = null;

    /// <summary>
    /// Reference the assembly that embed the JavaScript scripts.
    /// </summary>
    public static Assembly EmbedScriptAssembly = null;

    /// <summary>
    /// Load a file from the file system or as an embed resource
    /// </summary>
    /// <param name="name"></param>
    /// <param name="source"></param>
    public static void LoadScript(string name, StringBuilder source)

    /// <summary>
    /// Start the event loop
    /// </summary>
    /// <returns></returns>
    public static bool Start();

    /// <summary>
    /// RequestExecution the execution of one javaScript script by the event loop. 
    /// The method returns right away. 
    /// Start the AsyncronousEngine if needed.
    /// </summary>
    /// <param name="fileName">The filename or resource name to load and execute</param>
    /// <param name="block">If true after the execution, block until the event queue is empty</param>
    public static bool RequestExecution(string fileName, bool block = false);

    /// <summary>
    /// Kill the event loop
    /// </summary>
    public static void Kill();

    /// <summary>
    /// Stop the event loop
    /// </summary>
    public static void Stop();

    /// <summary>
    /// Wait until the event queue is empty
    /// </summary>
    public static void Wait();

    /// <summary>
    /// Clear the event queue
    /// </summary>
    public static void ClearQueue();

    /// <summary>
    /// Request the execution of a JavaScript callback function. This method should be called by 
    /// C# custom object that want to implement asynchronous api.
    /// </summary>
    /// <param name="callBackFunction"></param>
    /// <param name="parameters"></param>
    public static void RequestCallbackExecution(Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue> callBackFunction, List<JsValue>  parameters);

}
```