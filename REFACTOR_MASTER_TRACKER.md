# Refactor master tracker - ElTesoroDeMongli

Documento único de seguimiento para arquitectura, despliegue, API, WebGL, networking y gameplay.

Este archivo sustituye como punto de trabajo vivo a las auditorías/documentos separados. Los documentos anteriores quedan como histórico, pero a partir de ahora el seguimiento real debe hacerse aquí.

## Leyenda

```text
[ ] Pendiente
[/] En progreso
[x] Hecho
[!] Bloqueado / requiere decisión
[?] Requiere validación manual
```

## 0. Estado general actual

### Proyecto Unity

```text
Repo: Alejarkor/ElTesoroDeMongli
Unity: 2021.3.4f1
Escena activa de build: Assets/Scenes/MONGLICLIENT.unity
Networking actual: Mirror embebido en Assets/Mirror
Transporte WebGL disponible: SimpleWebTransport ws/wss
Target principal: navegador/WebGL, incluyendo iOS/Android
```

### API/backend

```text
Repo: sergio4anso/ElTesoroDeMongliAPI
Stack actual: PHP procedural + MySQL/MariaDB
Endpoints principales: register, login, validation, get_users, update_users
```

### Decisión arquitectónica base

```text
API/DB        -> login, registro, tokens, perfil, persistencia puntual.
Mirror/WSS    -> jugadores conectados, spawn, estado realtime, movimiento, animación.
WebGL client  -> input, UI, presentación, interpolación/predicción, conexión browser-safe.
```

Regla crítica:

```text
La base de datos no debe ser el bus principal de movimiento realtime.
```

---

# 1. Ramas/PRs generadas hasta ahora

## 1.1 Repo juego - Alejarkor/ElTesoroDeMongli

| PR | Rama | Tipo | Estado recomendado |
|---:|---|---|---|
| #8 | `refactor/project-cleanup-phase-1` | Limpieza `.gitignore` + auditoría técnica | Revisar y mergear pronto |
| #9 | `refactor/runtime-safety-phase-2` | Runtime safety en `Sensor.cs` | Probar en Unity antes de merge |
| #10 | `audit/gameplay-architecture-review` | Auditoría gameplay | Histórico, absorbido por este tracker |
| #11 | `docs/upgrade-networking-study` | Upgrade Unity/networking/WebGL | Histórico, absorbido por este tracker |
| #12 | `docs/windows-local-deployment-guide` | Guía despliegue Windows | Histórico, absorbido por este tracker |
| #13 | `docs/project-architecture-review` | Arquitectura global | Histórico, absorbido por este tracker |
| #14 | `docs/system-map-api-contract` | Sistemas + contrato API | Histórico, absorbido por este tracker |
| #15 | `docs/player-runtime-map` | Runtime player/network | Histórico, absorbido por este tracker |
| #16 | `docs/refactor-execution-plan` | Plan ejecución | Histórico, absorbido por este tracker |
| #17 | `refactor/unity-environment-config` | Config centralizada Unity | Rama activa de trabajo |
| #18 | `refactor/unity-api-client` | API client Unity, apilada sobre #17 | Rama activa, requiere #17 |

## 1.2 Repo API - sergio4anso/ElTesoroDeMongliAPI

| PR | Rama | Tipo | Estado recomendado |
|---:|---|---|---|
| #2 | `refactor/api-local-config` | Config local segura API + migración inicial | En prueba local |

---

# 2. Estrategia de ramas desde ahora

## 2.1 Rama única de planificación

```text
refactor/integrated-roadmap
```

Uso:

```text
Mantener este tracker.
Actualizar checkboxes.
Registrar decisiones.
No mezclar aquí refactors grandes de código.
```

## 2.2 Ramas de ejecución reales

A partir de ahora, cada refactor real debe salir del estado validado del proyecto y tener una intención única:

```text
refactor/api-local-config
refactor/unity-environment-config
refactor/unity-api-client
feature/webgl-diagnostics
refactor/network-identity-adapter
refactor/player-state-presentation
refactor/player-input-source
refactor/network-spawn-session
refactor/persistence-realtime-boundary
chore/reorganize-project-folders
upgrade/unity-2021-latest-lts
```

## 2.3 Regla de merge

```text
1. No mergear runtime sin probar en Unity.
2. No mergear WebGL sin build local.
3. No mergear API sin probar endpoints con XAMPP.
4. No migrar networking hasta tener baseline WebGL funcionando.
5. No mover carpetas grandes hasta separar responsabilidades.
```

