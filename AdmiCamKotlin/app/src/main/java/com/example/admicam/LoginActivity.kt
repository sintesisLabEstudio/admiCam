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
                !datos.idUsuario.equals(usuarioEsperado, ignoreCase = true) ->
                    mensaje("Los datos devueltos no corresponden al usuario ingresado.")
                datos.idEstado != 1 -> mensaje("El usuario está INACTIVO.")
                else -> {
                    Sesion.usuario = datos.idUsuario
                    Sesion.nombre = datos.nombrePersona
                    startActivity(Intent(this, MenuActivity::class.java))
                    finish()
                }
            }
        }
    }
}