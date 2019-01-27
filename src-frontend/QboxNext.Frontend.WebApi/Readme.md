# QboxNext.Frontend.WebApi

## Docker

### Build Docker for "Windows : nanoserver-1803"

Go to the root from this project

#### 1. Build

``` cmd
docker build -t sheyenrath/qboxnext-webapi-nanoserver-1803 -f .\src-frontend\QboxNext.Frontend.WebApi\Dockerfile.nanoserver-1803 .
```

#### 2. Tag

``` cmd
docker tag sheyenrath/qboxnext-webapi-nanoserver-1803:latest sheyenrath/qboxnext-webapi-nanoserver-1803:1.0.2
```

#### 3. Push

``` cmd
docker push sheyenrath/qboxnext-webapi-nanoserver-1803:latest
docker push sheyenrath/qboxnext-webapi-nanoserver-1803:1.0.2
```

### Build Docker for "Linux"

Go to the root from this project

#### 1. Build

``` cmd
docker build -t sheyenrath/qboxnext-webapi -f .\src-frontend\QboxNext.Frontend.WebApi\Dockerfile .
```

#### 2. Tag

``` cmd
docker tag sheyenrath/qboxnext-webapi-nanoserver-1803:latest sheyenrath/qboxnext-webapi:1.0.2
```

#### 3. Push

``` cmd
docker push sheyenrath/qboxnext-webapi:latest
docker push sheyenrath/qboxnext-webapi:1.0.2
```