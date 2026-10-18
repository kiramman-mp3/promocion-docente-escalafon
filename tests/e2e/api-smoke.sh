#!/usr/bin/env bash
# Prueba de punta a punta contra el stack de docker-compose recién creado (con SEED_DATA=true).
# Recorre el trámite completo de promoción: dictamen normativo, solicitud, evidencias, revisión
# documental de Talento Humano, votación de la Comisión Académica, acto administrativo, acción de
# personal, apelación ante el Consejo Universitario, auditoría y operación.
#
# Uso:  docker compose down -v && LOGIN_RATE_LIMIT=100 docker compose up -d --build
#       bash tests/e2e/api-smoke.sh
#
# Modifica datos (crea y resuelve solicitudes), así que ejecútalo sobre una base recién creada.
set -euo pipefail

API_URL="${API_URL:-http://localhost:5270}"
FRONTEND_URL="${FRONTEND_URL:-http://localhost:8080}"
CONTRASENA="${CONTRASENA_PRUEBAS:-Uta2026*}"
HOY="$(date -u +%F)"

TEMPORAL="$(mktemp -d)"
CUERPO="$TEMPORAL/cuerpo"
trap 'rm -rf "$TEMPORAL"' EXIT
FALLAS=0
TOTAL=0

# peticion METODO RUTA [TOKEN] [JSON]  -> deja el código HTTP en $CODIGO y la respuesta en $CUERPO
peticion() {
    local metodo=$1 ruta=$2 token=${3:-} datos=${4:-}
    local args=(-s -o "$CUERPO" -w '%{http_code}' -X "$metodo" "$API_URL$ruta")
    [[ -n "$token" ]] && args+=(-H "Authorization: Bearer $token")
    # El cuerpo va por archivo: en Windows los argumentos con tildes no llegan a curl como UTF-8
    if [[ -n "$datos" ]]; then
        printf '%s' "$datos" > "$TEMPORAL/datos.json"
        args+=(-H 'Content-Type: application/json; charset=utf-8' --data-binary "@$TEMPORAL/datos.json")
    fi
    CODIGO=$(curl "${args[@]}")
}

