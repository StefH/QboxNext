## Docker

### Build Docker for "Windows : nanoserver-1803"

Go to the root from this solution

#### 1. Build

``` cmd
docker build -t sheyenrath/qboxnext-datareceiver-nanoserver-1803 -f .\src-server\QboxNext.Server.DataReceiver\Dockerfile.nanoserver-1803 .
```

#### 2. Tag

``` cmd
docker tag sheyenrath/qboxnext-datareceiver-nanoserver-1803:latest sheyenrath/qboxnext-datareceiver-nanoserver-1803:1.0.2
```

#### 3. Push

``` cmd
docker push sheyenrath/qboxnext-datareceiver-nanoserver-1803:latest
docker push sheyenrath/qboxnext-datareceiver-nanoserver-1803:1.0.2
```

### Build Docker for "Linux"

Go to the root from this solution

#### 1. Build

``` cmd
docker build -t sheyenrath/qboxnext-datareceiver -f .\src-server\QboxNext.Server.DataReceiver\Dockerfile .
```

#### 2. Tag

``` cmd
docker tag sheyenrath/qboxnext-datareceiver:latest sheyenrath/qboxnext-datareceiver:1.0.2
```

#### 3. Push

``` cmd
docker push sheyenrath/qboxnext-datareceiver:latest
docker push sheyenrath/qboxnext-datareceiver:1.0.2
```


##### Cleanup all docker images and containers
Use this command:
``` cmd
docker system prune -a
```