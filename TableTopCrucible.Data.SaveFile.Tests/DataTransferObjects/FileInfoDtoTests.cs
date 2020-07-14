using Microsoft.VisualBasic.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.SaveFile.Tests.DataTransferObjects
{
    [TestClass]
    public class FileInfoDtoTests
    {
        public static FileInfo createTestEntity()
        => new FileInfo(
                new Uri(@"\test\test.file", UriKind.Relative),
                DateTime.Now.AddMinutes(-5),
                new FileHash(new byte[] { 00, 01, 02, 03, 04, 05 }),
                DateTime.Now,
                DirectorySetupId.New(),
                100,
                false);
    }
}