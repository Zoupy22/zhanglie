using UnityEngine;
using System.Text.RegularExpressions;

public class SampleMessageListener : MonoBehaviour
{
    [Header("拖拽需要控制的物体")]
    public Transform targetObject;

    /* —— 以下变量全部保持原样 —— */
    private float pitch, roll, yaw;
    private float calibPitch, calibRoll, calibYaw;
    private bool isCalibrated = false;

    public Vector3 axisMapping = new Vector3(1, -1, 1);
    private const float alpha = 0.98f;
    private float lastTime;
    private const float gyroSensitivity = 131f;
    /* ———————————————————————— */

    // 新增：同步开关
    private bool syncEnabled = false;

    void Start()
    {
        lastTime = Time.time;
        if (targetObject == null)
            targetObject = GameObject.Find("Character")?.transform;
    }

    /* 串口消息回调，保持原样 */
    void OnMessageArrived(string msg)
    {
        Debug.Log("Message arrived: " + msg);
        ParseSensorData(msg);
        // 只有“已校准”且“同步开关打开”才更新物体
        if (isCalibrated && syncEnabled && targetObject != null)
            UpdateObjectRotation();
    }

    void OnConnectionEvent(bool success)
    {
        Debug.Log(success ? "Connection established" : "Connection attempt failed or disconnection detected");
    }

    /* ******** 唯一改动的部分 ******** */
    void Update()
    {
        // 按 C 键循环切换同步状态
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!isCalibrated)
            {
                // 第一次按下：先校准，再打开同步
                CalibrateZero();
                syncEnabled = true;
            }
            else
            {
                // 之后每次按下：单纯切换同步开关
                syncEnabled = !syncEnabled;
                Debug.Log("同步 " + (syncEnabled ? "开启" : "关闭"));
            }
        }
    }
    /* ********************************* */

    #region 以下所有方法完全保持原样
    void ParseSensorData(string msg)
    {
        try
        {
            MatchCollection matches = Regex.Matches(msg, @"-?\d+");
            if (matches.Count < 6) return;

            short rawAcX = short.Parse(matches[0].Value);
            short rawAcY = short.Parse(matches[1].Value);
            short rawAcZ = short.Parse(matches[2].Value);
            short rawGyX = short.Parse(matches[3].Value);
            short rawGyY = short.Parse(matches[4].Value);
            short rawGyZ = short.Parse(matches[5].Value);

            CalculateAttitude(rawAcX, rawAcY, rawAcZ, rawGyX, rawGyY, rawGyZ);
        }
        catch (System.Exception e)
        {
            Debug.LogError("解析失败：" + e.Message);
        }
    }

    void CalculateAttitude(short acX, short acY, short acZ, short gyX, short gyY, short gyZ)
    {
        float deltaTime = Time.time - lastTime;
        lastTime = Time.time;

        float ax = acX / 32768f;
        float ay = acY / 32768f;
        float az = acZ / 32768f;
        float accelPitch = Mathf.Atan2(ay, Mathf.Sqrt(ax * ax + az * az)) * Mathf.Rad2Deg;
        float accelRoll = Mathf.Atan2(-ax, az) * Mathf.Rad2Deg;

        float gx = gyX / gyroSensitivity;
        float gy = gyY / gyroSensitivity;
        float gz = gyZ / gyroSensitivity;
        pitch += gx * deltaTime;
        roll += gy * deltaTime;
        yaw += gz * deltaTime;

        pitch = alpha * pitch + (1 - alpha) * accelPitch;
        roll = alpha * roll + (1 - alpha) * accelRoll;
    }

    void CalibrateZero()
    {
        calibPitch = pitch;
        calibRoll = roll;
        calibYaw = yaw;
        isCalibrated = true;
        Debug.Log("校准完成！零位角度：" + pitch + ", " + roll + ", " + yaw);
    }

    void UpdateObjectRotation()
    {
        float targetPitch = (pitch - calibPitch) * axisMapping.x;
        float targetRoll = (roll - calibRoll) * axisMapping.y;
        float targetYaw = (yaw - calibYaw) * axisMapping.z;

        targetObject.localEulerAngles = new Vector3(targetPitch, targetRoll, targetYaw);
    }
    #endregion
}