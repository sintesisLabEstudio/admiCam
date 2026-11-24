# admiCam
administrador de camiones para ramo de TNT en c$  webAPI  sqlserver - manejable luego con Kotlin

## Login: `USUARIO` es el ID y `NOM_PERSONA` es el nombre mostrado

> NOTA (24-11-2026): se documenta según el documento **original**
> (`Evaluacion 3 TNT -original.docx`) y `EV3.sql`, que son las fuentes que mandan.
> El original dice: *"por defecto debe ingresar con el **usuario ADMIN** y
> contraseña **ADMIN** que se encuentran en la base de datos"* y *"Bienvenido:
> NOM_PERSONA"*. En `EV3.sql` la fila con `USUARIO = 'ADMIN'` tiene
> `NOM_PERSONA = 'ADMINISTRADOR'`, `PASSWORD = 'ADMIN'` e `ID_ESTADO = 1`.
> Por tanto **en la pantalla se escribe el ID (`ADMIN`)** y `ADMINISTRADOR` es
> solo el que aparece en el mensaje de bienvenida.
> Las anotaciones que se agregaron después al doc (*"ES ID NO ES NOMBRE"*,
> *"ESTA MAL REDACTADO"* y el ejemplo con `USUARIO = 111`) **no son del profesor**.

```json
{
  "USUARIO": "ADMIN",
  "NOM_PERSONA": "ADMINISTRADOR",
  "PASSWORD": "ADMIN",
  "ID_ESTADO": 1
}
```

| Columna | Qué es realmente | Valor | Para qué se usa |
|---|---|---|---|
| `USUARIO` | ID de la cuenta (lo que se escribe en la app) | `ADMIN` | identificar la fila y grabarse en `SALIDA.USUARIO` |
| `NOM_PERSONA` | nombre de la persona | `ADMINISTRADOR` | se envía a `validaLogin` y se muestra en *Bienvenido* |
| `PASSWORD` | contraseña | `ADMIN` | **validar el acceso** |
| `ID_ESTADO` | activo (1) / inactivo (2) | `1` | rechazar usuarios inactivos |

### Cómo se cae en la trampa

- Mandar `{"USUARIO":"ADMIN","PASSWORD":"ADMIN"}` a `validaLogin` da
  `400` → *"El campo NOM_PERSONA es obligatorio."* (el endpoint no recibe
  `USUARIO`).
- Escribir `ADMINISTRADOR` en el campo "Usuario" de la app daba
  *"El usuario no existe."* porque la app buscaba por `USUARIO` (el ID), donde
  están `ADMIN`, `20`, `5`, `T01`... y `ADMIN` ni siquiera aparece en
  `listarUsuarios` (esa consulta lo excluye). Ahora `ADMINISTRADOR` se acepta
  como **alias** del administrador, sin cambiar la forma principal de entrar.

### Flujo correcto

1. En la app se escribe el **ID del usuario (`USUARIO`)**: `ADMIN`.
2. `LoginActivity` lo traduce a `NOM_PERSONA = ADMINISTRADOR` (atajo directo,
   porque `ADMIN` está excluido de `listarUsuarios`; para los demás usuarios se
   busca su ID con `listarUsuarios` y se toma su `NOM_PERSONA`).
3. Se envía `{"NOM_PERSONA":"ADMINISTRADOR","PASSWORD":"ADMIN"}` a `validaLogin`.
4. La API devuelve el `USUARIO` y la app comprueba que coincida con lo escrito
   (el `ORDER BY` de la consulta prioriza `USUARIO = 'ADMIN'`, así que la cuenta
   `20`, que también se llama `ADMINISTRADOR`, no puede suplantarlo).
5. El menú muestra **Bienvenido: ADMINISTRADOR**, que es el `NOM_PERSONA` leído
   de la tabla `USUARIOS`.

**Respuesta correcta en la app:** `ADMIN` / `ADMIN` (`ADMINISTRADOR` / `ADMIN`
también funciona como alias del mismo usuario).

## Pruebas desde celular físico (USB)

> NOTA (24-11-2026): se agrega esta sección para dejar documentado por qué la app
> apunta a `127.0.0.1:5000` y no a `10.0.2.2:5000`. El alias `10.0.2.2` solo existe
> en el emulador de Android Studio; en un celular físico la conexión falla con
> `failed to connect to /10.0.2.2 (port 5000)`. La dirección se configuró en
> `AdmiCamKotlin/gradle.properties` (`api.url`) y se inyecta en la app mediante
> `BuildConfig.URL_API`.

1. Tener la API corriendo en `http://localhost:5000` (perfil `http` o `https`, en
   Development ya no redirige a HTTPS).
2. Conectar el celular por USB con la depuración activada.
3. Ejecutar `AdmiCamKotlin/conectarApiUSB.bat` (o `adb reverse tcp:5000 tcp:5000`).
   Esto redirige el puerto 5000 del celular al PC; hay que repetirlo cada vez que
   se desconecte el cable o se reinicie el teléfono.
4. Abrir AdmiCam en el celular e iniciar sesión con `ADMIN` / `ADMIN`
   (`ADMINISTRADOR` / `ADMIN` también funciona como alias).

Para usar emulador en lugar de celular, cambiar `api.url` a `http://10.0.2.2:5000/`
en `gradle.properties` y volver a compilar/instalar.
