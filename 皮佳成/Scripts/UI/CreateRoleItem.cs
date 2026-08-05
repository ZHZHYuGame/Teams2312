using Games;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///   
/// </summary>
public class CreateRoleItem : MonoBehaviour
{
    public Text name_text, level_text, job_text;
    CreateRoleData mydata;
    [SerializeField] GameObject tips;
    public void Init(CreateRoleData createRoleData)
    {
        mydata = createRoleData;
        gameObject.SetActive(createRoleData != null);
        if (createRoleData != null)
        {
            name_text.text = createRoleData.RoleName;
            level_text.text = createRoleData.Level.ToString();
            tips.gameObject.SetActive(!createRoleData.IsCreated);
            switch ((RoleJobType)createRoleData.JobType)
            {
                case RoleJobType.none:
                    job_text.text = "无";
                    break;
                case RoleJobType.Warrior:
                    job_text.text = "战士";
                    break;
                case RoleJobType.Mage:
                    job_text.text = "法师";
                    break;
                case RoleJobType.Assassin:
                    job_text.text = "刺客";
                    break;
            }
        }
    }

    /// <summary>
    /// 刷新
    /// </summary>
    public void Refresh(bool flag)
    {
        if (flag)
        {
            if (mydata != null)
            {
                Debug.Log(mydata.IsCreated);
                if (mydata.IsCreated)
                {
                    CreateRole.ins.Refresh(mydata);
                }
                else
                {
                    CreateRole.ins.OpenSelectClass();
                }
            }
        }

    }
}
