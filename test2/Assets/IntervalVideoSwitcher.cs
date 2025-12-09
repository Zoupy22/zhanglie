// IntervalVideoSwitcher.cs
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class IntervalVideoSwitcher : MonoBehaviour
{
    [Header("视频片段列表")]
    [SerializeField] private System.Collections.Generic.List<VideoClip> clips = new System.Collections.Generic.List<VideoClip>();

    [Header("触发按键")]
    [SerializeField] private KeyCode switchKey = KeyCode.Space;

    [Header("间隔触发设置")]
    [Tooltip("true = 第1、3、5…次按键被屏蔽；false = 第2、4、6…次按键被屏蔽")]
    [SerializeField] private bool blockFirst = false;

    private VideoPlayer vp;
    private int currentIndex = 0;
    private int pressCount = 0;   // 记录按键次数（从 1 开始）

    void Awake()
    {
        vp = GetComponent<VideoPlayer>();
        if (clips.Count == 0)
        {
            Debug.LogWarning("[IntervalVideoSwitcher] 视频列表为空，请先赋值！");
            enabled = false;
            return;
        }

        // 初始播放第一段
        PlayByIndex(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            pressCount++;

            // 根据 blockFirst 决定奇偶次是否屏蔽
            bool shouldBlock = blockFirst ? (pressCount % 2 == 1)   // 屏蔽奇数
                                          : (pressCount % 2 == 0);  // 屏蔽偶数

            if (shouldBlock) return;   // 被屏蔽，什么都不做

            // 真正切换
            currentIndex = (currentIndex + 1) % clips.Count;
            PlayByIndex(currentIndex);
        }
    }

    private void PlayByIndex(int index)
    {
        vp.Stop();
        vp.clip = clips[index];
        vp.Play();
    }

    // 对外只读属性，方便调试或 UI 显示
    public int CurrentClipIndex => currentIndex;
    public int PressCount => pressCount;
}