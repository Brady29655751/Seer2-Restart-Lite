/*
UniGif
Copyright (c) 2015 WestHillApps (Hironari Nishioka)
This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary>
/// Texture Animation from GIF image
/// </summary>
public class UniGifImage : MonoBehaviour
{
    /// <summary>
    /// This component state
    /// </summary>
    public enum State
    {
        None,
        Loading,
        Ready,
        Playing,
        Pause,
    }

    // Target row image
    [SerializeField]
    private Image m_image;
    // Image Aspect Controller
    // [SerializeField]
    // private UniGifImageAspectController m_imgAspectCtrl;
    // Textures filter mode
    [SerializeField]
    private FilterMode m_filterMode = FilterMode.Point;
    // Textures wrap mode
    [SerializeField]
    private TextureWrapMode m_wrapMode = TextureWrapMode.Clamp;
    // Load from url on start
    [SerializeField]
    private bool m_loadOnStart;
    // GIF image url (WEB or StreamingAssets path)
    [SerializeField]
    private string m_loadOnStartUrl;
    // Rotating on loading
    [SerializeField]
    private bool m_rotateOnLoading;
    // Debug log flag
    [SerializeField]
    private bool m_outputDebugLog;

    // Decoded GIF texture list
    private List<UniGif.GifTexture> m_gifTextureList;
    // Delay time
    private float m_delayTime;
    // Texture index
    private int m_gifTextureIndex;
    // loop counter
    private int m_nowLoopCount;

    /// <summary>
    /// Now state
    /// </summary>
    public State nowState
    {
        get;
        private set;
    }

    /// <summary>
    /// Animation loop count (0 is infinite)
    /// </summary>
    public int loopCount
    {
        get;
        private set;
    }

    /// <summary>
    /// Texture width (px)
    /// </summary>
    public int width
    {
        get;
        private set;
    }

    /// <summary>
    /// Texture height (px)
    /// </summary>
    public int height
    {
        get;
        private set;
    }

    private void Start()
    {
        if (m_image == null)
        {
            m_image = GetComponent<Image>();
        }
        if (m_loadOnStart)
        {
            SetGifFromUrl(m_loadOnStartUrl);
        }
    }

    private void OnDestroy()
    {
        Clear();
    }

    private void Update()
    {
        switch (nowState)
        {
            case State.None:
                break;

            case State.Loading:
                if (m_rotateOnLoading)
                {
                    transform.Rotate(0f, 0f, 30f * Time.deltaTime, Space.Self);
                }
                break;

            case State.Ready:
                break;

            case State.Playing:
                if (m_image == null || m_gifTextureList == null || m_gifTextureList.Count <= 0)
                {
                    return;
                }
                if (m_delayTime > Time.time)
                {
                    return;
                }
                // Change texture
                m_gifTextureIndex++;
                if (m_gifTextureIndex >= m_gifTextureList.Count)
                {
                    m_gifTextureIndex = 0;

                    if (loopCount > 0)
                    {
                        m_nowLoopCount++;
                        if (m_nowLoopCount >= loopCount)
                        {
                            Stop();
                            return;
                        }
                    }
                }
                Texture2D texture2D = m_gifTextureList[m_gifTextureIndex].m_texture2d;
                if (texture2D != null)
                    m_image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0, 0), 100f, 0, SpriteMeshType.Tight);
                else
                    m_image.sprite = null;

                m_delayTime = Time.time + m_gifTextureList[m_gifTextureIndex].m_delaySec;
                break;

            case State.Pause:
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Set GIF texture from url
    /// </summary>
    /// <param name="url">GIF image url (WEB or StreamingAssets path)</param>
    /// <param name="autoPlay">Auto play after decode</param>
    public void SetGifFromUrl(string url, bool autoPlay = true, Sprite loadingSprite = null)
    {
        StartCoroutine(SetGifFromUrlCoroutine(url, autoPlay, loadingSprite));
    }

    /// <summary>
    /// Set GIF texture from url
    /// </summary>
    /// <param name="url">GIF image url (WEB or StreamingAssets path)</param>
    /// <param name="autoPlay">Auto play after decode</param>
    /// <returns>IEnumerator</returns>
    public IEnumerator SetGifFromUrlCoroutine(string url, bool autoPlay = true, Sprite loadingSprite = null)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("Gif URL is nothing.");
            yield break;
        }

        if (nowState == State.Loading)
        {
            Debug.LogWarning("Gif Already loading.");
            yield break;
        }
        nowState = State.Loading;

        string path;
        if (url.StartsWith("http"))
        {
            // from WEB
            path = url;
        }
        else
        {
            // from StreamingAssets
            path = $"file://{Application.persistentDataPath}/{url}";
        }

        // Load file
        using (var webRequest = new UnityWebRequest(path))
        {
            webRequest.downloadHandler = new DownloadHandlerBuffer();
        
            yield return webRequest.SendWebRequest();
        
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("File load error.\n" + webRequest.error);
                nowState = State.None;
                yield break;
            }
        
            Clear();
            if (m_image != null)
                m_image.sprite = loadingSprite;
        
            nowState = State.Loading;
        
            // Get GIF textures
            if (UniGif.gifCache.TryGetValue(path, out var cache) && (cache.Item1 != null))
            {
                SetTextureList(cache.Item1, cache.Item2, cache.Item3, cache.Item4, autoPlay);
                yield break;
            }

            yield return StartCoroutine(UniGif.GetTextureListCoroutine(webRequest.downloadHandler.data, (gifTexList, loopCount, width, height) =>
            {
                UniGif.gifCache[path] = (gifTexList, loopCount, width, height);
                SetTextureList(gifTexList, loopCount, width, height, autoPlay);
            },
            m_filterMode, m_wrapMode, m_outputDebugLog));
        }   
    }

    private void SetTextureList(List<UniGif.GifTexture> gifTexList, int loopCount, int width, int height, bool autoPlay = true)
    {
         if (gifTexList != null)
        {
            this.m_gifTextureList = gifTexList;
            this.loopCount = loopCount;
            this.width = width;
            this.height = height;
            
            nowState = State.Ready;
            if (m_rotateOnLoading)
            {
                transform.localEulerAngles = Vector3.zero;
            }

            if (autoPlay)
            {
                Play();
            }
        }
        else
        {
            Debug.LogError("Gif texture get error.");
            nowState = State.None;
        }
    }

    /// <summary>
    /// Clear GIF texture
    /// </summary>
    public void Clear()
    {
        if (m_image != null)
        {
            m_image.sprite = null;
        }

        if (m_gifTextureList != null)
        {
            //! Danger: We have UniGif.gifCache, so we should not destroy textures here. (UniGif will manage texture memory)
            /*
            for (int i = 0; i < m_gifTextureList.Count; i++)
            {
                if (m_gifTextureList[i] != null)
                {
                    if (m_gifTextureList[i].m_texture2d != null)
                    {
                        Destroy(m_gifTextureList[i].m_texture2d);
                        m_gifTextureList[i].m_texture2d = null;
                    }
                    m_gifTextureList[i] = null;
                }
            }
            m_gifTextureList.Clear();
            */
            m_gifTextureList = null;
        }

        nowState = State.None;
    }

    /// <summary>
    /// Play GIF animation
    /// </summary>
    public void Play()
    {
        if (nowState != State.Ready)
        {
            Debug.LogWarning("State is not READY.");
            return;
        }
        if (m_image == null || m_gifTextureList == null || m_gifTextureList.Count <= 0)
        {
            Debug.LogError("Image or GIF Texture is nothing.");
            return;
        }
        nowState = State.Playing;
        
        Texture2D texture2D = m_gifTextureList[0].m_texture2d;
        if (texture2D != null)
            m_image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0, 0), 100f, 0, SpriteMeshType.Tight);
        else
            m_image.sprite = null;

        m_delayTime = Time.time + m_gifTextureList[0].m_delaySec;
        m_gifTextureIndex = 0;
        m_nowLoopCount = 0;
    }

    /// <summary>
    /// Stop GIF animation
    /// </summary>
    public void Stop()
    {
        if (nowState != State.Playing && nowState != State.Pause)
        {
            Debug.LogWarning("State is not Playing and Pause.");
            return;
        }
        nowState = State.Ready;
    }

    /// <summary>
    /// Pause GIF animation
    /// </summary>
    public void Pause()
    {
        if (nowState != State.Playing)
        {
            Debug.LogWarning("State is not Playing.");
            return;
        }
        nowState = State.Pause;
    }

    /// <summary>
    /// Resume GIF animation
    /// </summary>
    public void Resume()
    {
        if (nowState != State.Pause)
        {
            Debug.LogWarning("State is not Pause.");
            return;
        }
        nowState = State.Playing;
    }
}