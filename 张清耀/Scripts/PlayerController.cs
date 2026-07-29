using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("鼠标视角设置")]
    public float mouseSensitivity = 2f;     // 鼠标灵敏度
    public Transform cameraTransform;       // 摄像机引用
    private float xRotation = 0f;           // X轴旋转角度（上下看）
    
    [Header("移动设置")]
    public float moveSpeed = 5f;            // 移动速度
    public CharacterController controller;  // 角色控制器
    public float gravity = -9.81f;          // 重力
    private float yVelocity = 0f;           // Y轴速度
    
    [Header("眨眼设置")]
    public float blinkInterval = 5f;        // 眨眼间隔（秒）
    public float blinkDuration = 0.2f;      // 眨眼持续时间（秒）
    private float blinkTimer = 0f;          // 眨眼计时器
    private bool isBlinking = false;        // 是否正在眨眼
    
    [Header("SCP检测")]
    public Transform scp173Transform;       // SCP-173引用
    public float viewAngle = 60f;           // 视野角度
    
    [Header("UI")]
    public GameObject blinkEffect;         // 眨眼效果组件

    void Start()
    {
        // 锁定鼠标到屏幕中心
        Cursor.lockState = CursorLockMode.Locked;
        
        // 获取角色控制器
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        if (GameDataManger.isGameOver) return;
        
        MouseLook();
        Move();
        BlinkLogic();
        CheckWatchingSCP();
    }

    // 鼠标视角控制
    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // 上下旋转摄像机（限制角度）
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // 左右旋转角色
        transform.Rotate(Vector3.up * mouseX);
    }

    // 移动控制（WASD相对摄像机方向）
    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        // 获取摄像机的前后左右方向（忽略Y轴）
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        
        // 计算移动方向
        Vector3 moveDirection = (forward * v + right * h).normalized;
        
        // 应用重力
        if (controller != null)
        {
            if (controller.isGrounded)
            {
                yVelocity = 0f;
            }
            else
            {
                yVelocity += gravity * Time.deltaTime;
            }
            
            // 设置Y轴速度
            moveDirection.y = yVelocity;
            
            // 使用CharacterController移动
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
        else
        {
            // 如果没有CharacterController，使用Translate移动（不考虑重力）
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        }
        
        // 动画控制
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("Move", h != 0 || v != 0);
        }
    }

    // 眨眼逻辑
    void BlinkLogic()
    {
        blinkTimer += Time.deltaTime;
        
        // 到达眨眼间隔，开始眨眼
        if (blinkTimer >= blinkInterval && !isBlinking)
        {
            StartCoroutine(Blink());//防止重复开线程
        }
    }

    // 眨眼协程
    IEnumerator Blink()
    {
        isBlinking = true;
        GameDataManger.isBlinking = true;
        
        // 开始眨眼效果
        if (blinkEffect != null)
        {
            blinkEffect.gameObject.SetActive(true);
        }
        
        // 等待眨眼持续时间
        yield return new WaitForSeconds(blinkDuration);
        
        // 结束眨眼
        isBlinking = false;
        GameDataManger.isBlinking = false;
        
        // 结束眨眼效果
        if (blinkEffect != null)
        {
            blinkEffect.gameObject.SetActive(false);
        }
        
        // 重置计时器
        blinkTimer = 0f;
    }

    // 检测玩家是否看着SCP-173
    void CheckWatchingSCP()
    {
        if (scp173Transform == null || cameraTransform == null) return;
        
        // 计算玩家到SCP-173的方向
        Vector3 toSCP = scp173Transform.position - cameraTransform.position;
        toSCP.Normalize();
        
        // 计算摄像机正前方方向
        Vector3 forward = cameraTransform.forward;
        forward.Normalize();
        
        // 计算夹角
        float angle = Vector3.Angle(forward, toSCP);
        
        // 检测中间是否有障碍物
        bool hasObstacle = false;
        if (Physics.Raycast(cameraTransform.position, toSCP, out RaycastHit hit, Mathf.Infinity))
        {
            // 如果射线击中的不是SCP-173，说明有障碍物
            if (hit.transform != scp173Transform && !hit.transform.IsChildOf(scp173Transform))
            {
                hasObstacle = true;
            }
        }
        
        // 如果夹角小于视野角度且没有障碍物，说明玩家看着SCP-173
        GameDataManger.isWatchingSCP = angle < viewAngle && !hasObstacle;
    }
}
