//========================================================================
/// 文件管理器
/// *仅用于FileStream能够使用的场合
/// *如Android的StreamingAssets目录由于Jar的原因，无法应用该文件管理器
/// @neoyang
/// @2015.01.06
//========================================================================

using UnityEngine;
using System.Collections;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System;
using System.Text;
using System.Collections.Generic;
//文件操作
public enum enFileOperation
{
    ReadFile,
    WriteFile,
    DeleteFile,
    CreateDirectory,
    DeleteDirectory,
};

public class CFileManager
{
    //Assets目录
    public const string c_assetsFolder = "Assets";

    //Custom资源目录
#if USE_CUSTOM_RESOURCES
    public const string s_customResourcesFolder = "CustomResources";
#else
    public const string s_customResourcesFolder = "Resources";
#endif

    //数据缓存路径
    private static string s_cachePath = null;

    public const string EXTRAIFS_NAME_PREFIX = "ExtraPack_";    

    //IFS解压路径
    public static string s_ifsExtractFolder = "Resources";
    public static string s_extraIfsExtractFolder = "ExtRes";
    //    private static string[] s_ifsExtractPath = new string[CPufferResMgr.k_commonResIfsId + 1];
    private static string[] s_ifsExtractPath = new string[1024 + 1];

    private static string s_extraIfsExtractPath = null;
    private static string s_streamingAssetsPath = null;

    //ifs mount path
    private static string s_ifsMountPath = null;

    //md5计算器
    private static MD5CryptoServiceProvider s_md5Provider = new MD5CryptoServiceProvider();

    //文件操作失败事件
    public delegate void DelegateOnOperateFileFail(string fullPath, string fileInfo, enFileOperation fileOperation, Exception ex);
    public static DelegateOnOperateFileFail s_delegateOnOperateFileFail = delegate { };

    //IFS文件相关操作
    public delegate bool DelegateIsFileExistInIFS(string filePathInIFS);
    public static DelegateIsFileExistInIFS s_isFileExistInIFS = delegate { return false; };

    public unsafe delegate uint DelegateLoadFileFromIFS(string filePathInIFS, sbyte* pBuffer, uint bufferSize);
    public static DelegateLoadFileFromIFS s_loadFileFromIFS = delegate { return 0; };

    //资源数据缓存（适用于保存原始数据并立即进行加工处理，并且处理完成之后不再需要原始数据的场合）<neoyang>
    //特别要注意的是，在从buffer处理原始数据的过程中，不能再次读取新的资源数据到buffer中
    public const uint c_fileBufferSize = 1024 * 1024 * 4;
    private static byte[] s_fileBuffer = new byte[c_fileBufferSize];
    private static bool s_isFileBufferLocked = false;

    public static readonly string ResourcesRootPath = Application.dataPath + "/" + CFileManager.s_customResourcesFolder;

    //----------------------------------------------
    /// 注册回调
    /// @isFileExistInIFS
    /// @loadFileFromIFS
    //----------------------------------------------
    public static void RegisterDelegate(DelegateIsFileExistInIFS isFileExistInIFS, DelegateLoadFileFromIFS loadFileFromIFS)
    {
        s_isFileExistInIFS = isFileExistInIFS;
        s_loadFileFromIFS = loadFileFromIFS;
    }

    //----------------------------------------------
    /// 文件是否存在
    /// @filePath
    //----------------------------------------------
    public static bool IsFileExist(string filePath)
    {
        return System.IO.File.Exists(filePath);
    }

    //----------------------------------------------
    /// 目录是否存在
    /// @directory
    //----------------------------------------------
    public static bool IsDirectoryExist(string directory)
    {
        return System.IO.Directory.Exists(directory);
    }

    //--------------------------------------------------
    /// 文件是否存在于StreamingAssets目录下
    /// @fileName
    //--------------------------------------------------
    public static bool IsFileExistInStreamingAssets(string fileName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return AndroidJavaUtility.Android_IsFileExistInStreamingAssets(fileName);
#else
        return IsFileExist(CFileManager.CombinePath(Application.streamingAssetsPath, fileName));
#endif
    }

    //----------------------------------------------
    /// 文件是否存在于IFS中
    /// @filePathInIFS
    //----------------------------------------------
    public static bool IsFileExistInIFS(string filePathInIFS)
    {
        return s_isFileExistInIFS(filePathInIFS);
    }

    //----------------------------------------------
    /// 重建目录
    /// @directory
    //----------------------------------------------
    public static void ReCreateDirectory(string directory)
    {
        if (IsDirectoryExist(directory))
        {
            DeleteDirectory(directory);
        }

        CreateDirectory(directory);
    }

