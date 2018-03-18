using FileSystemExtensionLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplicationTask1
{
    class Program
    {
        static void Main(string[] args)
        {
            string startPoint = "D:\\";

            Console.WriteLine("1: WITHOUT FILTER");
            Console.WriteLine("............................");
            var visitor = new FileSystemVisitor(startPoint);

            visitor.Start += (s, e) =>
            {
                Console.WriteLine("Search started");
            };

            visitor.Finish += (s, e) =>
            {
                Console.WriteLine("Search finished");
            };

            visitor.FileFinded += (s, e) =>
            {
                Console.WriteLine("Founded file: " + e.FindedItem.Name);
            };

            visitor.DirectoryFinded += (s, e) =>
            {
                Console.WriteLine("Founded directory: " + e.FindedItem.Name);
                //if (e.FindedItem.Name.Length == 4)
                //{
                //    e.ActionType = FileSystemExtensionLibrary.Action.StopSearch;
                //}
            };

            visitor.FilteredFileFinded += (s, e) =>
            {
                Console.WriteLine("Founded filtered file: " + e.FindedItem.Name);
            };

            visitor.FilteredDirectoryFinded += (s, e) =>
            {
                Console.WriteLine("Founded filtered directory: " + e.FindedItem.Name);
                //if (e.FindedItem.Name.Length == 4)
                //    e.ActionType = FileSystemExtensionLibrary.Action.StopSearch;
            };

            try
            {
                foreach (var fileSysInfo in visitor.GetFileSystemInfoSequence())
                {
                    Console.WriteLine(fileSysInfo);
                }
            }
            catch(UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine();
            Console.WriteLine("2: WITH FILTER");
            Console.WriteLine("............................");
            visitor = new FileSystemVisitor(startPoint, (info) => info.FullName.Contains("New"));

            visitor.Start += (s, e) =>
            {
                Console.WriteLine("Search started");
            };

            visitor.Finish += (s, e) =>
            {
                Console.WriteLine("Search finished");
            };

            visitor.FileFinded += (s, e) =>
            {
                Console.WriteLine("Founded file: " + e.FindedItem.Name);
            };

            visitor.DirectoryFinded += (s, e) =>
            {
                Console.WriteLine("Founded directory: " + e.FindedItem.Name);
                //if (e.FindedItem.Name.Length == 4)
                //{
                //    e.ActionType = FileSystemExtensionLibrary.Action.StopSearch;
                //}
            };

            visitor.FilteredFileFinded += (s, e) =>
            {
                Console.WriteLine("Founded filtered file: " + e.FindedItem.Name);
            };

            visitor.FilteredDirectoryFinded += (s, e) =>
            {
                Console.WriteLine("Founded filtered directory: " + e.FindedItem.Name);
                //if (e.FindedItem.Name.Length == 4)
                //    e.ActionType = FileSystemExtensionLibrary.Action.StopSearch;
            };

            try
            {
                foreach (var fileSysInfo in visitor.GetFileSystemInfoSequence())
                {
                    Console.WriteLine(fileSysInfo);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
    }
}
