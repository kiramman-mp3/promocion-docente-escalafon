#!/usr/bin/env bash
# Prueba de punta a punta contra el stack de docker-compose recién creado (con SEED_DATA=true).
# Recorre los tres roles: permisos, solicitudes, revisión de documentos y gestión de usuarios.
#
# Uso:  docker compose down -v && docker compose up -d --build
#       bash tests/e2e/api-smoke.sh
#
# Modifica datos (crea y resuelve solicitudes), así que ejecútalo sobre una base recién creada.
set -euo pipefail

API_URL="${API_URL:-http://localhost:5270}"
FRONTEND_URL="${FRONTEND_URL:-http://localhost:8080}"
CONTRASENA="${CONTRASENA_PRUEBAS:-Uta2026*}"

CUERPO="$(mktemp)"
trap 'rm -f "$CUERPO"' EXIT
FALLAS=0
TOTAL=0

# peticion METODO RUTA [TOKEN] [JSON]  -> deja el código HTTP en $CODIGO y la respuesta en $CUERPO
peticion() {
    local metodo=$1 ruta=$2 token=${3:-} datos=${4:-}
    local args=(-s -o "$CUERPO" -w '%{http_code}' -X "$metodo" "$API_URL$ruta")
    [[ -n "$token" ]] && args+=(-H "Authorization: Bearer $token")
    [[ -n "$datos" ]] && args+=(-H 'Content-Type: application/json' -d "$datos")
    CODIGO=$(curl "${args[@]}")
}

# campo '.propiedad' -> lee un valor del último cuerpo JSON
campo() {
    node -e "const d=JSON.parse(require('fs').readFileSync(0,'utf8')); const v=d$1; console.log(typeof v==='object'?JSON.stringify(v):v)" < "$CUERPO"
}

esperar() {
    local esperado=$1 descripcion=$2
    TOTAL=$((TOTAL + 1))
    if [[ "$CODIGO" == "$esperado" ]]; then
        echo "  ok    $descripcion ($CODIGO)"
    else
        FALLAS=$((FALLAS + 1))
        echo "  FALLA $descripcion: se esperaba $esperado y llegó $CODIGO -> $(head -c 300 "$CUERPO")"
    fi
}

igual() {
    local obtenido=$1 esperado=$2 descripcion=$3
    TOTAL=$((TOTAL + 1))
    if [[ "$obtenido" == "$esperado" ]]; then
        echo "  ok    $descripcion"
    else
        FALLAS=$((FALLAS + 1))
        echo "  FALLA $descripcion: se esperaba '$esperado' y llegó '$obtenido'"
    fi
}

login() {
    peticion POST /api/Auth/login "" "{\"correo\":\"$1\",\"contrasena\":\"$CONTRASENA\"}"
    [[ "$CODIGO" == 200 ]] || { echo "No se pudo iniciar sesión como $1 ($CODIGO)"; exit 1; }
    campo .token
}

echo "== Esperando a la API en $API_URL"
for _ in $(seq 1 60); do
    [[ "$(curl -s -o /dev/null -w '%{http_code}' "$API_URL/api/roles")" == 200 ]] && break
    sleep 2
done

echo "== Público"
peticion GET /api/roles
esperar 200 "catálogo de roles sin autenticación"
igual "$(campo .length)" 8 "el escalafón tiene 8 roles"
peticion GET /swagger/v1/swagger.json
esperar 200 "documento OpenAPI de Swagger"
peticion POST /api/Auth/login "" '{"correo":"ana.perez@uta.edu.ec","contrasena":"incorrecta"}'
esperar 401 "login con contraseña incorrecta"
peticion POST /api/importar/tiempo-rol "" '{"cedula":"1801234567"}'
esperar 401 "importar sin token"

ANA=$(login ana.perez@uta.edu.ec)
CARLOS=$(login carlos.gomez@uta.edu.ec)
RRHH=$(login rrhh@uta.edu.ec)
ADMIN=$(login admin@uta.edu.ec)

