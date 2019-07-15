using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RTCapture : MonoBehaviour {

    public Texture2D m_2dShotTextre = null;
    public Image m_image = null;

    public static RenderTexture m_2dshotRtt = null;
    public static Camera m_rts_cmr2dShot = null;
    public static int CamRenderFrameCount = -1;


    public int m_width = 128;
    public int m_height = 128;

    //bool drawRT = false;
    bool RenderOnce = false;

    void Awake()
    {

        if(m_rts_cmr2dShot == null) 
        {
            GameObject cam_obj = GameObject.Find("CameraRT");
            m_rts_cmr2dShot = cam_obj.GetComponent<Camera>();
            if (null == m_rts_cmr2dShot)
                return;
        }

        if(m_2dshotRtt == null)
        {
            m_2dshotRtt = RenderTexture.GetTemporary((int)m_width, (int)m_height, 24, RenderTextureFormat.ARGB32);
            m_2dshotRtt.DiscardContents();

            m_rts_cmr2dShot.enabled = false;
            m_rts_cmr2dShot.targetTexture = m_2dshotRtt;
        }

    }

    bool saveToRT = false;
	void Update ()
    {
        if(saveToRT)
        {
            //Debug.Log("Copy Texture:" + Time.frameCount);

            RTToTexture();
            saveToRT = false;
        }

        if (!RenderOnce && CamRenderFrameCount != Time.frameCount)
        {
            RenderOnce = true;
            DrawToRT();
        }

        //timer += Time.deltaTime;
        //if(timer>1)
        //{
        //    timer = 0;
        //    Image shot2DImage = GetShot2DImage();
        //    if(null != shot2DImage)
        //    {
        //        //shot2DImage.gameObject.SetActive(!shot2DImage.gameObject.activeInHierarchy);
        //        ////when UI is enabled , we force to RenderTexture Render and write to Texture2D
        //        //if(shot2DImage.gameObject.activeInHierarchy)
        //        {
        //            DrawToRT();
        //        }
        //    }
        //}
    }

    void DrawToRT()
    {
        if (CamRenderFrameCount == Time.frameCount)
            return;

        //Debug.Log("AAA  Render:" + Time.frameCount);

        CamRenderFrameCount = Time.frameCount;

        saveToRT = true;
        m_rts_cmr2dShot.Render();
    }

    Image GetShot2DImage()
    {
        //Image shot2DImage = CUIUtility.FindComponent<Image>(gameObject, k_2dShotImageName);
        Image shot2DImage = gameObject.GetComponentInChildren<Image>(true);
        return shot2DImage;
    }

    void RTToTexture()
    {
        RenderTexture.active = m_2dshotRtt;
            
        Image shot2DImage = GetShot2DImage();
        shot2DImage.enabled = true;
        if (m_2dShotTextre == null)
            m_2dShotTextre = new Texture2D(m_width, m_height, TextureFormat.ARGB32, false);
        m_2dShotTextre.ReadPixels(new Rect(0, 0, m_width, m_height), 0, 0);
        m_2dShotTextre.Apply();

        shot2DImage.sprite = Sprite.Create(m_2dShotTextre, new Rect(0, 0, m_width, m_height), new Vector2(0.5f, 0.5f));
    }


    const string c_iconTextureCacheFolderName = "avatar";
    //static CHttpFileCacher s_3dIconTextureCacher;

    void WriteToCache()
    {
         
    }

    /// <summary>
    /// Delete the cache directory, call it when resources are updated
    /// </summary>
    public static void DeleteIconTextureCacheDirectory()
    {
        //CFileManager.DeleteDirectory(CHttpFileCacher.GetCacheDirectory(c_iconTextureCacheFolderName));
    }

    public void LoadFromCache()
    {
        //string cachePath = s_3dIconTextureCacher.GetCachedFilePath(GetCacheKey(iconPath, m_width, m_height), 100);
        //if (!string.IsNullOrEmpty(cachePath))
        //{
        //    m_iconPath = iconPath;
        //    for (int i = 0; i < transform.childCount; i++)
        //    {
        //        transform.GetChild(i).gameObject.CustomSetActive(false);
        //    }

        //    byte[] data = CFileManager.ReadFile(cachePath);
        //    if (data != null && data.Length > 0)
        //    {
        //        if (m_2dShotTextre == null)
        //        {
        //            m_2dShotTextre = new Texture2D(m_width, m_height, TextureFormat.ARGB32, false);
        //        }
        //        m_shotStep = enTakeShotStep.NotActived;
        //        m_2dShotTextre.LoadImage(data);
        //        m_2dShotTextre.Apply();
        //        Image shot2DImage = GetShot2DImage();
        //        shot2DImage.enabled = true;
        //        shot2DImage.gameObject.CustomSetActive(true);
        //        shot2DImage.SetSprite(Sprite.Create(m_2dShotTextre, new Rect(0, 0, m_width, m_height), new Vector2(0.5f, 0.5f)), ImageAlphaTexLayout.None);
        //        return;
        //    }
        //}

    }

    public void WriteTexToCache()
    { 
        
    
    }
}
