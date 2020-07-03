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
                            SqlCommand cmd = new SqlCommand("Select periodo from registros_dias where idsap = @idsap and periodo = datepart(yyyy,getdate())", con);
                            cmd.Parameters.AddWithValue("@idsap", registro.idsap);

                            con.Open();
                            string periodo = (string)cmd.ExecuteScalar();

                            if(periodo == null) { 
                            
                                cmd = new SqlCommand("INSERT INTO registros_dias(idsap,fecha_creacion,periodo,registro_padre,dias,disponibles,caducidad) VALUES (@idsap,@fecha_creacion,@periodo,0,@dias,@dias,@caducidad) " + "SELECT CAST(scope_identity() AS int)", con);
                                cmd.Parameters.AddWithValue("@idsap", registro.idsap);
                                cmd.Parameters.AddWithValue("@fecha_creacion", registro.fecha_creacion);
                                cmd.Parameters.AddWithValue("@periodo", registro.periodo);
                                cmd.Parameters.AddWithValue("@dias", registro.disponibles);
                                cmd.Parameters.AddWithValue("@caducidad", registro.caducidad);

                                int reg = (Int32)cmd.ExecuteScalar();

                                listLogs.Add(new LogClass { idsap = registro.idsap, log = "Se genera registro:" + reg + " de " + registro.disponibles + " dias disponibles para el periodo: " + registro.periodo, idsap_creacion = 101010,fecha_creacion = registro.fecha_creacion });
                            }
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

                    if (inicio != "" && inicio != "all" && inicio != "start")
                    {
                        query = "SELECT idsap,GETDATE() as fecha_creacion,gen.genera_dias,datepart(yyyy,getdate()) as periodo,DATEADD(month,13,Convert(date,CONCAT(datepart(yyyy,getdate()),'-',(datepart(mm,e.fecha_ingreso_uen)),'-',datepart(dd,e.fecha_ingreso_uen)))) as caducidad " +
                        "FROM empleados e left join regla_genera_dias gen on DATEDIFF(month, e.fecha_ingreso_grupo, GETDATE()) BETWEEN gen.meses_min and gen.meses_max and gen.esquema = e.esquema " +
                        "WHERE e.tipo IN ('S','L') and DATEDIFF(month, e.fecha_ingreso_grupo, GETDATE()) >= 6 and CONVERT(date, CONCAT(datepart(yyyy, getdate()), '-', (datepart(mm, e.fecha_ingreso_uen)), '-', datepart(dd, e.fecha_ingreso_uen))) between  CONVERT(date,@inicio) and CONVERT(date, GETDATE()); ";

                    }
                    else if (inicio == "start")
                    {
                        query = "SELECT e.idsap,GETDATE() as fecha_creacion,e.dias_disponibles,datepart(yyyy,getdate()) as periodo,"+
                            "DATEADD(month, 13, Convert(date, CONCAT(datepart(yyyy, getdate()), '-', (datepart(mm, e.fecha_ingreso_uen)), '-', datepart(dd, e.fecha_ingreso_uen)))) as caducidad "+
                            "FROM empleados e WHERE e.tipo IN ('S','L'); ";

                    }
                    else if (inicio == "all")
                    {
                        query = "SELECT idsap,GETDATE() as fecha_creacion,gen.genera_dias,datepart(yyyy,getdate()) as periodo,DATEADD(month,13,Convert(date,CONCAT(datepart(yyyy,getdate()),'-',(datepart(mm,e.fecha_ingreso_uen)),'-',datepart(dd,e.fecha_ingreso_uen)))) as caducidad " +
                        "FROM empleados e left join regla_genera_dias gen on DATEDIFF(month, e.fecha_ingreso_grupo, GETDATE()) BETWEEN gen.meses_min and gen.meses_max and gen.esquema = e.esquema " +
                        "WHERE DATEDIFF(month, e.fecha_ingreso_grupo, GETDATE()) >= 6  and tipo IN('S','L'); ";

                    }
                    else
                    {
                        query = "SELECT idsap,GETDATE() as fecha_creacion,gen.genera_dias,datepart(yyyy,getdate()) as periodo,DATEADD(month,13,Convert(date,CONCAT(datepart(yyyy,getdate()),'-',(datepart(mm,e.fecha_ingreso_uen)),'-',datepart(dd,e.fecha_ingreso_uen)))) as caducidad " +
                        "FROM empleados e left join regla_genera_dias gen on DATEDIFF(month, e.fecha_ingreso_grupo, GETDATE()) BETWEEN gen.meses_min and gen.meses_max and gen.esquema = e.esquema " +
                        "WHERE DATEDIFF(month, e.fecha_ingreso_grupo, GETDATE()) >= 6 and CONVERT(date, CONCAT(datepart(yyyy, getdate()), '-', (datepart(mm, e.fecha_ingreso_uen)), '-', datepart(dd, e.fecha_ingreso_uen))) = CONVERT(date, GETDATE()) and tipo IN('S','L'); ";

                    }

                    SqlCommand cmd = new SqlCommand(query, con);

                    if(inicio != "")
                        cmd.Parameters.AddWithValue("@inicio", inicio);


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
                                caducidad = Convert.ToDateTime(rdr.IsDBNull(4) ? null : rdr[4])
                            });
                        }
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