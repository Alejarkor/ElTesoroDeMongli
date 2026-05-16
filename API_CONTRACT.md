# API contract - ElTesoroDeMongli API v1

Contrato funcional de la API actual observada en `sergio4anso/ElTesoroDeMongliAPI`.

Este documento define cómo debería consumir la API el cliente Unity/WebGL y qué comportamiento se espera de cada endpoint.

## 1. Base URL

Desarrollo local con XAMPP:

```text
http://localhost/ElTesoroDeMongliAPI/
```

Desde móvil en LAN:

```text
http://192.168.1.X/ElTesoroDeMongliAPI/
```

Producción:

```text
https://your-domain.example/ElTesoroDeMongliAPI/
```

## 2. Formato general

La API usa JSON.

Headers recomendados:

```http
Content-Type: application/json
Accept: application/json
```

La mayoría de endpoints actuales esperan `POST`, incluso `get_users`.

Respuesta estándar observada:

```json
{
  "error_code": 0,
  "content": {}
}
```

En errores, algunos endpoints devuelven solo:

```json
{
  "error_code": 3
}
```

## 3. Error codes conocidos

Los códigos se han inferido de los scripts actuales.

```text
0  -> OK
1  -> usuario ya registrado
2  -> email inválido o validación de formato fallida
3  -> método HTTP no permitido, normalmente no POST
4  -> JSON ausente, inválido o campos requeridos ausentes
5  -> nickname ausente
6  -> password ausente
7  -> mail ausente
8  -> password no cumple requisitos
9  -> nickname ya existe
10 -> error SQL / error interno de actualización/token
12 -> contraseña incorrecta
14 -> usuario no existe o no está activo
16 -> formato de array incorrecto
17 -> formato de usuario incorrecto en update_users
500 -> error de conexión DB recomendado para futura normalización
```

Recomendación futura:

```json
{
  "error_code": 12,
  "message": "Invalid credentials",
  "content": null
}
```

Pero para compatibilidad inicial, el cliente debe soportar respuestas sin `message`.

## 4. POST /register/

Registra un usuario nuevo.

### Request

```json
{
  "mail": "user@example.com",
  "password": "Test1234!",
  "nickname": "Tester"
}
```

### Validaciones actuales

Email:

```text
Debe ser email válido.
```

Password:

```text
Mínimo 9 caracteres.
Al menos una minúscula.
Al menos una mayúscula.
Al menos un número.
Al menos un símbolo, guion o guion bajo.
```

Nickname:

```text
Máximo 20 caracteres.
Único en DB.
```

### Response OK

```json
{
  "error_code": 0
}
```

### Errores relevantes

```text
1 -> mail ya registrado
2 -> mail inválido
5 -> falta nickname
6 -> falta password
7 -> falta mail
8 -> password no cumple requisitos
9 -> nickname ya existe
```

### Nota local

El registro actual puede disparar envío de email de activación. En desarrollo local se puede activar manualmente el usuario en DB.

## 5. POST /login/

Autentica usando email o nickname en el campo `mail`.

### Request

```json
{
  "mail": "user@example.com",
  "password": "Test1234!"
}
```

También puede usarse nickname:

```json
{
  "mail": "Tester",
  "password": "Test1234!"
}
```

### Response OK

```json
{
  "error_code": 0,
  "content": {
    "user_id": 1,
    "token": "SESSION_OR_ACCESS_TOKEN"
  }
}
```

### Errores relevantes

```text
3  -> no POST
4  -> falta JSON/campos
12 -> password incorrecta
14 -> usuario no existe o no activo
```

### Recomendación cliente

Guardar en memoria:

```text
CurrentUserId
CurrentToken
```

No asumir que el token no caduca. Aunque ahora no parezca caducar, el cliente debería estar preparado para expiración futura.

## 6. GET/POST /validation/

Endpoint usado por enlace de email.

Actualmente recibe parámetros por query string:

```text
/validation?user_id=1&token=abc
```

### Response

Devuelve HTML, no JSON.

Uso recomendado:

```text
Solo navegador/email, no Unity client.
```

## 7. POST /get_users/

Devuelve usuarios activos con `transform` no nulo.

### Request actual

```json
{}
```

### Response OK

```json
{
  "error_code": 0,
  "content": [
    {
      "id": 1,
      "nickname": "Tester",
      "transform": {
        "position": {"x": 0, "y": 1, "z": 2},
        "rotation": {"x": 0, "y": 0, "z": 0, "w": 1}
      }
    }
  ]
}
```

