using System;
using Moq;
using NUnit.Framework;
using System.IO;
using FileSystemExtensionLibrary;
using System.Collections.Generic;

namespace UnitTests
{
    [TestFixture]
    public class FileSystemVisitorTests
    {
        private Mock<FileSystemInfo> fileSystemInfoMock;
        private Mock<IDirectoryInfo> directoryInfoMock;
        private FileSystemVisitor fileSystemVisitor;

        [SetUp]
        public void TestInit()
        {
            fileSystemInfoMock = new Mock<FileSystemInfo>();
            fileSystemVisitor = new FileSystemVisitor();
            directoryInfoMock = new Mock<IDirectoryInfo>();

        }

        [Test]
        public void GetFileSystemInfoSequence_PassNullPath_ThrowArgumentNullException()
        {
            string path = null;

            Assert.Throws<ArgumentNullException>(delegate () {
                fileSystemVisitor.GetFileSystemInfoSequence(path);
            });
        }

        [Test]
        public void GetFileSystemInfoSequence_PassEmptyPath_ThrowArgumentNullException()
        {
            string path = null;

            Assert.Throws<ArgumentNullException>(delegate () {
                fileSystemVisitor.GetFileSystemInfoSequence(path);
            });
        }

        [Test]
        public void GetFileSystemInfoSequence_PassNullDirectory_ThrowArgumentNullException()
        {
            DirectoryInfo directory = null;

            Assert.Throws<ArgumentNullException>(delegate ()
            {
                foreach (var el in fileSystemVisitor.GetFileSystemInfoSequence(directory))
                { }
            });
        }

        [Test]
        public void GetFileSystemInfoSequence_NotAccessToDirectory_ThrowUnauthorizedAccessException()
        {
            string path = "C://";
            directoryInfoMock.Setup(a => a.EnumerateFileSystemInfos()).Throws<UnauthorizedAccessException>();

            Assert.Throws<UnauthorizedAccessException>(delegate ()
            {
                foreach (var el in fileSystemVisitor.GetFileSystemInfoSequence(path))
                { }
            });
        }

        [Test]
        public void GetFileSystemInfoSequence_StartEvent_EventHandled()
        {
            DirectoryInfo newDirectory = new DirectoryInfo("D://Test");

            int count = 0;
          
            fileSystemVisitor.Start += (s, e) =>
            {
                count++;
            };

            foreach (var el in fileSystemVisitor.GetFileSystemInfoSequence(newDirectory))
            { }

            Assert.AreEqual(1, count);
        }

        [Test]
        public void GetFileSystemInfoSequence_FinishEvent_EventHandled()
        {
            DirectoryInfo newDirectory = new DirectoryInfo("D://Test");

            int count = 0;
            
            fileSystemVisitor.Finish += (s, e) =>
            {
                count++;
            };

            foreach (var el in fileSystemVisitor.GetFileSystemInfoSequence(newDirectory))
            { }

            Assert.AreEqual(1, count);
        }

        [Test]
        public void GetFileSystemInfoSequence_FileFindedEvent_EventHandled()
        {
            fileSystemVisitor = new FileSystemVisitor(info => info.FullName.Contains("New"));
            DirectoryInfo newDirectory = new DirectoryInfo("D://Test");

            int count = 0;
            
            fileSystemVisitor.FileFinded += (s, e) =>
            {
                count++;
            };

            foreach (var el in fileSystemVisitor.GetFileSystemInfoSequence(newDirectory))
            { }

            Assert.AreEqual(3, count);
        }

        [Test]
        public void GetFileSystemInfoSequence_FileFilteredEvent_EventHandled()
        {
            fileSystemVisitor = new FileSystemVisitor(info => info.FullName.Contains("New"));
            DirectoryInfo newDirectory = new DirectoryInfo("D://Test");

            int count = 0;

            fileSystemVisitor.FilteredFileFinded += (s, e) =>
            {
                count++;
            };

            foreach (var el in fileSystemVisitor.GetFileSystemInfoSequence(newDirectory))
            { }

            Assert.AreEqual(2, count);
        }

        [Test]
        public void GetFileSystemInfoSequence_StopSearch_SearchStoped()
        {
            DirectoryInfo newDirectory = new DirectoryInfo("D://Test");

            int count = 0;

            fileSystemVisitor.FileFinded += (s, e) =>
            {
                count++;
                if (e.FindedItem.Name.Contains("new"))
                {
                    e.ActionType = FileSystemExtensionLibrary.Action.StopSearch;
                }
            };

            foreach (var el in fileSystemVisitor.GetFileSystemInfoSequence(newDirectory))
            { }

            Assert.AreEqual(3, count);
        }

        //Dances to Mock sealed class DirectoryInfo :)
        public interface IDirectoryInfo
        {
            IEnumerable<FileSystemInfo> EnumerateFileSystemInfos();
            DirectoryInfo wrappedInstance { get ; set; }
        }

        public class DirectoryInfoWrapper : IDirectoryInfo
        {
            public DirectoryInfo wrappedInstance { get { return new DirectoryInfo("C://"); } set { } }

            public DirectoryInfoWrapper()
            {
                this.wrappedInstance = new DirectoryInfo("C://");
            }

            public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos()
            {
               return this.wrappedInstance.EnumerateFileSystemInfos();
            }
        }
    }
}
