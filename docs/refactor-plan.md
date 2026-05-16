# Refactor plan

## Estado actual detectado

- Proyecto Unity con `ProjectVersion.txt` en `2021.3.4f1`.
- Dependencias relevantes: `Mirror`, `Input System`, `Addressables`, `Cinemachine`.
- Escenas principales actuales: `MONGLICLIENT`, `MONGLISERVER`, `CharacterControllerTest`.
- Hay dos líneas arquitectónicas conviviendo:
  - `Assets/_Project/Scripts`: capa nueva, más modular, con `namespace`, servicios e `EnvironmentConfig`.
  - `Assets/Scripts/Mongli`: capa legacy, con `Singleton`, clases grandes y más lógica acoplada a escena/runtime.

## Objetivos de refactor

1. Recuperar una base estable de compilación.
2. Unificar configuración, API y autenticación en una sola capa.
3. Reducir acoplamiento global en red, login y spawning.
4. Dividir controladores grandes en piezas con responsabilidad clara.
5. Dejar una estructura preparada para crecer sin depender de nombres de escena o singletons implícitos.

## Problemas prioritarios

### 1. Base inestable

- La solución no compila limpia en el entorno actual.
- Hay mezcla entre versión de proyecto Unity `2021.3.4f1` y referencias locales del editor `6000.3.4f1`, lo que puede introducir obsolescencias y falsos positivos de compilación.

### 2. Duplicidad de arquitectura

- Existe una capa nueva de API/config en `Assets/_Project/Scripts`.
- La lógica runtime sigue usando `MongliAPIConnector`, `LoginGrabber` y `MongliNetworkAuthenticator` legacy.
- Ahora mismo la capa nueva parece preparada pero no integrada en el flujo real de login/network.

### 3. Acoplamiento global

- Uso extendido de `Singleton<T>` con `FindObjectOfType`.
- `MongliEntitiesGlobalData`, `MongliEntitySpawner`, `MongliGameEntityInitializer` y `LoginGrabber` concentran dependencias globales.
- `MongliNetworkManager` coordina autenticación, reconexión, persistencia de entidad y actualización a API.

### 4. Clases demasiado grandes

- `SUPERMongliCharacterController.cs` supera ampliamente el tamaño razonable para un único `MonoBehaviour`.
- `MongliPlayerNetwork` mezcla input, predicción/movimiento, setup local/remoto y sincronización.

## Plan por fases

### Fase 1. Estabilización

- Corregir errores de compilación sin cambiar comportamiento.
- Separar warnings del código propio frente a third-party o assets importados.
- Confirmar con qué versión de Unity se va a trabajar realmente antes de refactors más profundos.

### Fase 2. Configuración y API

- Declarar `EnvironmentConfig` como fuente única de URLs y host/puerto.
- Hacer que la ruta legacy use esa configuración antes de sustituirla.
- Eliminar hardcodes de `localhost`, `127.0.0.1` y claves embebidas donde sea posible.

### Fase 3. Login y networking

- Separar autenticación, gestión de sesión y conexión de Mirror.
- Reducir responsabilidad de `MongliNetworkManager`.
- Encapsular spawn/despawn/reconnect de usuarios en un servicio dedicado.

### Fase 4. Gameplay y personaje

- Extraer de `SUPERMongliCharacterController` bloques como:
  - movimiento base
  - grounding
  - slide/crouch/jump
  - footsteps
  - interacción
- Revisar si conviene mantener herencia con `MongliCharacterController` o migrar a composición progresiva.

## Primeras tareas concretas

1. Dejar compilación verde o al menos acotar qué errores dependen del entorno/editor.
2. Integrar `EnvironmentConfig` en la capa de red/API legacy.
3. Crear una frontera clara entre:
   - datos/configuración
   - servicios runtime
   - `MonoBehaviours` de escena
4. Atacar después el controlador de personaje, porque es el mayor foco de complejidad.

## Riesgos

- Tocar `Mirror` y el flujo de autenticación sin una escena de prueba clara puede romper conexión cliente-servidor.
- Partir `SUPERMongliCharacterController` demasiado pronto puede romper físicas, animación o input.
- Si el proyecto va a seguir abriéndose con versiones distintas de Unity, parte de la refactorización puede quedar contaminada por problemas de compatibilidad de editor.
