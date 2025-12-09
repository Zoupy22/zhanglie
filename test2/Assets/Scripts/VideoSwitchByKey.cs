using UnityEngine;
using UnityEngine.Video;

public class VideoSwitchByKey : MonoBehaviour
{
    [Header("视频片段数组")]
    public VideoClip[] videoClips; // 拖入4个视频
    private VideoPlayer videoPlayer;
    private int currentIndex = 0; // 当前播放索引

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoClips.Length > 0)
            videoPlayer.clip = videoClips[currentIndex]; // 初始加载第一个
    }

    void Update()
    {
        // 按B键切换（循环切换）
        if (Input.GetKeyDown(KeyCode.B))
        {
            currentIndex = (currentIndex + 1) % videoClips.Length;
            videoPlayer.clip = videoClips[currentIndex];
            videoPlayer.Play(); // 切换后立即播放
        }
    }
}