echo "== Docente"
peticion POST /api/importar/tiempo-rol "$ANA" '{"cedula":"1801234567"}'
esperar 200 "consulta su propio tiempo en el rol"
peticion POST /api/importar/tiempo-rol "$ANA" '{"cedula":"1712345678"}'
esperar 403 "no puede consultar otra cédula"
peticion POST /api/promocion/validar "$ANA" '{"cedula":"1801234567","rolActualId":3}'
esperar 200 "valida su propia promoción"
igual "$(campo .rolObjetivo)" "Agregado 2" "el rol objetivo de Ana es Agregado 2"
peticion GET /api/usuarios "$ANA"
esperar 403 "no puede administrar usuarios"
peticion POST /api/solicitudes "$ANA" '{"cedula":"1801234567"}'
esperar 409 "no puede crear una segunda solicitud pendiente"
peticion POST /api/solicitudes "$CARLOS" '{"cedula":"1809876543"}'
esperar 201 "Carlos solicita su promoción"
SOLICITUD_CARLOS=$(campo .id)
peticion PUT "/api/solicitudes/$SOLICITUD_CARLOS/documentos" "$CARLOS" '{"tipoDocumento":"Obra","documentoId":5,"estado":"Aprobado"}'
esperar 403 "un docente no puede revisar documentos"

echo "== RRHH"
peticion GET "/api/solicitudes?estado=Pendiente" "$RRHH"
esperar 200 "bandeja de pendientes"
igual "$(campo .length)" 2 "hay dos solicitudes pendientes"
peticion POST /api/importar/tiempo-rol "$RRHH" '{"cedula":"1712345678"}'
esperar 200 "consulta cualquier cédula"
peticion PUT "/api/solicitudes/$SOLICITUD_CARLOS/documentos" "$RRHH" '{"tipoDocumento":"Obra","documentoId":5,"estado":"Rechazado"}'
esperar 400 "rechazar un documento exige motivo"
peticion PUT "/api/solicitudes/$SOLICITUD_CARLOS/documentos" "$RRHH" '{"tipoDocumento":"Obra","documentoId":5,"estado":"Rechazado","observacion":"Revista no indexada"}'
esperar 200 "rechaza un documento con motivo"
peticion PUT "/api/solicitudes/$SOLICITUD_CARLOS/resolucion" "$RRHH" '{"estado":"Aprobada"}'
esperar 409 "no puede aprobar con documentos rechazados"
peticion PUT "/api/solicitudes/$SOLICITUD_CARLOS/resolucion" "$RRHH" '{"estado":"Rechazada","observacion":"Complete los requisitos"}'
esperar 200 "rechaza la solicitud"
igual "$(campo .estado)" "Rechazada" "la solicitud queda rechazada"
peticion GET "/api/solicitudes/$SOLICITUD_CARLOS" "$CARLOS"
esperar 200 "el docente ve su solicitud resuelta"
igual "$(campo '.revisiones[0].observacion')" "Revista no indexada" "el docente ve el motivo del rechazo"

echo "== Administrador"
peticion PUT /api/solicitudes/1/documentos "$ADMIN" '{"tipoDocumento":"TiempoRol","documentoId":1,"estado":"Aprobado"}'
esperar 200 "aprueba un documento de Ana"
peticion PUT /api/solicitudes/1/resolucion "$ADMIN" '{"estado":"Aprobada"}'
esperar 200 "aprueba la promoción de Ana"
peticion POST /api/solicitudes "$ANA" '{"cedula":"1801234567"}'
esperar 409 "no se puede volver a pedir una promoción aprobada"
peticion POST /api/usuarios "$ADMIN" '{"correo":"e2e@uta.edu.ec","nombre":"Prueba","apellido":"E2E","rol":"RRHH","contrasena":"Temporal2026"}'
esperar 201 "crea un usuario"
NUEVO=$(campo .id)
peticion PUT "/api/usuarios/$NUEVO" "$ADMIN" '{"nombre":"Prueba","apellido":"E2E","rol":"RRHH","activo":false}'
esperar 200 "desactiva el usuario"
peticion POST /api/Auth/login "" '{"correo":"e2e@uta.edu.ec","contrasena":"Temporal2026"}'
esperar 401 "un usuario desactivado no puede entrar"
peticion GET /api/usuarios "$RRHH"
esperar 403 "RRHH no puede administrar usuarios"

echo "== Frontend"
CODIGO=$(curl -s -o "$CUERPO" -w '%{http_code}' "$FRONTEND_URL/")
esperar 200 "el frontend responde en $FRONTEND_URL"
TOTAL=$((TOTAL + 1))
if grep -q "Promoción docente UTA" "$CUERPO"; then echo "  ok    sirve la aplicación"; else FALLAS=$((FALLAS + 1)); echo "  FALLA el HTML no es la aplicación"; fi

echo
echo "$((TOTAL - FALLAS))/$TOTAL comprobaciones correctas"
[[ $FALLAS -eq 0 ]]
