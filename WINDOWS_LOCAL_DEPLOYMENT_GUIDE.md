# Windows local deployment guide - ElTesoroDeMongli

Guía para levantar en un PC Windows de desarrollo:

- Cliente Unity/WebGL.
- Servidor Unity/Mirror.
- API PHP.
- Base de datos MySQL/MariaDB.

No es una guía de producción. Es una guía práctica para desarrollo local.

## 1. Stack detectado

### Juego

```text
Repo: Alejarkor/ElTesoroDeMongli
Unity: 2021.3.4f1
Escena activa: Assets/Scenes/MONGLICLIENT.unity
Networking: Mirror
Transporte WebGL disponible: SimpleWebTransport ws/wss
```

Como el cliente es WebGL/navegador, la conexión de juego debe usar WebSocket:

```text
Desarrollo local HTTP: ws://localhost:7778
Producción HTTPS: wss://your-domain.example
```

### API

```text
Repo: sergio4anso/ElTesoroDeMongliAPI
Stack: PHP procedural + MySQL/MariaDB
Correo: PHPMailer
```

Endpoints detectados:

```text
/register
/login
/validation
/get_users
/update_users
/createActivationToken
/activationMail
```

La API espera estar bajo:

```text
/ElTesoroDeMongliAPI/
```

porque los scripts usan rutas basadas en `DOCUMENT_ROOT`.

En XAMPP debe quedar así:

```text
C:\xampp\htdocs\ElTesoroDeMongliAPI
```

## 2. Arquitectura local recomendada

```text
Windows PC
├── XAMPP Apache
│   ├── http://localhost/ElTesoroDeMongliAPI/
│   └── http://localhost/ElTesoroDeMongliWebGL/
├── XAMPP MariaDB/MySQL
│   └── database: eltesorodemongli
├── Unity Editor
└── Unity/Mirror server
    └── ws://localhost:7778
```

Para móvil en la misma Wi-Fi:

```text
http://192.168.1.X/ElTesoroDeMongliWebGL/
http://192.168.1.X/ElTesoroDeMongliAPI/
ws://192.168.1.X:7778
```

No uses `localhost` desde el móvil. En el móvil, `localhost` apunta al propio móvil.

## 3. Instalar herramientas

Instala:

```text
Git for Windows
Unity Hub
Unity 2021.3.4f1 o compatible 2021.3 LTS
WebGL Build Support para esa versión de Unity
XAMPP para Windows con Apache + MariaDB + phpMyAdmin
VS Code, Rider o similar
```

## 4. Clonar repositorios

Juego:

```bash
git clone https://github.com/Alejarkor/ElTesoroDeMongli.git C:\dev\ElTesoroDeMongli
```

API:

```bash
git clone https://github.com/sergio4anso/ElTesoroDeMongliAPI.git C:\xampp\htdocs\ElTesoroDeMongliAPI
```

Comprueba que existen:

```text
C:\xampp\htdocs\ElTesoroDeMongliAPI\connection.php
C:\xampp\htdocs\ElTesoroDeMongliAPI\login\index.php
C:\xampp\htdocs\ElTesoroDeMongliAPI\register\index.php
```

## 5. Arrancar Apache y MySQL

Abre XAMPP Control Panel y arranca:

```text
Apache
MySQL
```

Prueba:

```text
http://localhost/
http://localhost/phpmyadmin/
```

## 6. Crear base de datos

En phpMyAdmin, ejecuta:

```sql
CREATE DATABASE eltesorodemongli
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;
```

Crea un usuario local de desarrollo:

```sql
CREATE USER 'mongli_dev'@'localhost' IDENTIFIED BY 'CHANGE_ME_LOCAL_PASSWORD';
GRANT ALL PRIVILEGES ON eltesorodemongli.* TO 'mongli_dev'@'localhost';
FLUSH PRIVILEGES;
```

## 7. Crear tablas mínimas

Ejecuta dentro de `eltesorodemongli`:

```sql
CREATE TABLE users (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    mail VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    nickname VARCHAR(20) NOT NULL UNIQUE,
    active TINYINT(1) NOT NULL DEFAULT 0,
    transform JSON NULL,
    last_login DATETIME NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE access_tokens (
    user_id INT UNSIGNED NOT NULL PRIMARY KEY,
    token CHAR(64) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_access_tokens_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE,
    INDEX idx_access_tokens_token (token)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

Si tu MySQL/MariaDB falla con `JSON`, cambia:

```sql
transform JSON NULL
```

por:

```sql
transform LONGTEXT NULL
```

## 8. Configurar connection.php

Edita:

```text
C:\xampp\htdocs\ElTesoroDeMongliAPI\connection.php
```

Usa credenciales locales, no credenciales reales:

```php
<?php
$servername = "localhost";
$username = "mongli_dev";
$password = "CHANGE_ME_LOCAL_PASSWORD";
$dbname = "eltesorodemongli";

$conn = new mysqli($servername, $username, $password, $dbname);

