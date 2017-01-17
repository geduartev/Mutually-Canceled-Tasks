using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mutually_Canceled_Tasks
{
    public class Program
    {
        static CancellationTokenSource objetoTokenSource;
        static Task tareaLargaDuracion;

        static void Main(string[] args)
        {
            //EjecutarDosMetodosEnSerieEnParaleloConMetodoActual();
            //Debug.WriteLine("Escribiendo en Método Actual Despues de EjecutarDosMetodosEnSerieEnParaleloConMetodoActual");



            //EjecutarVariosMetodosSimultaneamenteEnParaleloConMetodoActual();
            //Debug.WriteLine("Escribiendo en Método Actual Despues de EjecutarVariosMetodosSimultaneamenteEnParaleloConMetodoActual");



            // La cancelación de una tarea de segundo plano es una labor colaborativa 
            // entre la aplicación que ha lanzado la tarea y la propia tarea.


            //Iniciar la tarea de larga duración
            objetoTokenSource = new CancellationTokenSource();
            Debug.WriteLine("Se crea la tarea que inicia la tarea infinita...");
            tareaLargaDuracion = Task.Factory.StartNew(TareaInfinita, (object) null, objetoTokenSource.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);

            
            Debug.WriteLine("Procede a esperar 3 segundos....");
            bool b = tareaLargaDuracion.Wait(10);

            Debug.WriteLine("Se procede a cancelar la tarea infinita...");
            objetoTokenSource.Cancel();
            
            

            Debug.WriteLine("Tarea de larga duración o infinita terminada correctamente.");
            Console.ReadLine();
        }




        // Se debe agregar async para poder esperar por la finalización de las tareas marcadas con await.
        static async void EjecutarDosMetodosEnSerieEnParaleloConMetodoActual()
        {
            // Gracias a la expresión await se espera a que la 1era tarea termine para continuar con otra.
            bool b = await Task.Factory.StartNew<bool>(TareaSegundoPlano, null);
            Debug.WriteLine("Hemos terminado de ejecutar la tarea 1 en segundo plano");

            int i = 2;
            b = await Task.Factory.StartNew<bool>((objState) =>
            {
                Debug.WriteLine("Tenemos acceso al dispatcher de este objeto = {0}", i);
                return false;
            }, null);

            Debug.WriteLine("Hemos terminado de ejecutar la tarea 2 en segundo plano");
        }

        static bool TareaSegundoPlano(object objState)
        {
            Debug.WriteLine("Tenemos acaceso al dispatcher de este objeto");
            return true;
        }





        static async void EjecutarVariosMetodosSimultaneamenteEnParaleloConMetodoActual()
        {
            

            Debug.WriteLine("Ejecución de Caso 2: EjecutarTareasParalelas dentro de una tarea.");
            await Task.Factory.StartNew(EjecutarTareasParalelas); // Caso 2
            Debug.WriteLine("Esta línea se ejecuta inmediatamente después de las tareas del Caso 2");

            Debug.WriteLine("Ejecución de Caso 1: EjecutarTareasParalelas");
            // Interfaz del usuario se bloquea mientras pasan los 5 segundos.
            EjecutarTareasParalelas(); // Caso 1: La interfaz se queda bloqueqda durante los 5 segundos que dura la ejecución de la tarea.
            Debug.WriteLine("Esta línea se ejecuta solo 5 segundos después de las tareas del Caso 1");

            Debug.WriteLine("TERMINO");
        }

        static void EjecutarTareasParalelas()
        {
            Task<bool> tb1;
            Task<bool> tb2;
            Task<bool> tb3;

            tb1 = Task.Factory.StartNew<bool>(MetodoLogico1);
            tb2 = Task.Factory.StartNew<bool>(MetodoLogico2);
            tb3 = Task.Factory.StartNew<bool>(MetodoLogico3);

            // Para esperar que todas las tareas terminen escribimos:
            Task.WaitAll(tb1, tb2, tb3);

            // Si únicamente necesitamos que alguna de ellas termine escribimos:
            //Task.WaitAny(tb1, tb2, tb3);
            
            // Se bloqueará el hilo actual mientras no terminen todos los métodos.
            Debug.WriteLine("Terminados todos los métodos.");

            //Si quisiéramos obtener el resultados de los métodos.
            bool r1 = tb1.Result;
            bool r2 = tb2.Result;
            bool r3 = tb3.Result;
        }
        
        static bool MetodoLogico1()
        {
            Debug.WriteLine(">>>> Ejecutando MetodoLogico 1");
            return true;
        }

        static bool MetodoLogico2()
        {
            Debug.WriteLine(">>>> Ejecutando MetodoLogico 2");
            return true;
        }

        static bool MetodoLogico3()
        {
            Debug.WriteLine(">>>> Ejecutando MetodoLogico 3");
            // Simular una duración de 5 segundos.
            System.Diagnostics.Stopwatch sw = Stopwatch.StartNew();

            do
            {
            } while (sw.ElapsedMilliseconds < 5 * 1000);

            return false;
        }





        // Tal como he comentado anteriormente, la cancelación de la tarea es colaborativa, 
        // esto quiere decir que la tarea debe estar pendiente del testigo de cancelación adecuado
        // para saber cuándo tiene que terminar y terminar por su cuenta. 
        static void TareaInfinita(object objData)
        {
            CancellationToken cToken = objetoTokenSource.Token;

            for (int n = 0; ;n++)
            {
                Debug.WriteLine("**** {0} ", n);
                // Primera forma de comprobar si hay que cancela una tarea
                // cToken.ThrowIfCancellationRequested();

                // Segunda forma de comprobar si hay que cancelar una tarea
                if (cToken.IsCancellationRequested)
                {
                    Debug.WriteLine("<<< Terminando tarea de larga duración");
                    return;
                }
            }
            Debug.WriteLine("Tarea infinita terminada");
        }
    }
}
