/*
 * Program.cs is the starting point for AGMGSK applications.
 * 
 * You should:
 * 1.  Delete the files game1.cs and program1.cs from your project
 * 2.  Rename this AAA-<group last name(s)>-<project number>.cs
 *       for example:
 *       AAA-Barnes-P1.cs			for a group of one, Mike Barnes
 *       AAA-BarnesSmart-P1.cs	for a group of two, Mike Barnes and Iam Smart 
 *         group size > 1 list last names alphabetically
 * 3.  Edit the last three lines in this comment appropriately
 * 
 * Group members:  Mike Barnes
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

