using System;
using System.Collections.Generic;
using System.IO;
using SysFileInfo = System.IO.FileInfo;
using System.Text;

using TableTopCrucible.Domain.Models.Sources;
using FileInfo = TableTopCrucible.Domain.Models.Sources.FileInfo;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Services
{
    public interface IFileInfoService : IDataService<FileInfo, FileInfoId, FileInfoChangeset> { }
    public class FileInfoService : DataServiceBase<FileInfo, FileInfoId, FileInfoChangeset>, IFileInfoService
    {
    }
}
