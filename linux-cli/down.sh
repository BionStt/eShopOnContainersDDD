#!/bin/bash

docker-compose -f ../docker-compose.yml -f ../docker-compose.override.yml down

docker rm $(docker ps -a -q)
docker rmi $(docker images -q -f dangling=true)
docker volume rm $(docker volume ls -qf dangling=true)

docker rmi eshop/elastic
docker rmi eshop/mongodb
docker rmi eshop/presentation
docker rmi eshop/domain
