#!/bin/bash

echo ##################################################################
echo Installing Docker
echo ##################################################################

# Installing Docker initially
curl -fsSL get.docker.com -o get-docker.sh && sh get-docker.sh

# Docker without sudo (remember to signout using "logout")
sudo groupadd docker

# Workaround for broken Docker version
sudo apt-get install docker-ce=18.06.1~ce~3-0~raspbian

echo ##################################################################
echo Installing Docker Compose
echo ##################################################################

# Install pip to get Docker Compose
sudo apt-get install -y python python-pip

# Install Docker Compose
sudo pip install docker-compose

echo ##################################################################
echo Docker and Docker Compose installed
echo ##################################################################