using UnityEngine;
using System.Text.RegularExpressions;

public class MPU6050FullRotation : MonoBehaviour
{
    public SerialController serialController;

    // 修正：用short替代int16_t
    private short rawAcX, rawAcY, rawAcZ;
    private short rawGyX, rawGyY, rawGyZ;

    private float pitch, roll, yaw;
    private float calibPitch, calibRoll, calibYaw;
    private bool isCalibrated = false;

    public Vector3 axisMapping = new Vector3(1, -1, 1);
    public Vector3 angleOffset = new Vector3(0, 0, 0);

    private const float alpha = 0.98f;
    private float lastTime;
    private const float gyroSensitivity = 131f;

    void Start()
    {
        lastTime = Time.time;
    }

    // 改用Ardity的回调接收消息
    void OnMessageArrived(string msg)
    {
        ParseRawDataWithText(msg);
    }

    // 连接状态回调（可选）
    void OnConnectionEvent(bool isConnected)
    {
        Debug.Log(isConnected ? "串口已连接" : "串口断开");
    }

    void Update()
    {
        CalculateAttitude();

        if (Input.GetKeyDown(KeyCode.C))
        {
            CalibrateZero();
        }

        if (isCalibrated)
        {
            ApplyRotationToModel();
        }
    }

    void ParseRawDataWithText(string msg)
    {
        try
        {
            if (string.IsNullOrEmpty(msg) || msg.Contains("failed") || msg.Contains("disconnection"))
                return;

            MatchCollection matches = Regex.Matches(msg, @"-?\d+");
            if (matches.Count < 6)
            {
                Debug.LogWarning("数据长度不足：" + matches.Count + "，原始数据：" + msg);
                return;
            }

            rawAcX = short.Parse(matches[0].Value);
            rawAcY = short.Parse(matches[1].Value);
            rawAcZ = short.Parse(matches[2].Value);
            rawGyX = short.Parse(matches[3].Value);
            rawGyY = short.Parse(matches[4].Value);
            rawGyZ = short.Parse(matches[5].Value);
        }
        catch (System.Exception e)
        {
            Debug.LogError("解析错误：" + e.Message + "，原始数据：" + msg);
        }
    }

    void CalculateAttitude()
    {
        float deltaTime = Time.time - lastTime;
        lastTime = Time.time;

        float ax = rawAcX / 32768f;
        float ay = rawAcY / 32768f;
        float az = rawAcZ / 32768f;

        float accelPitch = Mathf.Atan2(ay, Mathf.Sqrt(ax * ax + az * az)) * Mathf.Rad2Deg;
        float accelRoll = Mathf.Atan2(-ax, az) * Mathf.Rad2Deg;

        float gx = rawGyX / gyroSensitivity;
        float gy = rawGyY / gyroSensitivity;
        float gz = rawGyZ / gyroSensitivity;

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
        Debug.Log("校准完成！");
    }

    void ApplyRotationToModel()
    {
        float targetPitch = (pitch - calibPitch) * axisMapping.x + angleOffset.x;
        float targetRoll = (roll - calibRoll) * axisMapping.y + angleOffset.y;
        float targetYaw = (yaw - calibYaw) * axisMapping.z + angleOffset.z;

        // 平滑旋转（可选）
        Quaternion targetRot = Quaternion.Euler(targetPitch, targetRoll, targetYaw);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, Time.deltaTime * 5f);
    }
}