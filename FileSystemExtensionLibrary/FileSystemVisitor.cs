using FileSystemExtensionLibrary.EventArgs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Permissions;

namespace FileSystemExtensionLibrary
{
    public sealed class FileSystemVisitor
    {
        #region  Fields
        private readonly DirectoryInfo startDirectory;
        private readonly Func<FileSystemInfo, bool> filter;
        #endregion

        #region  Constructors
        public FileSystemVisitor(string path, 
            Func<FileSystemInfo, bool> filter = null): this(new DirectoryInfo(path), filter) { }

        public FileSystemVisitor(DirectoryInfo startDirectory,
            Func<FileSystemInfo, bool> filter = null)
        {
            this.startDirectory = startDirectory;
            this.filter = filter;
        }
        #endregion

        #region  Events
        public event EventHandler<StartEventArgs> Start;
        public event EventHandler<FinishEventArgs> Finish;
        public event EventHandler<ItemFindedEventArgs<FileInfo>> FileFinded;
        public event EventHandler<ItemFilteredEventArgs<FileInfo>> FilteredFileFinded;
        public event EventHandler<ItemFindedEventArgs<DirectoryInfo>> DirectoryFinded;
        public event EventHandler<ItemFilteredEventArgs<DirectoryInfo>> FilteredDirectoryFinded;
        #endregion

        public IEnumerable<FileSystemInfo> GetFileSystemInfoSequence()
        {
            OnEvent(Start, new StartEventArgs());
            foreach (var fileSystemInfo in GetAllSubFileSystemInfo(startDirectory))
            {
                yield return fileSystemInfo;
            }
            OnEvent(Finish, new FinishEventArgs());
        }

        #region  Private region
        private IEnumerable<FileSystemInfo> GetAllSubFileSystemInfo(DirectoryInfo directory)
        {
            if(directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            IEnumerable<FileSystemInfo> directories;

            try
            {
                directories = directory.EnumerateFileSystemInfos();
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException($"You don't have access to the {directory.FullName} directory");
            }

            foreach (var fileSystemInfo in directories)
            {
                FileInfo fileInfo = fileSystemInfo as FileInfo;
                if (fileInfo != null)
                {
                    ItemFindedEventArgs<FileInfo> fileEventArgs = new ItemFindedEventArgs<FileInfo> { FindedItem = fileInfo, ActionType = Action.ContinueSearch };
                    OnEvent(FileFinded, fileEventArgs);

                    //It Checks what the consumer want to do with the founded file 
                    if(fileEventArgs.ActionType == Action.StopSearch)
                    {
                        yield break;
                    }
                    else if (fileEventArgs.ActionType == Action.SkipElement)
                    {
                        continue;
                    }
                    else if (FileSystemInfoFilter(fileInfo, FilteredFileFinded))
                    {
                        yield return fileInfo;
                    }
                }

                DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
                if (directoryInfo != null)
                {
                    ItemFindedEventArgs<DirectoryInfo> directoryEventArgs = new ItemFindedEventArgs<DirectoryInfo> { FindedItem = directoryInfo, ActionType = Action.ContinueSearch };
                    OnEvent(DirectoryFinded, directoryEventArgs);

                    //It Checks what the consumer want to do with the founded directory
                    if (directoryEventArgs.ActionType == Action.StopSearch)
                    {
                        yield break;
                    }
                    else if ((directoryEventArgs.ActionType == Action.ContinueSearch) && FileSystemInfoFilter(directoryInfo, FilteredDirectoryFinded))
                    {
                        yield return directoryInfo;
                    }

                    foreach (var innerInfo in GetAllSubFileSystemInfo(directoryInfo))
                    {
                        yield return innerInfo;
                    }
                }
            }
           
        }


        private bool FileSystemInfoFilter<T> (T fileSystemInfo, EventHandler<ItemFilteredEventArgs<T>> filterEvent) where T : FileSystemInfo
        {
            if(fileSystemInfo == null)
            {
                throw new ArgumentNullException("fileSystemInfo");
            }

            if(filter == null)
            {
                return true;
            }

            if (filter(fileSystemInfo))
            {
                OnEvent(filterEvent, new ItemFilteredEventArgs<T> { FilteredItem = fileSystemInfo });
                return true;
            }

            return false;
        }

        private void OnEvent<TArgs>(EventHandler<TArgs> someEvent, TArgs args)
        {
            someEvent?.Invoke(this, args);
        }
        #endregion
    }
}
