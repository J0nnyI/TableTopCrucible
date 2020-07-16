using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using TableTopCrucible.Domain.Models.ValueTypes;
using System.Text.Json;
using System.IO;
using TableTopCrucible.Data.SaveFile.DataTransferObjects;

namespace TableTopCrucible.Data.SaveFile.Tests.DataTransferObjects
{
    [TestClass]
    public class DirectorySetupDtoTests
    {
        private DirectorySetup createTestEntity()
            => new DirectorySetup(new Uri(@"C:\test\test.test"), (DirectorySetupName)"testDir", (Description)"testDesc", DirectorySetupId.New(), DateTime.Now.AddMinutes(-5), DateTime.Now);

        void compareEntityToDto(DirectorySetup entity, DirectorySetupDTO dto)
        {

            Assert.AreEqual((Guid)entity.Id, dto.Id, "faulty id");
            Assert.AreEqual(entity.Created, dto.Created, "faulty creation timestamp");
            Assert.AreEqual(entity.LastChange, dto.LastChange, "faulty last change timestamp");
            Assert.AreEqual(entity.Path, dto.Path, "faulty path");
            Assert.AreEqual((string)entity.Name, dto.Name, "faulty name");
            Assert.AreEqual((string)entity.Description, dto.Description, "faulty description");
        }
        void compareEntities(DirectorySetup entityA, DirectorySetup entityB)
        {
            Assert.AreEqual(entityA.Id, entityB.Id, "faulty id");
            Assert.AreEqual(entityA.Created, entityB.Created, "faulty creation timestamp");
            Assert.AreEqual(entityA.LastChange, entityB.LastChange, "faulty last change timestamp");
            Assert.AreEqual(entityA.Path, entityB.Path, "faulty path");
            Assert.AreEqual(entityA.Name, entityB.Name, "faulty name");
            Assert.AreEqual(entityA.Description, entityB.Description, "faulty description");
            Assert.AreNotEqual(entityA.Identity, entityB.Identity, "it's the same instance");
        }

        [TestMethod]
        public void Entity_to_dto_works()
        {
            var entity = createTestEntity();

            var dto = new DirectorySetupDTO(entity);

            compareEntityToDto(entity, dto);
        }
        [TestMethod]
        public void Dto_to_entity_works()
        {

            var dto = new DirectorySetupDTO(createTestEntity());

            var entity = dto.ToEntity();

            compareEntityToDto(entity, dto);
        }
        [TestMethod]
        public void serialization_works()
        {
            var srcEntity = createTestEntity();
            var srcDto = new DirectorySetupDTO(srcEntity);

            var str = JsonSerializer.Serialize(srcDto);
            var dstDto = JsonSerializer.Deserialize<DirectorySetupDTO>(str);
            var dstEntity = dstDto.ToEntity();

            compareEntities(srcEntity, dstEntity);
        }
    }
}