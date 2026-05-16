# Refactor execution plan - ElTesoroDeMongli

Plan de ejecución para convertir las auditorías en refactors reales, por orden lógico y con bajo riesgo.

Objetivo: mejorar arquitectura, despliegue, seguridad, WebGL y mantenimiento sin romper la funcionalidad actual.

## Principios

1. No mover carpetas grandes al principio.
2. No cambiar Unity version y networking a la vez.
3. No tocar gameplay, API y despliegue en la misma PR.
4. Cada PR debe tener una intención única.
5. Todo cambio runtime debe ser testeable en Unity antes de merge.
6. La documentación se mergea pronto para que sirva de mapa.
7. Primero estabilizar, luego desacoplar, después reorganizar.

## Fase 0 - Consolidar documentación base

Tipo: documentación.

Objetivo: tener los mapas antes de tocar código serio.

PRs relacionadas:

```text
PR #8  - technical audit + gitignore
PR #10 - gameplay architecture review
PR #11 - Unity upgrade/networking/WebGL study
PR #12 - Windows local deployment guide
PR #13 - project architecture review
PR #14 - systems map + API contract
PR #15 - player runtime map
```

Acciones:

```text
1. Revisar que los documentos no contradicen el proyecto real.
2. Mergear documentación a main.
3. Dejar PR #9 aparte hasta probarla, porque toca runtime.
```

Criterio de salida:

```text
main contiene la documentación base de arquitectura, despliegue, sistemas y contrato API.
```

## Fase 1 - Baseline local y runtime map real

Tipo: verificación + documentación.

Objetivo: confirmar qué funciona hoy antes de refactorizar.

Nueva rama sugerida:

```text
baseline/runtime-validation
```

Acciones:

```text
1. Abrir proyecto en Unity 2021.3.x.
2. Confirmar si compila.
3. Abrir MONGLICLIENT.
4. Localizar NetworkManager.
5. Confirmar transporte real.
6. Confirmar si usa SimpleWebTransport.
7. Localizar Player Prefab.
8. Documentar componentes del Player Prefab.
9. Confirmar cómo se hace spawn.
10. Confirmar qué scripts son local-only, remote-only y server-only.
11. Confirmar dónde se configura la API.
12. Completar PLAYER_RUNTIME_MAP.md con datos reales.
```

Criterio de salida:

```text
Sabemos con certeza qué escena, prefab, transporte, spawn y autoridad usa el proyecto.
```

No hacer todavía:

```text
No migrar networking.
No mover assets.
No cambiar versión de Unity.
```

## Fase 2 - Despliegue local Windows funcionando

Tipo: entorno/despliegue.

Objetivo: que el stack completo corra en local.

Nueva rama sugerida:

```text
setup/windows-local-baseline
```

Acciones:

```text
1. Instalar XAMPP.
2. Clonar API en htdocs.
3. Crear DB local.
4. Crear tablas users/access_tokens.
5. Configurar connection.php localmente.
6. Probar register/login/get_users/update_users con PowerShell.
7. Abrir Unity y arrancar servidor Mirror.
8. Build WebGL a htdocs.
9. Probar desde localhost.
10. Probar desde móvil en LAN.
```

Criterio de salida:

```text
API + DB + servidor Mirror + cliente WebGL funcionan en el PC de desarrollo.
```

## Fase 3 - Seguridad/configuración mínima de API

Tipo: backend PHP.

Objetivo: sacar configuración sensible del código y preparar API para desarrollo seguro.

Nueva rama sugerida:

```text
refactor/api-local-config
```

Acciones:

```text
1. Crear connection.example.php.
2. Crear soporte para connection.local.php ignorado por Git.
3. Sacar credenciales reales de connection.php.
4. Añadir .gitignore específico si hace falta.
5. Añadir migrations/001_create_initial_schema.sql.
6. Añadir flag local para desactivar envío real de email.
7. Documentar configuración local.
```

Criterio de salida:

```text
La API puede desplegarse localmente sin credenciales reales en repo.
```

Riesgo:

```text
Bajo-medio. Toca API, pero no gameplay.
```

Pruebas:

```text
POST /register
POST /login
POST /get_users
POST /update_users
```

## Fase 4 - Normalización mínima API v1.1

Tipo: backend PHP.

Objetivo: mejorar API sin reescribirla.

Nueva rama sugerida:

```text
refactor/api-v1-safety
```

Acciones:

```text
1. Crear helper JsonResponse.
2. Crear helper RequestJson.
3. Crear funciones comunes de error.
4. Evitar warnings PHP que rompen JSON.
5. Añadir validación básica de JSON malformado.
6. Añadir token opcional a endpoints protegidos.
7. Añadir expiración futura o prepared field para token expiration.
```

