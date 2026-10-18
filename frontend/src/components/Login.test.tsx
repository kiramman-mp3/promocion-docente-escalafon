import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError, iniciarSesion } from "../api";
import { ROLES } from "../test/fixtures";
import { Login } from "./Login";

vi.mock("../api", async (original) => ({
  ...(await original<typeof import("../api")>()),
  iniciarSesion: vi.fn(),
}));

const sesionRrhh = {
  token: "jwt",
  id: 2,
  nombreCompleto: "Paola Sánchez",
  correo: "rrhh@uta.edu.ec",
  rol: "RRHH" as const,
  cedula: null,
};

async function ingresar(correo: string, contrasena: string) {
  await userEvent.type(screen.getByLabelText("Correo institucional"), correo);
  await userEvent.type(screen.getByLabelText("Contraseña"), contrasena);
  await userEvent.click(screen.getByRole("button", { name: "Ingresar" }));
}

describe("Login", () => {
  beforeEach(() => {
    vi.mocked(iniciarSesion).mockReset();
  });

  it("entrega la sesión al iniciar sesión correctamente", async () => {
    vi.mocked(iniciarSesion).mockResolvedValue(sesionRrhh);
    const onSesion = vi.fn();
    render(<Login roles={ROLES} errorRoles={null} onSesion={onSesion} />);

    await ingresar(" rrhh@uta.edu.ec ", "Uta2026*");

    expect(iniciarSesion).toHaveBeenCalledWith("rrhh@uta.edu.ec", "Uta2026*");
    expect(onSesion).toHaveBeenCalledWith(sesionRrhh);
  });

  it("explica en lenguaje claro unas credenciales inválidas", async () => {
    vi.mocked(iniciarSesion).mockRejectedValue(new ApiError(401, "Credenciales inválidas"));
    const onSesion = vi.fn();
    render(<Login roles={ROLES} errorRoles={null} onSesion={onSesion} />);

    await ingresar("rrhh@uta.edu.ec", "mala");

    expect(await screen.findByRole("alert")).toHaveTextContent("El correo o la contraseña no coinciden.");
    expect(onSesion).not.toHaveBeenCalled();
  });

  it("no deja enviar el formulario vacío", () => {
    render(<Login roles={ROLES} errorRoles={null} onSesion={vi.fn()} />);

    expect(screen.getByRole("button", { name: "Ingresar" })).toBeDisabled();
  });
});
