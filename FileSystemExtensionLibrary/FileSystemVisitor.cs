using FileSystemExtensionLibrary.EventArgs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Permissions;

namespace FileSystemExtensionLibrary
{
    public class FileSystemVisitor
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
        public event EventHandler<ItemFindedEventArgs<FileInfo>> FilteredFileFinded;
        public event EventHandler<ItemFindedEventArgs<DirectoryInfo>> DirectoryFinded;
        public event EventHandler<ItemFindedEventArgs<DirectoryInfo>> FilteredDirectoryFinded;
        #endregion

        public IEnumerable<FileSystemInfo> GetFileSystemInfoSequence()
        {
            OnEvent(Start, new StartEventArgs());
            foreach (var fileSystemInfo in GetAllSubFileSystemInfo(startDirectory /*,CurrentAction.ContinueSearch*/))
            {
                yield return fileSystemInfo;
            }
            OnEvent(Finish, new FinishEventArgs());
        }

        private IEnumerable<FileSystemInfo> GetAllSubFileSystemInfo(DirectoryInfo directory/*, CurrentAction currentAction*/)
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
                    //currentAction.Action = ProcessFile(fileSystemInfo);
                    OnEvent(FileFinded, new ItemFindedEventArgs<FileInfo> { FindedItem = fileInfo });

                    if (FileSystemInfoFilter(fileInfo, FilteredFileFinded))
                    {
                        yield return fileInfo;
                    }

                    //if(filter != null)
                    //{
                    //    if (filter(fileInfo))
                    //    {
                    //        OnEvent(FilteredFileFinded, new ItemFindedEventArgs<FileInfo> { FindedItem = fileInfo });
                    //        yield return fileInfo;
                    //    }
                    //}
                    //else
                    //{
                    //    yield return fileInfo;
                    //}
                }

                DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
                if (directoryInfo != null)
                {
                    OnEvent(DirectoryFinded, new ItemFindedEventArgs<DirectoryInfo> { FindedItem = directoryInfo });

                    if (FileSystemInfoFilter(directoryInfo, FilteredDirectoryFinded))
                    {
                        yield return directoryInfo;
                    }

                    foreach (var innerInfo in GetAllSubFileSystemInfo(directoryInfo)/*, currentAction*/)
                    {
                        yield return innerInfo;
                    }
                    continue;
                }

                //if (currentAction.Action == Action.StopSearch)
                //{
                //    yield break;
                //}
            }
           
        }

        private bool FileSystemInfoFilter<T> (T fileSystemInfo, EventHandler<ItemFindedEventArgs<T>> filterEvent) where T : FileSystemInfo
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
                OnEvent(filterEvent, new ItemFindedEventArgs<T> { FindedItem = fileSystemInfo });
                return true;
            }

            return false;
        }

        //private bool DirectoryFilter(DirectoryInfo directoryInfo)
        //{
        //    if (directoryInfo == null)
        //    {
        //        throw new ArgumentNullException("directoryInfo");
        //    }

        //    if (filter == null)
        //    {
        //        return true;
        //    }

        //    if (filter(directoryInfo))
        //    {
        //        OnEvent(FilteredDirectoryFinded, new ItemFindedEventArgs<DirectoryInfo> { FindedItem = directoryInfo });
        //        return true;
        //    }

        //    return false;
        //}

        private void OnEvent<TArgs>(EventHandler<TArgs> someEvent, TArgs args)
        {
            someEvent?.Invoke(this, args);
        }

        //private class CurrentAction
        //{
        //    public Action Action { get; set; }
        //    public static CurrentAction ContinueSearch => new CurrentAction { Action = Action.ContinueSearch };
        //}

    }
}
