const fechaLarga = new Intl.DateTimeFormat("es-EC", { day: "numeric", month: "long", year: "numeric" });
const fechaMes = new Intl.DateTimeFormat("es-EC", { month: "short", year: "numeric" });
const numero = new Intl.NumberFormat("es-EC", { maximumFractionDigits: 2 });

export const formatoFecha = (iso: string) => fechaLarga.format(new Date(iso));
export const formatoMes = (iso: string) => fechaMes.format(new Date(iso));
export const formatoNumero = (n: number) => numero.format(n);

export function plural(n: number, singular: string, pluralTexto = `${singular}s`) {
  return `${formatoNumero(n)} ${n === 1 ? singular : pluralTexto}`;
}
