# Isolde's Adventure

This is a text-based adventure I wrote to learn C#. There are various improvements I could make, but I'm happy with the way it turned out, and I think it's best to keep moving forward onto other projects.

# Quick Start

## Run with Docker

### Build from source

```
docker build -t isolde-adventure .
docker run -it isolde-adventure
```

### Run from Docker Hub

```
docker pull stuartleeallen/isolde:latest
docker run -it stuartleeallen/isolde:latest
```

## Run with .NET

```
cd src
dotnet run
```
