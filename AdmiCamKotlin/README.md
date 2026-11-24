# AdmiCam

Aplicación Android desarrollada con Kotlin y vistas XML. Consume una API REST mediante Retrofit y muestra información de SQL Server a través del servidor.

## Configuración

1. Abrir esta carpeta en Android Studio y sincronizar Gradle.
2. Iniciar la API `Ev3Api` con su conexión a SQL Server y verificar su respuesta en `http://localhost:5000/api/Camiones/listarPlantas`.
3. Ejecutar `app` en el emulador de Android Studio.
4. Para el emulador, `RetrofitCliente.kt` utiliza `http://10.0.2.2:5000/`. En un celular físico se debe indicar una dirección accesible desde la red y configurar el servidor para aceptar esas conexiones.

## Módulos

- Acceso y menú principal.
- Registro y administración de usuarios.
- Registro y estado de conductores y camiones.
- Registro de plantas.
- Registro de salidas y retornos.

## Integración

El endpoint `validaLogin` recibe el campo `NOM_PERSONA`. La pantalla permite ingresar el código `USUARIO` y resuelve el nombre correspondiente antes de enviar la solicitud. Para `ADMIN`, el nombre asociado en los datos de referencia es `ADMINISTRADOR`. El código de la API y la base de datos se administran por separado.

Los mensajes de error se muestran dentro de la aplicación. La conexión requiere que la API esté iniciada y disponible desde el dispositivo donde se ejecuta Android.
