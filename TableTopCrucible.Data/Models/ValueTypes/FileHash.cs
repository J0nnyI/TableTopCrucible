using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Cryptography;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;

namespace TableTopCrucible.Domain.Models.ValueTypes
{
    public struct FileHash
    {
        public FileHash(byte[] data)
        {
            this.Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public byte[] Data { get; }

        public override bool Equals(object obj)
            => obj is FileHash hash &&
            (this.Data != null && hash.Data != null && Enumerable.SequenceEqual(this.Data, hash.Data) ||
            (this.Data == null && hash.Data == null));
        public override int GetHashCode()
            => this.ToString().GetHashCode();
        public override string ToString()
            => BitConverter.ToString(this.Data);

        public static bool operator !=(FileHash hashA, FileHash hashB)
            => !hashA.Equals(hashB);
        public static bool operator ==(FileHash hashA, FileHash hashB)
            => hashA.Equals(hashB);
        public static FileHash Create(string path, HashAlgorithm hashAlgorithm)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"could not find file {path}");

            using FileStream stream = File.OpenRead(path);
            byte[] data = hashAlgorithm.ComputeHash(stream);
            return new FileHash(data);
        }
        public static FileHash Create(string path)
        {
            using var hashAlgorithm = SHA512.Create();
            return Create(path, hashAlgorithm);
        }
        public static ITaskProgressionInfo CreateMany<T>(IEnumerable<T> fileModel,Func<T,string> pathReader, Action<T, FileHash> hashWriter, int threadcount)
        {
            using HashAlgorithm hashAlgorithm = SHA512.Create();
            var prog = new TaskProgression();
            prog.Title = "Hashing";
            prog.Details = $"with {threadcount} threads";
            prog.RequiredProgress = fileModel.Count();
            var _lock = new object();
            var results = fileModel.ToList()
                .SplitEvenly(threadcount)
                .Select(group => Observable.Start(() =>
                {
                    group.ToList().ForEach(file =>
                    {
                        lock (_lock)
                            prog.CurrentProgress++;
                        hashWriter(file, Create(pathReader(file)));
                    });
                }
                , RxApp.TaskpoolScheduler))
                .ToArray();
            results.CombineLatest()
                .Subscribe(results =>
                {
                    try
                    {
                        prog.State = TaskState.Done;
                        prog.Details = "done";
                    }
                    catch (Exception ex)
                    {
                        prog.State = TaskState.Failed;
                        prog.Details = "could not trigger the next step: " + ex.Message;
                    }
                },
                ex =>
                {
                    prog.State = TaskState.Failed;
                    prog.Details = "failed: " + ex.Message;
                });
            return prog;

        }
    }
}
