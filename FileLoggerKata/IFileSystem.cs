using System;

namespace FileLoggerKata
{
    public interface IFileSystem
    {
        void Append(string path, string message);
        void Create(string path);
        bool Exists(string path);
        DateTime GetLastWriteTime(string path);
        void Rename(string currentPath, string newPath);
    }
}