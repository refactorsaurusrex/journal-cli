using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using JournalCli.Core;
using JournalCli.Infrastructure;
using Xunit;

namespace JournalCli.Tests
{
    public class WindowsEncryptedStoreTests
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void Save_WritesCipherAndEntropyToDisk()
        {
            var fileSystem = A.Fake<IFileSystem>();
            var store = new WindowsEncryptedStore<UserSettings>(fileSystem);
            var settings = new UserSettings { BackupPassword = "secret" };
            store.Save(settings);

            A.CallTo(() => fileSystem.File.WriteAllBytes(A<string>.Ignored, A<byte[]>.Ignored)).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public void Load_CorrectlyRestoresSavedValue()
        {
            var fileSystem = new MockFileSystem();
            var store = new WindowsEncryptedStore<UserSettings>(fileSystem);
            var settings = _fixture.Create<UserSettings>();
            store.Save(settings);

            var result = store.Load();
            result.Should().BeEquivalentTo(settings);
        }

        [Fact]
        public void WindowsEncryptedStore_CreatesDataDirectory_WhenItDoesntExist()
        {
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            var dataDirectory = fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JournalCli");

            fileSystem.AllDirectories.Should().NotContain(dataDirectory);

            var store = new WindowsEncryptedStore<UserSettings>(fileSystem);
            var settings = _fixture.Create<UserSettings>();
            store.Save(settings);

            fileSystem.AllDirectories.Should().Contain(dataDirectory);
        }

        [Fact]
        public void Load_ReturnsEmptyObject_WhenNoSavedObjectExists()
        {
            var fileSystem = new MockFileSystem();
            var store = new WindowsEncryptedStore<UserSettings>(fileSystem);

            var result = store.Load();
            result.Should().BeEquivalentTo(new UserSettings());
        }

        [Fact]
        public void Save_UsesSavedTypesSimpleNameAsCipherName()
        {
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            var store = new WindowsEncryptedStore<UserSettings>(fileSystem);
            var settings = _fixture.Create<UserSettings>();
            store.Save(settings);

            fileSystem.AllFiles.Select(f => fileSystem.Path.GetFileName(f)).Should()
                .Contain("UserSettings", because: "If this name ever changes, it means a breaking change because Load will not find previously saved items.");
        }
    }
}