Criterio de salida:

```text
La API devuelve JSON estable y empieza a separar lógica común.
```

No hacer todavía:

```text
No migrar a Laravel/Slim todavía.
No cambiar URLs públicas todavía.
```

## Fase 5 - Configuración centralizada en Unity

Tipo: Unity cliente.

Objetivo: quitar configuración dispersa de API/red.

Nueva rama sugerida:

```text
refactor/unity-environment-config
```

Acciones:

```text
1. Crear ScriptableObject EnvironmentConfig.
2. Añadir API base URL.
3. Añadir game server host/port.
4. Añadir flags local/staging/production.
5. Añadir inspector claro para cambiar entorno.
6. Reemplazar URLs hardcodeadas que se encuentren.
```

Criterio de salida:

```text
El entorno se cambia desde un único punto.
```

Riesgo:

```text
Medio, depende de dónde estén ahora las URLs.
```

## Fase 6 - API Client en Unity

Tipo: Unity cliente/infrastructure.

Objetivo: que UI/gameplay no llame HTTP directamente.

Nueva rama sugerida:

```text
refactor/unity-api-client
```

Acciones:

```text
1. Crear IAuthApiClient.
2. Crear IUserApiClient.
3. Crear modelos LoginRequest/LoginResponse.
4. Crear modelos RegisterRequest/RegisterResponse.
5. Crear modelos GetUsers/UpdateUsers.
6. Crear implementación PhpApiClient con UnityWebRequest.
7. Crear AuthService y SessionService básicos.
8. Adaptar UI/login actual si existe.
```

Criterio de salida:

```text
La API se consume desde servicios centralizados, no desde scripts sueltos.
```

Riesgo:

```text
Medio. Requiere probar login/registro.
```

## Fase 7 - Validación WebGL/móvil

Tipo: WebGL/browser.

Objetivo: detectar problemas antes de migrar networking o Unity.

Nueva rama sugerida:

```text
feature/webgl-diagnostics
```

Acciones:

```text
1. Crear pantalla o HTML de diagnóstico.
2. Mostrar WebGL/WebAssembly/WebSocket/AudioContext/Touch.
3. Añadir build version/commit visible.
4. Añadir Tap to start para desbloquear audio.
5. Añadir mensajes claros de error API/red.
6. Preparar smoke test scene mínima.
```

Criterio de salida:

```text
Se puede probar en iOS/Android sabiendo si falla navegador, API, red o Unity.
```

## Fase 8 - Seguridad runtime: Sensor.cs y audio

Tipo: Unity runtime.

Objetivo: arreglos pequeños antes de arquitectura grande.

Ramas/PRs relacionadas:

```text
PR #9 - Sensor.cs runtime safety
```

Nueva rama sugerida para audio:

```text
refactor/audio-control-safety
```

Acciones:

```text
1. Probar PR #9 en Unity.
2. Mergear PR #9 si movimiento funciona.
3. Revisar AudioControl.
4. Añadir null guards.
5. Evitar arrays vacíos de clips.
6. Mover suscripciones a OnEnable/OnDisable.
7. Evitar audio en servidor/headless si aplica.
```

Criterio de salida:

```text
Menos riesgo de NullReference/IndexOutOfRange y estado inconsistente.
```

## Fase 9 - Network identity adapter

Tipo: Unity networking.

Objetivo: que gameplay no dependa directamente de Mirror en todos lados.

Nueva rama sugerida:

```text
refactor/network-identity-adapter
```

Acciones:

```text
1. Crear INetworkPlayerIdentity.
2. Crear MirrorNetworkPlayerIdentity.
3. Exponer IsLocalPlayer, IsOwner, IsServer, NetworkId.
4. Cambiar scripts propios para preguntar a esta capa donde sea fácil.
5. No tocar Mirror interno.
```

Criterio de salida:

```text
La identidad local/remota queda encapsulada.
```

Riesgo:

```text
Medio. Hay que validar local/remoto/host.
```

## Fase 10 - Player state y separación presentación

Tipo: Unity gameplay.

Objetivo: separar movimiento, estado, animación y audio.

Nueva rama sugerida:

```text
refactor/player-state-presentation
```

Acciones:

```text
1. Crear PlayerState.
2. Mover datos de grounded, velocity, jump, movement mode.
3. Crear PlayerAnimationPresenter.
4. Crear PlayerAudioPresenter.
5. Cambiar AnimationControl para leer estado, no decidir red.
6. Cachear Animator.StringToHash.
```

Criterio de salida:

```text
Movimiento calcula estado. Presenters muestran estado. Red sincroniza estado.
```

Riesgo:

```text
Medio-alto. Hacer en sub-PRs pequeñas.
```

## Fase 11 - Input abstraction

Tipo: Unity gameplay/input.

