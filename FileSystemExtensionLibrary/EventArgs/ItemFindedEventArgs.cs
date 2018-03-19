using System;
using System.Collections.Generic;
using System.IO;


namespace FileSystemExtensionLibrary.EventArgs
{
    public class ItemFindedEventArgs<T> : System.EventArgs where T : FileSystemInfo
    {
        public T FindedItem { get; set; }
        public Action ActionType { get; set; }
    }
}