    //----------------------------------------------
    /// 创建目录
    /// @directory
    //----------------------------------------------
    public static bool CreateDirectory(string directory)
    {
        if (IsDirectoryExist(directory))
        {
            return true;
        }

        int tryCount = 0;

        while (true)
        {
            try
            {
                System.IO.Directory.CreateDirectory(directory);
                return true;
            }
            catch (Exception ex)
            {
                tryCount++;

                if (tryCount >= 3)
                {
                    //Debug.Log("Create Directory " + directory + " Error! Exception = " + ex.ToString());

                    //派发事件
                    s_delegateOnOperateFileFail(directory, string.Format("Directory.CreateDirectory() Exception, TryCount = {0}", tryCount), enFileOperation.CreateDirectory, ex);
                    
                    return false;
                }
            }
        }
    }

    //----------------------------------------------
    /// 删除目录
    /// @directory
    //----------------------------------------------
    public static bool DeleteDirectory(string directory)
    {
        if (!IsDirectoryExist(directory))
        {
            return true;
        }

        int tryCount = 0;

        while (true)
        {
            try
            {
                System.IO.Directory.Delete(directory, true);
                return true;
            }
            catch (System.Exception ex)
            {
                tryCount++;

                if (tryCount >= 3)
                {
                    //Debug.Log("Delete Directory " + directory + " Error! Exception = " + ex.ToString());

                    //派发事件
                    s_delegateOnOperateFileFail(directory, string.Format("Directory.Delete() Exception, TryCount = {0}", tryCount), enFileOperation.DeleteDirectory, ex);

                    return false;
                }
            }
        }
    }

