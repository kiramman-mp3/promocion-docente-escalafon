namespace ApiREST_UTA.Domain.Entities
{
    // Cálculos de tiempo con fecha completa (día y mes), no solo restando años
    public static class Fechas
    {
        public static int MesesCompletos(DateTime desde, DateTime hasta)
        {
            if (hasta < desde)
                return 0;
            var meses = (hasta.Year - desde.Year) * 12 + hasta.Month - desde.Month;
            if (hasta.Day < desde.Day)
                meses--;
            return Math.Max(meses, 0);
        }

        public static int AniosCompletos(DateTime desde, DateTime hasta) => MesesCompletos(desde, hasta) / 12;

        // "4 años y 3 meses"
        public static string Duracion(int meses)
        {
            var anios = meses / 12;
            var resto = meses % 12;
            var partes = new List<string>();
            if (anios > 0) partes.Add(anios == 1 ? "1 año" : $"{anios} años");
            if (resto > 0) partes.Add(resto == 1 ? "1 mes" : $"{resto} meses");
            return partes.Count == 0 ? "0 meses" : string.Join(" y ", partes);
        }

        // Meses en que el periodo [inicio, fin] se superpone con la ventana [desde, hasta]
        public static int MesesSuperpuestos(DateTime inicio, DateTime? fin, DateTime desde, DateTime hasta)
        {
            var a = inicio > desde ? inicio : desde;
            var b = (fin ?? hasta) < hasta ? (fin ?? hasta) : hasta;
            return b <= a ? 0 : MesesCompletos(a, b);
        }
    }
}