Si no hay usuarios:

```json
{
  "error_code": 0,
  "content": []
}
```

### Errores relevantes

```text
3 -> no POST
```

### Advertencia importante

Este endpoint no debería ser la base de sincronización realtime de jugadores. Para realtime, usar Mirror/WebSocket.

Uso aceptable:

```text
Persistencia puntual.
Debug.
Recuperar estado inicial.
```

## 8. POST /update_users/

Actualiza el campo `transform` de uno o varios usuarios.

### Request

```json
{
  "usersUpdateData": [
    {
      "id": 1,
      "transform": {
        "position": {"x": 0, "y": 1, "z": 2},
        "rotation": {"x": 0, "y": 0, "z": 0, "w": 1}
      }
    }
  ]
}
```

### Response OK

```json
{
  "error_code": 0
}
```

### Errores relevantes

```text
3  -> no POST
4  -> falta usersUpdateData
10 -> error SQL/preparación/ejecución
16 -> usersUpdateData no es array
17 -> usuario sin id/transform válidos
```

### Riesgo actual

Este endpoint debería validar token antes de permitir cambios.

Contrato recomendado futuro:

```json
{
  "token": "SESSION_TOKEN",
  "usersUpdateData": [
    {
      "id": 1,
      "transform": {}
    }
  ]
}
```

O mejor, actualizar solo el usuario autenticado:

```text
POST /users/me/transform
```

## 9. POST /createActivationToken/

No debería consumirse directamente desde Unity.

Uso interno actual:

```text
/register
/login
```

Responsabilidad:

```text
Generar token y guardarlo en access_tokens.
```

## 10. /activationMail/

No debería consumirse directamente desde Unity.

Uso interno actual:

```text
/register -> createActivationToken -> activationMail
```

Responsabilidad:

```text
Enviar email de activación.
```

En entorno local debería poder desactivarse.

## 11. Modelo de datos actual/inferido

### users

```text
id          int
mail        string unique
password    password hash
nickname    string unique
active      bool/tinyint
transform   JSON/LONGTEXT nullable
last_login  datetime nullable
```

### access_tokens

```text
user_id     int primary/unique
token       string
created_at  datetime/timestamp recommended
```

## 12. Cliente Unity recomendado

No llamar endpoints desde scripts sueltos.

Crear:

```text
IAuthApiClient
IUserApiClient
AuthService
SessionService
ApiConfig
```

Flujo recomendado:

```text
LoginScreen
  -> AuthService.Login(mail, password)
      -> IAuthApiClient.LoginAsync
          -> POST /login/
      -> SessionService.Store(user_id, token)
```

Para get/update:

```text
UserPersistenceService
  -> IUserApiClient.GetUsersAsync
  -> IUserApiClient.UpdateUsersAsync
```

## 13. Contrato de entorno

### Local PC

```text
API: http://localhost/ElTesoroDeMongliAPI/
Game server: ws://localhost:7778
```

### Móvil LAN

```text
API: http://192.168.1.X/ElTesoroDeMongliAPI/
Game server: ws://192.168.1.X:7778
```

### Producción

```text
API: https://your-domain.example/ElTesoroDeMongliAPI/
Game server: wss://your-domain.example/game
```

## 14. Cambios recomendados en API v1.1

Sin romper demasiado:

```text
1. Sacar credenciales a config local/env.
2. Añadir .env.example o config.example.php.
3. Añadir migraciones SQL.
4. Normalizar respuesta JSON.
5. Añadir validación de token en get_users/update_users.
6. Añadir expiración de token.
7. Separar token de activación de token de sesión.
8. Permitir desactivar emails en local.
```

## 15. Cambios recomendados en API v2

Si se reestructura más adelante:

```text
POST /auth/register
POST /auth/login
GET  /auth/validate-email?user_id=...&token=...
GET  /users/active
PUT  /users/me/transform
GET  /users/me
POST /auth/logout
```

Y respuestas normalizadas:

```json
{
  "ok": true,
  "error_code": 0,
  "message": null,
  "content": {}
}
```

## 16. Regla final

La API debe tratar datos persistentes.

El estado realtime del juego debe tratarlo el servidor Mirror/WebSocket.

La base de datos no debería ser el bus principal de movimiento de jugadores.
