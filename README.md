# NefariusGame

## Build & Publish to raspberrypi

cd NefariusWebApp

dotnet publish --configuration Release --framework netcoreapp2.1 --runtime debian-arm

## Copy "publish" folder to raspberrypi

cd publish

sudo chmod 777 ./NefariusWebApp

## Run
./NefariusWebApp
