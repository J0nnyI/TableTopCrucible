using Microsoft.VisualStudio.TestTools.UnitTesting;
using TableTopCrucible.Data.Files.Scanner;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using ReactiveUI;

namespace TableTopCrucible.Data.Files.Scanner.Tests
{
    [TestClass()]
    public class DirectoryScannerTests
    {
        private string directory;
        private DirectoryScanner scanner;
        private Subject<Unit> done;
        private string subdir;
        private IList<DirectoryUpdate> mostRecentResult;
        private string firstFile;
        private string subDirFile;
        private string renamedFile;

        public DirectoryScannerTests()
        {

            this.directory = Path.Combine(Directory.GetCurrentDirectory(), @"__TestFileDirectory__");
            Directory.CreateDirectory(directory);
            this.scanner = new DirectoryScanner(directory, RxApp.TaskpoolScheduler);
            this.done = new Subject<Unit>();
            this.subdir = Path.Combine(directory, @"subdir");
            this.firstFile = Path.Combine(directory, "firstFileTest.txt");
            this.subDirFile = Path.Combine(subdir, "subDirFile.txt");
            this.renamedFile = Path.Combine(subdir, "subDirFileRenamed.txt");

        }

        [TestMethod()]
        public void RootDirFileTest()
        {
            scanner
                .Buffer(done)
                .Take(1)
                .Subscribe(
                    res =>
                    {
                        checkFileUpdates(
                            res,
                            firstFile,
                            WatcherChangeTypes.Created,
                            WatcherChangeTypes.Changed,
                            WatcherChangeTypes.Deleted);
                    }
                );

            File.WriteAllText(firstFile, "first file");
            File.WriteAllText(firstFile, "overwritten first file");
            File.Delete(firstFile);
            Thread.Sleep(10);
            done.OnNext(new Unit());

        }
        [TestMethod()]
        public void DirectoryTest()
        {
            scanner.Buffer(done)
                .Take(1)
                .Subscribe(res =>
                {
                    checkFileUpdates(
                        res,
                        firstFile,
                        WatcherChangeTypes.Created,
                        WatcherChangeTypes.Renamed,
                        WatcherChangeTypes.Deleted);
                });

            Directory.CreateDirectory(subdir);
            Directory.Move(subdir, subdir + "moved");
            Directory.Delete(subdir + "moved");
            Thread.Sleep(10);
            done.OnNext(new Unit());
        }

        private void checkFileUpdate(DirectoryUpdate eventPattern, string firstFile, WatcherChangeTypes created)
        {
            Assert.AreEqual(eventPattern.ChangeType, created);
            Assert.AreEqual(eventPattern.Path, firstFile);
        }
        private void checkFileUpdates(IEnumerable<DirectoryUpdate> patterns, string file, params WatcherChangeTypes[] mods)
        {
            Assert.AreEqual(patterns.Count(), mods.Length, "there have been more or less change events triggered than expected");
            for (int i = 0; i < patterns.Count(); i++)
                checkFileUpdate(patterns.ElementAt(i), file, mods[i]);
        }
    }
}