# subir RUTA TOKEN ARCHIVO TIPO DESCRIPCION  -> envío multipart de una evidencia
subir() {
    CODIGO=$(curl -s -o "$CUERPO" -w '%{http_code}' -X POST "$API_URL$1" -H "Authorization: Bearer $2" \
        -F "archivo=@$3" -F "tipo=$4" -F "descripcion=$5")
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

# revisar_todo ID TOKEN -> aprueba cada documento del expediente de la solicitud
revisar_todo() {
    peticion GET "/api/solicitudes/$1" "$2"
    local documentos
    documentos=$(node -e "const d=JSON.parse(require('fs').readFileSync(0,'utf8')); for (const x of d.documentos ?? []) console.log(x.tipo + ' ' + x.id)" < "$CUERPO")
    local aprobados=0 tipo id
    while read -r tipo id; do
        [[ -z "$tipo" ]] && continue
        peticion PUT "/api/solicitudes/$1/documentos" "$2" "{\"tipoDocumento\":\"$tipo\",\"documentoId\":$id,\"estado\":\"Aprobado\"}"
        [[ "$CODIGO" == 200 ]] && aprobados=$((aprobados + 1))
    done <<< "$documentos"
    igual "$aprobados" "$(grep -c . <<< "$documentos")" "Talento Humano revisa los documentos del expediente $1"
}

# votar ID TOKEN VOTO [CERTIFICA]
votar() {
    local extra=""
    [[ -n "${4:-}" ]] && extra=",\"certificaPresupuesto\":$4"
    peticion PUT "/api/solicitudes/$1/votos" "$2" "{\"voto\":\"$3\",\"motivacion\":\"Revisado el expediente y el dictamen\"$extra}"
}

echo "== Esperando a la API en $API_URL"
for _ in $(seq 1 90); do
    [[ "$(curl -s -o /dev/null -w '%{http_code}' "$API_URL/api/roles")" == 200 ]] && break
    sleep 2
done

echo "== Público"
peticion GET /api/roles
esperar 200 "catálogo de roles sin autenticación"
igual "$(campo .length)" 8 "el escalafón tiene 8 roles"
igual "$(campo '.filter(r => !r.promocionable).map(r => r.nombre).join()')" "Agregado 3,Principal 3" "Agregado 3 y Principal 3 no tienen promoción (CES art. 76)"
peticion GET /api/requisitos
esperar 200 "requisitos de promoción vigentes"
igual "$(campo .length)" 6 "hay requisitos para los seis grados promocionables"
peticion GET /health
esperar 200 "las cinco bases responden"
peticion GET /swagger/v1/swagger.json
esperar 200 "documento OpenAPI de Swagger"
peticion POST /api/Auth/login "" '{"correo":"ana.perez@uta.edu.ec","contrasena":"incorrecta"}'
esperar 401 "login con contraseña incorrecta"
peticion POST /api/importar/tiempo-rol "" '{"cedula":"1801234567"}'
esperar 401 "importar sin token"

ANA=$(login ana.perez@uta.edu.ec)
CARLOS=$(login carlos.gomez@uta.edu.ec)
JORGE=$(login jorge.ruiz@uta.edu.ec)
MARTA=$(login marta.villacres@uta.edu.ec)
LUCIA=$(login lucia.mora@uta.edu.ec)
RRHH=$(login rrhh@uta.edu.ec)
ADMIN=$(login admin@uta.edu.ec)
TERNA=$(login profesor.terna@uta.edu.ec)
HCU=$(login profesor.hcu@uta.edu.ec)
FINANCIERO=$(login financiero@uta.edu.ec)
peticion POST /api/Auth/login "" "{\"correo\":\"vicerrectorado@uta.edu.ec\",\"contrasena\":\"$CONTRASENA\"}"
igual "$(campo .esPresidenteComision)" true "el Vicerrector Académico preside la Comisión"
VICERRECTOR=$(campo .token)

echo "== Dictamen normativo"
peticion POST /api/importar/tiempo-rol "$ANA" '{"cedula":"1801234567"}'
esperar 200 "el docente consulta su propio tiempo en el rol"
peticion POST /api/importar/tiempo-rol "$ANA" '{"cedula":"1712345678"}'
esperar 403 "no puede consultar otra cédula"
peticion POST /api/promocion/validar "$ANA" '{"cedula":"1801234567"}'
esperar 200 "Ana obtiene su dictamen"
igual "$(campo .rolObjetivo)" "Agregado 2" "el rol objetivo de Ana es Agregado 2"
igual "$(campo .cumpleTodo)" true "Ana cumple todos los requisitos"
peticion POST /api/promocion/validar "$CARLOS" '{"cedula":"1809876543"}'
igual "$(campo .cumpleTodo)" false "Carlos no cumple los requisitos"
peticion POST /api/promocion/validar "$MARTA" '{"cedula":"1701111111"}'
igual "$(campo .cumpleTodo)" true "Marta cumple gracias a las excepciones del art. 77 y la transitoria quinta"
peticion POST /api/promocion/validar "$JORGE" '{"cedula":"1805555555"}'
igual "$(campo .promocionable)" false "Agregado 3 no se promueve: pasa a Principal por concurso"

echo "== Solicitudes"
peticion GET /api/usuarios "$ANA"
esperar 403 "un docente no administra usuarios"
peticion POST /api/solicitudes "$ANA" '{"cedula":"1801234567"}'
esperar 409 "Ana no puede tener dos solicitudes en trámite"
peticion POST /api/solicitudes "$CARLOS" '{"cedula":"1809876543"}'
esperar 400 "Carlos no cumple la permanencia mínima para solicitar (CES art. 99)"
peticion POST /api/solicitudes "$JORGE" '{"cedula":"1805555555"}'
esperar 400 "Jorge no puede solicitar desde Agregado 3"
peticion POST /api/solicitudes "$LUCIA" '{"cedula":"1712345678"}'
esperar 201 "Lucía solicita su promoción a Principal 2"
SOLICITUD_LUCIA=$(campo .id)
peticion POST /api/solicitudes "$MARTA" '{"cedula":"1701111111"}'
esperar 201 "Marta solicita su promoción"

echo "== Evidencias"
printf '%%PDF-1.4\n%% certificado de congreso\n%%%%EOF\n' > "$TEMPORAL/certificado.pdf"
printf 'no es un pdf' > "$TEMPORAL/foto.pdf"
subir /api/solicitudes/1/evidencias "$ANA" "$TEMPORAL/certificado.pdf" CertificadoCongreso "Ponencia en congreso internacional"
esperar 201 "Ana anexa un certificado en PDF"
EVIDENCIA=$(campo .id)
subir /api/solicitudes/1/evidencias "$ANA" "$TEMPORAL/foto.pdf" Otro "Archivo que no es PDF"
esperar 400 "se rechaza un archivo que no es PDF"
subir /api/solicitudes/1/evidencias "$CARLOS" "$TEMPORAL/certificado.pdf" Otro "Evidencia ajena"
esperar 403 "nadie más anexa evidencias a la solicitud de Ana"
CODIGO=$(curl -s -o "$CUERPO" -w '%{http_code}' -H "Authorization: Bearer $RRHH" "$API_URL/api/solicitudes/1/evidencias/$EVIDENCIA")
esperar 200 "Talento Humano descarga la evidencia"
igual "$(head -c 5 "$CUERPO")" "%PDF-" "la evidencia descargada es el PDF original"

echo "== Revisión documental"
peticion GET "/api/solicitudes?estado=RevisionDocumental" "$RRHH"
esperar 200 "bandeja de revisión documental"
igual "$(campo .length)" 3 "Ana, Lucía y Marta esperan revisión"
peticion POST /api/solicitudes/1/enviar-comision "$RRHH"
esperar 409 "no se envía a la Comisión sin revisar todos los documentos"
peticion PUT /api/solicitudes/1/documentos "$RRHH" '{"tipoDocumento":"TiempoRol","documentoId":1,"estado":"Rechazado"}'
esperar 400 "rechazar un documento exige motivo"
peticion PUT /api/solicitudes/1/documentos "$RRHH" '{"tipoDocumento":"Obra","documentoId":999,"estado":"Aprobado"}'
esperar 400 "no se revisa un documento ajeno al expediente"
peticion PUT /api/solicitudes/1/documentos "$ANA" '{"tipoDocumento":"TiempoRol","documentoId":1,"estado":"Aprobado"}'
esperar 403 "un docente no revisa documentos"
revisar_todo 1 "$RRHH"
peticion POST /api/solicitudes/1/enviar-comision "$RRHH"
esperar 200 "Talento Humano envía el expediente de Ana a la Comisión"
igual "$(campo .estado)" EnComision "la solicitud de Ana está en la Comisión"
subir /api/solicitudes/1/evidencias "$ANA" "$TEMPORAL/certificado.pdf" Otro "Fuera de plazo"
esperar 409 "ya no se anexan evidencias fuera de la revisión documental"

echo "== Comisión Académica: aprobación"
peticion GET /api/comision/miembros "$VICERRECTOR"
esperar 200 "integrantes de la Comisión"
igual "$(campo .length)" 6 "la Comisión tiene seis integrantes (UTA art. 47)"
votar 1 "$ANA" AFavor
esperar 403 "quien no integra la Comisión no vota"
votar 1 "$TERNA" AFavor true
esperar 400 "solo el Director Financiero certifica el presupuesto"
votar 1 "$VICERRECTOR" AFavor
esperar 200 "vota el Vicerrector Académico"
votar 1 "$HCU" AFavor
esperar 200 "vota el profesor designado por el HCU"
peticion POST /api/solicitudes/1/cerrar-votacion "$VICERRECTOR" '{"motivacion":"Cumple los requisitos"}'
esperar 409 "no se cierra la votación sin quórum"
votar 1 "$TERNA" AFavor
esperar 200 "vota un profesor de la terna"
votar 1 "$FINANCIERO" AFavor true
esperar 200 "el Director Financiero vota y certifica el presupuesto"
peticion POST /api/solicitudes/1/cerrar-votacion "$HCU" '{"motivacion":"Cumple los requisitos"}'
esperar 403 "solo la presidencia cierra la votación"
peticion POST /api/solicitudes/1/cerrar-votacion "$VICERRECTOR" '{"motivacion":"Cumple los requisitos del art. 76 del Reglamento de Carrera."}'
esperar 200 "la presidencia cierra la votación"
igual "$(campo .estado)" Aprobada "la promoción de Ana queda aprobada"
igual "$(campo .numeroResolucion)" "CAP-$(date -u +%Y)-0001" "la resolución tiene número"
igual "$(campo .actoDisponible)" true "se generó el acto administrativo"

CODIGO=$(curl -s -o "$CUERPO" -w '%{http_code}' -H "Authorization: Bearer $ANA" "$API_URL/api/solicitudes/1/acto")
esperar 200 "Ana descarga su resolución"
igual "$(head -c 5 "$CUERPO")" "%PDF-" "la resolución es un PDF"
CODIGO=$(curl -s -o "$CUERPO" -w '%{http_code}' -H "Authorization: Bearer $CARLOS" "$API_URL/api/solicitudes/1/acto")
esperar 403 "otro docente no descarga la resolución de Ana"

peticion PUT /api/solicitudes/1/accion-personal "$RRHH" "{\"numero\":\"AP-2026-0150\",\"fecha\":\"2099-01-01\"}"
esperar 400 "la acción de personal no puede tener fecha futura"
peticion PUT /api/solicitudes/1/accion-personal "$RRHH" "{\"numero\":\"AP-2026-0150\",\"fecha\":\"$HOY\"}"
esperar 200 "Talento Humano registra la acción de personal"
igual "$(campo .estado)" Ejecutada "la promoción de Ana queda ejecutada"
peticion POST /api/solicitudes "$ANA" '{"cedula":"1801234567"}'
esperar 409 "no se vuelve a pedir una promoción ya aprobada"

echo "== Comisión Académica: negativa y apelación"
revisar_todo "$SOLICITUD_LUCIA" "$ADMIN"
peticion POST "/api/solicitudes/$SOLICITUD_LUCIA/enviar-comision" "$ADMIN"
esperar 200 "el Administrador envía el expediente de Lucía a la Comisión"
votar "$SOLICITUD_LUCIA" "$LUCIA" AFavor
esperar 403 "Lucía no vota su propia solicitud (conflicto de interés)"
votar "$SOLICITUD_LUCIA" "$VICERRECTOR" EnContra
votar "$SOLICITUD_LUCIA" "$HCU" EnContra
votar "$SOLICITUD_LUCIA" "$TERNA" EnContra
votar "$SOLICITUD_LUCIA" "$FINANCIERO" Abstencion
esperar 200 "votan cuatro miembros"
peticion POST "/api/solicitudes/$SOLICITUD_LUCIA/cerrar-votacion" "$VICERRECTOR" '{"motivacion":"Las obras presentadas no son relevantes para el campo de conocimiento."}'
esperar 200 "la presidencia cierra la votación"
igual "$(campo .estado)" Rechazada "la promoción de Lucía queda negada"
peticion GET "/api/solicitudes/$SOLICITUD_LUCIA" "$LUCIA"
igual "$(campo .puedeApelar)" true "Lucía puede apelar dentro del término"
peticion POST "/api/solicitudes/$SOLICITUD_LUCIA/apelacion" "$ANA" '{"fundamentos":"No me corresponde"}'
esperar 403 "solo el docente afectado apela"
peticion POST "/api/solicitudes/$SOLICITUD_LUCIA/apelacion" "$LUCIA" '{"fundamentos":"Las obras sí pertenecen a mi campo de conocimiento; adjunto la certificación del par externo."}'
esperar 200 "Lucía apela ante el Consejo Universitario"
igual "$(campo .estado)" Apelada "la solicitud queda apelada"
peticion PUT "/api/solicitudes/$SOLICITUD_LUCIA/apelacion/resolucion" "$RRHH" '{"aceptada":true,"resolucionHCU":"0200-HCU-2026","motivacion":"x"}'
esperar 403 "solo el Administrador registra lo que resolvió el HCU"
peticion PUT "/api/solicitudes/$SOLICITUD_LUCIA/apelacion/resolucion" "$ADMIN" '{"aceptada":true,"resolucionHCU":"0200-HCU-2026","motivacion":"El par externo certifica la pertinencia de las obras."}'
esperar 200 "el HCU acepta la apelación"
igual "$(campo .estado)" Aprobada "la promoción de Lucía queda aprobada en apelación"
peticion POST "/api/solicitudes/$SOLICITUD_LUCIA/apelacion" "$LUCIA" '{"fundamentos":"Otra vez"}'
esperar 409 "la decisión del HCU es definitiva"

echo "== Administración y operación"
peticion POST /api/requisitos "$RRHH" '{}'
esperar 403 "Talento Humano no cambia los requisitos"
peticion POST /api/requisitos "$ADMIN" '{"gradoOrigen":3,"normativa":"Prueba","resolucion":"0000-HCU-2026","vigenteDesde":"2099-01-01","obrasTotales":1,"obrasEnGrado":1,"evaluacionMinima":60,"capacitacionHoras":96,"capacitacionPctPedagogica":25,"proyectosMeses":0,"requiereDireccionProyecto":false,"trabajosTitulacion":0,"titulacionNivelMinimo":"Grado"}'
esperar 400 "no se aceptan requisitos por debajo del mínimo del CES (evaluación 75 %)"
peticion GET "/api/auditoria?accion=Solicitudes.CerrarVotacion" "$ADMIN"
esperar 200 "bitácora de auditoría"
igual "$(campo '.total >= 2')" true "la bitácora registra los cierres de votación"
peticion GET /api/auditoria "$RRHH"
esperar 403 "Talento Humano no lee la bitácora"
peticion GET /api/reportes/uso "$ADMIN"
esperar 200 "reporte de utilización"
peticion POST /api/usuarios "$ADMIN" '{"correo":"e2e@uta.edu.ec","nombre":"Prueba","apellido":"E2E","rol":"Autoridad","contrasena":"Temporal2026"}'
esperar 201 "crea un usuario con rol Autoridad"
NUEVO=$(campo .id)
peticion PUT "/api/usuarios/$NUEVO" "$ADMIN" '{"nombre":"Prueba","apellido":"E2E","rol":"Autoridad","activo":false}'
esperar 200 "desactiva el usuario"
peticion POST /api/Auth/login "" '{"correo":"e2e@uta.edu.ec","contrasena":"Temporal2026"}'
esperar 401 "un usuario desactivado no puede entrar"
peticion GET /api/usuarios "$RRHH"
esperar 403 "Talento Humano no administra usuarios"

echo "== Frontend"
CODIGO=$(curl -s -o "$CUERPO" -w '%{http_code}' "$FRONTEND_URL/")
esperar 200 "el frontend responde en $FRONTEND_URL"
TOTAL=$((TOTAL + 1))
if grep -q "Promoción docente UTA" "$CUERPO"; then echo "  ok    sirve la aplicación"; else FALLAS=$((FALLAS + 1)); echo "  FALLA el HTML no es la aplicación"; fi

echo
echo "$((TOTAL - FALLAS))/$TOTAL comprobaciones correctas"
[[ $FALLAS -eq 0 ]]
