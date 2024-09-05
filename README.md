docker build . -t gfms-monkey
docker run -p 80:80 --env=API_KEY=ApiKey_3592 gfms-monkey