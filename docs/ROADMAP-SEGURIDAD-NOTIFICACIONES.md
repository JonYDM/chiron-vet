# 🐾 Chiron — Roadmap: Seguridad, Roles y Notificaciones

> Decisiones estratégicas tomadas el 2026-09-26 para implementar más adelante.
> Este documento registra el QUÉ y el PORQUÉ; la implementación se hará en sus épicas.

---

## 1. GIRO: WhatsApp → Notificaciones Push (in-app)

### Decisión
Los recordatorios (vacunas, desparasitación, baño, citas) se entregarán mediante
**notificaciones push / in-app** dentro de la propia app web, NO por WhatsApp (al menos en la v1).

### Por qué (justificación de negocio)
- **Costo cero**: las push notifications no se cobran por mensaje (WhatsApp sí).
- **Sin fricción en LATAM**: los veterinarios de Morelos difícilmente querrán crear una
  cuenta de Meta Business, verificar negocio y pagar por mensajes. Es una barrera de venta.
- **Control propio**: no dependemos de precios/políticas de Meta (que cambian cada trimestre).
- **Plus competitivo**: se habilita un **rol para el dueño de la mascota** (ver abajo).

### WhatsApp queda como FEATURE FUTURA (issue)
- Se implementará solo si un cliente real lo pide/necesita.
- El diseño ya está preservado en `INTEGRACION-WHATSAPP.md` (abstracción `IServicioMensajeria`,
  cuenta por veterinaria/Modelo A, ventana de 24h para atención gratis, plantillas de utilidad).
- Se reaprovecha la lógica de detección de recordatorios (Épica 5) — solo cambia el canal
  de entrega gracias a la abstracción `IServicioMensajeria`.

### Notas técnicas de push (a considerar en implementación)
- **Web Push API** (gratis) requiere consentimiento del navegador; funciona mejor como **PWA**.
- **iOS/Safari**: push web limitada (requiere que el usuario "instale" la PWA). Android/Chrome: OK.
- Alternativa futura para app móvil: Firebase Cloud Messaging (FCM), también gratis.
- Para v1: recordatorios visibles **in-app** + push web donde el navegador lo soporte.

---

## 2. Nuevo rol: Dueño de mascota

### Decisión
Agregar un rol para el **cliente final** (dueño de la mascota) con acceso limitado a la app web.

### Qué puede hacer (alcance)
- Ver **solo sus** mascotas y su historial médico.
- Ver sus próximas vacunas/citas (recordatorios).
- (Futuro) Agendar/solicitar citas.
- Recibir notificaciones push de recordatorios.

### Qué NO puede hacer
- Ver datos de otros clientes ni información interna de la veterinaria.
- Acceder a ventas, catálogo, otros expedientes, etc.

### Valor de negocio
Diferenciador fuerte: da al cliente final control y visibilidad, y hace el producto
más completo para vender a la veterinaria.

---

## 3. ÉPICA DE SEGURIDAD: Autenticación y Autorización (CRÍTICA)

> La seguridad se valida SIEMPRE en el backend, nunca solo en el frontend.

### Componentes a implementar
1. **Autenticación con JWT** (JSON Web Tokens, estándar nativo de .NET):
   - Login (correo + contraseña) → backend valida → emite token JWT firmado con clave secreta.
   - El token incluye el rol del usuario y su VeterinariaId (tenant).
   - El frontend envía el token en cada petición; el backend verifica firma y autoriza.
   - Contraseñas almacenadas con **hash seguro** (ej. BCrypt/PBKDF2), nunca en texto plano.

2. **Roles predefinidos en el backend** y autorización por endpoint:
   - `[Authorize(Roles = "...")]` en cada endpoint según quién puede accederlo.
   - Aunque alguien llame la API directamente, el backend rechaza (401/403) sin el rol correcto.

3. **Aislamiento multi-tenant reforzado**:
   - Cada petición usa el `VeterinariaId` del token; un usuario solo ve datos de SU veterinaria.

