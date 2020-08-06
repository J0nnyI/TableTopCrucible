using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Linq;
using System.Text.Json;

using TableTopCrucible.Data.SaveFile.DataTransferObjects;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;

namespace TableTopCrucible.Data.SaveFile.Tests.DataTransferObjects
{
    [TestClass]
    public class FileItemLinkDtoTests
    {
        private FileItemLink createTestEntity()
            => new FileItemLink(
                FileItemLinkId.New(),
                ItemId.New(),
                new FileInfoHashKey(FileInfoDtoTests.createTestEntity()),
                null,
                new Version(1, 2, 3),
                DateTime.Now.AddMinutes(-5),
                DateTime.Now);

        void compareEntityToDto(FileItemLink entity, FileItemLinkDTO dto)
        {

            Assert.AreEqual((Guid)entity.Id, dto.Id, "faulty id");
            Assert.AreEqual(entity.Created, dto.Created, "faulty creation timestamp");
            Assert.AreEqual(entity.LastChange, dto.LastChange, "faulty last change timestamp");

            Assert.AreEqual((Guid)entity.ItemId, dto.ItemId, "faulty item-id");
            Assert.AreEqual(entity.FileKey, dto.FileKey.ToEntity(), "faulty FileHash");
            Assert.AreEqual(entity.Version, dto.Version.ToEntity(), "faulty version");
        }
        void compareEntities(FileItemLink entityA, FileItemLink entityB)
        {
            Assert.AreEqual(entityA.Id, entityB.Id, "faulty id");
            Assert.AreEqual(entityA.Created, entityB.Created, "faulty creation timestamp");
            Assert.AreEqual(entityA.LastChange, entityB.LastChange, "faulty last change timestamp");

            Assert.AreEqual(entityA.ItemId, entityB.ItemId, "faulty item-id");
            Assert.AreEqual(entityA.FileKey, entityB.FileKey, "faulty FileHash");
            Assert.AreEqual(entityA.Version, entityB.Version, "faulty version");
        }

        [TestMethod]
        public void Entity_to_dto_works()
        {
            var entity = createTestEntity();

            var dto = new FileItemLinkDTO(entity);

            compareEntityToDto(entity, dto);
        }
        [TestMethod]
        public void Dto_to_entity_works()
        {

            var dto = new FileItemLinkDTO(createTestEntity());

            var entity = dto.ToEntity();

            compareEntityToDto(entity, dto);
        }
        [TestMethod]
        public void serialization_works()
        {
            var srcEntity = createTestEntity();
            var srcDto = new FileItemLinkDTO(srcEntity);

            var str = JsonSerializer.Serialize(srcDto);
            var dstDto = JsonSerializer.Deserialize<FileItemLinkDTO>(str);
            var dstEntity = dstDto.ToEntity();

            compareEntities(srcEntity, dstEntity);
        }
    }
}
