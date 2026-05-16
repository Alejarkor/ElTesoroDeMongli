# Project architecture review - ElTesoroDeMongli

Auditoría enfocada en la arquitectura general del proyecto, separación por sistemas y propuesta de reorganización progresiva.

## 1. Resumen ejecutivo

El proyecto tiene pinta de haber evolucionado como prototipo: funciona por acumulación de escenas, paquetes, scripts, endpoints y pruebas. Eso no es malo para validar una idea, pero ahora mismo dificulta:

- entender qué sistema hace qué;
- actualizar Unity/Mirror/API;
- separar cliente WebGL, servidor de juego y API;
- probar iOS/Android/navegadores;
- evitar bugs por dependencias cruzadas;
- escalar el juego con más sistemas.

Mi recomendación no es reescribirlo entero. La mejor ruta es una reorganización progresiva por capas y sistemas, manteniendo funcionalidad.

## 2. Estado detectado

### Juego Unity

- Unity `2021.3.4f1`.
- Escena activa de build: `Assets/Scenes/MONGLICLIENT.unity`.
- Hay escenas desactivadas de pruebas/servidor: `GameScene`, `CharacterControllerTest`, `MONGLISERVER`, `Empty` y una sample del Input System.
- Paquetes principales: Addressables, Cinemachine, Input System, TextMeshPro, Timeline.
- Mirror está embebido dentro de `Assets/Mirror`.
- Existe `SimpleWebTransport`, con soporte `ws` y `wss`, importante para WebGL/navegador.
- También hay paquetes/terceros dentro de `Assets`, como `Character Movement Fundamentals` e `IngameDebugConsole`.

### API/backend

Repo analizado:

```text
sergio4anso/ElTesoroDeMongliAPI
```

Stack detectado:

```text
PHP procedural
MySQL/MariaDB
PHPMailer
Endpoints por carpeta con index.php
```

Endpoints vistos:

```text
/register
/login
/validation
/get_users
/update_users
/createActivationToken
/activationMail
```

La API actualmente mezcla:

```text
HTTP request handling
validación
SQL
modelo de datos
respuesta JSON
lógica de token
correo
```

en los mismos scripts.

## 3. Problema principal de arquitectura

El proyecto no está dividido por responsabilidades. Está dividido por procedencia histórica:

```text
Assets/Mirror
Assets/Character Movement Fundamentals
Assets/Scenes
Assets/Animation
Assets/Models
Assets/Plugins
API endpoints sueltos
```

Eso hace difícil saber dónde vive realmente cada sistema:

```text
Auth
Perfil usuario
Estado jugador
Input
Movimiento
Animación
Audio
Networking
Spawn
Persistencia
WebGL/browser compatibility
```

El objetivo debería ser que cada sistema tenga un sitio claro y pocas dependencias.

## 4. Arquitectura objetivo recomendada

### Vista global

```text
Browser WebGL Client
        |
        | HTTP/JSON
        v
PHP API / future API service ----> MySQL/MariaDB
        |
        | ws/wss
        v
Unity/Mirror Game Server
```

Separación conceptual:

```text
API: login, registro, tokens, persistencia, datos lentos.
Game Server: sesión en tiempo real, jugadores conectados, autoridad/sync.
WebGL Client: input, presentación, UI, predicción/interpolación.
Database: usuarios, tokens, progreso, estado persistente.
```

No conviene usar la base de datos como sincronización principal de movimiento en tiempo real. Puede servir para persistir estado, pero no para actualizar posiciones a alta frecuencia.

## 5. Estructura propuesta para Unity

Estructura gradual:

```text
Assets/
  _Project/
    Scenes/
      Boot/
      Client/
      Server/
      Tests/
    Scripts/
      Core/
      Config/
      Infrastructure/
      Gameplay/
      Networking/
      UI/
      Audio/
      WebGL/
      Diagnostics/
    Prefabs/
      Player/
      UI/
      Network/
    ScriptableObjects/
      Config/
      Gameplay/
    Art/
      Characters/
      Environment/
      Materials/
      Shaders/
    Addressables/
  _ThirdParty/
    Mirror/
    CharacterMovementFundamentals/
    IngameDebugConsole/
```

Importante: mover assets dentro del editor de Unity para conservar GUIDs. No mover carpetas grandes a mano desde el explorador salvo que se controle muy bien.

## 6. Capas recomendadas en cliente Unity

```text
Presentation/UI
    ↓
Application / Use Cases
    ↓
Gameplay Domain
    ↓
Infrastructure adapters
```

Ejemplo práctico:

```text
LoginScreen
  -> AuthService
      -> IAuthApiClient
          -> UnityWebRequestAuthApiClient
```

Movimiento:

```text
IPlayerInputSource
  -> PlayerMovementController
      -> PlayerState
          -> PlayerAnimationPresenter
          -> PlayerAudioPresenter
          -> PlayerNetworkSync
```

Regla sana:

```text
Input no sabe de red.
Movimiento no sabe de UI.
Animación no sabe de HTTP.
Audio no sabe de base de datos.
Networking no debería estar mezclado con el controlador de movimiento.
```

## 7. Sistemas recomendados

### 7.1 Core

Responsable de:

- bootstrap de aplicación;
- configuración global;
- service locator ligero o composición de dependencias;
- eventos globales si hacen falta;
- logging.

Carpeta:

```text
Assets/_Project/Scripts/Core
```

Ejemplo:

```text
GameBootstrap
GameContext
SceneLoader
BuildInfo
```

### 7.2 Config

Centralizar URLs, puertos y flags.

No conviene tener URLs hardcodeadas en scripts sueltos.

```text
GameEnvironmentConfig
ApiConfig
NetworkConfig
WebGLConfig
```

Idealmente como ScriptableObjects para Unity:

```text
Assets/_Project/ScriptableObjects/Config/LocalDevConfig.asset
Assets/_Project/ScriptableObjects/Config/StagingConfig.asset
Assets/_Project/ScriptableObjects/Config/ProductionConfig.asset
```

### 7.3 API client

Sistema para hablar con la API PHP actual o una futura API mejorada.

```text
IAuthApiClient
IUserApiClient
IGamePersistenceApiClient
```

Implementación actual:

```text
PhpAuthApiClient
PhpUserApiClient
```

Esto permite cambiar la API más adelante sin tocar UI/gameplay.

### 7.4 Auth/session

Responsable de:

- login;
- register;
- token;
- usuario actual;
- expiración/relogin;
- persistencia local de sesión si hace falta.

```text
AuthService
SessionService
CurrentUser
```

No debería estar mezclado con escenas concretas ni con scripts de UI.

### 7.5 Player/input

Crear una abstracción clara:

```csharp
public interface IPlayerInputSource
{
    Vector2 Move { get; }
    Vector2 Look { get; }
    bool JumpPressed { get; }
    bool SprintHeld { get; }
    bool AttackPressed { get; }
}
```

Implementaciones:

```text
DesktopInputSource
MobileTouchInputSource
GamepadInputSource
BotInputSource
```

El movimiento consume `IPlayerInputSource`, no teclado/touch directamente.

### 7.6 Movement/gameplay state

Separar:

```text
PlayerMovementController: calcula movimiento.
PlayerState: estado actual del jugador.
PlayerAbilities: acciones del jugador.
PlayerStats: vida, energía, etc.
```

El controlador de movimiento no debería enviar HTTP ni tocar DB.

### 7.7 Animation/audio presentation

Propuesta:

```text
PlayerAnimationPresenter
PlayerAudioPresenter
```

Estos escuchan `PlayerState` y actualizan Animator/AudioSource.

Evitar que animación decida si algo es local/remoto. Eso debería venir de una capa de identidad/red.

### 7.8 Networking realtime

Responsable de:

- conexión Mirror;
- spawn/despawn;
- autoridad local/remota;
- sincronización de transform/estado;
- reconexión;
- debug de red.

```text
NetworkSessionService
PlayerSpawnService
NetworkPlayerIdentity
PlayerNetworkSync
```

Para navegador/WebGL, mantener `SimpleWebTransport` o un transporte ws/wss equivalente.

### 7.9 WebGL/browser compatibility

Sistema propio para:

- detectar browser;
- mostrar errores claros;
- tap-to-start/audio unlock;
- pantalla de diagnóstico;
- fullscreen/touch/orientation;
- build info visible.

```text
WebGLDiagnostics
BrowserFeatureReport
MobileStartGate
```

### 7.10 Diagnostics/dev tools

Tener herramientas de desarrollo separadas:

```text
DebugOverlay
NetworkStatsPanel
ApiHealthPanel
BuildInfoPanel
```

Esto ayuda mucho en WebGL/iOS donde el fallo puede ser navegador, API, red o Unity.

## 8. API/backend: estructura recomendada

La API actual puede mantenerse PHP, pero conviene reorganizarla.

Estructura propuesta sin framework pesado:

```text
ElTesoroDeMongliAPI/
  public/
    index.php
  src/
    Config/
      database.php
      mail.php
    Http/
      JsonResponse.php
      Request.php
    Auth/
      AuthController.php
      AuthService.php
      TokenService.php
    Users/
      UserController.php
      UserRepository.php
    Mail/
      MailService.php
    Database/
      ConnectionFactory.php
  migrations/
    001_create_users.sql
    002_create_access_tokens.sql
  config.example.php
  .env.example
```