---

# 3. Fase A - Consolidación y baseline local

Objetivo: tener un punto de partida funcional antes de seguir refactorizando.

## A1. API local

- [x] Crear rama API `refactor/api-local-config`.
- [x] Crear `config_loader.php`.
- [x] Crear `config.example.php`.
- [x] Ignorar `config.local.php` en Git.
- [x] Refactorizar `connection.php` para no tener credenciales hardcodeadas.
- [x] Refactorizar `activationMail` para leer config local/env.
- [x] Permitir mail desactivado en local.
- [x] Crear migración inicial SQL.
- [x] Probar registro local desde PowerShell.
- [x] Corregir error `Cannot redeclare mongli_array_merge_recursive_distinct()`.
- [x] Activar usuario manualmente en DB.
- [x] Probar login local con usuario nuevo.
- [ ] Probar `get_users`.
- [ ] Probar `update_users`.
- [ ] Revisar si la tabla real tiene columnas extra como `equipment` y actualizar migración si procede.
- [ ] Mergear API PR #2 cuando endpoints básicos pasen.

Validación mínima API:

```text
POST /register      -> error_code 0 o usuario ya existe controlado
POST /login         -> error_code 0 + user_id + token
POST /get_users     -> error_code 0
POST /update_users  -> error_code 0
```

## A2. Unity baseline

- [ ] Abrir proyecto en Unity 2021.3.4f1 o compatible 2021.3 LTS.
- [ ] Confirmar si compila limpio.
- [ ] Abrir `MONGLICLIENT.unity`.
- [ ] Localizar `NetworkManager`.
- [ ] Confirmar transporte activo.
- [ ] Confirmar si usa `SimpleWebTransport`.
- [ ] Confirmar puerto real.
- [ ] Confirmar prefab jugador activo.
- [ ] Documentar componentes del prefab jugador.
- [ ] Confirmar dónde se configura API ahora mismo.
- [ ] Confirmar si `get_users/update_users` se usa para transforms realtime.
- [ ] Confirmar escena/flujo servidor.
- [ ] Completar sección `Player runtime real` en este tracker.

## A3. Stack Windows completo

- [x] XAMPP funcionando.
- [x] DB existente detectada.
- [x] Tablas `users` y `access_tokens` existen.
- [x] Usuario local creado/probado.
- [ ] Servidor Mirror arrancado desde Unity o build.
- [ ] Build WebGL generado a `htdocs`.
- [ ] Cliente WebGL abre en `http://localhost/...`.
- [ ] Cliente WebGL conecta con API local.
- [ ] Cliente WebGL conecta con servidor Mirror local.
- [ ] Prueba desde móvil en LAN usando IP del PC.

---

# 4. Fase B - Configuración centralizada Unity

Objetivo: no tener URLs, puertos y entornos dispersos por scripts.

Rama activa:

```text
refactor/unity-environment-config
```

PR:

```text
#17
```

## B1. Implementado

- [x] Crear `EnvironmentType`.
- [x] Crear `EnvironmentConfig` como ScriptableObject.
- [x] Incluir API base URL.
- [x] Incluir game server host.
- [x] Incluir game server port.
- [x] Incluir flag `UseSecureWebSocket`.
- [x] Añadir `EnvironmentConfigProvider`.
- [x] Documentar assets Localhost/LAN/Production.

## B2. Pendiente de Unity Editor

- [ ] Crear asset `LocalhostEnvironmentConfig.asset`.
- [ ] Crear asset `LocalNetworkEnvironmentConfig.asset`.
- [ ] Crear asset `ProductionEnvironmentConfig.asset`.
- [ ] Añadir un GameObject `EnvironmentConfigProvider` en escena boot/cliente.
- [ ] Asignar asset Localhost.
- [ ] Verificar log al arrancar.
- [ ] Decidir si se necesita escena `Boot` separada.

## B3. Pendiente de integración

- [ ] Localizar scripts actuales con URLs hardcodeadas.
- [ ] Cambiar scripts existentes para leer desde `EnvironmentConfigProvider.Current`.
- [ ] Evitar romper UI/login actual.
- [ ] Mergear PR #17 cuando compile.

---

# 5. Fase C - API client Unity

Objetivo: centralizar llamadas HTTP a la API.

Rama activa:

```text
refactor/unity-api-client
```

PR:

```text
#18, apilada sobre #17
```

## C1. Implementado

