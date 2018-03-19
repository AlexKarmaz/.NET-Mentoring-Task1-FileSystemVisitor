using System;
using System.Collections.Generic;
using System.IO;

namespace FileSystemExtensionLibrary.EventArgs
{
    public class ItemFilteredEventArgs<T> : System.EventArgs where T : FileSystemInfo
    {
        public T FilteredItem { get; set; }
    }
}
