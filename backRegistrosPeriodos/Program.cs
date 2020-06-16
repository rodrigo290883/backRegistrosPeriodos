using backRegistrosPeriodos.DAL;
using backRegistrosPeriodos.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
namespace repordatorio
{
    class Program
    {
        private static IConfiguration _iconfiguration;
        static void Main(string[] args)
        {
            GetAppSettingsFile();
            ProcesaRegistrosPeriodo();        
        }
        static void GetAppSettingsFile()
        {
            var builder = new ConfigurationBuilder()
                                 .SetBasePath(Directory.GetCurrentDirectory())
                                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _iconfiguration = builder.Build();
        }
        static void ProcesaRegistrosPeriodo()
        {
            var registrosDAL = new RegistrosPeriodoDAL(_iconfiguration);
            var listRegistrosModel = registrosDAL.GetList();
            var logs = registrosDAL.InsertarRegistros(listRegistrosModel);
            var logDal = new LogDAL(_iconfiguration);
            foreach(LogClass log in logs)
            {
                logDal.createLog(log);
            }

            Console.WriteLine("Final del Proceso");
        }
    }
}