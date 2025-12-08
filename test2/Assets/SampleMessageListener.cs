using UnityEngine;
using System.Text.RegularExpressions;

public class SampleMessageListener : MonoBehaviour
{
    // 拖拽需要控制的物体1到这里（Inspector面板中赋值）
    public Transform targetObject;

    // 传感器解算角度
    private float pitch, roll, yaw;
    // 校准零位
    private float calibPitch, calibRoll, calibYaw;
    private bool isCalibrated = false;

    // 坐标系映射（根据实际方向调整）
    public Vector3 axisMapping = new Vector3(1, -1, 1);
    // 互补滤波参数
    private const float alpha = 0.98f;
    private float lastTime;
    private const float gyroSensitivity = 131f; // MPU6050陀螺仪灵敏度

    void Start()
    {
        lastTime = Time.time;
        // 若未赋值目标物体，自动查找场景中名为“物体1”的对象
        if (targetObject == null)
            targetObject = GameObject.Find("Character")?.transform;
    }

    // 接收串口消息时调用
    void OnMessageArrived(string msg)
    {
        Debug.Log("Message arrived: " + msg);
        // 解析传感器数据并计算角度
        ParseSensorData(msg);
        // 应用角度到目标物体
        if (isCalibrated && targetObject != null)
            UpdateObjectRotation();
    }

    void OnConnectionEvent(bool success)
    {
        if (success)
            Debug.Log("Connection established");
        else
            Debug.Log("Connection attempt failed or disconnection detected");
    }

    void Update()
    {
        // 按C键校准（传感器水平静止时按）
        if (Input.GetKeyDown(KeyCode.C))
            CalibrateZero();
    }

    // 解析Arduino输出的带文字数据（AcX = xxx | AcY = xxx...）
    void ParseSensorData(string msg)
    {
        try
        {
            // 提取所有数字（包括负数）
            MatchCollection matches = Regex.Matches(msg, @"-?\d+");
            if (matches.Count < 6) return;

            // 转换为原始数据
            short rawAcX = short.Parse(matches[0].Value);
            short rawAcY = short.Parse(matches[1].Value);
            short rawAcZ = short.Parse(matches[2].Value);
            short rawGyX = short.Parse(matches[3].Value);
            short rawGyY = short.Parse(matches[4].Value);
            short rawGyZ = short.Parse(matches[5].Value);

            // 计算姿态角度
            CalculateAttitude(rawAcX, rawAcY, rawAcZ, rawGyX, rawGyY, rawGyZ);
        }
        catch (System.Exception e)
        {
            Debug.LogError("解析失败：" + e.Message);
        }
    }

    // 互补滤波解算绝对角度
    void CalculateAttitude(short acX, short acY, short acZ, short gyX, short gyY, short gyZ)
    {
        float deltaTime = Time.time - lastTime;
        lastTime = Time.time;

        // 加速度计计算角度（绝对参考）
        float ax = acX / 32768f;
        float ay = acY / 32768f;
        float az = acZ / 32768f;
        float accelPitch = Mathf.Atan2(ay, Mathf.Sqrt(ax * ax + az * az)) * Mathf.Rad2Deg;
        float accelRoll = Mathf.Atan2(-ax, az) * Mathf.Rad2Deg;

        // 陀螺仪积分计算角度（相对变化）
        float gx = gyX / gyroSensitivity;
        float gy = gyY / gyroSensitivity;
        float gz = gyZ / gyroSensitivity;
        pitch += gx * deltaTime;
        roll += gy * deltaTime;
        yaw += gz * deltaTime;

        // 融合数据（抑制漂移）
        pitch = alpha * pitch + (1 - alpha) * accelPitch;
        roll = alpha * roll + (1 - alpha) * accelRoll;
    }

    // 校准零位（传感器水平静止时执行）
    void CalibrateZero()
    {
        calibPitch = pitch;
        calibRoll = roll;
        calibYaw = yaw;
        isCalibrated = true;
        Debug.Log("校准完成！零位角度：" + pitch + ", " + roll + ", " + yaw);
    }

    // 更新物体1的旋转角度
    void UpdateObjectRotation()
    {
        // 计算相对零位的角度，并映射坐标系
        float targetPitch = (pitch - calibPitch) * axisMapping.x;
        float targetRoll = (roll - calibRoll) * axisMapping.y;
        float targetYaw = (yaw - calibYaw) * axisMapping.z;

        // 直接控制物体旋转（1:1匹配传感器角度）
        targetObject.localEulerAngles = new Vector3(targetPitch, targetRoll, targetYaw);
    }
}