- [x] Crear modelos base `ApiResponse`.
- [x] Crear modelos `LoginRequest`, `LoginResponse`, `RegisterRequest`, `RegisterResponse`.
- [x] Crear modelos `GetUsersResponse`, `UpdateUsersRequest`, `UpdateUsersResponse`.
- [x] Crear `ApiException`.
- [x] Crear awaiter para `UnityWebRequest`.
- [x] Crear `IAuthApiClient`.
- [x] Crear `IUserApiClient`.
- [x] Crear `PhpApiClient`.
- [x] Crear `UserSession`.
- [x] Crear `SessionService`.
- [x] Crear `AuthService`.
- [x] Añadir README de uso.

## C2. Requiere validación de compilación

- [?] Validar que `async/await` compila correctamente con la configuración C# del proyecto.
- [?] Validar que `UnityWebRequest.Result` está disponible en Unity 2021.3.
- [?] Validar serialización `JsonUtility` con los modelos actuales.
- [?] Validar `Vector3`/`Quaternion` dentro de modelos de API.

## C3. Pendiente de integración UI

- [ ] Localizar scripts actuales de login/register.
- [ ] Adaptar UI para usar `AuthService`.
- [ ] Mostrar errores según `error_code`.
- [ ] Guardar sesión tras login.
- [ ] Conectar flujo post-login con escena/game server.
- [ ] Mergear PR #18 después de #17 y validación UI mínima.

---

# 6. Fase D - WebGL/mobile diagnostics

Objetivo: que los problemas de navegador/iOS/Android sean diagnosticables.

Rama sugerida:

```text
feature/webgl-diagnostics
```

## D1. Diagnóstico navegador

- [ ] Crear pantalla o HTML de diagnóstico.
- [ ] Mostrar User Agent.
- [ ] Mostrar plataforma.
- [ ] Mostrar resolución y devicePixelRatio.
- [ ] Detectar WebGL 1.
- [ ] Detectar WebGL 2.
- [ ] Detectar WebAssembly.
- [ ] Detectar WebSocket.
- [ ] Detectar AudioContext.
- [ ] Detectar Touch Events.
- [ ] Detectar Pointer Events.
- [ ] Detectar Fullscreen API.
- [ ] Detectar IndexedDB/localStorage.

## D2. UX móvil

- [ ] Añadir pantalla `Tap to start`.
- [ ] Desbloquear audio tras gesto de usuario.
- [ ] Evitar que gameplay dependa de fullscreen real en iOS.
- [ ] Probar orientación vertical/horizontal.
- [ ] Prevenir scroll/zoom accidental si aplica.

## D3. Build info

- [ ] Mostrar versión build.
- [ ] Mostrar commit/hash o versión manual.
- [ ] Mostrar entorno activo.
- [ ] Mostrar API base URL.
- [ ] Mostrar Game Server URI.

## D4. Smoke test WebGL

- [ ] Crear escena mínima de prueba.
- [ ] Probar carga Unity.
- [ ] Probar input.
- [ ] Probar audio unlock.
- [ ] Probar llamada API.
- [ ] Probar conexión WebSocket.
- [ ] Probar en Chrome desktop.
- [ ] Probar en Android Chrome.
- [ ] Probar en iOS Safari.

---

# 7. Fase E - Runtime safety

Objetivo: corregir bugs pequeños antes de cambiar arquitectura grande.

## E1. Sensor.cs

Rama/PR:

```text
refactor/runtime-safety-phase-2 / PR #9
```

- [x] Evitar `IndexOutOfRange` en getters.
- [x] Evitar nulls en ignore list.
- [x] Restaurar capas con `try/finally`.
- [?] Validar movimiento en Unity.
- [?] Validar suelo/saltos/pendientes.
- [?] Validar que no rompe Character Movement Fundamentals.
- [ ] Mergear PR #9 si pasa pruebas.

## E2. AudioControl

Rama sugerida:

```text
refactor/audio-control-safety
```

- [ ] Localizar `AudioControl.cs` activo.
- [ ] Añadir null guards.
- [ ] Evitar errores con arrays de clips vacíos.
- [ ] Mover suscripciones a `OnEnable/OnDisable`.
- [ ] Evitar audio en servidor/headless si aplica.
- [ ] Diferenciar audio local/remoto si aplica.

## E3. AnimationControl

