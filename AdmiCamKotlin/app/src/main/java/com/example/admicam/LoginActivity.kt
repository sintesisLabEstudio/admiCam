package com.example.admicam

import android.content.Intent
import android.os.Bundle
import android.widget.Button
import android.widget.EditText

class LoginActivity : PantallaBase() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_login)

        val txtUsuario = findViewById<EditText>(R.id.txtUsuario)
        val txtPassword = findViewById<EditText>(R.id.txtPassword)
        val btnIngresar = findViewById<Button>(R.id.btnIngresar)

        findViewById<Button>(R.id.btnLimpiar).setOnClickListener {
            txtUsuario.setText("")
            txtPassword.setText("")
            mensaje("")
        }

        btnIngresar.setOnClickListener {
            val usuario = txtUsuario.text.toString().trim().uppercase()
            val password = txtPassword.text.toString().trim()
            if (usuario.isEmpty() || password.isEmpty()) {
                mensaje("Debe ingresar usuario y contraseña.")
                return@setOnClickListener
            }
            // ============================================================
            // CÓDIGO ORIGINAL (no eliminado, solo comentado):
            // El servicio de acceso valida el nombre de la persona.
            // if (usuario == "ADMIN") {
            //     iniciarSesion("ADMINISTRADOR", password, usuario, btnIngresar)
            // } else {
            //     consultar(btnIngresar, { RetrofitCliente.ruta.listarUsuarios() }) { usuarios, _ ->
            //         val registrado = usuarios.orEmpty().firstOrNull {
            //             it.usuario.equals(usuario, ignoreCase = true)
            //         }
            //         when {
            //             registrado == null -> mensaje("El usuario no existe.")
            //             registrado.idEstado != 1 -> mensaje("El usuario está INACTIVO.")
            //             registrado.nombrePersona.isBlank() -> mensaje("El usuario no tiene un nombre asociado.")
            //             else -> iniciarSesion(registrado.nombrePersona, password, usuario, btnIngresar)
            //         }
            //     }
            // }
            // ============================================================
            // CÓDIGO ANTERIOR (no eliminado, solo comentado): se había cambiado la
            // búsqueda a nombrePersona creyendo que el campo debía ser NOM_PERSONA.
            // val registrado = usuarios.orEmpty().firstOrNull {
            //     it.nombrePersona.equals(usuario, ignoreCase = true)
            // }
            // CAMBIO (24-11-2026): RE-AJUSTE al documento ORIGINAL ("Evaluacion 3 TNT
            // -original.docx") y a EV3.sql, que son los que mandan.
            // Motivo: el original dice "ingresar con el usuario ADMIN y contraseña
            // ADMIN que se encuentran en la base de datos". En EV3.sql la fila con
            // USUARIO = 'ADMIN' tiene NOM_PERSONA = 'ADMINISTRADOR', PASSWORD='ADMIN'
            // e ID_ESTADO=1. Por tanto EN LA PANTALLA SE ESCRIBE EL ID (ADMIN) y
            // ADMINISTRADOR es solo el nombre que se muestra en "Bienvenido:".
            // Se vuelve a buscar por idUsuario para los demás usuarios (sus IDs).
            // ADMINISTRADOR se conserva como alias extra del administrador.
            when {
                usuario == "ADMIN" || usuario == "ADMINISTRADOR" ->
                    iniciarSesion("ADMINISTRADOR", password, "ADMIN", btnIngresar)
                else -> consultar(btnIngresar, { RetrofitCliente.ruta.listarUsuarios() }) { usuarios, _ ->
                    val registrado = usuarios.orEmpty().firstOrNull {
                        it.idUsuario.equals(usuario, ignoreCase = true)
                    }
                    when {
                        registrado == null -> mensaje("El usuario no existe.")
                        registrado.idEstado != 1 -> mensaje("El usuario está INACTIVO.")
                        registrado.nombrePersona.isBlank() -> mensaje("El usuario no tiene un nombre asociado.")
                        else -> iniciarSesion(registrado.nombrePersona, password, registrado.idUsuario, btnIngresar)
                    }
                }
            }
        }
    }

    private fun iniciarSesion(nombrePersona: String, clave: String, usuarioEsperado: String, boton: Button) {
        consultar(boton, { RetrofitCliente.ruta.validarLogin(Credenciales(nombrePersona, clave)) }) { datos, _ ->
            when {
                datos == null -> mensaje("La API no entregó los datos del usuario.")
                // CAMBIO (24-11-2026): se explica esta comprobación, que ya existía.
                // Evita que otra cuenta con el mismo NOM_PERSONA (USUARIO 20 también es
                // ADMINISTRADOR) suplante al ID que se escribió en la pantalla.
                // CÓDIGO ORIGINAL (no eliminado, solo comentado):
                // !datos.usuario.equals(usuarioEsperado, ignoreCase = true) ->
                //     mensaje("Los datos devueltos no corresponden al usuario ingresado.")
                // CAMBIO (24-11-2026): se usa idUsuario porque la propiedad del modelo
                // se renombró para que el código diga la verdad: USUARIO es el ID.
                !datos.idUsuario.equals(usuarioEsperado, ignoreCase = true) ->
                    mensaje("Los datos devueltos no corresponden al usuario ingresado.")
                datos.idEstado != 1 -> mensaje("El usuario está INACTIVO.")
                else -> {
                    // CÓDIGO ORIGINAL (no eliminado, solo comentado):
                    // Sesion.usuario = datos.usuario
                    // CAMBIO (24-11-2026): mismo renombraje semántico, el JSON sigue siendo "USUARIO".
                    Sesion.usuario = datos.idUsuario
                    Sesion.nombre = datos.nombrePersona
                    startActivity(Intent(this, MenuActivity::class.java))
                    finish()
                }
            }
        }
    }
}
