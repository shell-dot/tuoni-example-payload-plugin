using System;
using System.IO;
using System.Text;

namespace DotNetAgent
{
    internal static class NativeCommandLsHelper
    {
        private const byte TLV_DIRECTORY = 0x1;
        private const byte TLV_FILE = 0x2;
        private const byte TLV_NAME = 0x1;
        private const byte TLV_FILE_SIZE = 0x2;
        private const byte TLV_CHILDREN = 0x3;
        private const byte TLV_CREATION_TIME = 0x4;
        private const byte TLV_LAST_ACCESS_TIME = 0x5;
        private const byte TLV_LAST_WRITE_TIME = 0x6;

        public static long ToUnixTimestamp(DateTime dt)
        {
            return new DateTimeOffset(dt.ToUniversalTime()).ToUnixTimeSeconds();
        }

        public static bool ListDirFiles(string dir, uint depth, TLV resultTlv, bool isRoot)
        {
            try
            {
                DirectoryInfo dirInfo;

                if (isRoot && depth <= 1)
                {
                    if (!Directory.Exists(dir))
                    {
                        if (File.Exists(dir))
                        {
                            FileInfo singleFile = new FileInfo(dir);
                            AddFileTlv(resultTlv, singleFile);
                            return true;
                        }
                        return false;
                    }
                    dirInfo = new DirectoryInfo(dir);
                }
                else
                {
                    dirInfo = new DirectoryInfo(dir);
                    if (!dirInfo.Exists)
                        return !isRoot;
                }

                foreach (DirectoryInfo subDir in dirInfo.GetDirectories())
                {
                    TLV dirTlv = new TLV(TLV_DIRECTORY);
                    dirTlv.AddChild(new TLV(TLV_NAME, Encoding.UTF8.GetBytes(subDir.Name)));

                    TLV subTlv = new TLV(TLV_CHILDREN);
                    if (depth > 1)
                    {
                        if (!ListDirFiles(subDir.FullName, depth - 1, subTlv, false))
                        {
                            return false;
                        }
                    }

                    dirTlv.AddChild(subTlv);
                    dirTlv.AddChild(new TLV(TLV_CREATION_TIME, BitConverter.GetBytes(ToUnixTimestamp(subDir.CreationTime))));
                    dirTlv.AddChild(new TLV(TLV_LAST_ACCESS_TIME, BitConverter.GetBytes(ToUnixTimestamp(subDir.LastAccessTime))));
                    dirTlv.AddChild(new TLV(TLV_LAST_WRITE_TIME, BitConverter.GetBytes(ToUnixTimestamp(subDir.LastWriteTime))));
                    resultTlv.AddChild(dirTlv);
                }

                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    AddFileTlv(resultTlv, file);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Warning($"ListDirFiles error: {ex.Message}");
                return false;
            }
        }

        private static void AddFileTlv(TLV resultTlv, FileInfo file)
        {
            TLV fileTlv = new TLV(TLV_FILE);
            fileTlv.AddChild(new TLV(TLV_NAME, Encoding.UTF8.GetBytes(file.Name)));
            fileTlv.AddChild(new TLV(TLV_FILE_SIZE, BitConverter.GetBytes(file.Length)));
            fileTlv.AddChild(new TLV(TLV_CREATION_TIME, BitConverter.GetBytes(ToUnixTimestamp(file.CreationTime))));
            fileTlv.AddChild(new TLV(TLV_LAST_ACCESS_TIME, BitConverter.GetBytes(ToUnixTimestamp(file.LastAccessTime))));
            fileTlv.AddChild(new TLV(TLV_LAST_WRITE_TIME, BitConverter.GetBytes(ToUnixTimestamp(file.LastWriteTime))));
            resultTlv.AddChild(fileTlv);
        }
    }
}
