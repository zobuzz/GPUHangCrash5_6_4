using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RTCapture : MonoBehaviour {

    public RenderTexture m_2dshotRtt = null;
    public Texture2D m_2dShotTextre = null;
    public Image m_image = null;

    public Camera m_rts_cmr2dShot = null;
    public GameObject m_cmrObject = null;

    public int m_width = 128;
    public int m_height = 128;


    bool drawRT = false;
    void Awake()
    {
        if (null == m_cmrObject)
            return;
        m_rts_cmr2dShot = m_cmrObject.GetComponent<Camera>();
        if (null == m_rts_cmr2dShot)
            return;

        m_2dshotRtt = RenderTexture.GetTemporary((int)m_width, (int)m_height, 24, RenderTextureFormat.ARGB32);
        m_2dshotRtt.DiscardContents();

        m_rts_cmr2dShot.enabled = false;
        m_rts_cmr2dShot.targetTexture = m_2dshotRtt;
    }

    // Use this for initialization
    void Start () {
    }

    private float timer = 0;
	// Update is called once per frame
	void Update () 
    {
		if(drawRT)
        {
            drawRT = false;
            DrawToRT();
            RTToTexture();
        }

        timer += Time.deltaTime;
        if(timer>1)
        {
            timer = 0;
            Image shot2DImage = GetShot2DImage();
            if(null != shot2DImage)
            {
                shot2DImage.enabled = !shot2DImage.enabled;
            }
        }
    }


    void DrawToRT()
    {
        m_rts_cmr2dShot.Render();
    }


    Image GetShot2DImage()
    {
        //Image shot2DImage = CUIUtility.FindComponent<Image>(gameObject, k_2dShotImageName);
        Image shot2DImage = gameObject.GetComponent<Image>();
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

    void OnGUI()
    {
        if(GUI.Button(new Rect(Screen.width - 200,0,200,150),"RenderRT"))
        {
            drawRT = true;
        }
    }
}
