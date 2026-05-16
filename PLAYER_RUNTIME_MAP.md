# Player runtime map - ElTesoroDeMongli

Mapa operativo del runtime de jugador, red y escenas.

Este documento sirve para guiar refactors sin romper funcionalidad. Parte de la información está inferida desde el repositorio y debe validarse en Unity.

## 1. Estado detectado desde repo

### Unity

```text
Unity version: 2021.3.4f1
```

### Build settings

Escenas detectadas:

```text
Assets/Scenes/GameScene.unity                                disabled
Assets/Samples/Input System/1.3.0/On-Screen Controls/...      disabled
Assets/Scenes/CharacterControllerTest.unity                   disabled
Assets/Scenes/MONGLICLIENT.unity                              enabled
Assets/Scenes/MONGLISERVER.unity                              disabled
Assets/Scenes/Empty.unity                                     disabled
```

Escena activa de build:

```text
Assets/Scenes/MONGLICLIENT.unity
```

### Networking

Mirror está incluido en:

```text
Assets/Mirror
```

El proyecto contiene `SimpleWebTransport`:

```text
Assets/Mirror/Transports/SimpleWeb/SimpleWebTransport.cs
```

Ese transporte soporta:

```text
ws://
wss://
```

Esto es importante porque el cliente objetivo es WebGL/navegador.

## 2. Elementos que hay que validar en Unity

Hay cosas que no conviene asumir solo leyendo YAML desde GitHub. Validar manualmente:

```text
1. Qué GameObject contiene el NetworkManager en MONGLICLIENT.
2. Si MONGLICLIENT es realmente solo cliente o también puede arrancar Host/Server.
3. Qué transporte tiene asignado el NetworkManager.
4. Si el transporte activo es SimpleWebTransport.
5. Puerto real configurado.
6. Si usa ws o wss según entorno.
7. Qué prefab está asignado como Player Prefab.
8. Qué componentes tiene el Player Prefab.
9. Qué componente decide si el jugador es local/remoto.
10. Dónde se hace spawn/despawn.
11. Dónde se actualiza movimiento.
12. Dónde se sincroniza transform/animación.
13. Dónde se conecta con la API PHP.
```

## 3. Preguntas clave del runtime

Para poder refactorizar con seguridad, hay que responder estas preguntas:

### Escenas

```text
¿MONGLICLIENT es cliente puro?
¿MONGLISERVER se usa todavía?
¿Hay una escena Boot o el cliente entra directo al juego?
¿La escena de login está integrada en MONGLICLIENT?
¿El servidor de juego se ejecuta desde escena separada o desde la misma escena?
```

### NetworkManager

```text
¿Dónde está el NetworkManager?
¿Qué clase concreta usa?
¿Es NetworkManager base de Mirror o una clase custom?
¿Qué transport usa?
¿Qué prefab de jugador instancia?
¿Qué eventos de conexión/desconexión maneja?
```

### Player prefab

```text
Ruta del prefab:
Componentes del root:
Componentes hijos:
NetworkIdentity:
NetworkTransform o equivalente:
Animator:
Controller de movimiento:
Audio:
Input:
Scripts propios:
```

### Autoridad

```text
¿Quién mueve al jugador local?
¿El cliente envía input o transform?
¿El servidor valida algo?
¿El servidor reenvía estado a otros clientes?
¿Los remotos interpolan o aplican directamente transform?
```

### API

```text
¿Login/register se hacen desde UnityWebRequest?
¿Dónde se guarda user_id/token?
¿get_users/update_users se siguen usando para transform?
¿La API se usa solo para login/persistencia o también para pseudo-realtime?
```

## 4. Modelo recomendado

### Separación correcta

```text
API/DB
  -> auth
  -> usuario
  -> token
  -> persistencia puntual

Mirror/WebSocket
  -> jugadores conectados
  -> spawn/despawn
  -> movimiento realtime
  -> animación/estado realtime

Cliente WebGL
  -> input
  -> UI
  -> presentación
  -> interpolación/predicción
```

La base de datos no debería ser el canal principal de movimiento realtime.

