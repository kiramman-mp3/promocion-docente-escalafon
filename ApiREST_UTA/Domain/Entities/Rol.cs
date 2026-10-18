namespace ApiREST_UTA.Domain.Entities
{
    // Grado del escalafón (RRHH.dbo.Roles). El Id coincide con el grado escalafonario (CES art. 63).
    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Categoria { get; set; }
        public int? Nivel { get; set; }
        public int? Grado { get; set; }

        // Permanencia mínima en el grado antes de poder promoverse (CES art. 63 y 76 a)
        public int? PermanenciaAnios { get; set; }

        // Falso en el grado 5 (se pasa a Principal 1 por concurso) y en el 8 (último grado)
        public bool Promocionable { get; set; }

        // Columnas heredadas del primer modelo; las reemplaza RequisitoPromocion
        public int RequiereAniosEnRol { get; set; }
        public int RequiereObras { get; set; }
        public decimal RequiereEvaluacion { get; set; }
        public int RequiereCapacitacionHoras { get; set; }
        public int RequiereInvestigacionMeses { get; set; }
    }
}
