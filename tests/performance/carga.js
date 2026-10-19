// Prueba de carga de la API (Reglamento DITIC art. 23 f: pruebas de rendimiento).
// Uso:  docker run --rm -i --network host -e API_URL=http://localhost:5270 grafana/k6:0.54.0 run - < tests/performance/carga.js
import http from "k6/http";
import { check, sleep } from "k6";

const API = __ENV.API_URL || "http://localhost:5270";
const CONTRASENA = __ENV.CONTRASENA_PRUEBAS || "Uta2026*";

export const options = {
  scenarios: {
    consultas: {
      executor: "ramping-vus",
      startVUs: 1,
      stages: [
        { duration: "20s", target: 10 },
        { duration: "40s", target: 25 },
        { duration: "20s", target: 0 },
      ],
    },
  },
  // Umbrales de aceptación: menos de 1 % de errores; 95 % de las respuestas en menos de 800 ms
  // y el dictamen (que consulta las cinco bases) en menos de 1,5 s
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<800"],
    "http_req_duration{endpoint:dictamen}": ["p(95)<1500"],
  },
};

// Un solo inicio de sesión: el login tiene límite de intentos por IP
export function setup() {
  const respuesta = http.post(`${API}/api/Auth/login`, JSON.stringify({ correo: "rrhh@uta.edu.ec", contrasena: CONTRASENA }), {
    headers: { "Content-Type": "application/json" },
  });
  check(respuesta, { "login 200": (r) => r.status === 200 });
  return { token: respuesta.json("token") };
}

export default function ({ token }) {
  const params = (endpoint) => ({
    headers: { Authorization: `Bearer ${token}`, "Content-Type": "application/json" },
    tags: { endpoint },
  });
  const cedulas = ["1801234567", "1712345678", "1701111111", "1809876543"];
  const cedula = cedulas[Math.floor(Math.random() * cedulas.length)];

  check(http.get(`${API}/api/roles`, { tags: { endpoint: "roles" } }), { "roles 200": (r) => r.status === 200 });
  check(http.post(`${API}/api/promocion/validar`, JSON.stringify({ cedula }), params("dictamen")), {
    "dictamen 200": (r) => r.status === 200,
  });
  check(http.post(`${API}/api/importar/obras`, JSON.stringify({ cedula }), params("obras")), { "obras 200": (r) => r.status === 200 });
  check(http.get(`${API}/api/solicitudes`, params("solicitudes")), { "solicitudes 200": (r) => r.status === 200 });
  sleep(1);
}
