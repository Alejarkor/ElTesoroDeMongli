# Environment config assets

Create these assets from Unity Editor:

```text
Right click in this folder
Create -> El Tesoro de Mongli -> Config -> Environment Config
```

Recommended assets:

```text
LocalhostEnvironmentConfig.asset
LocalNetworkEnvironmentConfig.asset
ProductionEnvironmentConfig.asset
```

## Localhost

```text
Environment Type: Localhost
API Base URL: http://localhost/ElTesoroDeMongliAPI/
Game Server Host: localhost
Game Server Port: 7778
Use Secure WebSocket: false
```

## Local network

Replace `192.168.1.X` with the Windows PC LAN IP.

```text
Environment Type: LocalNetwork
API Base URL: http://192.168.1.X/ElTesoroDeMongliAPI/
Game Server Host: 192.168.1.X
Game Server Port: 7778
Use Secure WebSocket: false
```

## Production

```text
Environment Type: Production
API Base URL: https://your-domain.example/ElTesoroDeMongliAPI/
Game Server Host: your-domain.example
Game Server Port: 443
Use Secure WebSocket: true
```

Assign the selected config to an `EnvironmentConfigProvider` GameObject in the boot/client scene.
