using System;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using JournalCli.Core;
using JournalCli.Infrastructure;
using Xunit;

namespace JournalCli.Tests
{
    public class MacEncryptedStoreTests
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void Constructor_CreatesKeys_IfNotExists()
        {
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            var dataDirectory = fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JournalCli");
            fileSystem.AllDirectories.Should().NotContain(dataDirectory);

            var store = new MacEncryptedStore<UserSettings>(fileSystem);
            fileSystem.AllDirectories.Should().Contain(dataDirectory);
            fileSystem.Directory.GetFiles(dataDirectory).Length.Should().Be(2, "There should be an encryption key and an auth key present.");
        }

        [Fact]
        public void Save_WritesCipherToDisk()
        {
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            var dataDirectory = fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JournalCli");

            var savingStore = new MacEncryptedStore<UserSettings>(fileSystem);
            fileSystem.Directory.GetFiles(dataDirectory).Length.Should().Be(2);
            var settings = _fixture.Create<UserSettings>();
            savingStore.Save(settings);

            fileSystem.Directory.GetFiles(dataDirectory).Length.Should().Be(3);
        }

        [Fact]
        public void Load_CorrectlyRestoresSavedValue()
        {
            var fileSystem = new MockFileSystem();
            var store = new MacEncryptedStore<UserSettings>(fileSystem);
            var settings = _fixture.Create<UserSettings>();
            store.Save(settings);

            var result = store.Load();
            result.Should().BeEquivalentTo(settings);
        }

        [Fact]
        public void Load_ReturnsEmptyObject_WhenNoSavedObjectExists()
        {
            var fileSystem = new MockFileSystem();
            var store = new MacEncryptedStore<UserSettings>(fileSystem);

            var result = store.Load();
            result.Should().BeEquivalentTo(new UserSettings());
        }

        [Fact]
        public void Save_UsesSavedTypesSimpleNameAsCipherName()
        {
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            var store = new MacEncryptedStore<UserSettings>(fileSystem);
            var settings = _fixture.Create<UserSettings>();
            store.Save(settings);

            fileSystem.AllFiles.Select(f => fileSystem.Path.GetFileName(f)).Should()
                .Contain("UserSettings", because: "If this name ever changes, it means a breaking change because Load will not find previously saved items.");
        }
    }
}