- [ ] Localizar clases referenciadas: `MongliWallkerController`, `MongliAnimatorNetworkController`, `MongliAnimatorLocalController`.
- [ ] Confirmar si existen y compilan.
- [ ] Cachear `Animator.StringToHash`.
- [ ] Evitar ramas duplicadas local/network.
- [ ] Preparar futura abstracción `IAnimatorDriver`.

---

# 8. Fase F - Network identity adapter

Objetivo: encapsular Mirror para que gameplay no dependa directamente de él.

Rama sugerida:

```text
refactor/network-identity-adapter
```

## F1. Diseño

- [ ] Crear `INetworkPlayerIdentity`.
- [ ] Crear `MirrorNetworkPlayerIdentity`.
- [ ] Exponer `IsLocalPlayer`.
- [ ] Exponer `IsOwner`.
- [ ] Exponer `IsServer`.
- [ ] Exponer `NetworkId`.

## F2. Integración progresiva

- [ ] Localizar scripts propios que preguntan a Mirror directamente.
- [ ] Cambiar solo scripts propios, no Mirror interno.
- [ ] Probar local client.
- [ ] Probar remote client.
- [ ] Probar host/server si aplica.

---

# 9. Fase G - Player state / presentation

Objetivo: separar movimiento, estado, animación y audio.

Rama sugerida:

```text
refactor/player-state-presentation
```

## G1. PlayerState

- [ ] Crear `PlayerState`.
- [ ] Centralizar velocidad.
- [ ] Centralizar grounded/jumping/falling.
- [ ] Centralizar estado de movimiento.
- [ ] Centralizar eventos de salto/aterrizaje si aplica.

## G2. Presenters

- [ ] Crear `PlayerAnimationPresenter`.
- [ ] Crear `PlayerAudioPresenter`.
- [ ] Animation lee `PlayerState`, no input directo.
- [ ] Audio lee `PlayerState`, no movimiento interno.
- [ ] NetworkSync puede leer/escribir estado de forma controlada.

## G3. Objetivo final

```text
Input -> Movement -> PlayerState -> Animation/Audio
                              -> NetworkSync
```

Evitar:

```text
Movement <-> Network <-> Animation <-> UI
```

---

# 10. Fase H - Input abstraction

Objetivo: unificar desktop, móvil, gamepad y futuros bots.

Rama sugerida:

```text
refactor/player-input-source
```

- [ ] Crear `IPlayerInputSource`.
- [ ] Crear `DesktopInputSource`.
- [ ] Crear `MobileTouchInputSource` si existe UI móvil.
- [ ] Crear `GamepadInputSource` si aplica.
- [ ] Movimiento consume interfaz, no dispositivos.
- [ ] Validar WebGL desktop.
- [ ] Validar Android/iOS touch.

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

---

# 11. Fase I - Spawn/session centralizados

Objetivo: que haya un sitio claro donde nacen/mueren jugadores.

Rama sugerida:

```text
refactor/network-spawn-session
```

- [ ] Localizar `NetworkManager` real.
- [ ] Confirmar si hay custom NetworkManager.
- [ ] Crear `PlayerSpawnService`.
- [ ] Crear `ConnectedPlayersRegistry`.
- [ ] Documentar spawn points.
- [ ] Manejar disconnect.
- [ ] Manejar reconnect si aplica.
- [ ] Evitar lógica de spawn dentro de movement/UI.

---

# 12. Fase J - Persistencia vs realtime

Objetivo: que la API/DB no se use como sincronización principal.

Rama sugerida:

```text
refactor/persistence-realtime-boundary
```

- [ ] Confirmar si `get_users/update_users` se usan en gameplay realtime.
- [ ] Si se usan por frame, eliminar ese patrón.
- [ ] Mantener transform realtime en Mirror.
- [ ] Usar API solo para persistencia puntual.
- [ ] Crear `UserPersistenceService`.
- [ ] Guardar estado solo en eventos o intervalos bajos.
- [ ] Añadir token a update futuro.

---

# 13. Fase K - Reorganización física de carpetas

Objetivo: ordenar el proyecto una vez que los sistemas estén separados.

Rama sugerida:

```text
chore/reorganize-project-folders
```

No hacer todavía.

## K1. Estructura objetivo

```text
Assets/
  _Project/
    Scenes/
    Scripts/
      Core/
      Config/
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
    ScriptableObjects/
    Art/
  _ThirdParty/
    Mirror/
    CharacterMovementFundamentals/
    IngameDebugConsole/
```

## K2. Reglas

