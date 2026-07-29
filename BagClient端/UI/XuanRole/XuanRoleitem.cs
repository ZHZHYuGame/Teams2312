using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;

public class XuanRoleitem : MonoBehaviour
{
    private RoleData myData;
    [SerializeField] Text roleName;
    public void Init(RoleData Data)
    {
        if (Data == null)
        {
            return;
        }
        myData = Data;
        roleName.text=Data.RoleName;
        gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        gameObject.GetComponent<Button>().onClick.AddListener((() =>
        {
            for (int i = 0; i < GameObject.Find("CreateRole").transform.childCount; i++)
            {
                Destroy(GameObject.Find("CreateRole").transform.GetChild(i).gameObject);
            }
            GameObject go=GameObject.Instantiate(Resources.Load<GameObject>(Data.Path),GameObject.Find("CreateRole").transform);
            go.name = "Player";
            NetManager.GetInstance().RoleGuid = Data.Roleid;
            
        }));
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
