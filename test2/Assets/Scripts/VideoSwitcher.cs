using UnityEngine;
using UnityEngine.Video;

public class VideoSwitcher : MonoBehaviour
{
    [Header("视频播放器设置")]
    [Tooltip("第一个视频播放器")]
    public VideoPlayer videoPlayerA;

    [Tooltip("第二个视频播放器")]
    public VideoPlayer videoPlayerB;

    [Header("视频列表")]
    [Tooltip("要切换的视频片段列表")]
    public VideoClip[] videoClips;

    [Header("状态信息")]
    [Tooltip("下次应该切换哪个播放器")]
    [SerializeField] private bool switchPlayerB = true;

    [Tooltip("A播放器当前视频索引")]
    [SerializeField] private int indexA = 0;

    [Tooltip("B播放器当前视频索引")]
    [SerializeField] private int indexB = 0;

    [Tooltip("是否处于重置阶段")]
    [SerializeField] private bool isResetPhase = false;

    private void Start()
    {
        // 验证设置
        if (videoPlayerA == null || videoPlayerB == null)
        {
            Debug.LogError("请将A和B物体上的VideoPlayer组件拖拽到脚本的相应字段！");
            enabled = false;
            return;
        }

        if (videoClips == null || videoClips.Length == 0)
        {
            Debug.LogError("请添加至少一个VideoClip到视频列表！");
            enabled = false;
            return;
        }

        // 设置初始视频
        videoPlayerA.clip = videoClips[0];
        videoPlayerB.clip = videoClips[0];
        indexA = 0;
        indexB = 0;

        Debug.Log($"初始化完成。A: V{indexA + 1}, B: V{indexB + 1}");
        UpdateStatusDisplay();
    }

    private void Update()
    {
        // 检测B键按下
        if (Input.GetKeyDown(KeyCode.B))
        {
            HandleBKeyPress();
        }
    }

    /// <summary>
    /// 处理B键按下
    /// </summary>
    private void HandleBKeyPress()
    {
        if (videoClips.Length == 0) return;

        // 如果处于重置阶段
        if (isResetPhase)
        {
            HandleResetPhase();
            return;
        }

        // 正常切换阶段
        if (switchPlayerB)
        {
            // 切换B播放器
            SwitchPlayerB();
        }
        else
        {
            // 切换A播放器
            SwitchPlayerA();
        }

        // 切换目标
        switchPlayerB = !switchPlayerB;

        // 检查是否需要进入重置阶段（B播放到最后一个视频后）
        CheckForResetPhase();

        UpdateStatusDisplay();
    }

    /// <summary>
    /// 切换B播放器的视频
    /// </summary>
    private void SwitchPlayerB()
    {
        // 计算B的下一个索引
        int nextIndexB = indexB + 1;

        if (nextIndexB >= videoClips.Length)
        {
            nextIndexB = 0; // 循环回到第一个
        }

        indexB = nextIndexB;
        videoPlayerB.clip = videoClips[indexB];

        Debug.Log($"切换B到 V{indexB + 1}: {videoClips[indexB].name}");
    }

    /// <summary>
    /// 切换A播放器的视频
    /// </summary>
    private void SwitchPlayerA()
    {
        // 计算A的下一个索引
        int nextIndexA = indexA + 1;

        if (nextIndexA >= videoClips.Length)
        {
            nextIndexA = 0; // 循环回到第一个
        }

        indexA = nextIndexA;
        videoPlayerA.clip = videoClips[indexA];

        Debug.Log($"切换A到 V{indexA + 1}: {videoClips[indexA].name}");
    }

    /// <summary>
    /// 检查是否需要进入重置阶段
    /// </summary>
    private void CheckForResetPhase()
    {
        // 当B播放到最后一个视频，且下一次应该切换A时，进入重置阶段
        if (indexB == videoClips.Length - 1 && !switchPlayerB)
        {
            isResetPhase = true;
            Debug.Log("B已播放到最后一个视频，进入重置阶段");
            Debug.Log("下次按B键：A重置到V1，B保持V4");
        }
    }

    /// <summary>
    /// 处理重置阶段
    /// </summary>
    private void HandleResetPhase()
    {
        // 重置阶段的第一步：重置A到第一个视频
        if (indexA != 0)
        {
            indexA = 0;
            videoPlayerA.clip = videoClips[0];
            switchPlayerB = true; // 下次应该切换B

            Debug.Log($"重置A到 V1: {videoClips[0].name}");
            Debug.Log("下次按B键：B重置到V2，A保持V1");
        }
        else
        {
            // 重置阶段的第二步：重置B到第二个视频（跳过第一个，因为初始状态已经是V1）
            indexB = 1; // 直接设置为第二个视频
            if (indexB >= videoClips.Length) indexB = 0; // 安全检查
            videoPlayerB.clip = videoClips[indexB];

            // 重置状态，恢复正常循环
            isResetPhase = false;
            switchPlayerB = false; // 下次应该切换A

            Debug.Log($"重置B到 V{indexB + 1}: {videoClips[indexB].name}");
            Debug.Log("重置完成，恢复正常交替切换");
        }

        UpdateStatusDisplay();
    }

    /// <summary>
    /// 更新状态显示
    /// </summary>
    private void UpdateStatusDisplay()
    {
        Debug.Log($"当前状态: A-V{indexA + 1}, B-V{indexB + 1}, " +
                 $"下次切换: {(switchPlayerB ? "B" : "A")}, " +
                 $"重置阶段: {isResetPhase}");
    }

    /// <summary>
    /// 手动触发切换（可用于UI按钮）
    /// </summary>
    public void ManualSwitch()
    {
        HandleBKeyPress();
    }

    /// <summary>
    /// 重置到初始状态
    /// </summary>
    public void ResetToInitial()
    {
        indexA = 0;
        indexB = 0;
        videoPlayerA.clip = videoClips[0];
        videoPlayerB.clip = videoClips[0];
        switchPlayerB = true;
        isResetPhase = false;

        Debug.Log("已重置到初始状态");
        UpdateStatusDisplay();
    }

    /// <summary>
    /// 获取当前状态信息
    /// </summary>
    public string GetCurrentStatus()
    {
        return $"A: V{indexA + 1} ({videoClips[indexA].name})\n" +
               $"B: V{indexB + 1} ({videoClips[indexB].name})\n" +
               $"下次切换: {(switchPlayerB ? "Player B" : "Player A")}\n" +
               $"重置阶段: {isResetPhase}";
    }

    /// <summary>
    /// 强制设置A和B的播放索引（调试用）
    /// </summary>
    public void SetPlaybackIndex(int aIndex, int bIndex)
    {
        if (aIndex >= 0 && aIndex < videoClips.Length &&
            bIndex >= 0 && bIndex < videoClips.Length)
        {
            indexA = aIndex;
            indexB = bIndex;
            videoPlayerA.clip = videoClips[aIndex];
            videoPlayerB.clip = videoClips[bIndex];
            isResetPhase = false;

            Debug.Log($"强制设置: A-V{aIndex + 1}, B-V{bIndex + 1}");
            UpdateStatusDisplay();
        }
    }
}