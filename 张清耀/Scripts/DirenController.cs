using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class DirenController : MonoBehaviour
{
    [Header("玩家引用")]
    public GameObject player;                 // 玩家对象
    private Transform playerTransform;       // 玩家变换组件
    private PlayerController playerController; // 玩家控制器
    
    [Header("移动设置")]
    public float normalSpeed = 0;           // 正常移动速度
    public float blinkSpeed = 50f;           // 眨眼时的高速移动速度
    public float teleportDistance = 10f;     // 瞬移距离
    
    [Header("死亡检测")]
    public float killDistance = 2f;          // 击杀距离（小于此距离下次眨眼死亡）
    public float backDetectionAngle = 160f;  // 背对检测角度（大于此角度判定为背对）
    
    public string deathAnimationName = "Death"; // 玩家死亡动画名称
    
    private bool canMove = false;            // 是否可以移动
    private bool isCloseEnoughToKill = false; // 是否足够近可以击杀
    private bool justTeleported = false;     // 是否刚瞬移过
    
    void Start()
    {
        // 获取玩家组件
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
        }
        
     
    }

    void Update()
    {
        if (GameDataManger.isGameOver) return;
        
        // 始终面向玩家
        transform.LookAt(playerTransform);
        
        // 检测是否足够近可以击杀
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        isCloseEnoughToKill = distance < killDistance;
        
        // 检测玩家是否背对
        bool isPlayerBack = IsPlayerBack();
        
        // 如果玩家背对，直接击杀
        if (isPlayerBack)
        {
            KillPlayer();
            return;
        }
        
        // 判断是否可以移动
        // 条件：玩家眨眼 或者 玩家没看着SCP-173
        canMove = GameDataManger.isBlinking || !GameDataManger.isWatchingSCP;
        
        if (canMove)
        {
            Move();
        }
        // 如果足够近且正在眨眼，击杀玩家
        if (isCloseEnoughToKill && GameDataManger.isBlinking)
        {
            KillPlayer();
        }
    }

    // 移动逻辑
    void Move()
    {
        float currentSpeed = normalSpeed;
        
        // 如果正在眨眼，使用高速移动或瞬移
        if (GameDataManger.isBlinking)
        {
            // 随机选择瞬移或高速移动
            if (Random.value < 0.3f && !justTeleported)
            {
                Teleport();
                justTeleported = true;
                return;
            }
            currentSpeed = blinkSpeed;
            justTeleported = false;
        }
        else
        {
            justTeleported = false;
        }
        
        // 计算移动方向
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0f;  // 保持在地面
        
        // 移动
        transform.Translate(direction * currentSpeed * Time.deltaTime, Space.World);
        
    }

    // 瞬移
    void Teleport()
    {
        // 计算瞬移方向（朝向玩家）
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0f;
        
        // 瞬移到玩家附近
        Vector3 newPosition = playerTransform.position - direction * teleportDistance;
        
        // 确保瞬移位置在地面上
        newPosition.y = transform.position.y;
        
        // 检测瞬移位置是否有障碍物
        if (!IsPositionBlocked(newPosition))
        {
            // 设置新位置
            transform.position = newPosition;
        }
        else
        {
            // 如果瞬移位置被阻挡，改为高速移动
            justTeleported = false;
        }
    }
    
    // 检测位置是否被阻挡
    bool IsPositionBlocked(Vector3 position)
    {
        // 使用SphereCast检测位置是否有障碍物
        float checkRadius = 1f;
        Collider[] hitColliders = Physics.OverlapSphere(position, checkRadius);
        
        foreach (Collider collider in hitColliders)
        {
            // 忽略玩家和自身
            if (collider.transform != playerTransform && 
                collider.transform != transform &&
                !collider.transform.IsChildOf(playerTransform) &&
                !collider.transform.IsChildOf(transform))
            {
                return true;
            }
        }
        
        return false;
    }

    // 检测玩家是否背对
    bool IsPlayerBack()
    {
        if (playerTransform == null) return false;
        
        // 计算玩家到SCP-173的方向
        Vector3 toSCP = transform.position - playerTransform.position;
        toSCP.y = 0f;
        toSCP.Normalize();
        
        // 计算玩家正前方方向
        Vector3 playerForward = playerTransform.forward;
        playerForward.y = 0f;
        playerForward.Normalize();
        
        // 计算夹角
        float angle = Vector3.Angle(playerForward, toSCP);
        
        // 如果夹角大于背对检测角度，说明玩家背对
        return angle > backDetectionAngle;
    }

    // 击杀玩家
    void KillPlayer()
    {
        GameDataManger.isGameOver = true;
        
        // 播放玩家死亡动画
        Animator playerAnim = player.GetComponent<Animator>();
        if (playerAnim != null && !string.IsNullOrEmpty(deathAnimationName))
        {
            playerAnim.Play(deathAnimationName);
        }
        
        // 停止玩家控制
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // 显示死亡提示
        Debug.Log("玩家被SCP-173杀死了！");
    }
}
