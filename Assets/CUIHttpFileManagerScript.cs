//==================================================================================
/// CUIHttpFileManagerScript 控件
/// @根据传入的url下载、缓存url对应的文件
/// @joselv
/// @2018.08.11
//==================================================================================
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace Assets.Scripts.UI
{
    ////state
    //public enum enHttpFileDownloadState
    //{
    //    Unload,
    //    Waiting,
    //    Loading,
    //    Loaded,
    //    Error
    //};

    //public delegate void HttpFileDownloadErrorDelegate(string msg);
    //public delegate void HttpFileDownloadSuccesDelegate(string path);

    //public class CUIHttpFileManagerScript : CUIComponent
    //{
    //    //cdn文件地址
    //    public string m_fileUrl;

    //    //缓存类型
    //    public enHttpFileCacheType m_cacheType = enHttpFileCacheType.Video;

    //    //缓存文件有效期(单位:天数)
    //    public float m_cachedFileValidDays = 2f;

    //    ////缓存的文件后缀
    //    public string m_cacheFileExtension = ".mp4";

    //    //Loading时显示的GameObject，下载成功前都会enable该对象
    //    public GameObject m_loadingCover;

    //    private HttpFileDownloadErrorDelegate m_errorDelegate = null;

    //    private HttpFileDownloadSuccesDelegate m_loadedDelegate = null;

    //    //加载状态
    //    private enHttpFileDownloadState m_downloadState = enHttpFileDownloadState.Unload;

    //    //缓存相关
    //    private static CHttpFileCacher m_fileCacher;

    //    //唯一ID
    //    private int m_instanceID;

    //    //异步加载维护(避免开太多协程)
    //    private static List<int> s_downloadingInstanceList = new List<int>();
    //    private const int c_downloadingInstanceMaxCount = 5;

    //    //超时阀值及开始下载的时间戳
    //    private const float c_downloadingOverTime = 30f;
    //    private float m_downloadingStartTimestamp = 0f;

    //    //重试次数
    //    private const int c_downloadingRetryMaxTimes = 3;
    //    private int m_downloadingRetryTimes = 0;

    //    //--------------------------------------------------
    //    /// 初始化
    //    //--------------------------------------------------
    //    public override void Initialize(CUIFormScript formScript)
    //    {
    //        if (!m_isInitialized)
    //        {
    //            m_instanceID = this.GetInstanceID();

    //            m_customUpdateFlags |= (int)enCustomUpdateFlag.Update;

    //            base.Initialize(formScript);

    //            //创建缓存文件管理器
    //            if (m_fileCacher == null)
    //            {
    //                m_fileCacher = new CHttpFileCacher(30, "Video", m_cacheFileExtension);
    //            }

    //            //初始状态默认为Unload
    //            EnterUnloadState();

    //            m_downloadingStartTimestamp = 0;
    //            m_downloadingRetryTimes = 0;

    //            m_isInitialized = true;
    //        }
    //    }

    //    //--------------------------------------------------
    //    /// 清理
    //    //--------------------------------------------------
    //    public override void UnInitialize()
    //    {
    //        if (m_isInitialized)
    //        {
    //            //如果正处于Loading状态，停止下载协程
    //            if (m_downloadState == enHttpFileDownloadState.Loading)
    //            {
    //                StopDownloading();
    //            }

    //            //状态设置为Unload
    //            EnterUnloadState();

    //            m_errorDelegate = null;
    //            m_loadedDelegate = null;

    //            m_fileUrl = null;
    //            m_loadingCover = null;

    //            base.UnInitialize();

    //            m_isInitialized = false;
    //        }
    //    }

    //    //--------------------------------------------------
    //    /// 销毁
    //    //--------------------------------------------------
    //    protected override void OnDestroy()
    //    {
    //        UnInitialize();

    //        base.OnDestroy();
    //    }

    //    void OnDisable()
    //    {
    //        //这里需要将正在下载的协程停掉
    //        if (m_isInitialized && m_downloadState == enHttpFileDownloadState.Loading)
    //        {
    //            StopDownloading();
    //            EnterWaitingState();
    //        }
    //    }

    //    public override void Hide()
    //    {
    //        base.Hide();

    //        //这里需要将正在下载的协程停掉
    //        if (m_isInitialized && m_downloadState == enHttpFileDownloadState.Loading)
    //        {
    //            StopDownloading();
    //            EnterWaitingState();
    //        }
    //    }

    //    //--------------------------------------------------
    //    /// 自定义Update，在此驱动状态
    //    //--------------------------------------------------
    //    public override void CustomUpdate()
    //    {
    //        if (m_isInitialized)
    //        {
    //            if (string.IsNullOrEmpty(m_fileUrl))
    //            {
    //                return;
    //            }

    //            switch (m_downloadState)
    //            {
    //                case enHttpFileDownloadState.Unload:
    //                    {
    //                        string path = TryGetLoadedFilePath(m_fileUrl);
    //                        if (!string.IsNullOrEmpty(path))//已有缓存文件
    //                        {
    //                            EnterLoadedState(path);
    //                        }
    //                        else
    //                        {
    //                            EnterWaitingState();
    //                        }
    //                    }
    //                    break;

    //                case enHttpFileDownloadState.Waiting:
    //                    {
    //                        if (s_downloadingInstanceList.Count < c_downloadingInstanceMaxCount)
    //                        {
    //                            EnterDownloadingState();
    //                        }
    //                    }
    //                    break;

    //                case enHttpFileDownloadState.Loading:
    //                    {
    //                        //超时判断
    //                        if (Time.realtimeSinceStartup - m_downloadingStartTimestamp >= c_downloadingOverTime)
    //                        {
    //                            StopDownloading();

    //                            //如果超过最大重试次数，状态置为error
    //                            if (m_downloadingRetryTimes++ >= c_downloadingRetryMaxTimes)
    //                            {
    //                                EnterErrorState("time out");
    //                            }else
    //                            {
    //                                EnterWaitingState();//等待重试
    //                            }
    //                        }
    //                    }
    //                    break;
    //                default:
    //                    break;
    //            }
    //        }
    //    }

    //    private void EnterLoadedState(string path)
    //    {
    //        m_downloadState = enHttpFileDownloadState.Loaded;
    //        m_loadingCover.CustomSetActive(false);
    //        //派发事件
    //        if (m_loadedDelegate != null)
    //        {
    //            m_loadedDelegate(path);
    //        }
    //    }

    //    private void EnterWaitingState()
    //    {
    //        m_downloadState = enHttpFileDownloadState.Waiting;
    //        m_loadingCover.CustomSetActive(true);
    //    }

    //    private void EnterDownloadingState()
    //    {
    //        m_downloadState = enHttpFileDownloadState.Loading;
    //        m_downloadingStartTimestamp = 0;
    //        m_loadingCover.CustomSetActive(true);
    //        StartCoroutine(DownloadFileEnumerator());
    //    }

    //    private void EnterUnloadState()
    //    {
    //        m_downloadState = enHttpFileDownloadState.Unload;
    //        m_loadingCover.CustomSetActive(true);
    //    }

    //    private void EnterErrorState(string errMsg)
    //    {
    //        ocsys.NSLog(string.Format("视频错误, error={0}", errMsg));
    //        m_downloadState = enHttpFileDownloadState.Error;
    //        m_loadingCover.CustomSetActive(true);
    //        if (m_errorDelegate != null)
    //        {
    //            m_errorDelegate(errMsg);
    //        }
    //    }

    //    //--------------------------------------------------
    //    /// Set Image 链接
    //    /// @url
    //    /// @forceSetImageUrl  是否强制设置图片地址（若为false，SetImageUrl时若当前url与上次url一致则不会重置url）
    //    /// @keepLastSprite  是否保持上次的图片sprite （若为false， 会刷回默认sprite）
    //    //--------------------------------------------------
    //    public void SetFileUrl(string url, HttpFileDownloadErrorDelegate errorDelegate, HttpFileDownloadSuccesDelegate loadedDelegate, bool forceSetFileUrl = false)
    //    {
    //        m_errorDelegate = errorDelegate;
    //        m_loadedDelegate = loadedDelegate;

    //        if (!forceSetFileUrl && string.Equals(url, m_fileUrl))
    //        {
    //            EnterErrorState("repeat video url");
    //            return;
    //        }

    //        if (string.IsNullOrEmpty(url))
    //        {
    //            EnterErrorState("para cdn url is empty");
    //            return;
    //        }

    //        m_fileUrl = url;

    //        ocsys.NSLog(string.Format("set file url={0}", url));
    //        //这里需要停掉正在Loading的协程
    //        if (m_downloadState == enHttpFileDownloadState.Loading)
    //        {
    //            StopDownloading();
    //        }

    //        //强制将状态置回Unload
    //        EnterUnloadState();

    //        //重置
    //        m_downloadingStartTimestamp = 0;
    //        m_downloadingRetryTimes = 0;
    //    }

    //    //
    //    public string TryGetLoadedFilePath(string cdnUrl)
    //    {
    //        if (string.IsNullOrEmpty(cdnUrl))
    //        {
    //            return string.Empty;
    //        }

    //        return m_fileCacher.GetCachedFilePath(cdnUrl, m_cachedFileValidDays);
    //    }

    //    //--------------------------------------------------
    //    /// 在最后加个随机数， 避免拉不到的情况
    //    //--------------------------------------------------
    //    private void AddRandomParam(ref string url)
    //    {
    //        url = string.Format("{0}?{1}", url, UnityEngine.Random.Range(Int32.MinValue, Int32.MaxValue));
    //    }

    //    private void ResolveLoadedFile(WWW www)
    //    {
    //        m_fileCacher.AddFile(m_fileUrl, www.bytes);
    //    }

    //    //--------------------------------------------------
    //    /// 下载文件
    //    //--------------------------------------------------

    //    private IEnumerator DownloadFileEnumerator()
    //    {
    //        //将唯一ID添加到下载列表
    //        if (!s_downloadingInstanceList.Contains(m_instanceID))
    //        {
    //            s_downloadingInstanceList.Add(m_instanceID);
    //        }

    //        //记录开始下载的时间戳
    //        m_downloadingStartTimestamp = Time.realtimeSinceStartup;

    //        string wwwUrl = m_fileUrl;

    //        //IOS和Android都用https协议
    //        if (wwwUrl != null)
    //        {
    //            ApolloHelper.Url2Https(ref wwwUrl, true);
    //        }

    //        AddRandomParam(ref wwwUrl);
    //        WWW www = new WWW(wwwUrl);
    //        yield return www;

    //        //结束下载
    //        RemoveFromLoadingList();

    //        if (string.IsNullOrEmpty(www.error))
    //        {
    //            ResolveLoadedFile(www);
    //            string path = TryGetLoadedFilePath(m_fileUrl);
    //            if(string.IsNullOrEmpty(path) == true)
    //            {
    //                EnterErrorState("www.error is null, but file path is empty");
    //            }else
    //            {
    //                EnterLoadedState(path);
    //            }
    //        }
    //        else
    //        {
    //            if (m_downloadingRetryTimes++ < c_downloadingRetryMaxTimes)
    //            {
    //                ocsys.NSLog(string.Format("下载失败,开始重试第{0}次,url={1}, error={2}" , m_downloadingRetryTimes, wwwUrl, www.error));
    //                //尝试重试
    //                EnterUnloadState();
    //            }
    //            else
    //            {
    //                //将状态设置为Error
    //                EnterErrorState(www.error);
    //            }
    //        }

    //        //这里需要释放www的数据
    //        www.Dispose();
    //    }

    //    //--------------------------------------------------
    //    /// 结束下载，从列表中移除
    //    //--------------------------------------------------
    //    private void RemoveFromLoadingList()
    //    {
    //        s_downloadingInstanceList.Remove(m_instanceID);
    //    }

    //    //--------------------------------------------------
    //    /// 停止下载
    //    /// @resetLoadState : 是否重置状态为Unload
    //    //--------------------------------------------------
    //    private void StopDownloading()
    //    {
    //        StopAllCoroutines();
    //        RemoveFromLoadingList();
    //    }
    //};

    public enum enHttpFileCacheType
    {
        Default = 0,
        Hero2dImage = 1,//英雄的2d图单独列一个出来，以保证这种类型有足够的缓存空间
        Video = 2,
        Count,
    };

    //public class CHttpFileCacherManager
    //{
    //    static DictionaryView<int, CHttpFileCacher> cacherMap = new DictionaryView<int, CHttpFileCacher>();
    //    static int[] maxArray = new int[(int)enHttpFileCacheType.Count] { 100, 100, 30 };
    //    static string[] fileExtArray = new string[(int)enHttpFileCacheType.Count] { ".bytes", ".bytes", ".mp4" };

    //    public static CHttpFileCacher GetCacher(enHttpFileCacheType cacheType)
    //    {
    //        int idx = (int)cacheType;
    //        if (cacherMap.ContainsKey(idx) == false)
    //        {
    //            cacherMap.Add(idx, new CHttpFileCacher(maxArray[idx], cacheType.ToString(), fileExtArray[idx], cacheType));
    //        }
    //        return cacherMap[idx];
    //    }
    //}

    //--------------------------------------------------
    /// http文件缓存器
    //--------------------------------------------------
    public class CHttpFileCacher
    {
        //缓存文件个数上限
        private int m_maxCnt;

        //缓存文件存放目录
        private string m_dir;

        //缓存的信息文件路径
        private string m_metaFilePath;

        private CCachedFileInfoSet m_cachedFileInfoSet;

        private string m_fileExtension;

        private static string s_httpFileRootDir = CFileManager.CombinePath(CFileManager.GetCachePath(), "http");
        //--------------------------------------------------
        /// 构造函数
        //--------------------------------------------------
        public CHttpFileCacher(int max, string folderName, string fileExtension)
        {
            m_maxCnt = max;
            m_dir = GetCacheDirectory(folderName.ToLower());
            m_metaFilePath = CFileManager.CombinePath(m_dir, folderName.ToLower() + ".bytes");
            m_fileExtension = fileExtension;

            m_cachedFileInfoSet = new CCachedFileInfoSet();

            MakeDirReady();

            if (CFileManager.IsFileExist(m_metaFilePath))
            {
                byte[] buffer = CFileManager.LockFileBuffer();

                try
                {
                    uint fileLength = CFileManager.ReadFile(m_metaFilePath, buffer, (uint)buffer.Length);
                    int offset = 0;

                    int ret = m_cachedFileInfoSet.Read(buffer, offset, fileLength);

                    if (ret < 0)//读取出错时，删除所有文件
                    {
                        Debug.LogError("读取出错，删除所有文件");
                        CFileManager.ClearDirectory(m_dir);
                    }
                }
                finally
                {
                    CFileManager.UnLockFileBuffer();
                }                
            }
        }

        public static string GetCacheDirectory(string folderName)
        {
            return CFileManager.CombinePath(s_httpFileRootDir, folderName.ToLower());
        }

        //--------------------------------------------------
        /// 获取缓存的资源文件路径
        /// @url        : 地址
        /// @validDays  : 有效天数
        //--------------------------------------------------
        public string GetCachedFilePath(string url, float validDays)
        {
            string key = CFileManager.GetMd5(url.ToLower());

            CCachedFileInfo cachedFileInfo = m_cachedFileInfoSet.GetFileInfo(key);
            //不存在
            if (cachedFileInfo == null)
            {
                return string.Empty;
            }

            //检查是否过期
            if ((DateTime.Now - cachedFileInfo.m_lastModifyTime).TotalDays >= validDays)
            {
                RemoveFile(key);
                return string.Empty;
            }

            string cachedFileFullPath = CFileManager.CombinePath(m_dir, key + m_fileExtension);

            //检查文件是否存在
            if (CFileManager.IsFileExist(cachedFileFullPath))
            {
                //通过校验文件长度，来判断是否被串改
                if (cachedFileInfo.m_fileLength == (int)CFileManager.GetFileLength(cachedFileFullPath))
                {
                    return cachedFileFullPath;
                }
                else
                {
                    RemoveFile(key);
                }
            }

            return string.Empty;
        }

        public CCachedFileInfo GetCacheFileInfo(string url)
        {
            string key = CFileManager.GetMd5(url.ToLower());
            return m_cachedFileInfoSet.GetFileInfo(key);
        }

        public DateTime GetFileLastModifyTime(string url)
        {
            string key = CFileManager.GetMd5(url.ToLower());
            CCachedFileInfo cacheFileInfo = m_cachedFileInfoSet.GetFileInfo(key);
            if (cacheFileInfo != null)
            {
                return cacheFileInfo.m_lastModifyTime;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public void SetFileLastModifyTime(string url, ref DateTime dt)
        {
            string key = CFileManager.GetMd5(url.ToLower());
            CCachedFileInfo cacheFileInfo = m_cachedFileInfoSet.GetFileInfo(key);
            if (cacheFileInfo != null)
            {
                cacheFileInfo.m_lastModifyTime = dt;
            }
        }

        public void RemoveFile(string key)
        {
            string cachedFileFullPath = CFileManager.CombinePath(m_dir, key + m_fileExtension);
            if (CFileManager.IsFileExist(cachedFileFullPath))
            {
                CFileManager.DeleteFile(cachedFileFullPath);
            }

            m_cachedFileInfoSet.RemoveFileInfo(key);
        }

        //--------------------------------------------------
        /// 添加缓存文件
        /// @url
        /// @cacheType
        /// @width
        /// @height
        /// @isGif
        /// @data
        //--------------------------------------------------
        public void AddFile(string url, byte[] data, int tagInt1 = 0, int tagInt2 = 0, bool tagBool = false)
        {
            string key = CFileManager.GetMd5(url.ToLower());
            List<CCachedFileInfo> cachedFileInfoList = m_cachedFileInfoSet.m_cachedFileInfos;
            if (cachedFileInfoList == null)
            {
                return;
            }

            if (m_cachedFileInfoSet.m_cachedFileInfoMap.ContainsKey(key))
            {
                CCachedFileInfo cachedFileInfo = null;
                m_cachedFileInfoSet.m_cachedFileInfoMap.TryGetValue(key, out cachedFileInfo);

                Debug.Assert(cachedFileInfoList != null && cachedFileInfoList.Contains(cachedFileInfo), "zen me ke neng?");

                //修改信息
                cachedFileInfo.m_fileLength = data.Length;
                cachedFileInfo.m_lastModifyTime = DateTime.Now;
                cachedFileInfo.m_tagInt1 = tagInt1;
                cachedFileInfo.m_tagInt2 = tagInt2;
                cachedFileInfo.m_tagBool = tagBool;
            }
            else
            {
                //如果数量达到上限，移除排在最前面的文件
                if (cachedFileInfoList.Count >= m_maxCnt)
                {
                    string removeKey = m_cachedFileInfoSet.RemoveEarliestFileInfo();

                    //删除缓存文件
                    if (!string.IsNullOrEmpty(removeKey))
                    {
                        string removeCachedFileFullPath = CFileManager.CombinePath(m_dir, removeKey + m_fileExtension);
                        if (CFileManager.IsFileExist(removeCachedFileFullPath))
                        {
                            CFileManager.DeleteFile(removeCachedFileFullPath);
                        }
                    }
                }

                CCachedFileInfo cachedFileInfo = new CCachedFileInfo();
                cachedFileInfo.m_key = key;
                cachedFileInfo.m_fileLength = data.Length;
                cachedFileInfo.m_lastModifyTime = DateTime.Now;
                cachedFileInfo.m_tagInt1 = tagInt1;
                cachedFileInfo.m_tagInt2 = tagInt2;
                cachedFileInfo.m_tagBool = tagBool;

                m_cachedFileInfoSet.AddFileInfo(key, cachedFileInfo);
            }

            //排序
            m_cachedFileInfoSet.m_cachedFileInfos.Sort();

            //写入信息文件
            byte[] buffer = CFileManager.LockFileBuffer();
            try
            {
                MakeDirReady();
                int offset = 0;
                m_cachedFileInfoSet.Write(buffer, ref offset);

                if (CFileManager.IsFileExist(m_metaFilePath))
                {
                    CFileManager.DeleteFile(m_metaFilePath);
                }

                CFileManager.WriteFile(m_metaFilePath, buffer, 0, offset);
            }
            finally
            {
                CFileManager.UnLockFileBuffer();
            }            

            //写入数据文件
            string cachedFileFullPath = CFileManager.CombinePath(m_dir, key + m_fileExtension);
            if (CFileManager.IsFileExist(cachedFileFullPath))
            {
                CFileManager.DeleteFile(cachedFileFullPath);
            }

            CFileManager.WriteFile(cachedFileFullPath, data);
        }

        private void MakeDirReady()
        {
            if (CFileManager.IsDirectoryExist(s_httpFileRootDir) == false)
            {
                CFileManager.CreateDirectory(s_httpFileRootDir);
            }

            if (!CFileManager.IsDirectoryExist(m_dir))
            {
                CFileManager.CreateDirectory(m_dir);
            }
        }

    }

    //--------------------------------------------------
    /// 缓存纹理信息集
    /// @二进制数据结构
    /// @Length     (4 byte)
    /// @Version    (2 byte)
    /// @Amount     (2 byte)
    /// @{
    /// @   info.key       (string)
    /// @   info.width     (2 byte)
    /// @   info.height    (2 byte)
    /// @   info.dateTime  (byte[])
    /// @   info.isGif      (1 byte)
    /// @}
    //--------------------------------------------------
    public class CCachedFileInfoSet
    {
        //版本号，当CCachedFileInfo数据结构发生变化时候一定要修改版本号，版本号不兼容的话不能从已存储的二进制文件读取数据
        public const int c_version = 10007;

        public List<CCachedFileInfo> m_cachedFileInfos;
        public Dictionary<string, CCachedFileInfo> m_cachedFileInfoMap;

        public CCachedFileInfoSet()
        {
            m_cachedFileInfos = new List<CCachedFileInfo>();
            m_cachedFileInfoMap = new Dictionary<string, CCachedFileInfo>();
        }

        //--------------------------------------------------
        /// 写入数据
        /// @buffer
        /// @offset
        //--------------------------------------------------
        public void Write(byte[] buffer, ref int offset)
        {
            int startOffset = offset;

            //跳过文件长度
            offset += 4;

            //写入版本号
            CMemoryManager.WriteShort((short)c_version, buffer, ref offset);

            //写入info数量
            CMemoryManager.WriteShort((short)m_cachedFileInfos.Count, buffer, ref offset);

            //写入数据
            for (int i = 0; i < m_cachedFileInfos.Count; i++)
            {
                m_cachedFileInfos[i].Write(buffer, ref offset);
            }

            //写入文件长度
            CMemoryManager.WriteInt(offset - startOffset, buffer, ref startOffset);
        }

        //--------------------------------------------------
        /// 读出数据
        /// @buffer
        /// @offset
        /// @dataLength
        //--------------------------------------------------
        public int Read(byte[] buffer, int offset, uint dataLength)
        {
            m_cachedFileInfos.Clear();
            m_cachedFileInfoMap.Clear();

            //数据长度不合法，不能读取数据
            if (dataLength < 6 || offset + (int)dataLength > buffer.Length)
            {
                return -1;
            }

            //校验数据长度
            int storedDataLength = CMemoryManager.ReadInt(buffer, ref offset);
            if (storedDataLength < 6 || storedDataLength != (int)dataLength)
            {
                return -2;
            }

            //校验版本号
            int version = CMemoryManager.ReadShort(buffer, ref offset);
            if (version != c_version)
            {
                return -3;
            }

            //读取数据
            int infoAmount = CMemoryManager.ReadShort(buffer, ref offset);
            for (int i = 0; i < infoAmount; i++)
            {
                CCachedFileInfo cacheFileInfo = new CCachedFileInfo();
                cacheFileInfo.Read(buffer, ref offset);

                //防止key重复
                if (m_cachedFileInfoMap.ContainsKey(cacheFileInfo.m_key) == false)
                {
                    m_cachedFileInfoMap.Add(cacheFileInfo.m_key, cacheFileInfo);
                    m_cachedFileInfos.Add(cacheFileInfo);
                }
            }
            //按最后修改时间排序
            m_cachedFileInfos.Sort();
            return 0;
        }

        //--------------------------------------------------
        /// 返回缓存文件信息
        /// @key
        //--------------------------------------------------
        public CCachedFileInfo GetFileInfo(string key)
        {
            CCachedFileInfo cachedFileInfo = null;
            if (m_cachedFileInfoMap.TryGetValue(key, out cachedFileInfo))
            {
                return cachedFileInfo;
            }
            else
            {
                return null;
            }
        }

        //--------------------------------------------------
        /// 添加数据
        /// @url
        /// @data
        //--------------------------------------------------
        public void AddFileInfo(string key, CCachedFileInfo cachedTextureInfo)
        {
            if (m_cachedFileInfoMap.ContainsKey(key))
            {
                return;
            }

            if (m_cachedFileInfos != null)
            {
                m_cachedFileInfoMap.Add(key, cachedTextureInfo);
                m_cachedFileInfos.Add(cachedTextureInfo);
            }
        }

        //--------------------------------------------------
        /// 移除最早的数据
        /// @return key
        //--------------------------------------------------
        public string RemoveEarliestFileInfo()
        {
            if (m_cachedFileInfos == null || m_cachedFileInfos.Count <= 0)
            {
                return null;
            }

            CCachedFileInfo removeCachedFileInfo = m_cachedFileInfos[0];

            m_cachedFileInfos.RemoveAt(0);
            m_cachedFileInfoMap.Remove(removeCachedFileInfo.m_key);

            return removeCachedFileInfo.m_key;
        }

        public void RemoveFileInfo(string key)
        {
            if (m_cachedFileInfos == null || m_cachedFileInfos.Count <= 0)
            {
                return;
            }

            for (int i = m_cachedFileInfos.Count - 1; i >= 0; i--)
            {
                if (m_cachedFileInfos[i].m_key == key)
                {
                    m_cachedFileInfos.RemoveAt(i);
                }
            }

            if (m_cachedFileInfoMap.ContainsKey(key) == true)
            {
                m_cachedFileInfoMap.Remove(key);
            }
        }
    };

    //--------------------------------------------------
    /// 缓存纹理信息
    //--------------------------------------------------
    public class CCachedFileInfo : IComparable
    {
        //通用项
        public string m_key;
        public int m_fileLength;
        public DateTime m_lastModifyTime;

        //扩展项
        public int m_tagInt1;
        public int m_tagInt2;
        public bool m_tagBool;

        public void Write(byte[] buffer, ref int offset)
        {
            CMemoryManager.WriteString(m_key, buffer, ref offset);
            CMemoryManager.WriteInt(m_fileLength, buffer, ref offset);
            CMemoryManager.WriteDateTime(ref m_lastModifyTime, buffer, ref offset);
            CMemoryManager.WriteInt(m_tagInt1, buffer, ref offset);
            CMemoryManager.WriteInt(m_tagInt2, buffer, ref offset);
            CMemoryManager.WriteByte((byte)(m_tagBool ? 1 : 0), buffer, ref offset);
        }

        public void Read(byte[] buffer, ref int offset)
        {
            m_key = CMemoryManager.ReadString(buffer, ref offset);
            m_fileLength = CMemoryManager.ReadInt(buffer, ref offset);
            m_lastModifyTime = CMemoryManager.ReadDateTime(buffer, ref offset);
            m_tagInt1 = CMemoryManager.ReadInt(buffer, ref offset);
            m_tagInt2 = CMemoryManager.ReadInt(buffer, ref offset);
            m_tagBool = (CMemoryManager.ReadByte(buffer, ref offset) > 0);
        }

        //--------------------------------------
        /// 排序函数
        /// @按m_lastModifyTime升序排列
        //--------------------------------------
        public int CompareTo(object obj)
        {
            CCachedFileInfo cachedTextureInfo = obj as CCachedFileInfo;

            if (m_lastModifyTime > cachedTextureInfo.m_lastModifyTime)
            {
                return 1;
            }
            else if (m_lastModifyTime == cachedTextureInfo.m_lastModifyTime)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
    };
    
};