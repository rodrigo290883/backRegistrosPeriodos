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
            if(args.Length == 0)
            {
                GetAppSettingsFile();
                ProcesaRegistrosPeriodo("");
            }
            else
            {
                GetAppSettingsFile();
                ProcesaRegistrosPeriodo(args[0]);
            }
                  
        }
        static void GetAppSettingsFile()
        {
            var builder = new ConfigurationBuilder()
                                 .SetBasePath(Directory.GetCurrentDirectory())
                                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _iconfiguration = builder.Build();
        }
        static void ProcesaRegistrosPeriodo(string inicio)
        {
            var registrosDAL = new RegistrosPeriodoDAL(_iconfiguration);
            var listRegistrosModel = registrosDAL.GetList(inicio);
            var logs = registrosDAL.InsertarRegistros(listRegistrosModel,inicio);
            var logDal = new LogDAL(_iconfiguration);
            foreach(LogClass log in logs)
            {
                logDal.createLog(log);
                Console.WriteLine("Se Genero el registro: "+log.registro+" ,idsap: "+log.idsap+" ,log: "+log.log);
            }

            Console.WriteLine("Final del Proceso");
        }
    }
}