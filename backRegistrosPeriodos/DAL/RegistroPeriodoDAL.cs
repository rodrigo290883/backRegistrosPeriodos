using backRegistrosPeriodos.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace backRegistrosPeriodos.DAL
{
    public class RegistrosPeriodoDAL
    {
        private string _connectionString;

        public RegistrosPeriodoDAL(IConfiguration iconfiguration)
        {
            _connectionString = iconfiguration.GetConnectionString("MyConnection");
        }

        public List<LogClass> InsertarRegistros(List<RegistroPeridoClass> registros)
        {
            var listLogs = new List<LogClass>();
            
            if (registros != null)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(_connectionString))
                    {
                        foreach (RegistroPeridoClass registro in registros) {

                            SqlCommand cmd = new SqlCommand("Select registro,tipo,dias,disponibles from registros_dias where idsap = @idsap and periodo = datepart(yyyy,getdate()) and caducidad >= getdate()", con);
                            cmd.Parameters.AddWithValue("@idsap", registro.idsap);

                            con.Open();
     
                            SqlDataReader rdr = cmd.ExecuteReader();

                            if (rdr.Read()) //Existe registro en el mismo año
                            {
                                int reg_ant = Convert.ToInt32(rdr[0]);
                                int tipo = rdr.IsDBNull(1)?0:Convert.ToInt32(rdr[1]);
                                int dias = Convert.ToInt32(rdr[2]);
                                int dias_disponibles = Convert.ToInt32(rdr[3]);

                                dias = dias + registro.dias;
                                dias_disponibles = dias_disponibles + registro.dias;

                                if (tipo == 2) //Existe Registro de anticipo
                                {
                                    SqlCommand cmd2 = new SqlCommand("UPDATE registros_dias SET fecha_creacion = @fecha_creacion,periodo=@periodo,dias=@dias,disponibles=@dias_disponibles,anticipo = 1 WHERE regisro = @registro", con);
                                    cmd2.Parameters.AddWithValue("@fecha_creacion", registro.fecha_creacion);
                                    cmd2.Parameters.AddWithValue("@periodo", registro.periodo);
                                    cmd2.Parameters.AddWithValue("@dias", dias);
                                    cmd2.Parameters.AddWithValue("@disponibles", dias_disponibles);
                                    cmd2.Parameters.AddWithValue("@registro", reg_ant);

                                    cmd2.ExecuteNonQuery();

                                    listLogs.Add(new LogClass { idsap = registro.idsap, log = "Se actualizo registro de anticipo:" + reg_ant + " a " + dias_disponibles + " dias disponibles para el periodo: " + registro.periodo, idsap_creacion = 101010, fecha_creacion = registro.fecha_creacion });
                                }
                                else if (tipo == 1) //Existe Registro de anticipo
                                {
                                    SqlCommand cmd2 = new SqlCommand("UPDATE registros_dias SET fecha_creacion = @fecha_creacion,periodo=@periodo,dias=@dias,disponibles=@dias_disponibles,anticipo = 0 WHERE regisro = @registro", con);
                                    cmd2.Parameters.AddWithValue("@fecha_creacion", registro.fecha_creacion);
                                    cmd2.Parameters.AddWithValue("@periodo", registro.periodo);
                                    cmd2.Parameters.AddWithValue("@dias", dias);
                                    cmd2.Parameters.AddWithValue("@disponibles", dias_disponibles);
                                    cmd2.Parameters.AddWithValue("@registro", reg_ant);

                                    cmd2.ExecuteNonQuery();

                                    listLogs.Add(new LogClass { idsap = registro.idsap, log = "Se actualizo registro de anticipo:" + reg_ant + " a " + dias_disponibles + " dias disponibles para el periodo: " + registro.periodo, idsap_creacion = 101010, fecha_creacion = registro.fecha_creacion });
                                }
                            }
                            else // No hay registro en el año
                            {
                                SqlCommand cmd2 = new SqlCommand("INSERT INTO registros_dias(idsap,fecha_creacion,periodo,registro_padre,dias,disponibles,caducidad,anticipo) VALUES (@idsap,@fecha_creacion,@periodo,0,@dias,@dias,@caducidad,@anticipo) " + "SELECT CAST(scope_identity() AS int)", con);
                                cmd2.Parameters.AddWithValue("@idsap", registro.idsap);
                                cmd2.Parameters.AddWithValue("@fecha_creacion", registro.fecha_creacion);
                                cmd2.Parameters.AddWithValue("@periodo", registro.periodo);
                                cmd2.Parameters.AddWithValue("@dias", registro.disponibles);
                                cmd2.Parameters.AddWithValue("@caducidad", registro.caducidad);
                                cmd2.Parameters.AddWithValue("@anticipo", registro.tipo);

                                int reg = (Int32)cmd2.ExecuteScalar();

                                listLogs.Add(new LogClass { idsap = registro.idsap, log = "Se genera registro:" + reg + " de " + registro.disponibles + " dias disponibles para el periodo: " + registro.periodo, idsap_creacion = 101010, fecha_creacion = registro.fecha_creacion });
                            }

                            rdr.Close();
                            con.Close();
                        }
                        
                        return listLogs;
                    }
                }
                catch(Exception ex)
                {

                   listLogs.Add( new LogClass { idsap = 0, log = ex.ToString(), idsap_creacion = 101010});

                    return listLogs ;
                }

            }

            return listLogs;

        }

        public List<RegistroPeridoClass> GetList(string inicio)
        {
            var listRegistrosModel = new List<RegistroPeridoClass>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "";
                    string query2 = "";

                    if (inicio != "")
                    {
                        query = "SELECT idsap,GETDATE() as fecha_creacion,gen.genera_dias,datepart(yyyy,getdate()) as periodo,DATEADD(month,13,Convert(date,CONCAT(datepart(yyyy,getdate()),'-',(datepart(mm,e.fecha_ingreso_uen)),'-',datepart(dd,e.fecha_ingreso_uen)))) as caducidad " +
                        "FROM empleados e left join regla_genera_dias gen on DATEDIFF(month, e.fecha_ingreso_grupo, GETDATE()) BETWEEN gen.meses_min and gen.meses_max and gen.esquema = e.esquema " +
                        "WHERE e.tipo IN ('S','L') and DATEDIFF(month, e.fecha_ingreso_grupo, GETDATE()) >= 12 and CONVERT(date, CONCAT(datepart(yyyy, getdate()), '-', (datepart(mm, e.fecha_ingreso_uen)), '-', datepart(dd, e.fecha_ingreso_uen))) =  CONVERT(date,@inicio); ";

                        query2 = "SELECT idsap,GETDATE() as fecha_creacion,gen.genera_dias,datepart(yyyy,getdate()) as periodo,DATEADD(month,13,Convert(date,CONCAT(datepart(yyyy,getdate()),'-',(datepart(mm,e.fecha_ingreso_uen)),'-',datepart(dd,e.fecha_ingreso_uen)))) as caducidad " +
                        "FROM empleados e left join regla_genera_dias gen on DATEDIFF(month, e.fecha_ingreso_grupo, GETDATE()) BETWEEN gen.meses_min and gen.meses_max and gen.esquema = e.esquema " +
                        "WHERE e.tipo IN ('S','L') and DATEDIFF(month, e.fecha_ingreso_grupo, GETDATE()) < 12 and DATEADD(month,-6,CONVERT(date, CONCAT(datepart(yyyy, getdate()), '-', (datepart(mm, e.fecha_ingreso_uen)), '-', datepart(dd, e.fecha_ingreso_uen)))) = CONVERT(date,@inicio); ";

                        
                    }
                    else
                    {
                        query = "SELECT idsap,GETDATE() as fecha_creacion,gen.genera_dias,datepart(yyyy,getdate()) as periodo,DATEADD(month,13,Convert(date,CONCAT(datepart(yyyy,getdate()),'-',(datepart(mm,e.fecha_ingreso_uen)),'-',datepart(dd,e.fecha_ingreso_uen)))) as caducidad " +
                        "FROM empleados e left join regla_genera_dias gen on DATEDIFF(month, e.fecha_ingreso_grupo, GETDATE()) BETWEEN gen.meses_min and gen.meses_max and gen.esquema = e.esquema " +
                        "WHERE DATEDIFF(month, e.fecha_ingreso_grupo, GETDATE()) >= 12 and CONVERT(date, CONCAT(datepart(yyyy, getdate()), '-', (datepart(mm, e.fecha_ingreso_uen)), '-', datepart(dd, e.fecha_ingreso_uen))) = CONVERT(date, GETDATE()) and tipo IN('S','L'); ";

                        query2 = "SELECT idsap,GETDATE() as fecha_creacion,gen.genera_dias,datepart(yyyy, getdate()) as periodo,DATEADD(month, 13, Convert(date, CONCAT(datepart(yyyy, getdate()), '-', (datepart(mm, e.fecha_ingreso_uen)), '-', datepart(dd, e.fecha_ingreso_uen)))) as caducidad " +
                        "FROM empleados e left join regla_genera_dias gen on DATEDIFF(month, e.fecha_ingreso_grupo, GETDATE()) BETWEEN gen.meses_min and gen.meses_max and gen.esquema = e.esquema " +
                        "WHERE DATEDIFF(month, e.fecha_ingreso_grupo, GETDATE()) < 12 and DATEADD(month,-6,CONVERT(date, CONCAT(datepart(yyyy, getdate()), '-', (datepart(mm, e.fecha_ingreso_uen)), '-', datepart(dd, e.fecha_ingreso_uen)))) = CONVERT(date, GETDATE()) and tipo IN('S', 'L'); ";

                    }

                    SqlCommand cmd = new SqlCommand(query, con);

                    if(inicio != "")
                        cmd.Parameters.AddWithValue("@inicio", inicio);

                    SqlCommand cmd2 = new SqlCommand(query2, con);

                    if (inicio != "")
                        cmd2.Parameters.AddWithValue("@inicio", inicio);


                    con.Open();
                    try
                    {
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            listRegistrosModel.Add(new RegistroPeridoClass
                            {

                                idsap = Convert.ToInt32(rdr[0]),
                                fecha_creacion = Convert.ToDateTime(rdr.IsDBNull(1) ? null : rdr[1]),
                                disponibles = Convert.ToInt32(rdr[2]),
                                periodo = rdr[3].ToString(),
                                caducidad = Convert.ToDateTime(rdr.IsDBNull(4) ? null : rdr[4]),
                                tipo = 0
                            });
                        }

                        rdr.Close();
                        SqlDataReader rdr2 = cmd2.ExecuteReader();
                        while (rdr2.Read())
                        {
                            listRegistrosModel.Add(new RegistroPeridoClass
                            {

                                idsap = Convert.ToInt32(rdr2[0]),
                                fecha_creacion = Convert.ToDateTime(rdr2.IsDBNull(1) ? null : rdr2[1]),
                                disponibles = Convert.ToInt32(rdr2[2]),
                                periodo = rdr2[3].ToString(),
                                caducidad = Convert.ToDateTime(rdr2.IsDBNull(4) ? null : rdr2[4]),
                                tipo = 1
                            });
                        }
                        rdr2.Close();

                    }
                    catch(SqlException ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return listRegistrosModel;
        }
    }
}