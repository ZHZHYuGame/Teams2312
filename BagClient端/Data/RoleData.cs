namespace DefaultNamespace
{
    public class RoleData
    {
        public string RoleName;
        public string Path;
        public int Roleid;

        public RoleData(string roleName, string path, int roleid)
        {
            RoleName = roleName;
            Path = path;
            Roleid = roleid;
        }
    }
}