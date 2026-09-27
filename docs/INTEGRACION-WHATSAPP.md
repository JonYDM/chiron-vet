# 🐾 Chiron — Integración con WhatsApp (Meta)

> Requisitos oficiales de la WhatsApp Business Platform para los recordatorios (diferenciador de Chiron).
> Fuente: documentación oficial de Meta (developers.facebook.com/docs/whatsapp/pricing), consultada el 2026-09-26 (doc actualizada 2026-09-10).
> Última actualización: 2026-09-26

---

## Plataforma correcta

- Servicio oficial: **WhatsApp Business Platform → Cloud API** (alojada por Meta).
- ❌ Prohibido: WhatsApp normal / WhatsApp Business (app) para envíos automáticos, y librerías no oficiales (automatización de WhatsApp Web) → riesgo de baneo del número.
- ✅ Solo la vía oficial para un producto que se renta.

---

## Requisitos para poder enviar

1. **Cuenta de Meta Business** + **verificación de negocio** (documentos de la empresa).
2. **WhatsApp Business Account (WABA)** + **número de teléfono dedicado** (no usado en la app normal).
3. **App** en el panel de desarrolladores de Meta (tokens de acceso).
4. **Plantillas de mensaje aprobadas** por Meta (ver abajo).
5. **Opt-in del usuario**: Meta EXIGE consentimiento previo del destinatario para recibir mensajes. → Chiron debe registrar que el cliente aceptó recibir WhatsApp.
6. **Método de pago** configurado en el Billing Hub de Meta.

---

## Mensajes con plantilla (Template Messages)

- Para **iniciar** una conversación (caso de los recordatorios) NO se puede enviar texto libre: hay que usar una **plantilla pre-aprobada**.
- Categoría correcta para recordatorios: **"Utility" (utilidad)**.
- Las plantillas usan variables `{{1}}`, `{{2}}`... que se rellenan al enviar.
- Ejemplo:
  > "Hola {{1}}, le recordamos que {{2}} tiene {{3}} el {{4}} en {{5}}. Responda para confirmar."
- La aprobación de plantillas por Meta tarda de minutos a un par de días.

### Ventana de servicio al cliente (24h)
- Si el cliente escribe primero, hay 24h para responder con texto libre.
- Los recordatorios los inicia la veterinaria (fuera de ventana) → se usa plantilla y se cobra.

---

## 💰 Precios (modelo por mensaje) y CAMBIO DEL 1-OCT-2026

- Modelo vigente (desde 1-jul-2025): se cobra **por mensaje de plantilla entregado**, según **categoría** y **país del destinatario**. Sin cuota mensual por la API.
- Meta actualiza precios solo el 1 de ene/abr/jul/oct.

### ⚠️ Cambios a partir del 1 de octubre de 2026 (confirmado en doc oficial)
1. **Mensajes de servicio pasan a ser de pago** (antes gratis). Nivel gratuito: **1,000 mensajes de servicio por número de negocio al mes**; luego se cobran.
2. **Plantillas de utilidad dentro de la ventana de 24h dejan de ser gratis** (antes gratis).
3. **México: aumento en tarifas de marketing**.

### Tarifas de referencia (verificar SIEMPRE el CSV oficial en MXN al lanzar)
- Norteamérica: utilidad y autenticación ≈ **$0.0034 USD/mensaje**; marketing ≈ $0.025 USD.
- Meta publica hojas de tarifas oficiales en **MXN** (CSV/PDF) — usar esas al momento de lanzar.

---

## Impacto en el modelo de negocio de Chiron

- Los recordatorios tienen **costo variable por mensaje** (bajo, pero se multiplica por volumen).
- Decisión pendiente de cobro a la veterinaria:
  - (a) Incluir X recordatorios en la mensualidad y cobrar el excedente, o
  - (b) La veterinaria conecta su propia cuenta y paga directo a Meta.
- El cambio del 1-oct-2026 encarece el escenario (menos gratis) → contemplarlo en el precio de renta.

### Modelos de proveedor (multi-tenant)
- **Modelo A**: cada veterinaria con su propia WABA/número (paga lo suyo; config compleja para negocios chicos).
- **Modelo B**: Chiron como **Tech Provider / Solution Partner** de Meta (gestiona la infra; más requisitos para nosotros).
- Diseño recomendado: guardar la **config de WhatsApp por veterinaria (tenant)** para soportar ambos modelos.

---

## Decisiones de diseño para el código (Épica 5)

1. **Abstracción `IServicioMensajeria`** en Application → el dominio NO depende de Meta.
2. **Implementación de prueba** (`MensajeriaConsola`) para desarrollar/probar sin depender de trámites.
3. **Implementación real** (`MensajeriaWhatsAppCloud`) después, contra la Cloud API, con plantillas de utilidad.
4. Registrar **opt-in de WhatsApp** en el Cliente (requisito de Meta).
5. Modelar el concepto de **plantilla/tipo de recordatorio**.
6. Guardar **config de WhatsApp por Veterinaria** (número, tokens) para multi-tenant.

> Al activar WhatsApp real: reconsultar la doc oficial y el CSV de tarifas MXN de ese día (los precios cambian trimestralmente).
