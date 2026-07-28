namespace DefaultNamespace
{
    public class ItemData
    {
        public int id;
        public string name;
        public string icon;
        public string inventoryType;
        public string equipType;
        public string sale;
        public string starLeve;
        public string quality;
        public string damage;
        public string hp;
        public string power;
        public string Des;
        public int Itemnum;

        public ItemData(int id, string name, string icon, string inventoryType, string equipType, string sale, string starLeve, string quality, string damage, string hp, string power, string des, int itemnum)
        {
            this.id = id;
            this.name = name;
            this.icon = icon;
            this.inventoryType = inventoryType;
            this.equipType = equipType;
            this.sale = sale;
            this.starLeve = starLeve;
            this.quality = quality;
            this.damage = damage;
            this.hp = hp;
            this.power = power;
            Des = des;
            Itemnum = itemnum;
        }
    }
}