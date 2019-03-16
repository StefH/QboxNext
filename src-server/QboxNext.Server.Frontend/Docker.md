## Docker

### Build Docker for "Linux"

Go to the root from this solution

#### 1. Build

``` cmd
docker build -t sheyenrath/qboxnext-frontend --build-arg BuildNumber=preview-01 -f .\src-server\QboxNext.Server.frontend\Dockerfile . 
```

#### 2. Tag

``` cmd
docker tag sheyenrath/qboxnext-frontend-nanoserver-1803:latest sheyenrath/qboxnext-frontend:1.0.2
```

#### 3. Push

``` cmd
docker push sheyenrath/qboxnext-frontend:latest
docker push sheyenrath/qboxnext-frontend:1.0.2
```

#### 4. Run

``` cmd
docker run -it --rm --env-file C:\Users\StefHeyenrath\Documents\GitHub\qboxnext-env.txt -p 4200:80 sheyenrath/qboxnext-frontend:latest
```


### Build Docker for "Windows : nanoserver-1803"

Go to the root from this solution

#### 1. Build

``` cmd
docker build -t sheyenrath/qboxnext-frontend-nanoserver-1803 --build-arg BuildNumber=preview-01 -f .\src-server\QboxNext.Server.frontend\Dockerfile.nanoserver-1803 .
```

#### 2. Tag

``` cmd
docker tag sheyenrath/qboxnext-frontend-nanoserver-1803:latest sheyenrath/qboxnext-frontend-nanoserver-1803:1.0.2
```

#### 3. Push

``` cmd
docker push sheyenrath/qboxnext-frontend-nanoserver-1803:latest
docker push sheyenrath/qboxnext-frontend-nanoserver-1803:1.0.2
```

#### 4. Run

``` cmd
docker run -it --rm --env-file C:\Users\StefHeyenrath\Documents\GitHub\qboxnext-env.txt -p 4200:80 sheyenrath/qboxnext-frontend-nanoserver-1803:latest
```


##### Cleanup all docker images and containers
Use this command:
``` cmd
docker system prune -a
```
