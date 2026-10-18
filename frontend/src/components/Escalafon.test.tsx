import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ROLES } from "../test/fixtures";
import { Escalafon } from "./Escalafon";

describe("Escalafon", () => {
  it("lista los roles de mayor a menor", () => {
    render(<Escalafon roles={ROLES} />);

    const nombres = screen.getAllByRole("listitem").map((li) => li.textContent);
    expect(nombres[0]).toBe("Principal 3");
    expect(nombres[nombres.length - 1]).toBe("Auxiliar 1");
  });

  it("marca el rol actual y el siguiente", () => {
    render(<Escalafon roles={ROLES} rolActualId={3} />);

    expect(screen.getByRole("listitem", { current: "step" })).toHaveTextContent("Agregado 1");
    expect(screen.getByText("Siguiente rol").closest("li")).toHaveTextContent("Agregado 2");
  });

  it("en el último rol no hay siguiente", () => {
    render(<Escalafon roles={ROLES} rolActualId={8} />);

    expect(screen.queryByText("Siguiente rol")).not.toBeInTheDocument();
  });
});
