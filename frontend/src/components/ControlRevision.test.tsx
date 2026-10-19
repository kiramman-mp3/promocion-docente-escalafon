import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { ControlRevision } from "./ControlRevision";

describe("ControlRevision", () => {
  it("aprueba el documento con un clic", async () => {
    const onRevisar = vi.fn().mockResolvedValue(undefined);
    render(<ControlRevision editable etiqueta="la obra" onRevisar={onRevisar} />);

    expect(screen.getByText("Sin revisar")).toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: "Aprobar la obra" }));

    expect(onRevisar).toHaveBeenCalledWith("Aprobado", undefined);
  });

  it("exige un motivo antes de confirmar el rechazo", async () => {
    const onRevisar = vi.fn().mockResolvedValue(undefined);
    render(<ControlRevision editable etiqueta="la obra" onRevisar={onRevisar} />);

    await userEvent.click(screen.getByRole("button", { name: "Rechazar la obra" }));
    const confirmar = screen.getByRole("button", { name: "Confirmar rechazo" });
    expect(confirmar).toBeDisabled();

    await userEvent.type(screen.getByLabelText("Motivo del rechazo de la obra"), "  PDF ilegible ");
    await userEvent.click(confirmar);

    expect(onRevisar).toHaveBeenCalledWith("Rechazado", "PDF ilegible");
  });

  it("muestra el estado y el motivo sin botones cuando no es editable", () => {
    render(
      <ControlRevision
        editable={false}
        etiqueta="la obra"
        onRevisar={vi.fn()}
        revision={{
          tipoDocumento: "Obra",
          documentoId: 1,
          estado: "Rechazado",
          observacion: "La revista no está indexada",
          revisadoPor: "Paola Sánchez",
          fechaRevision: "2026-10-17T10:00:00Z",
        }}
      />,
    );

    expect(screen.getByText("Rechazado")).toBeInTheDocument();
    expect(screen.getByText("La revista no está indexada")).toBeInTheDocument();
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
  });

  it("muestra el error si la API rechaza la revisión", async () => {
    const onRevisar = vi.fn().mockRejectedValue(new Error("La solicitud ya está aprobada."));
    render(<ControlRevision editable etiqueta="la obra" onRevisar={onRevisar} />);

    await userEvent.click(screen.getByRole("button", { name: "Aprobar la obra" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("La solicitud ya está aprobada.");
  });
});