Si se quiere algo más sólido, pasaría a Slim PHP o Laravel pequeño, pero no lo haría como primer paso.

## 9. Problemas concretos de API detectados

### 9.1 Configuración sensible en código

`connection.php` contiene configuración de conexión directamente en el código.

Recomendación:

```text
.env local
config.example.php
connection.local.php ignorado por git
```

### 9.2 Endpoints protegidos sin token claro

`login` genera token, pero `get_users` y `update_users` no parecen validar token en el propio endpoint.

Riesgo:

```text
Cualquiera que llame a update_users podría modificar transforms si el endpoint está público.
```

Para desarrollo local vale. Para producción, no.

### 9.3 Movimiento/transform en base de datos

`update_users` guarda `transform` en DB y `get_users` lee transforms activos.

Esto puede servir para prototipo o persistencia simple, pero no es buena base para sincronización realtime.

Recomendación:

```text
Realtime transform -> servidor de juego/Mirror.
Persistencia puntual -> API/DB.
```

### 9.4 Email acoplado al registro

El registro dispara lógica de token y correo desde el mismo flujo.

Recomendación:

```text
RegisterController
  -> AuthService.register
  -> TokenService.createActivationToken
  -> MailService.sendActivationMail
```

Así se puede desactivar correo en local sin romper registro.

## 10. Propuesta de monorepo o multi-repo

Ahora mismo hay dos repos:

```text
ElTesoroDeMongli
ElTesoroDeMongliAPI
```

Está bien mantenerlos separados.

Pero conviene crear documentación cruzada:

```text
ElTesoroDeMongli/docs/api-contract.md
ElTesoroDeMongliAPI/docs/api-contract.md
```

Y versionar el contrato:

```text
API v1
POST /login
POST /register
POST /get_users
POST /update_users
```

## 11. Contratos de datos recomendados

### Login request

```json
{
  "mail": "user@example.com",
  "password": "..."
}
```

### Login response

```json
{
  "error_code": 0,
  "content": {
    "user_id": 1,
    "token": "..."
  }
}
```

### Player transform persistente

```json
{
  "id": 1,
  "transform": {
    "position": {"x": 0, "y": 0, "z": 0},
    "rotation": {"x": 0, "y": 0, "z": 0, "w": 1}
  }
}
```

Pero para realtime conviene usar Mirror state sync, no polling de API.

## 12. Orden recomendado de refactor

### Fase 1 - Documentar mapa real

Crear:

```text
PLAYER_RUNTIME_MAP.md
SYSTEMS_MAP.md
API_CONTRACT.md
```

Objetivo: saber qué escenas, prefabs y scripts están activos realmente.

### Fase 2 - Seguridad/configuración API

- sacar credenciales del código;
- `.env.example`;
- `connection.local.php` ignorado;
- crear SQL de migración inicial;
- activar/desactivar correo con flag local.

### Fase 3 - Cliente API en Unity

- crear `IAuthApiClient`;
- crear `IUserApiClient`;
- centralizar URL base;
- eliminar URLs hardcodeadas.

### Fase 4 - Runtime/network map

- localizar `NetworkManager`;
- confirmar transporte real;
- documentar spawn;
- documentar autoridad local/remota;
- separar `PlayerNetworkSync` de movimiento.

### Fase 5 - Input/movement/presentation

- introducir `IPlayerInputSource`;
- separar movimiento, estado, animación y audio;
- evitar que animación dependa de red directamente.

### Fase 6 - WebGL diagnostics

- pantalla de diagnóstico;
- build info;
- tap-to-start;
- errores claros de API/red/WebGL.

### Fase 7 - Reorganización física de carpetas

Solo cuando lo anterior esté claro.

Mover gradualmente a:

```text
Assets/_Project
Assets/_ThirdParty
```

## 13. Arquitectura final deseada

```text
Client WebGL
  UI
  Input
  Gameplay Presentation
  Local Prediction / Interpolation
  API Client
  Network Client

Game Server
  Session
  Spawn
  Authority
  Realtime Sync
  Disconnect/Reconnect

API Server
  Auth
  Users
  Tokens
  Persistence
  Mail

Database
  Users
  Tokens
  Persistent game data
```

## 14. Mi recomendación práctica

No empezaría moviendo carpetas. Empezaría creando capas nuevas y migrando sistemas poco a poco.

Primera PR útil de arquitectura:

```text
docs: add system map and API contract
```

Segunda PR útil:

```text
refactor(api): extract local config and database connection
```

Tercera PR útil en Unity:

```text
refactor(client): centralize API base URL and auth client
```

La arquitectura objetivo debe permitir que mañana cambies PHP por otra API, Mirror por otro networking o XAMPP por Docker sin tener que tocar medio juego.