### Jerarquía de roles

```
🔑 SuperAdmin (dueño de Chiron = JonYDM)
   - Da de alta / baja veterinarias (tenants)
   - Activa / desactiva por estado de pago (suscripción)
   - Ve todas las veterinarias
   ─────────────────────────────
   Veterinaria (tenant)
     ├── Administrador  (dueño/gerente de la veterinaria)
     ├── Veterinario    (atiende, expedientes)
     ├── Recepcionista  (agenda, registro)
     └── Dueño de mascota (ve solo lo suyo)
```

### Endpoints de SuperAdmin (pendientes)
- Crear veterinaria + su usuario administrador inicial.
- Activar / desactivar veterinaria (control de suscripción/pago).
- Listar todas las veterinarias y su estado.
- ⚠️ Actualmente `POST /api/veterinarias` está ABIERTO — debe protegerse solo para SuperAdmin.

### Control de suscripción (impago)
- La entidad `Veterinaria` ya tiene `Activa` + métodos `Activar()`/`Desactivar()`.
- Falta: endpoints de SuperAdmin para usarlos, y **bloqueo de acceso** (backend) a
  veterinarias desactivadas (rechazar login/peticiones si el tenant está inactivo).

---

## Orden sugerido de implementación (futuro)
1. Épica de Seguridad (JWT + roles + hash de contraseñas) — base para todo lo demás.
2. Rol SuperAdmin + endpoints de gestión de veterinarias y suscripción.
3. Rol Dueño de mascota (acceso limitado).
4. Notificaciones push / recordatorios in-app (reusando lógica de Épica 5).
5. (Futuro / bajo demanda) WhatsApp real.

---

## Pendientes técnicos anotados
- Proteger `POST /api/veterinarias` (hoy abierto) → solo SuperAdmin.
- Agregar entidad/tabla de credenciales y contraseñas de Usuario (con hash).
- La entidad Veterinaria necesitará (si vuelve WhatsApp) campos de credenciales de WhatsApp por tenant.

---

## Puesta en marcha en PRODUCCIÓN (bootstrap del SuperAdmin)

Implementado (rama H9.5): al arrancar en modo PostgreSQL, la API crea el SuperAdmin inicial
**solo si no existe** y **solo si** están configuradas las variables de entorno. Credenciales
NUNCA en el código.

### Variables de entorno a configurar en Railway (servicio de la API)
- `Jwt__Clave` = clave secreta larga (mín. 32 caracteres) para firmar los JWT. **Obligatoria en prod.**
- `SuperAdmin__Usuario` = identificador del SuperAdmin (ej. "jonocampo").
- `SuperAdmin__Pin` = PIN de 6 dígitos del SuperAdmin.
- (Ya configurada) `ConnectionStrings__Chiron` = cadena de PostgreSQL.

> Nota: el doble guion bajo `__` es la convención de .NET para representar secciones
> anidadas (`Jwt:Clave`, `SuperAdmin:Usuario`) en variables de entorno.

### Flujo de alta una vez desplegado
1. Configurar las variables → redeploy → se crea el SuperAdmin automáticamente.
2. SuperAdmin hace login (`POST /api/auth/login`).
3. SuperAdmin crea veterinarias (`POST /api/admin/veterinarias`) y su admin (`POST /api/admin/usuarios-admin`).
4. El Administrador de cada veterinaria crea su staff (`POST /api/usuarios/staff`) y accesos de dueños (`POST /api/usuarios/dueno`).
5. Control de pago: activar/desactivar veterinarias desde los endpoints de SuperAdmin.

### Seguridad recomendada tras el primer arranque
- Una vez creado el SuperAdmin, se pueden **quitar** las variables `SuperAdmin__*` (ya no se necesitan; el usuario queda en la BD).
- Cambiar el PIN del SuperAdmin por uno definitivo.