    //----------------------------------------------
    /// 获取文件长度
    /// @filePath
    //----------------------------------------------
    public static uint GetFileLength(string filePath)
    {
        if (!IsFileExist(filePath))
        {
            return 0;
        }

        int tryCount = 0;

        while (true)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return (uint)fileInfo.Length;
            }
            catch (Exception ex)
            {
                tryCount++;

                if (tryCount >= 3)
                {
                    Debug.Log("Get FileLength of " + filePath + " Error! Exception = " + ex.ToString());
                    return 0;
                }
            }
        }
    }

    //----------------------------------------------
    /// 读取文件
    /// @filePath
    //----------------------------------------------
    public static byte[] ReadFile(string filePath)
    {
        if (!IsFileExist(filePath))
        {
            return null;
        }

        byte[] data = null;
        int tryCount = 0;

        while (true)
        {
            System.Exception CurEexception = null;
            
            try
            {
                data = System.IO.File.ReadAllBytes(filePath);
            }
            catch (System.Exception ex)
            {
                //Debug.Log("Read File " + filePath + " Error! Exception = " + ex.ToString() + ", TryCount = " + tryCount);
                data = null;
                CurEexception = ex;
            }

            if (data == null || data.Length <= 0)
            {
                tryCount++;

                if (tryCount >= 3)
                {
                    //Debug.Log("Read File " + filePath + " Fail!, TryCount = " + tryCount);

                    //派发事件
                    s_delegateOnOperateFileFail(filePath, string.Format("File.ReadAllBytes() Exception, TryCount = {0}", tryCount), enFileOperation.ReadFile, CurEexception);

                    return null;
                }
            }
            else
            {
                return data;
            }
        }
    }

    //----------------------------------------------
    /// 读取文件
    /// @filePath
    /// @buffer
    /// @bufferSize
    //----------------------------------------------
    public static uint ReadFile(string filePath, byte[] buffer, uint bufferSize)
    {
        if (!IsFileExist(filePath))
        {
            return 0;
        }

        uint fileLength = 0;
        int tryCount = 0;

        while (true)
        {
            System.Exception CurEexception = null;

            FileStream fileStream = null;

            try
            {
                fileStream = System.IO.File.OpenRead(filePath);

                //DebugHelper.Assert(bufferSize > (uint)fileStream.Length, string.Format("FileLength is larger than buffer!!! FilePath = {0}, FileLength = {1}, BufferSize = {2}", filePath, fileStream.Length, bufferSize));

                fileLength = (uint)fileStream.Read(buffer, 0, (int)fileStream.Length);
            }
            catch (System.Exception ex)
            {
                //Debug.Log("Read File " + filePath + " Error! Exception = " + ex.ToString() + ", TryCount = " + tryCount);
                fileLength = 0;
                CurEexception = ex;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }

            if (fileLength == 0)
            {
                tryCount++;

                if (tryCount >= 3)
                {
                    //Debug.Log("Read File " + filePath + " Fail!, TryCount = " + tryCount);

                    //派发事件
                    s_delegateOnOperateFileFail(filePath, string.Format("File.ReadAllBytes() Exception, TryCount = {0}", tryCount), enFileOperation.ReadFile, CurEexception);

                    return 0;
                }
            }
            else
            {
                return fileLength;
            }
        }
    }

    //----------------------------------------------
    /// 按文本方式读取文件
    /// @filePath
    //----------------------------------------------
    public static string ReadAllText(string filePath)
    {
        string result = string.Empty;

        if (IsFileExist(filePath))
        {
            try
            {
                result = System.IO.File.ReadAllText(filePath);
            }
            catch (Exception)
            {
                result = string.Empty;
            }
        }

        return result;
    }

    //----------------------------------------------
    /// 按文本方式读取文件
    /// @filePath
    /// @encoding
    //----------------------------------------------
    public static string ReadAllText(string filePath, System.Text.Encoding encoding)
    {
        string result = string.Empty;

        if (IsFileExist(filePath))
        {
            try
            {
                result = System.IO.File.ReadAllText(filePath, encoding);
            }
            catch (Exception)
            {
                result = string.Empty;
            }
        }

        return result;
    }

    //----------------------------------------------
    /// 从IFS读取文件
    /// @filePathInIFS
    /// @buffer
    /// @bufferSize
    //----------------------------------------------
    public static unsafe uint ReadFileFromIFS(string filePathInIFS, byte[] buffer, uint bufferSize)
    {
        fixed (byte* pBuffer = buffer)
        {
            return s_loadFileFromIFS(filePathInIFS, (sbyte*)pBuffer, bufferSize);
        }
    }

    //----------------------------------------------
    /// 写入文件
    /// @filePath
    /// @data
    //----------------------------------------------
    public static bool WriteFile(string filePath, byte[] data)
    {
        DeleteFile(filePath);

        int tryCount = 0;

        while (true)
        {
            try
            {
                System.IO.File.WriteAllBytes(filePath, data);
                return true;
            }
            catch (System.Exception ex)
            {
                tryCount++;

                if (tryCount >= 3)
                {
                    //Debug.Log("Write File " + filePath + " Error! Exception = " + ex.ToString());

                    //这里应该删除文件以防止数据错误
                    DeleteFile(filePath);

                    //派发事件
                    s_delegateOnOperateFileFail(filePath, string.Format("File.WriteAllBytes() Exception, TryCount = {0}", tryCount), enFileOperation.WriteFile, ex);

                    return false;
                }
            }
        }
    }

    //----------------------------------------------
    /// 写入文件
    /// @filePath
    /// @data
    /// @offset
    /// @length
    //----------------------------------------------
    public static bool WriteFile(string filePath, byte[] data, int offset, int length)
    {
        DeleteFile(filePath);

        FileStream fileStream = null;

        int tryCount = 0;

        while (true)
        {
            try
            {
                fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                fileStream.Write(data, offset, length);
                fileStream.Close();
                fileStream.Dispose();

                return true;
            }
            catch (System.Exception ex)
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }                

                tryCount++;

                if (tryCount >= 3)
                {
                    Debug.LogError("Write File " + filePath + " Error! Exception = " + ex.ToString());

                    //这里应该删除文件以防止数据错误
                    DeleteFile(filePath);

                    //派发事件
                    s_delegateOnOperateFileFail(filePath, string.Format("FileStream.Write() Exception, TryCount = {0}", tryCount), enFileOperation.WriteFile, ex);

                    return false;
                }
            }            
        }
    }

    //----------------------------------------------
    /// 删除文件
    /// @filePath
    //----------------------------------------------
    public static bool DeleteFile(string filePath)
    {
        if (!IsFileExist(filePath))
        {
            return true;
        }

        int tryCount = 0;

        while (true)
        {
            try
            {
                System.IO.File.Delete(filePath);
                return true;
            }
            catch (System.Exception ex)
            {
                tryCount++;

                if (tryCount >= 3)
                {
                    //Debug.Log("Delete File " + filePath + " Error! Exception = " + ex.ToString());

                    //派发事件
                    s_delegateOnOperateFileFail(filePath, string.Format("File.Delete() Exception, TryCount = {0}", tryCount), enFileOperation.DeleteFile, ex);

                    return false;
                }
            }
        }
    }

    //----------------------------------------------
    /// 拷贝文件
    /// @srcFile
    /// @dstFile
    //----------------------------------------------
    public static void CopyFile(string srcFile, string dstFile)
    {
        System.IO.File.Copy(srcFile, dstFile, true);
    }

    //----------------------------------------------
    /// 剪切or重命名文件
    /// @srcFile
    /// @dstFile
    //----------------------------------------------
    public static void MoveFile(string srcFile, string dstFile)
    {
        System.IO.File.Move(srcFile, dstFile);
    }

    //----------------------------------------------
    /// 从StreamingAssets拷贝文件
    /// @srcFile
    /// @dstFile
    //----------------------------------------------
    public static bool CopyFileFromStreamingAssets(string srcFile, string dstFile)
    {
        bool success = false;

        WWW www = new WWW(GetStreamingAssetsPathWithHeader(srcFile));
        while (!www.isDone)
        {}

        if (string.IsNullOrEmpty(www.error))
        {
            success = WriteFile(dstFile, www.bytes);
        }
        else
        {
            success = false;
        }
       
        www.Dispose();

        return success;
    }

    //----------------------------------------------
    /// 按规则返回指定目录下的所有文件
    /// @directory
    /// @searchPattern
    /// @searchAllDirectories : 是否需要遍历diretory下的子目录
    //----------------------------------------------
    public static string[] GetFilesInDirectory(string directory, string searchPattern, bool searchAllDirectories)
    {
        return System.IO.Directory.GetFiles(directory, searchPattern, (searchAllDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
    }

    //----------------------------------------------
    /// 返回文件md5
    /// @filePath
    //----------------------------------------------
    public static string GetFileMd5(string filePath)
    {
        if (!IsFileExist(filePath))
        {
            return string.Empty;
        }

        return System.BitConverter.ToString(s_md5Provider.ComputeHash(ReadFile(filePath))).Replace("-", "");
    }

    //----------------------------------------------
    /// 返回字节流md5
    /// @data
    //----------------------------------------------
    public static string GetMd5(byte[] data)
    {
        return System.BitConverter.ToString(s_md5Provider.ComputeHash(data)).Replace("-", "");
    }

    //----------------------------------------------
    /// 返回字节流md5
    /// @data
    /// @offset
    /// @length
    //----------------------------------------------
    public static string GetMd5(byte[] data, int offset, int length)
    {
        return System.BitConverter.ToString(s_md5Provider.ComputeHash(data, offset, length)).Replace("-", "");
    }

    //----------------------------------------------
    /// 返回字符串md5
    /// @str
    //----------------------------------------------
    public static string GetMd5(string str)
    {
        return System.BitConverter.ToString(s_md5Provider.ComputeHash(Encoding.UTF8.GetBytes(str))).Replace("-", "");
    }

    //----------------------------------------------
    /// 合并路径
    /// @path1
    /// @path2
    //----------------------------------------------
    public static string CombinePath(string path1, string path2)
    {
        if (path1.LastIndexOf('/') != path1.Length - 1)
        {
            path1 += "/";
        }

        if (path2.IndexOf('/') == 0)
        {
            path2 = path2.Substring(1);
        }

        return FormatSlash(path1 + path2);
        //return System.IO.Path.Combine(path1, path2);
    }

    //----------------------------------------------
    /// 合并路径
    /// @values
    //----------------------------------------------
    public static string CombinePaths(params string[] values)
    {
		try
		{
			if (values == null || values.Length <= 0)
			{
				return string.Empty;
			}
			else if (values.Length == 1)
			{
				return CombinePath(values[0], string.Empty);
			}
			else if (values.Length > 1)
			{
				string path = CombinePath(values[0] == null?string.Empty : values[0], values[1] == null?string.Empty : values[1]);

				for (int i = 2; i < values.Length; i++)
				{
					path = CombinePath(path, values[i]);
				}

				return path;
			}
		}
        catch (Exception)
		{
			string exStr = "combine path exception : ";
			if (values != null)
			{
				for (int i = 0 ; i < values.Length; i++)
				{
					exStr += values[i] + " ";
				}
			}

			Debug.LogError(exStr);
		}
        
        return string.Empty;
    }

    /// <summary>
    /// 相对路径转绝对路径，主要是去掉路径中的.. 和 .等相对路径符号
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    public static string RelativeToAbsolutePath(string relativePath)
    {
        List<string> resultStack = new List<string>();
        relativePath = relativePath.Replace('\\', '/');
        string[] splites = relativePath.Split('/');
        foreach(string sec in splites)
        {
            if(sec == ".")
            {
                continue;
            }
            else if(sec == "..")
            {
                if(resultStack.Count > 0)
                {
                    resultStack.RemoveAt(resultStack.Count - 1);
                    continue;
                }
                else
                {
                    resultStack.Add(sec);
                    continue;
                }
            }
            else
            {
                resultStack.Add(sec);
            }
        }

        return string.Join("/", resultStack.ToArray());
    }

    //----------------------------------------------
    /// 返回StreamingAssets路径
    /// @返回值为带上file:///的可用于www方式加载的路径
    //----------------------------------------------
    public static string GetStreamingAssetsPathWithHeader(string fileName)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_IPHONE
            return GetLocalPathHeader() + System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
#elif UNITY_ANDROID
            return System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
#endif
    }

    //----------------------------------------------
    /// 返回Cache文件存储路径
    /// @返回值为标准路径
    //----------------------------------------------
    public static string GetCachePath()
    {
        //Android上temporaryCachePath中的数据在磁盘空间不足的情况下有可能会被删除，而persistentDataPath不会
        //IOS上persistentDataPath中的数据会被同步至iCloud，并且使用该目录可能导致审核被拒
        if (s_cachePath == null)
        {
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
            s_cachePath = string.Format("{0}/../Cache", Application.dataPath);
#elif UNITY_ANDROID
            s_cachePath = Application.persistentDataPath;
#elif UNITY_IPHONE
            s_cachePath = Application.temporaryCachePath;
            string bundleID = ocsys.GetBundleIDEx();
            if(string.IsNullOrEmpty(bundleID))
            {
#if USE_CE || IIPS_COMPETITION_OFFICIAL
                bundleID = "com.tx.smobace";
#else
                bundleID = "com.tencent.smoba";
#endif
            }

            ///Library/Application Support/com.tencent.mta
            string iosPath = "Application Support/" + bundleID;

            s_cachePath = s_cachePath.Replace("Caches", iosPath);

#else
            s_cachePath = Application.temporaryCachePath;
#endif
        }

        return s_cachePath;
    }

    //----------------------------------------------
    /// 返回Cache文件存储路径
    /// @返回值为标准路径
    //----------------------------------------------
    public static string GetCachePath(string fileName)
    {
        return CombinePath(GetCachePath(), fileName);
    }

    //----------------------------------------------
    /// 返回Cache文件存储路径
    /// @返回值为带上file:///的可用于www方式加载的路径
    //----------------------------------------------
    public static string GetCachePathWithHeader(string fileName)
    {
        return (GetLocalPathHeader() + GetCachePath(fileName));
    }

    //------------------------------------------------------------
    /// 返回IFS解压路径
    /// @extraIfsPack : > 0 表示需要返回ExtraIFS的解压目录下的路径
    //------------------------------------------------------------
    public static string GetIFSExtractPath(uint extraIfsPack = 0)
    {
        //Cache for Optimize
        if (s_ifsExtractPath[extraIfsPack] == null)
        {
            if (extraIfsPack == 0)
            {
                s_ifsExtractPath[extraIfsPack] = FormatSlash(CombinePath(GetCachePath(), s_ifsExtractFolder));
            }
            else
            {
                s_ifsExtractPath[extraIfsPack] = FormatSlash(CombinePath(GetExtResExtractDir(), (EXTRAIFS_NAME_PREFIX + extraIfsPack)));
            }
        }

        return s_ifsExtractPath[extraIfsPack];
    }

    //------------------------------------------------------------
    /// 返回ExtraIFS解压路径
    //------------------------------------------------------------
    public static string GetExtResExtractDir()
    {
        if (s_extraIfsExtractPath == null)
        {
            s_extraIfsExtractPath = FormatSlash(CombinePath(GetCachePath(), s_extraIfsExtractFolder));
        }

        return s_extraIfsExtractPath;
    }

    //----------------------------------------------
    /// 返回格式化'\'为'/'的StreamingAssets路径
    //----------------------------------------------
    public static string GetStreamingAssetsPath()
    {
        if (s_streamingAssetsPath == null)
        {
            s_streamingAssetsPath = FormatSlash(Application.streamingAssetsPath);
        }

        return s_streamingAssetsPath;
    }

    //----------------------------------------------
    /// 返回带扩展名的全名
    /// @fullPath : 带扩展名的完整路径
    //----------------------------------------------
    public static string GetFullName(string fullPath)
    {        
        if (fullPath == null)
        {
            return null;
        }

        int index = fullPath.LastIndexOf("/");

        if (index > 0)
        {
            return fullPath.Substring(index + 1, fullPath.Length - index - 1);
        }
        else
        {
            return fullPath;
        }
    }

#if UNITY_EDITOR
    //从全路径获取assets开始的目录
    public static string GetAssetPathFromFullPath(string fullPath)
    {
        string dataPath = Application.dataPath;
        string retval = fullPath.Replace('\\', '/');
        retval = "Assets" + retval.Replace(dataPath, "");
        return retval;
    }

    public static string GetFullPathFromAssetPath(string assetPath)
    {
        return Application.dataPath + "/../" + assetPath;
    }
#endif

    //----------------------------------------------
    /// 移除扩展名
    //----------------------------------------------
    public static string EraseExtension(string fullName)
    {
        if (fullName == null)
        {
            return null;
        }

        int index = fullName.LastIndexOf('.');

        if (index > 0)
        {
            return fullName.Substring(0, index);
        }
        else
        {
            return fullName;
        }
    }

    public static string GetFileNameWithoutExtension(string fullPath)
    {
        if(fullPath == null)
        {
            return null;
        }

        string fileName = GetFullName(fullPath);
        return EraseExtension(fileName);
    }

    //----------------------------------------------
    /// 返回扩展名
    /// @返回值包括"."
    //----------------------------------------------
    public static string GetExtension(string fullName)
    {
        int index = fullName.LastIndexOf('.');

        if (index > 0 && index + 1 < fullName.Length)
        {
            return fullName.Substring(index);
        }
        else
        {
            return string.Empty;
        }
    }

    //----------------------------------------------
    /// 返回完整目录
    /// @注意:"a/b/c"会返回"a/c"
    /// @"a/b/c/"才是我们想要的效果
    /// @fullPath
    //----------------------------------------------
    public static string GetFullDirectory(string fullPath)
    {
        return System.IO.Path.GetDirectoryName(fullPath).Replace('\\', '/');
    }

    //----------------------------------------------
    /// 清除目录下所有文件及文件夹，并保留目录
    /// @fullPath
    //----------------------------------------------
    public static bool ClearDirectory(string fullPath)
    {
        try
        {
            //删除文件
            string[] files = System.IO.Directory.GetFiles(fullPath);
            for (int i = 0; i < files.Length; i++)
            {
                System.IO.File.Delete(files[i]);
            }

            //删除文件夹
            string[] dirs = System.IO.Directory.GetDirectories(fullPath);
            for (int i = 0; i < dirs.Length; i++)
            {
                System.IO.Directory.Delete(dirs[i], true);
            }

            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    //----------------------------------------------
    /// 清除目录下指定文件及文件夹，并保留目录
    /// @fullPath
    //----------------------------------------------
    public static bool ClearDirectory(string fullPath, string[] fileExtensionFilter, string[] folderFilter)
    {
        try
        {
            //删除文件
            if (fileExtensionFilter != null)
            {               
                string[] files = System.IO.Directory.GetFiles(fullPath);
                for (int i = 0; i < files.Length; i++)
                {
                    if (fileExtensionFilter != null && fileExtensionFilter.Length > 0)
                    {
                        for (int j = 0; j < fileExtensionFilter.Length; j++)
                        {
                            if (files[i].Contains(fileExtensionFilter[j]))
                            {
                                DeleteFile(files[i]);
                                break;
                            }
                        }
                    }
                }
            }

            //删除文件夹
            if (folderFilter != null)
            {
                string[] dirs = System.IO.Directory.GetDirectories(fullPath);
                for (int i = 0; i < dirs.Length; i++)
                {
                    if (folderFilter != null && folderFilter.Length > 0)
                    {
                        for (int j = 0; j < folderFilter.Length; j++)
                        {
                            if (dirs[i].Contains(folderFilter[j]))
                            {                                
                                DeleteDirectory(dirs[i]);
                                break;
                            }
                        }
                    }
                }
            }

            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    //--------------------------------------------------------------
    /// 从fileFullPath解析出在Resources下面的路径
    /// @fileFullPath
    //--------------------------------------------------------------
    public static string GetFullPathInResources(string fileFullPath)
    {
        fileFullPath = fileFullPath.Replace(@"\", @"/");

        string key = string.Format("Assets/{0}/", s_customResourcesFolder);

        int index = fileFullPath.IndexOf(key);
        if (index >= 0)
        {
            return fileFullPath.Substring(index + key.Length);
        }

        return string.Empty;
    }

    //--------------------------------------------------------------
    /// 将反斜杠修改为正斜杠
    /// @filePath
    //--------------------------------------------------------------
    public static string FormatSlash(string filePath)
    {
        return filePath.Replace(@"\", @"/");
    }

    //--------------------------------------------------------------
    /// 从fileFullPath解析出在Assets下面的路径
    /// @fileFullPath
    //--------------------------------------------------------------
    public static string GetFullPathInProject(string fileFullPath)
    {
        fileFullPath = fileFullPath.Replace(@"\", @"/");

        string key = "Project/";

        int index = fileFullPath.IndexOf(key);
        if (index >= 0)
        {
            return fileFullPath.Substring(index + key.Length);
        }

        return string.Empty;
    }

    //--------------------------------------------------------------
    /// 从fullPathInResources解析出在Project下面的路径
    /// @fullPathInResources
    //--------------------------------------------------------------
    public static string GetFullPathInProjectByFullPathInResources(string fullPathInResources)
    {
        return CombinePaths(c_assetsFolder, s_customResourcesFolder, fullPathInResources);
    }

#if UNITY_EDITOR
    //--------------------------------------------------------------
    /// 根据Resources下的全路径返回文件真正的全路径
    /// @fullPathInResources
    //--------------------------------------------------------------
    public static string GetFullPathByPathInResources(string fullPathInResources)
    {
        return CFileManager.CombinePaths(Application.dataPath, s_customResourcesFolder, fullPathInResources);
    }

    //--------------------------------------------------------------
    /// 加密文件
    /// @srcFileFullPath
    /// @dstFileFullPath
    /// @encryption
    /// @buffer
    /// @bufferLength
    //--------------------------------------------------------------
    public static void EncryptFile(string srcFileFullPath, string dstFileFullPath, string publicKey, string privateKey, byte[] buffer, uint bufferLength)
    {
        if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(privateKey))
        {
            CopyFile(srcFileFullPath, dstFileFullPath);
            return;
        }
        
        uint dataLength = ReadFile(srcFileFullPath, buffer, bufferLength);

        int publicKeyOffset = ((int)dataLength & 0xFF);
        int privateKeyOffset = (((int)dataLength >> 1) & 0xFF);

        byte[] publicKeyData = System.Text.Encoding.UTF8.GetBytes(publicKey);
        int publicKeyLength = publicKeyData.Length;

        byte[] privateKeyData = System.Text.Encoding.UTF8.GetBytes(privateKey);
        int privateKeyLength = privateKeyData.Length;

        for (int i = 0; i < dataLength; i++)
        {
            buffer[i] ^= publicKeyData[(i + publicKeyOffset) % publicKeyLength];
            buffer[i] ^= privateKeyData[(i + privateKeyOffset) % privateKeyLength];
        }

        CFileManager.WriteFile(dstFileFullPath, buffer, 0, (int)dataLength);
    }
#endif

    //----------------------------------------------
    /// 清空缓存
    //----------------------------------------------
    //public static bool ClearCachePath()
    //{
    //    return ClearDirectory(GetCachePath());
    //}

    //--------------------------------------------------
    /// 获取本地路径前缀(file:///)
    //--------------------------------------------------
    public static string GetLocalPathHeader()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return "file:///";
#elif UNITY_ANDROID
        return "file://";
#elif UNITY_IPHONE
        return "file:///";
#else
        return "file:///";
#endif
    }

    //--------------------------------------------------
    /// 获取字节数显示，小数点后两位
    /// @byteSize 传入大小
    //--------------------------------------------------
    public static string GetBytesSizeDispStr(long byteSize)
    {
        int kSizeHolder = 1024 * 1024;
        if (byteSize < 1024)
        {
            if (byteSize < 11)
            {//avoid return 0.00KB
                return "0.01KB";
            }
            return (byteSize / (double)1024).ToString("0.00") + "KB";
        }   
        else
            return (byteSize / (double)kSizeHolder).ToString("0.00") + "MB";
    }

    //--------------------------------------------------------------------------
    /// 锁定文件数据Buffer
    /// @请配合Try-Finally使用
    //--------------------------------------------------------------------------
    public static byte[] LockFileBuffer()
    {
        ////加入上报<neoyang>
        //if (s_isFileBufferLocked)
        //{
        //    BuglyAgent.ReportException(new Exception("FileBufferLocked"), "FileBuffer has been locked!!!");
        //}

        //DebugHelper.Assert(!s_isFileBufferLocked, "Buffer has been locked!!!");

        s_isFileBufferLocked = true;
        return s_fileBuffer;
    }

    //--------------------------------------------------------------------------
    /// 解锁文件数据Buffer
    /// @需要在Finally块中调用
    //--------------------------------------------------------------------------
    public static void UnLockFileBuffer()
    {
        s_isFileBufferLocked = false;
    }

    public static string m_RobotVideoResFolderName = "RobotVideo";
    public static string GetRobotVideoResourcePath()
    {
#if UNITY_EDITOR
        return System.IO.Path.Combine(Application.dataPath, string.Format("{0}/{1}/", s_customResourcesFolder, CFileManager.m_RobotVideoResFolderName));
#else
        string ResPath = GetIFSExtractPath();
        return System.IO.Path.Combine(ResPath, string.Format("{0}/", CFileManager.m_RobotVideoResFolderName));
#endif
    }

    //----------------------------------------------
    /// 构建IFSMountPoint
    /// @用于区别哪种路径需要从IFS中读取
    //----------------------------------------------
    public static string MakeMountPoint(string ifsFileName)
    {
        if (s_ifsMountPath == null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            s_ifsMountPath = CFileManager.CombinePath(Application.dataPath, string.Format("/assets/{0}", ifsFileName));
#else
            s_ifsMountPath = CFileManager.CombinePath(Application.streamingAssetsPath, string.Format("/{0}", ifsFileName));
#endif
        }

        return s_ifsMountPath;
    }

    //----------------------------------------------
    /// 构建IFSConfig
    /// @用于Apollo创建IFSReader
    //----------------------------------------------
    public static string MakeIfsConfig(string ifsFileName, uint ifsFileLength)
    {
        /*
                //TODO_neoyang(下面那个链接是用来干蛋的?)
                var dwnIfsConfigBuilder = new StringBuilder();
                dwnIfsConfigBuilder.Append("{\"ifs\":{")
                    .Append("\"password\":{")
                    .Append("\"haspassword\":").Append("false")
                    .Append(",\"value\":\"").Append("")
                    .Append("\"},")
                    .Append("\"filelist\":[{")
                    .Append("\"url\":").Append("\"http://defulturl\",")
                    .Append("\"filename\":").Append(string.Format("\"{0}\",", ifsFileName))
        #if UNITY_ANDROID && !UNITY_EDITOR
                    .Append("\"filepath\":\"").Append("apk://" + Application.dataPath + string.Format("?assets/{0}", ifsFileName))
        #else
                    .Append("\"filepath\":\"").Append(Application.streamingAssetsPath + string.Format("/{0}", ifsFileName))
        #endif
                    .Append("\",\"filesize\":").Append(string.Format("{0},", ifsFileLength))
                    .Append("\"readonly\":").Append("true")
                    .Append("}],")
                    .Append("\"hasifs\":").Append("true")
                    .Append("},");
                dwnIfsConfigBuilder.Append("\"download\":{")
                    .Append("\"max_download_speed\":").Append(1024)
                    .Append(",\"max_predownload_speed\":").Append(1024)
                    .Append(",\"max_downloads_per_task\":").Append(3)
                    .Append(",\"max_running_task\":").Append(3)
                    .Append(",\"max_running_task_in_predownload\":").Append(1)
                    .Append(",\"download_play_race_control_lowerpriority\":").Append(0)
                    .Append(",\"download_play_samepriority_backtofront\":").Append(0)
                    .Append(",\"download_play_samepriority_backtofront_racetocontrol\":").Append(0)
                    .Append(",\"download_only_down_highpriority\":").Append(0)
                    .Append(",\"enable_predownload\":").Append(1)
                    .Append(",\"max_timeout_deaderror\":").Append(30000)
                    .Append("}}");

                return dwnIfsConfigBuilder.ToString();
        */

        string config = string.Format(@"
        {{
            ""ifs"" : 
            {{
                ""filelist"" : 
                [
                   {{
                        ""filemetaurl"" : """",
                        ""filename"" : ""{0}"",
                        ""filepath"" : ""{1}"",
                        ""filesize"" : {2},
                        ""readonly"" : true,
                        ""resfilename"" : """",
                        ""url"" : ""{1}""
                    }}
                 ],
                  ""hasifs"" : true,
                  ""password"" : 
	              {{
                     ""haspassword"" : false,
                     ""value"" : ""1231231""
                  }}
             }},
           ""log_debug"" : false
         }}",
         ifsFileName,
#if UNITY_ANDROID && !UNITY_EDITOR
         ("apk://" + Application.dataPath + string.Format("?assets/{0}", ifsFileName)),
#else
         (Application.streamingAssetsPath + string.Format("/{0}", ifsFileName)),
#endif
         ifsFileLength
         );

        //Debug.Log("=======================================================");
        //Debug.Log("IFS CONFIG = " + config);
        //Debug.Log("=======================================================");

        return config;
    }
};

