using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Linq;
using System.Text.Json;

using TableTopCrucible.Data.SaveFile.DataTransferObjects;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.SaveFile.Tests.DataTransferObjects
{
    [TestClass]
    public class ItemDtoTests
    {
        private Item createTestEntity()
            => new Item(
                ItemId.New(),
                (ItemName)"itenName",
                new string[] { "tag 1", "tag 2" }.Select(x=>(Tag)x).ToArray(),
                new FileInfoHashKey(FileInfoDtoTests.createTestEntity()),
                (Thumbnail)@"C:\test\test.png",
                DateTime.Now.AddMinutes(-5),
                DateTime.Now);

        void compareEntityToDto(Item entity, ItemDTO dto)
        {

            Assert.AreEqual((Guid)entity.Id, dto.Id, "faulty id");
            Assert.AreEqual(entity.Created, dto.Created, "faulty creation timestamp");
            Assert.AreEqual(entity.LastChange, dto.LastChange, "faulty last change timestamp");

            Assert.AreEqual(entity.Tags?.Select(x=>(string)x)?.Except(dto.Tags)?.Count(), 0, "faulty tags");
            Assert.AreEqual((string)entity.Name, dto.Name, "faulty name");
            Assert.AreEqual((string)entity.Thumbnail, dto.Thumbnail, "faulty thumbnail");
            Assert.AreEqual(entity.File, dto.File.ToEntity(), "faulty files");
        }
        void compareEntities(Item entityA, Item entityB)
        {
            Assert.AreEqual(entityA.Id, entityB.Id, "faulty id");
            Assert.AreEqual(entityA.Created, entityB.Created, "faulty creation timestamp");
            Assert.AreEqual(entityA.LastChange, entityB.LastChange, "faulty last change timestamp");
            Assert.AreEqual(entityA.Tags.Except(entityB.Tags).Count(), 0, "faulty tags");
            Assert.AreEqual(entityA.Name, entityB.Name, "faulty name");
            Assert.AreEqual(entityA.Thumbnail, entityB.Thumbnail, "faulty thumbnail");
            Assert.AreEqual(entityA.File, entityB.File, "faulty files");
        }

        [TestMethod]
        public void Entity_to_dto_works()
        {
            var entity = createTestEntity();

            var dto = new ItemDTO(entity);

            compareEntityToDto(entity, dto);
        }
        [TestMethod]
        public void Dto_to_entity_works()
        {

            var dto = new ItemDTO(createTestEntity());

            var entity = dto.ToEntity();

            compareEntityToDto(entity, dto);
        }
        [TestMethod]
        public void serialization_works()
        {
            var srcEntity = createTestEntity();
            var srcDto = new ItemDTO(srcEntity);

            var str = JsonSerializer.Serialize(srcDto);
            var dstDto = JsonSerializer.Deserialize<ItemDTO>(str);
            var dstEntity = dstDto.ToEntity();

            compareEntities(srcEntity, dstEntity);
        }
    }
}
