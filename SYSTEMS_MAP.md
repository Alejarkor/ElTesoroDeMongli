# Systems map - ElTesoroDeMongli

Mapa funcional del proyecto para orientar refactors, despliegue y futuras migraciones.

Este documento describe los sistemas principales, sus responsabilidades, dependencias y límites recomendados.

## 1. Vista general

```text
WebGL Client
  ├── UI/Auth
  ├── Input
  ├── Gameplay presentation
  ├── API client
  └── Realtime network client

Unity/Mirror Game Server
  ├── Session
  ├── Spawn/despawn
  ├── Authority
  ├── Realtime state sync
  └── Disconnect/reconnect

PHP API
  ├── Auth/register/login
  ├── User data
  ├── Tokens
  ├── Mail activation
  └── Persistence access

Database
  ├── users
  ├── access_tokens
  └── future persistent game data
```

## 2. Regla base de arquitectura

Separar datos lentos y realtime:

```text
API/DB        -> login, registro, tokens, perfil, progreso, persistencia puntual.
Game server   -> movimiento, jugadores conectados, estado realtime, spawn, autoridad.
WebGL client  -> input, UI, representación visual, interpolación/predicción.
```

No usar la base de datos como mecanismo principal de sincronización de movimiento en tiempo real.

## 3. Sistemas del cliente Unity/WebGL

### 3.1 Bootstrap / Core

Responsable de:

- inicializar servicios;
- elegir configuración local/staging/production;
- cargar escena inicial;
- exponer versión/build;
- arrancar diagnóstico si es WebGL.

Ubicación recomendada:

```text
Assets/_Project/Scripts/Core
```

Clases futuras posibles:

```text
GameBootstrap
GameContext
SceneLoader
BuildInfo
```

### 3.2 Configuración

Responsable de:

- URL base de API;
- host/puerto de game server;
- entorno actual;
- flags de desarrollo;
- configuración WebGL.

Ubicación recomendada:

```text
Assets/_Project/Scripts/Config
Assets/_Project/ScriptableObjects/Config
```

Ejemplos:

```text
LocalDevConfig.asset
StagingConfig.asset
ProductionConfig.asset
```

### 3.3 API Client

Responsable de hablar con la API HTTP/JSON.

Interfaces recomendadas:

```csharp
public interface IAuthApiClient
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
}

public interface IUserApiClient
{
    Task<GetUsersResponse> GetUsersAsync();
    Task<UpdateUsersResponse> UpdateUsersAsync(UpdateUsersRequest request);
}
```

Implementación actual/futura:

```text
PhpAuthApiClient
PhpUserApiClient
```

El resto del juego no debería conocer rutas como `/login/` o `/update_users/`.

### 3.4 Auth / Session

Responsable de:

- login;
- registro;
- token actual;
- usuario actual;
- cierre de sesión;
- estado autenticado/no autenticado.

Clases recomendadas:

```text
AuthService
SessionService
CurrentUser
```

La UI llama a `AuthService`, no directamente a `UnityWebRequest`.

### 3.5 Input

Responsable de unificar teclado, ratón, mando, móvil/touch y posibles bots.

Interfaz objetivo:

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

Implementaciones posibles:

```text
DesktopInputSource
MobileTouchInputSource
GamepadInputSource
BotInputSource
```

El movimiento no debe depender directamente de teclado, touch ni UI.

### 3.6 Gameplay / Player

Responsable de lógica del jugador no dependiente de UI/API.

Submódulos recomendados:

```text
PlayerMovementController
PlayerState
PlayerAbilities
PlayerStats
PlayerInventory, si aplica
```

Regla:

```text
Gameplay no habla directamente con MySQL/PHP.
Gameplay no debería construir URLs.
Gameplay no debería depender de botones de UI concretos.
```

### 3.7 Animation / Audio Presentation

Responsable de traducir estado del jugador a presentación.

Clases recomendadas:

```text
PlayerAnimationPresenter
PlayerAudioPresenter
```

Entrada ideal:

```text
PlayerState
NetworkPlayerIdentity
```

Salida:

```text
Animator parameters
AudioSource playback
VFX
```

Evitar que animación decida directamente lógica de red o login.

### 3.8 Realtime Networking

Responsable de Mirror/WebSocket y estado en tiempo real.

Clases recomendadas:

```text
NetworkSessionService
PlayerSpawnService
NetworkPlayerIdentity
PlayerNetworkSync
```

Responsabilidades:

- conectar/desconectar;
- identificar jugador local/remoto;
- spawn/despawn;
- sincronizar transform/estado;
- reconectar si procede;
- exponer estado de conexión.

Para WebGL, el transporte del cliente debe ser compatible con `ws://` y `wss://`.

### 3.9 WebGL / Browser Compatibility

Responsable de problemas específicos de navegador:

- diagnóstico de WebGL/WebAssembly/WebSocket;
- pantalla `Tap to start`;
- desbloqueo de audio móvil;
- fullscreen/orientación;
- errores amigables;
- versión/commit visible.

Clases futuras:

```text
WebGLDiagnostics
BrowserFeatureReport
MobileStartGate
WebGLErrorOverlay
```

### 3.10 UI

Responsable de pantallas, botones y feedback visual.

