using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TableTopCrucible.Domain.Models.ValueTypes;

namespace TableTopCrucible.Domain.Library
{
    public interface IFileManagementService
    {
        FileHash HashFile(HashAlgorithm hashAlgorithm, string path);
        FileHash HashFile(string path);
    }

    public class FileManagementService:IFileManagementService
    {
        public FileHash HashFile(HashAlgorithm hashAlgorithm, string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"could not find file {path}");

            FileHash result;

            using (FileStream stream = File.OpenRead(path))
            {
                byte[] data = hashAlgorithm.ComputeHash(stream);
                result = new FileHash(data);
            }


            return result;
        }
        public FileHash HashFile(string path)
        {
            using HashAlgorithm hashAlgorithm = SHA512.Create();
            return HashFile(hashAlgorithm, path);
        }

    }
}