- [ ] Mover desde Unity Editor cuando sea posible.
- [ ] No mover Mirror hasta tener todo probado.
- [ ] No mover assets grandes hasta final.
- [ ] Validar escenas después de cada bloque.
- [ ] Mantener GUIDs.

---

# 14. Fase L - Upgrade Unity / networking

No hacer todavía.

Objetivo: modernizar sin romper WebGL.

## L1. Unity

- [ ] Probar upgrade dentro de Unity 2021 LTS.
- [ ] Validar WebGL.
- [ ] Validar Mirror.
- [ ] Validar Input System.
- [ ] Validar Addressables.
- [ ] Probar Unity 6 en rama separada.

## L2. Networking

Orden recomendado:

```text
1. Mirror + SimpleWebTransport limpio.
2. Prototipo NGO WebGL/WSS.
3. Prototipo FishNet WebGL/WSS.
4. Evaluar Photon solo si compensa dependencia/precio.
```

Checklist:

- [ ] Confirmar Mirror actual funciona con WebGL.
- [ ] Confirmar `ws://` local.
- [ ] Confirmar `wss://` producción.
- [ ] Crear prototipo `Prototype_MirrorWebSocket`.
- [ ] Crear prototipo `Prototype_NGO_WebSocket`.
- [ ] Crear prototipo `Prototype_FishNet_WebSocket`.
- [ ] Decidir con datos, no por teoría.

---

# 15. Player runtime real - pendiente de rellenar en Unity

Rellenar manualmente al abrir Unity:

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
Login UI script:
Register UI script:
Movement script:
Animation script:
Audio script:
Network sync script:
```

---

# 16. Decisiones pendientes

## 16.1 Email desarrollo

Estado actual:

```text
Mail desactivado en local.
Usuarios activados manualmente en DB.
```

Decisión recomendada:

```text
Mantener mail desactivado mientras se desarrolla.
Crear correo propio del proyecto más adelante.
No usar correo personal en producción.
```

- [ ] Crear correo técnico del proyecto.
- [ ] Configurar SMTP solo en servidor/entorno seguro.
- [ ] No commitear credenciales.

## 16.2 Dominio / HTTPS / WSS

- [ ] Decidir dominio.
- [ ] Decidir hosting local/VPS.
- [ ] Decidir reverse proxy.
- [ ] Configurar HTTPS.
- [ ] Configurar WSS.

## 16.3 Autoridad de red

Decisión pendiente:

```text
Cliente autoritativo con validación básica
Servidor autoritativo completo
Modelo híbrido controlado
```

Recomendación actual:

```text
Mantener cliente autoritativo si ya funciona, limpiar arquitectura, y no migrar networking todavía.
```

---

# 17. Orden inmediato de trabajo

## Paso 1 - terminar prueba API local

- [x] Register probado.
- [x] Login probado tras activar usuario.
- [ ] Probar `get_users`.
- [ ] Probar `update_users`.
- [ ] Mergear API PR #2 si pasa.

## Paso 2 - validar PR #17 en Unity

- [ ] Abrir rama `refactor/unity-environment-config`.
- [ ] Confirmar compila.
- [ ] Crear asset `LocalhostEnvironmentConfig`.
- [ ] Añadir provider a escena.
- [ ] Confirmar log.

## Paso 3 - validar PR #18 en Unity

- [ ] Abrir rama `refactor/unity-api-client`.
- [ ] Confirmar compila.
- [ ] Crear objeto de test temporal si hace falta.
- [ ] Llamar login desde `AuthService`.
- [ ] Conectar UI existente después.

## Paso 4 - WebGL diagnostics

- [ ] Crear rama `feature/webgl-diagnostics`.
- [ ] Añadir diagnóstico navegador.
- [ ] Añadir `Tap to start`.
- [ ] Probar iOS pronto.

---

# 18. Qué puede hacer ChatGPT desde GitHub

Puede:

```text
crear ramas
crear PRs
modificar C#/PHP/SQL/docs
preparar interfaces
preparar servicios
refactorizar scripts concretos
arreglar errores que pegues aquí
actualizar este tracker
```

No puede validar directamente:

```text
Unity Editor
Play Mode
prefabs/escenas visualmente
build WebGL real
iOS/Android real
firewall/red local
```

Flujo recomendado:

```text
1. ChatGPT crea PR pequeña.
2. Alejarkor prueba.
3. Alejarkor pega error/captura.
4. ChatGPT corrige.
5. Se marca checkbox.
6. Merge.
```