## 5. Player runtime ideal

Estructura de componentes sugerida:

```text
PlayerRoot
  NetworkIdentity
  NetworkPlayerIdentity
  PlayerInputAdapter
  PlayerMovementController
  PlayerState
  PlayerNetworkSync
  PlayerAnimationPresenter
  PlayerAudioPresenter
  PlayerVisualRoot
  CameraTarget
```

### NetworkPlayerIdentity

Responsable de encapsular Mirror:

```csharp
public interface INetworkPlayerIdentity
{
    bool IsLocalPlayer { get; }
    bool IsOwner { get; }
    bool IsServer { get; }
    ulong NetworkId { get; }
}
```

El resto del gameplay debería preguntar a esta interfaz, no directamente a Mirror en todas partes.

### PlayerInputAdapter

Responsable de seleccionar input real:

```text
DesktopInputSource
MobileTouchInputSource
GamepadInputSource
```

### PlayerMovementController

Responsable de movimiento puro.

No debería:

```text
hacer HTTP
consultar DB
abrir/cerrar escenas
crear UI
mandar emails
```

### PlayerNetworkSync

Responsable de sincronizar estado realtime.

Debe decidir:

```text
qué se envía
cada cuánto se envía
quién tiene autoridad
cómo se interpola remoto
```

### PlayerAnimationPresenter

Responsable de traducir estado a Animator.

No debería decidir login, conexión ni spawn.

### PlayerAudioPresenter

Responsable de sonidos locales/remotos.

Debe distinguir:

```text
local player audio
remote spatial audio
server/headless no audio
```

## 6. Mapa de datos recomendado

### Datos de sesión

```text
user_id
token
nickname
is_authenticated
```

Vive en:

```text
SessionService
```

### Datos persistentes

```text
perfil
progreso
última posición opcional
inventario futuro
```

Vive en:

```text
API + DB
```

### Datos realtime

```text
posición actual
rotación actual
velocidad
estado grounded/jumping/attacking
animación remota
```

Vive en:

```text
Mirror/Game Server
```

## 7. Validación manual recomendada

Abrir Unity y rellenar esta tabla:

```text
NetworkManager GameObject:
NetworkManager class:
Transport component:
Transport port:
Transport ws/wss setting:
Player Prefab path:
Player Prefab root scripts:
Local-only scripts:
Remote-only scripts:
Server-only scripts:
Scene that starts server:
Scene that starts client:
API config location:
```

## 8. Riesgos actuales a vigilar

```text
1. Cliente WebGL intentando usar transporte no compatible con navegador.
2. Movimiento realtime apoyado en API/DB en vez de Mirror.
3. Scripts de animación/audio mezclados con red.
4. Spawn logic repartido entre escena, UI y player.
5. URLs/API hardcodeadas.
6. Diferencias entre localhost y LAN móvil.
7. Server headless intentando reproducir audio o lógica de cámara.
8. Player local y remoto usando el mismo script sin guards claros.
```

## 9. Primeros refactors recomendados

### PR 1 - Runtime map validation

Completar este documento desde Unity.

### PR 2 - Network identity adapter

Crear una capa pequeña:

```text
INetworkPlayerIdentity
MirrorNetworkPlayerIdentity
```

Objetivo: que gameplay no dependa de Mirror directamente.

### PR 3 - API config centralizada

Crear:

```text
ApiConfig
EnvironmentConfig
```

Objetivo: quitar URLs sueltas.

### PR 4 - Player state

Crear `PlayerState` como fuente central para:

```text
movement state
animation state
audio events
network sync
```

### PR 5 - Animation/audio presenters

Separar presentación del controlador de movimiento.

## 10. Criterio de avance

No avanzar a migrar networking o mover carpetas grandes hasta poder responder con certeza:

```text
Cuál es el prefab activo del jugador.
Qué scripts se ejecutan solo en local.
Qué scripts se ejecutan en remoto.
Qué scripts se ejecutan en servidor.
Qué transporte usa WebGL.
Dónde se configura la API.
Qué datos van por API y qué datos van por Mirror.
```
