using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

/// <summary>
/// 配置表
/// </summary>
public class UserSQLMgr : Singleton<UserSQLMgr>
{
    List<User> users;

    string path;

    public void Start()
    {
        users = new List<User>();
        path = Path.Combine("User");
        LoadFile();
    }

    /// <summary>
    /// 存账号
    /// </summary>
    /// <param name="user"></param>
    public void SaveFile(User user)
    {
        if (Find(user.account) == null)
        {
            string[] strings = { $"{user.account}/{user.password}" };
            if (!Directory.Exists(path)) //如果不存在 该路径 就
            {
                Directory.CreateDirectory(path);
            }

            File.AppendAllLines("User/User.txt", strings);
            LoadFile();
        }
    }

    /// <summary>
    /// 查找
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    public User Find(string account)
    {
        if (users.Count == 0)
        {
            return null;
        }
        return users.Find(x => x.account == account);
    }

    /// <summary>
    /// 加载 用户 数据
    /// </summary>
    /// <returns></returns>
    public void LoadFile()
    {
        users.Clear(); //先清空数据
        if (Directory.Exists(path)) //如果不存在 该路径 就
        {
            string[] str = File.ReadAllLines("User/User.txt");
            for (int i = 0; i < str.Length; i++)
            {
                string[] arr = str[i].Split('/');
                users.Add(new User(arr[0], arr[1]));
            }
        }
    }
}

/// <summary>
/// 用户 数据
/// </summary>
public class User
{
    /// <summary>
    /// 角色 id
    /// </summary>
    public int GUID;

    /// <summary>
    /// 用户名
    /// </summary>
    public string account;

    /// <summary>
    /// 用户密码
    /// </summary>
    public string password;

    public User(string account, string password)
    {
        this.account = account;
        this.password = password;
    }
}