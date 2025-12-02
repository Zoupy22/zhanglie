using UnityEngine;

public class RotationToBlur : MonoBehaviour
{
    [Header("目标材质")]
    public Material targetMaterial;
    [Header("Shader参数名")]
    public string blurParamName = "_strength";
    [Header("清晰的目标欧拉角列表")]
    public Vector3[] clearEulerAnglesList = new Vector3[]
    {
        new Vector3(-90f, 90f, 270f),  // 第一个清晰状态
        new Vector3(0f, 360f, 0f)      // 第二个清晰状态（0,360,0）
    };
    [Header("最大虚化值")]
    public float maxBlur = 0.5f;
    [Header("角度容差（差值和小于此值才清晰）")]
    public float sumTolerance = 3f; // 三轴差值和的容差（推荐3°~5°）
    [Header("渐变上限（差值和达到此值时最大模糊）")]
    public float gradientMaxSum = 180f; // 差值和上限（如180°=各轴平均60°）

    void Update()
    {
        if (targetMaterial == null)
        {
            Debug.LogError("请给targetMaterial赋值材质！");
            return;
        }

        // 获取物体当前的欧拉角并归一化
        Vector3 currentEuler = transform.eulerAngles;
        currentEuler = NormalizeEulerAngles(currentEuler);

        // 标记是否处于清晰状态，记录最小差值和
        bool isClear = false;
        float minSumDiff = float.MaxValue; // 与最近清晰状态的差值和

        // 遍历所有清晰欧拉角，计算差值和
        foreach (Vector3 targetEuler in clearEulerAnglesList)
        {
            Vector3 normalizedTarget = NormalizeEulerAngles(targetEuler);

            // 计算三轴角度差值的绝对值
            float diffX = Mathf.Abs(Mathf.DeltaAngle(currentEuler.x, normalizedTarget.x));
            float diffY = Mathf.Abs(Mathf.DeltaAngle(currentEuler.y, normalizedTarget.y));
            float diffZ = Mathf.Abs(Mathf.DeltaAngle(currentEuler.z, normalizedTarget.z));

            // 计算三轴差值的绝对值之和
            float sumDiff = diffX + diffY + diffZ;

            // 更新最小差值和
            if (sumDiff < minSumDiff)
            {
                minSumDiff = sumDiff;
            }

            // 判断是否清晰：差值和小于容差
            if (sumDiff < sumTolerance)
            {
                isClear = true;
                break; // 找到匹配的清晰状态，无需继续检查
            }
        }

        // 计算虚化值：差值和越小越清晰，反之越模糊
        float blurAmount = 0;
        if (!isClear)
        {
            // 差值和从sumTolerance到gradientMaxSum渐变到maxBlur
            float t = Mathf.InverseLerp(sumTolerance, gradientMaxSum, minSumDiff);
            t = Mathf.SmoothStep(0, 1, t); // 非线性渐变，过渡更自然
            blurAmount = t * maxBlur;
            blurAmount = Mathf.Clamp(blurAmount, 0.01f, maxBlur); // 强制非清晰状态有模糊
        }

        // 传递参数到Shader
        targetMaterial.SetFloat(blurParamName, blurAmount);

        // 调试日志（查看差值和与虚化值）
        Debug.Log($"差值和：{minSumDiff:F1}° | 虚化值：{blurAmount:F3}");
    }

    // 归一化Vector3欧拉角的每个轴到[-180, 180]（处理360°=0°）
    private Vector3 NormalizeEulerAngles(Vector3 euler)
    {
        euler.x = NormalizeSingleAngle(euler.x);
        euler.y = NormalizeSingleAngle(euler.y);
        euler.z = NormalizeSingleAngle(euler.z);
        return euler;
    }

    // 归一化单个角度到[-180, 180]
    private float NormalizeSingleAngle(float angle)
    {
        angle %= 360;
        return angle > 180 ? angle - 360 : angle;
    }
}