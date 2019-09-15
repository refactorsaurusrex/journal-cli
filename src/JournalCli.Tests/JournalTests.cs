using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using JournalCli.Core;
using JournalCli.Infrastructure;
using Xunit;

namespace JournalCli.Tests
{
    public class JournalTests
    {
        [Fact]
        public void CreateNewEntry_CreatesFileNames_ThatMatchTodaysDate()
        {
            //IFileSystem fileSystem = new MockFileSystem();
            //IJournalReaderFactory readerFactory = new JournalReaderFactory(fileSystem);
            //var journal = Journal.Open(readerFactory, fileSystem, systemProcess, rootDirectory);
        }
    }
}
