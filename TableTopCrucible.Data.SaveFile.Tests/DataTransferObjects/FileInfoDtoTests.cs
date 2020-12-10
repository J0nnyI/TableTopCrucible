using Microsoft.VisualBasic.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

using TableTopCrucible.Data.SaveFile.DataTransferObjects;
using TableTopCrucible.Data.Models.Sources;
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

        public static FileInfo createIncompleteTestEntity()
            => new FileInfo(
                new Uri(@"\test\test.file", UriKind.Relative),
                DateTime.Now.AddMinutes(-5),
                null,
                DateTime.Now,
                DirectorySetupId.New(),
                100,
                false);


        void compareEntityToDto(FileInfo entity, FileInfoDTO dto)
        {
            Assert.AreEqual(entity.Path, dto.Path, "faulty Path");
            Assert.AreEqual(entity.FileCreationTime, dto.FileCreationTime, "faulty FileCreationTime");
            Assert.AreEqual(entity.FileHash?.Data, dto.FileHash, "faulty FileHash");
            Assert.AreEqual(entity.LastWriteTime, dto.LastWriteTime, "faulty LastWriteTime");
            Assert.AreEqual(entity.DirectorySetupId, (DirectorySetupId)dto.DirectorySetupId, "faulty DirectorySetupId");
            Assert.AreEqual(entity.IsAccessible, dto.IsAccessible, "faulty IsAccessible");
            Assert.AreEqual(entity.FileSize, dto.FileSize, "faulty FileSize");
        }
        void compareEntities(FileInfo entityA, FileInfo entityB)
        {
            Assert.AreEqual(entityA.HashKey, entityB.HashKey, "faulty HashKey");
            Assert.AreEqual(entityA.Path, entityB.Path, "faulty Path");
            Assert.AreEqual(entityA.FileCreationTime, entityB.FileCreationTime, "faulty FileCreationTime");
            Assert.AreEqual(entityA.FileHash, entityB.FileHash, "faulty FileHash");
            Assert.AreEqual(entityA.LastWriteTime, entityB.LastWriteTime, "faulty LastWriteTime");
            Assert.AreEqual(entityA.DirectorySetupId, entityB.DirectorySetupId, "faulty DirectorySetupId");
            Assert.AreEqual(entityA.IsAccessible, entityB.IsAccessible, "faulty IsAccessible");
            Assert.AreEqual(entityA.FileSize, entityB.FileSize, "faulty FileSize");

            Assert.AreNotEqual(entityA.Identity, entityB.Identity, "it's the same instance");


            Assert.AreEqual(entityA.Id, entityB.Id, "faulty id");
            Assert.AreEqual(entityA.Created, entityB.Created, "faulty creation timestamp");
            Assert.AreEqual(entityA.LastChange, entityB.LastChange, "faulty last change timestamp");
        }

        [TestMethod]
        public void Entity_to_dto_works()
        {
            var entity = createTestEntity();

            var dto = new FileInfoDTO(entity);

            compareEntityToDto(entity, dto);
        }
        [TestMethod]
        public void Dto_to_entity_works()
        {

            var dto = new FileInfoDTO(createTestEntity());

            var entity = dto.ToEntity();

            compareEntityToDto(entity, dto);
        }
        [TestMethod]
        public void serialization_works()
        {
            var srcEntity = createTestEntity();
            var srcDto = new FileInfoDTO(srcEntity);

            var str = JsonSerializer.Serialize(srcDto);
            var dstDto = JsonSerializer.Deserialize<FileInfoDTO>(str);
            var dstEntity = dstDto.ToEntity();

            compareEntities(srcEntity, dstEntity);
        }
        [TestMethod]
        public void Incomplete_entity_to_dto_works()
        {
            var entity = createIncompleteTestEntity();

            var dto = new FileInfoDTO(entity);

            compareEntityToDto(entity, dto);
        }
        [TestMethod]
        public void Incomplete_dto_to_entity_works()
        {

            var dto = new FileInfoDTO(createIncompleteTestEntity());

            var entity = dto.ToEntity();

            compareEntityToDto(entity, dto);
        }
        [TestMethod]
        public void incomplete_serialization_works()
        {
            var srcEntity = createIncompleteTestEntity();
            var srcDto = new FileInfoDTO(srcEntity);

            var str = JsonSerializer.Serialize(srcDto);
            var dstDto = JsonSerializer.Deserialize<FileInfoDTO>(str);
            var dstEntity = dstDto.ToEntity();

            compareEntities(srcEntity, dstEntity);
        }
    }
}