Objetivo: unificar desktop, móvil, gamepad y futuro bot/replay.

Nueva rama sugerida:

```text
refactor/player-input-source
```

Acciones:

```text
1. Crear IPlayerInputSource.
2. Crear DesktopInputSource.
3. Crear MobileTouchInputSource si aplica.
4. Adaptar movimiento para consumir interfaz.
5. Mantener compatibilidad con Input System actual.
```

Criterio de salida:

```text
El movimiento no sabe si input viene de teclado, móvil o mando.
```

## Fase 12 - Spawn/session centralizados

Tipo: Unity networking/game server.

Objetivo: centralizar creación/destrucción de jugadores.

Nueva rama sugerida:

```text
refactor/network-spawn-session
```

Acciones:

```text
1. Localizar NetworkManager custom/base.
2. Crear PlayerSpawnService.
3. Crear ConnectedPlayersRegistry.
4. Documentar spawn points.
5. Separar spawn de UI/player movement.
6. Manejar disconnect/reconnect.
```

Criterio de salida:

```text
Hay un sitio claro donde nace/muere un jugador.
```

## Fase 13 - Persistencia vs realtime

Tipo: arquitectura gameplay/backend.

Objetivo: dejar claro qué va por DB/API y qué por Mirror.

Nueva rama sugerida:

```text
refactor/persistence-realtime-boundary
```

Acciones:

```text
1. Evitar usar update_users/get_users para realtime si se usa ahora.
2. Usar Mirror para transform/estado en vivo.
3. Usar API para guardar estado puntual si hace falta.
4. Crear UserPersistenceService.
5. Añadir intervalo bajo o evento para persistencia, no cada frame.
```

Criterio de salida:

```text
DB deja de ser bus realtime.
```

## Fase 14 - Reorganización física de carpetas

Tipo: Unity project organization.

Objetivo: ordenar proyecto después de separar sistemas.

Nueva rama sugerida:

```text
chore/reorganize-project-folders
```

Acciones:

```text
1. Crear Assets/_Project.
2. Crear Assets/_ThirdParty.
3. Mover solo scripts propios primero.
4. Mover prefabs/escenas después.
5. Mover third-party con cuidado.
6. Validar escenas tras cada movimiento.
```

Criterio de salida:

```text
Proyecto ordenado por sistemas sin referencias rotas.
```

Riesgo:

```text
Alto si se hace a mano fuera de Unity. Mejor mover desde Unity Editor.
```

## Fase 15 - Upgrade Unity/Mirror/networking

Tipo: plataforma.

Objetivo: modernizar sin romper WebGL.

No hacer antes de estabilizar runtime.

Ramas sugeridas:

```text
upgrade/unity-2021-latest-lts
upgrade/unity-6-compatibility
prototype/networking-ngo-webgl
prototype/networking-fishnet-webgl
```

Acciones:

```text
1. Primero subir dentro de 2021 LTS.
2. Validar WebGL.
3. Mantener Mirror.
4. Probar Unity 6 separado.
5. Prototipar NGO/FishNet solo con WebGL + WSS real.
6. Migrar networking solo si prototipo gana claramente.
```

Criterio de salida:

```text
Decisión de networking basada en prototipo, no en teoría.
```

## Orden resumido recomendado

```text
0. Mergear documentación base.
1. Validar runtime real en Unity.
2. Levantar stack local Windows.
3. Arreglar configuración/seguridad API.
4. Centralizar config Unity.
5. Centralizar API client Unity.
6. Añadir diagnósticos WebGL/móvil.
7. Aplicar fixes runtime pequeños.
8. Encapsular identidad Mirror.
9. Separar PlayerState/Animation/Audio.
10. Abstraer input.
11. Centralizar spawn/session.
12. Separar persistencia y realtime.
13. Reorganizar carpetas.
14. Actualizar Unity/networking.
```

## Trabajo que puede hacer ChatGPT desde GitHub

Sí puede:

```text
crear ramas
crear PRs
modificar scripts C#/PHP
crear interfaces y clases
crear migraciones SQL
crear documentación
preparar configuración local
hacer refactors de bajo/medio riesgo
```

Necesita validación humana para:

```text
abrir Unity
mover assets grandes desde Editor
validar prefabs/escenas
probar Play Mode
compilar WebGL
probar iOS/Android
verificar input/touch/fullscreen/audio
```

## Estrategia de colaboración recomendada

```text
1. ChatGPT crea una PR pequeña.
2. Alejarkor prueba en Unity/Windows/WebGL.
3. Alejarkor pega errores o capturas.
4. ChatGPT corrige en la misma rama.
5. Merge cuando pase la prueba concreta.
```

No hacer PRs gigantes con muchos sistemas mezclados.
