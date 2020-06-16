using backRegistrosPeriodos.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;

namespace backRegistrosPeriodos.DAL
{
    public class LogDAL
    {
        private string _connectionString;

        public LogDAL(IConfiguration iconfiguration)
        {
            _connectionString = iconfiguration.GetConnectionString("MyConnection");
        }

        public int createLog(LogClass log)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO log_vacaciones(idsap,fecha_creacion,log,idsap_creacion) VALUES (@idsap,@fecha_creacion,@log,101010) " + "SELECT CAST(scope_identity() AS int) ", con);
                    cmd.Parameters.AddWithValue("@idsap", log.idsap);
                    cmd.Parameters.AddWithValue("@fecha_creacion", log.fecha_creacion);
                    cmd.Parameters.AddWithValue("@log", log.log);
                  
                    con.Open();

                    int reg = (Int32)cmd.ExecuteScalar();

                    con.Close();

                    return reg;
                }
            }
            catch (Exception ex)
            {

                return 0;
            }
            
        }
    }
}