/*
  Crea las 5 bases de datos que consume ApiREST_UTA.
  Cada una simula el sistema de una dirección de la UTA:
    DITIC -> usuarios del sistema (login)
    RRHH  -> tiempo en el rol y catálogo de roles del escalafón
    DIDE  -> obras, investigaciones y participación en proyectos
    DEaDV -> cursos de capacitación, evaluaciones docentes y capacitación impartida
    DAC   -> tesis de doctorado dirigidas
*/
IF DB_ID('DITIC') IS NULL CREATE DATABASE DITIC;
GO
IF DB_ID('RRHH') IS NULL CREATE DATABASE RRHH;
GO
IF DB_ID('DIDE') IS NULL CREATE DATABASE DIDE;
GO
IF DB_ID('DEaDV') IS NULL CREATE DATABASE DEaDV;
GO
IF DB_ID('DAC') IS NULL CREATE DATABASE DAC;
GO