if ($conn->connect_error) {
    http_response_code(500);
    die(json_encode([
        "error_code" => 500,
        "message" => "Database connection failed"
    ]));
}

$conn->set_charset("utf8mb4");
?>
```

Importante:

```text
No commitear credenciales reales.
Rotar cualquier credencial antigua antes de exponer el proyecto.
```

## 9. Probar API con PowerShell

### Register

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost/ElTesoroDeMongliAPI/register/" `
  -ContentType "application/json" `
  -Body '{"mail":"test@example.com","password":"Test1234!","nickname":"Tester"}'
```

Respuesta esperada:

```json
{"error_code":0}
```

### Activar usuario manualmente en desarrollo

El registro intenta enviar email. Para desarrollo, puedes activar manualmente:

```sql
UPDATE users SET active = 1 WHERE mail = 'test@example.com';
```

### Login

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost/ElTesoroDeMongliAPI/login/" `
  -ContentType "application/json" `
  -Body '{"mail":"test@example.com","password":"Test1234!"}'
```

Respuesta esperada:

```json
{
  "error_code": 0,
  "content": {
    "user_id": 1,
    "token": "..."
  }
}
```

### Update users

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost/ElTesoroDeMongliAPI/update_users/" `
  -ContentType "application/json" `
  -Body '{"usersUpdateData":[{"id":1,"transform":{"position":{"x":0,"y":1,"z":2},"rotation":{"x":0,"y":0,"z":0,"w":1}}}]}'
```

### Get users

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost/ElTesoroDeMongliAPI/get_users/" `
  -ContentType "application/json" `
  -Body '{}'
```

## 10. Configurar Unity cliente

En Unity busca referencias a:

```text
ApiCaller
LoginController
ElTesoroDeMongliAPI
login/
register/
get_users/
update_users/
```

Para pruebas en el mismo PC:

```text
API base URL: http://localhost/ElTesoroDeMongliAPI/
```

Para móvil en LAN:

```text
API base URL: http://192.168.1.X/ElTesoroDeMongliAPI/
```

## 11. Configurar Mirror/WebGL

En Unity abre la escena activa y localiza el `NetworkManager`.

Confirma:

```text
Transport: SimpleWebTransport
Puerto: 7778 o el configurado
Local HTTP: ws://
Producción HTTPS: wss://
```

Si sirves el juego por `https://`, el navegador bloqueará `ws://`. En ese caso necesitas `wss://`.

## 12. Ejecutar servidor de juego

### Desde Unity Editor

1. Abre la escena de servidor si existe, por ejemplo `MONGLISERVER`.
2. Confirma `SimpleWebTransport`.
3. Dale a Play.
4. Arranca Server/Host desde la UI o lógica del proyecto.

### Build Windows

Build Standalone Windows x86_64 y ejecútalo:

```powershell
cd C:\dev\ElTesoroDeMongliBuilds\Server
.\ElTesoroDeMongli.exe -batchmode -nographics -logFile server.log
```

Si abre ventana normal, no pasa nada para desarrollo, pero no es un servidor headless puro.

## 13. Build WebGL

En Unity:

```text
Build Settings -> WebGL
Scene: Assets/Scenes/MONGLICLIENT.unity
Build path: C:\xampp\htdocs\ElTesoroDeMongliWebGL
```

Abre:

```text
http://localhost/ElTesoroDeMongliWebGL/
```

Desde móvil:

```text
http://192.168.1.X/ElTesoroDeMongliWebGL/
```

## 14. Firewall Windows

Permite entrada para:

```text
Apache / puerto 80
Unity game server
Puerto del juego, por ejemplo TCP 7778
```

## 15. Orden de prueba recomendado

```text
1. Arrancar Apache y MySQL.
2. Crear DB y tablas.
3. Configurar connection.php.
4. Probar register/login/get_users/update_users con PowerShell.
5. Abrir Unity.
6. Arrancar servidor Mirror.
7. Compilar WebGL a htdocs.
8. Probar en http://localhost.
9. Probar desde móvil con IP LAN.
10. Solo después pensar en HTTPS/WSS público.
```

## 16. Problemas comunes

### Login devuelve error_code 14

Usuario no existe o no está activo.

Solución local:

```sql
UPDATE users SET active = 1 WHERE mail = 'test@example.com';
```

### WebGL en móvil no conecta

Comprueba:

```text
No usar localhost en móvil
Usar IP LAN del PC
Firewall abierto
Móvil y PC en misma Wi-Fi
```

### HTTPS bloquea WebSocket

```text
http://  -> ws://
https:// -> wss://
```

## 17. Antes de exponer públicamente

Obligatorio:

```text
Rotar credenciales antiguas.
No commitear credenciales reales.
Usar HTTPS.
Usar WSS.
Quitar phpMyAdmin de acceso público.
Añadir expiración de tokens.
Validar token en endpoints protegidos.
Añadir CORS si API y juego están en dominios distintos.
```

Ahora mismo `get_users` y `update_users` no parecen validar token. Vale para desarrollo local, no para producción.
