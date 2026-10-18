using System.Globalization;
using System.Text;

namespace ApiREST_UTA.Infrastructure.Documentos
{
    // Generador de PDF mínimo (A4, Helvetica estándar con WinAnsiEncoding) sin dependencias externas.
    // Suficiente para actas y resoluciones de texto: párrafos con ajuste de línea, líneas y pie de página.
    public class DocumentoPdf
    {
        private const float Ancho = 595f;
        private const float Alto = 842f;
        private const float Margen = 56f;
        private const float AnchoUtil = Ancho - 2 * Margen;

        private readonly List<StringBuilder> _paginas = new();
        private StringBuilder _actual = null!;
        private float _y;

        public DocumentoPdf()
        {
            NuevaPagina();
        }

        public void Texto(string texto, float tamano = 10, bool negrita = false, float sangria = 0, float espacioDespues = 4)
        {
            foreach (var parrafo in texto.Replace("\r", "").Split('\n'))
            {
                foreach (var linea in Ajustar(parrafo, tamano, AnchoUtil - sangria, negrita))
                {
                    Asegurar(tamano * 1.4f);
                    _actual.Append(CultureInfo.InvariantCulture,
                        $"BT /{(negrita ? "F2" : "F1")} {N(tamano)} Tf {N(Margen + sangria)} {N(_y)} Td ({Escapar(linea)}) Tj ET\n");
                    _y -= tamano * 1.4f;
                }
            }
            _y -= espacioDespues;
        }

        public void Linea()
        {
            Asegurar(10);
            _actual.Append(CultureInfo.InvariantCulture, $"0.6 G 0.5 w {N(Margen)} {N(_y)} m {N(Ancho - Margen)} {N(_y)} l S 0 G\n");
            _y -= 12;
        }

        public void Espacio(float puntos) => _y -= puntos;

        // Línea para firma con el cargo debajo
        public void Firma(string nombre, string cargo)
        {
            Asegurar(60);
            _y -= 30;
            _actual.Append(CultureInfo.InvariantCulture, $"0 G 0.5 w {N(Margen)} {N(_y)} m {N(Margen + 220)} {N(_y)} l S\n");
            _y -= 14;
            Texto(nombre, 10, negrita: true, espacioDespues: 0);
            Texto(cargo, 9, espacioDespues: 6);
        }

        public byte[] Generar(string pie)
        {
            var objetos = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "", // páginas: se completa al final
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"
            };

            var idsPaginas = new List<int>();
            for (var i = 0; i < _paginas.Count; i++)
            {
                var contenido = _paginas[i].ToString()
                    + $"BT /F1 8 Tf {N(Margen)} 30 Td ({Escapar($"{pie} | Página {i + 1} de {_paginas.Count}")}) Tj ET\n";
                var bytes = Encoding.Latin1.GetBytes(contenido);
                objetos.Add($"<< /Length {bytes.Length} >>\nstream\n{contenido}endstream");
                var idContenido = objetos.Count;
                objetos.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {N(Ancho)} {N(Alto)}] " +
                            $"/Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {idContenido} 0 R >>");
                idsPaginas.Add(objetos.Count);
            }
            objetos[1] = $"<< /Type /Pages /Kids [{string.Join(" ", idsPaginas.Select(id => $"{id} 0 R"))}] /Count {idsPaginas.Count} >>";

            using var salida = new MemoryStream();
            void Escribir(string s) { var b = Encoding.Latin1.GetBytes(s); salida.Write(b, 0, b.Length); }

            Escribir("%PDF-1.4\n%\xE2\xE3\xCF\xD3\n");
            var posiciones = new List<long>();
            for (var i = 0; i < objetos.Count; i++)
            {
                posiciones.Add(salida.Position);
                Escribir($"{i + 1} 0 obj\n{objetos[i]}\nendobj\n");
            }
            var inicioXref = salida.Position;
            Escribir($"xref\n0 {objetos.Count + 1}\n0000000000 65535 f \n");
            foreach (var p in posiciones)
                Escribir($"{p:D10} 00000 n \n");
            Escribir($"trailer\n<< /Size {objetos.Count + 1} /Root 1 0 R >>\nstartxref\n{inicioXref}\n%%EOF\n");
            return salida.ToArray();
        }

        private void NuevaPagina()
        {
            _actual = new StringBuilder();
            _paginas.Add(_actual);
            _y = Alto - Margen;
        }

        private void Asegurar(float alto)
        {
            if (_y - alto < Margen + 20)
                NuevaPagina();
        }

        // Ajuste de línea aproximado con el ancho medio de Helvetica
        private static IEnumerable<string> Ajustar(string texto, float tamano, float ancho, bool negrita)
        {
            var maxCaracteres = Math.Max(10, (int)(ancho / (tamano * (negrita ? 0.56f : 0.52f))));
            var palabras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (palabras.Length == 0)
            {
                yield return "";
                yield break;
            }

            var linea = new StringBuilder();
            foreach (var palabra in palabras)
            {
                if (linea.Length > 0 && linea.Length + 1 + palabra.Length > maxCaracteres)
                {
                    yield return linea.ToString();
                    linea.Clear();
                }
                if (linea.Length > 0) linea.Append(' ');
                linea.Append(palabra);
            }
            if (linea.Length > 0)
                yield return linea.ToString();
        }

        // Los caracteres fuera de Latin-1 se reemplazan; se escapan los delimitadores de cadena del PDF
        private static string Escapar(string texto)
        {
            var sb = new StringBuilder(texto.Length);
            foreach (var c in texto)
            {
                var ch = c switch
                {
                    '–' or '—' => '-',
                    '“' or '”' => '"',
                    '‘' or '’' => '\'',
                    _ => c <= 'ÿ' ? c : '?'
                };
                if (ch is '\\' or '(' or ')') sb.Append('\\');
                sb.Append(ch);
            }
            return sb.ToString();
        }

        private static string N(float valor) => valor.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
