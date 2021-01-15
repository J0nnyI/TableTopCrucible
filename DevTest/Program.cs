using System;

using TableTopCrucible.Data.Files.Scanner;

namespace DevTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var scanner = new DirectoryScanner(@"D:\tmp\tests");
            scanner.Subscribe(changes =>
            {
                Console.WriteLine($"{changes.ChangeType} | {changes.Path} | {changes.OldPath}");
            });


            Console.ReadKey();
        }
    }
}
