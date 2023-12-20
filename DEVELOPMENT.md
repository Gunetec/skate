# Development Guide

This page contains steps to setup Skateboard3Server for local development.

## Requirements

* [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* [RPCS3](https://rpcs3.net/)
* PS3 copy of Skate 3
* Optional but recommended is Visual Studio with ASP.NET Core workload

## Running

* Create a copy of `src/Skateboard3Server.Host/appsettings.json` as `src/Skateboard3Server.Host/appsettings.Development.json`
  * This `appsettings.Development.json` will not be committed to the repo, so you can make local config changes here
* Setup/Create RPCN account in RPCS3 (if you have already done this, skip this step)
* Setup Skate 3 in RPCS3 to redirect to your local instance. In the `Network` tab of the Custom Configuration for Skate 3:
  * Set Network Status to `Connected`
  * Set PSN Status to `RPCN`
  * Keep DNS as `8.8.8.8`
  * Set IP/Host Switches to `gosredirector.ea.com=127.0.0.1&&downloads.skate.online.ea.com=127.0.0.1&&gosredirector.stest.ea.com=127.0.0.1&&downloads.skate.test.online.ea.com=127.0.0.1`

### Docker

`docker compose up --build`

### Windows

Open `Skateboard3Server.sln` in Visual Studio and set `Skateboard3Server.Host` as the startup project and hit Build & Run

### Linux

Set `Kestrel.Endpoints.Web.Url` in `appsettings.Development.json` or env var `KESTREL__ENDPOINTS__WEB__URL` to `http://*:8080`

Redirect traffic from port 80 to 8080 by running: `sudo iptables -t nat -A OUTPUT -o lo -p tcp --dport 80 -j REDIRECT --to-port 8080`

Then you can run `dotnet run --project src/Skateboard3Server.Host/Skateboard3Server.Host.csproj` to start the server

## Repository projects

### Skateboard3Server.Blaze

All Blaze handlers/events live here

### Skateboard3Server.Web

All the required web parts (SkateFeed, SkateProfile, etc)

### Skateboard3Server.Common

Stuff used by both Skateboard3Server.Blaze and Skateboard3Server.Web

### Skateboard3Server.Data

Database project (includes migrations and models)

### Skateboard3Server.Host

Sets up hosting for Blaze and Web, logging, dependcy injection, etc (this is the main entrypoint for the server)