Pantallas sugeridas:

```text
BootScreen
LoginScreen
RegisterScreen
ConnectionScreen
MainMenuScreen
GameHud
ErrorDialog
```

Regla:

```text
UI llama a servicios.
UI no contiene SQL, rutas API ni lógica de red de bajo nivel.
```

### 3.11 Diagnostics / Dev Tools

Responsable de herramientas de desarrollo.

```text
NetworkStatsPanel
ApiHealthPanel
BuildInfoPanel
DebugConsoleAdapter
```

Deben poder desactivarse en producción.

## 4. Sistemas del servidor Unity/Mirror

### 4.1 Game Server Bootstrap

Responsable de arrancar servidor en modo desarrollo/headless.

```text
ServerBootstrap
ServerConfig
ServerLogger
```

### 4.2 Network Session

Responsable de aceptar conexiones y mantener sesión.

```text
NetworkSessionService
ConnectedPlayersRegistry
```

### 4.3 Player Spawn

Responsable de decidir cuándo y dónde aparece un jugador.

```text
PlayerSpawnService
SpawnPointProvider
```

La lógica de spawn no debería vivir dentro del controlador de movimiento.

### 4.4 Authority / Sync

Responsable de decidir quién manda sobre qué.

Decisión pendiente:

```text
Cliente autoritativo con validación básica
Servidor autoritativo completo
Modelo híbrido controlado
```

Para prototipo WebGL, cliente autoritativo con validación básica puede ser aceptable. Para producción competitiva, mejor servidor autoritativo.

## 5. Sistemas de API/backend

### 5.1 Auth

Responsable de:

- registro;
- login;
- hash de password;
- activación;
- tokens.

Endpoints actuales relacionados:

```text
/register
/login
/validation
/createActivationToken
```

### 5.2 Users

Responsable de datos de usuario.

Endpoints actuales:

```text
/get_users
/update_users
```

Advertencia:

```text
get_users/update_users no deberían usarse como transporte realtime principal.
```

### 5.3 Mail

Responsable de emails de activación.

Endpoint/sistema actual:

```text
/activationMail
```

Debería poder desactivarse en entorno local.

### 5.4 Database

Responsable de conexión y queries.

Actual:

```text
connection.php global
SQL dentro de cada endpoint
```

Objetivo:

```text
ConnectionFactory
UserRepository
TokenRepository
```

## 6. Base de datos

Tablas mínimas actuales/inferidas:

```text
users
access_tokens
```

Responsabilidades correctas:

```text
users         -> identidad, email, password hash, nickname, active, datos persistentes.
access_tokens -> tokens de sesión/activación, expiración futura.
```

Pendiente recomendable:

```text
Añadir expiración de tokens.
Añadir migraciones SQL versionadas.
Separar tokens de activación y tokens de sesión si el proyecto crece.
```

## 7. Dependencias recomendadas

Dirección ideal:

```text
UI -> Services -> API Client / Network Client
Input -> Movement -> PlayerState -> Presentation / NetworkSync
Network -> PlayerState
API -> Session/Persistence
```

Evitar:

```text
UI -> SQL
Movement -> HTTP
Animation -> NetworkManager
API Client -> Scene objects
Database -> realtime movement loop
```

## 8. Estructura física recomendada

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
      API/
      Auth/
      Gameplay/
      Input/
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
  _ThirdParty/
    Mirror/
    CharacterMovementFundamentals/
    IngameDebugConsole/
```

No mover de golpe. Primero crear carpetas nuevas y migrar sistema a sistema.

## 9. Orden recomendado de trabajo

### Paso 1 - Mapas vivos

Crear y mantener:

```text
SYSTEMS_MAP.md
API_CONTRACT.md
PLAYER_RUNTIME_MAP.md
WEBGL_BASELINE_NOTES.md
```

### Paso 2 - Configuración segura

- API sin credenciales hardcodeadas.
- Config local ignorada por Git.
- Config Unity centralizada.

### Paso 3 - Cliente API centralizado

- `IAuthApiClient`.
- `IUserApiClient`.
- URLs en `ApiConfig`.

### Paso 4 - Runtime player map

- documentar prefab player activo;
- componentes reales;
- NetworkIdentity;
- transporte;
- spawn;
- autoridad.

### Paso 5 - Separar gameplay

- input separado;
- movement separado;
- state separado;
- animation/audio presenters;
- network sync separado.

### Paso 6 - WebGL diagnostics

- pantalla diagnóstico;
- `Tap to start`;
- build info;
- errores visibles de API/red.

### Paso 7 - Reorganización física

- mover a `_Project` y `_ThirdParty` poco a poco;
- validar escenas/prefabs después de cada movimiento.

## 10. Criterio de éxito

El refactor va bien si puedes responder rápido:

```text
Dónde se configura la API.
Dónde se conecta Mirror.
Dónde se crea un jugador.
Dónde se decide si un jugador es local/remoto.
Dónde se lee input.
Dónde se calcula movimiento.
Dónde se actualizan animaciones.
Dónde se guarda estado persistente.
Dónde se prueba WebGL/iOS.
```

Si alguna respuesta es “depende” o “está repartido por varios scripts raros”, ese sistema necesita frontera mejor.
