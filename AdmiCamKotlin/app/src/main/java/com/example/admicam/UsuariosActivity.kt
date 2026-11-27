package com.example.admicam

import android.os.Bundle
import android.widget.ArrayAdapter
import android.widget.Button
import android.widget.EditText
import android.widget.ListView

class UsuariosActivity : PantallaBase() {
    private var usuarios = listOf<Usuario>()
    private lateinit var listado: ListView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_usuarios)
        listado = findViewById(R.id.listaRegistros)
        findViewById<Button>(R.id.btnVolver).setOnClickListener { finish() }
        val txtUsuario = findViewById<EditText>(R.id.txtNuevoUsuario)
        val txtNombre = findViewById<EditText>(R.id.txtNombre)
        val txtPassword = findViewById<EditText>(R.id.txtNuevaClave)
        val btnGuardar = findViewById<Button>(R.id.btnGuardar)

        btnGuardar.setOnClickListener {
            val usuario = txtUsuario.text.toString().trim().uppercase()
            val nombre = txtNombre.text.toString().trim()
            val clave = txtPassword.text.toString().trim()
            if (usuario.isEmpty() || nombre.isEmpty() || clave.isEmpty()) {
                mensaje("Todos los campos son obligatorios.")
            } else if (usuario.length > 20) {
                mensaje("El usuario no puede superar los 20 caracteres.")
            } else if (nombre.length > 50) {
                mensaje("El nombre completo no puede superar los 50 caracteres.")
            } else if (clave.length > 20) {
                mensaje("La contraseña no puede superar los 20 caracteres.")
            } else {
                // CÓDIGO ORIGINAL (no eliminado, solo comentado):
                // consultar(btnGuardar, { RetrofitCliente.ruta.crearUsuario(Usuario(usuario, nombre, clave, 1)) }) { _, aviso ->
                //     mensaje(aviso)
                //     txtUsuario.setText("")
                //     txtNombre.setText("")
                //     txtPassword.setText("")
                //     cargar()
                // }
                // CAMBIO (24-11-2026): se verifica que el NOM_PERSONA no esté
                // repetido antes de crear. Motivo: el login resuelve por nombre y
                // la API solo evita duplicar USUARIO (el ID); con dos personas con
                // el mismo nombre, una de las cuentas queda inaccesible.
                consultar(btnGuardar, { RetrofitCliente.ruta.listarUsuarios() }) { datos, _ ->
                    val repetido = nombre.equals("ADMINISTRADOR", ignoreCase = true) ||
                        datos.orEmpty().any { it.nombrePersona.equals(nombre, ignoreCase = true) }
                    if (repetido) {
                        mensaje("Ya existe un usuario con ese nombre completo.")
                    } else {
                        consultar(btnGuardar, { RetrofitCliente.ruta.crearUsuario(Usuario(usuario, nombre, clave, 1)) }) { _, aviso ->
                            mensaje(aviso)
                            txtUsuario.setText("")
                            txtNombre.setText("")
                            txtPassword.setText("")
                            cargar()
                        }
                    }
                }
            }
        }
        listado.setOnItemClickListener { _, _, posicion, _ ->
            val elegido = usuarios[posicion]
            val nuevoEstado = if (elegido.idEstado == 1) 2 else 1
            val accion = if (nuevoEstado == 1) "ACTIVO" else "INACTIVO"
            // CÓDIGO ORIGINAL (no eliminado, solo comentado):
            // confirmar("¿Cambiar el estado de ${elegido.usuario} a $accion?") {
            //     consultar(llamada = { RetrofitCliente.ruta.estadoUsuario(elegido.usuario, Estado(nuevoEstado)) }) { _, aviso ->
            // CAMBIO (24-11-2026): se usa idUsuario (antes usuario) porque la columna
            // USUARIO es el ID de la cuenta, no el nombre de usuario.
            confirmar("¿Cambiar el estado de ${elegido.idUsuario} a $accion?") {
                consultar(llamada = { RetrofitCliente.ruta.estadoUsuario(elegido.idUsuario, Estado(nuevoEstado)) }) { _, aviso ->
                    mensaje(aviso)
                    cargar()
                }
            }
        }
    }

    override fun onResume() {
        super.onResume()
        cargar()
    }

    private fun cargar() {
        consultar(llamada = { RetrofitCliente.ruta.listarUsuarios() }) { datos, _ ->
            // CÓDIGO ORIGINAL (no eliminado, solo comentado):
            // usuarios = datos.orEmpty().filter { !it.usuario.equals("ADMIN", true) }
            // listado.adapter = ArrayAdapter(this, android.R.layout.simple_list_item_1,
            //     usuarios.map { "${it.usuario} - ${it.nombrePersona}\n${if (it.idEstado == 1) "ACTIVO" else "INACTIVO"}" })
            // CAMBIO (24-11-2026): mismo renombraje a idUsuario. El filtro de ADMIN
            // se mantiene: ese ID no se muestra porque el login lo resuelve con un atajo.
            usuarios = datos.orEmpty().filter { !it.idUsuario.equals("ADMIN", true) }
            listado.adapter = ArrayAdapter(this, android.R.layout.simple_list_item_1,
                usuarios.map { "${it.idUsuario} - ${it.nombrePersona}\n${if (it.idEstado == 1) "ACTIVO" else "INACTIVO"}" })
        }
    }
}
