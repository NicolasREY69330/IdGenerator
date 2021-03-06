# Id generator

1. Based on Mongodb official tutorial https://www.mongodb.com/docs/v3.0/tutorial/create-an-auto-incrementing-field/

    Here are basically the same approach with both a .net 6 application and a Go application exposing controllers returning the next auto incremented id.

    Each app has it's own counter in MongoDb (available with Mongo Express here http://localhost:8081/db/orderid_sequence/sequence)

    We suppose you already have the latest .net 6 runtime installed, as well as a valid Go setup environment. The Go project should be placed into your %GOPATH%.

2. Run the infrastructure stack, next to the docker-compose file and from a Terminal :
    ```properties
    docker-compose up -d
    ```

3. To launch the .net application, go into the .net folder and from a Terminal
    ```properties
    dotnet run --configuration Release
    ```

4.  You can reach the .net urls here
    ```properties
    # standard hello world
    http://localhost:8001/api/hello

    # generating next counter from MongoDb (http://localhost:8081/db/orderid_sequence/sequence/"globalcsharp")
    http://localhost:8001/api/getNextCounter

    # generating next counter from MongoDb using semaphore to throttle threads, 100 at the same time (http://localhost:8081/db/orderid_sequence/sequence/"globalcsharp")  
    http://localhost:8001/api/Semaphore/GenerateNextSequence
    ```

5. To launch the Go application, go into the Go folder and from a Terminal
    ```properties
    go run go
    ```
6.  You can reach the Go urls here
    ```properties
    # standard hello world
    http://localhost:8000/api/hello

    # generating next counter from MongoDb (http://localhost:8081/db/orderid_sequence/sequence/"globalgo")
    http://localhost:8000/api/getNextCounter
    ```
7. To make load test, I use Baton (https://github.com/americanexpress/baton). Typically we can put the apps under http call pressure with the following

    ```properties
    baton -z XXX.csv  -c XXX -r XXX
    ```

    Example 
    ```properties
    baton -z helloworldCsharp.csv  -c 500 -r 10000
    baton -z idgeneratorCsharp.csv  -c 500 -r 10000
    baton -z idgeneratorCsharpSemaphore.csv  -c 500 -r 10000

    baton -z helloworldgo.csv  -c 500 -r 10000
    baton -z idgeneratorgo.csv  -c 500 -r 10000
    ```

8. The .net driver throws `MongoWaitQueueFullException` when it's under http hard call pressure
    ```properties
    # Launch several instance of baton simultaneously
    baton -z idgeneratorCsharp.csv  -c 200 -r 8000
    baton -z idgeneratorCsharp.csv  -c 200 -r 8000
    baton -z idgeneratorCsharp.csv  -c 200 -r 8000
    baton -z idgeneratorCsharp.csv  -c 200 -r 8000
    ```
    It basically indicates that the internal threads queue maintained by the driver as reached it's maximum. Obviously we can tweak some settings on the client (ie `MinConnectionPoolSize`, `MaxConnectionPoolSize`, `MaxConnecting`, `WaitQueueTimeout` ... ) but the only one that can have an impact on it is `WaitQueueSize` which is deprecated and 
    which will disapear on next releases.

    The solution appears to handling manually thread throttling before sending request to MongoDb through the driver. It's done in the `SequenceIncrementorService` using SemaphoreSlim implementation. It works pretty well, so under http hard call pressure, the exception is gone and the driver performs similarly like the Go driver.

    ```properties
    # Launch several instance of baton simultaneously
    baton -z idgeneratorCsharpSemaphore.csv  -c 200 -r 8000
    baton -z idgeneratorCsharpSemaphore.csv  -c 200 -r 8000
    baton -z idgeneratorCsharpSemaphore.csv  -c 200 -r 8000
    baton -z idgeneratorCsharpSemaphore.csv  -c 200 -r 8000
    ```