/* Arnold Santos   <arnold2020@yahoo.com>
 * Cesar Zalzalah  <7701707@gmail.com>
 * Dani Odicho     <dannykaka2009@hotmail.com>
 * Ernie Ledezma   <eledezma518@gmail.com>
 * 
 * Project 1
 * Spring 2015
 */

using System;

namespace AGMGSKv6
{
#if WINDOWS || LINUX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Stage stage = new Stage())
            {
                stage.Run();
            }
        }
    }
#endif
}

