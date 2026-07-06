using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public class InventoryManager : NormalSingleton<InventoryManager> 
    {
        private List<InventoryInfo> inventoryInfoList=new List<InventoryInfo>();//存储当前背包拥有的物品的信息
        private Dictionary<int,ItemInfo> itemData=new Dictionary<int, ItemInfo>();//用来存储物品基本信息的数据的容器，根据具体项目要求，可通过SO或者读表方式获取数据
        private int maxCapacity=30;

        public InventoryManager()
        {
            InitItemInfo();
        }

        public int AddItem(int id,int count)
        {
            if(count<=0)
            {
                Debug.Log($"添加的物品数量为{count},违法不符合要求");
                return 0;
            }
            ItemInfo itemInfo=GetItemInfo(id);
            if(itemInfo==null)
            {
                return 0;
            }
            if(itemInfo.CanStack)
            {
                int itemInInventoryIndex = GetItemInInventoryIndex(id);
                if (itemInInventoryIndex != -1)
                {
                    if (itemInfo.CanStack)
                    {
                        inventoryInfoList[itemInInventoryIndex].Count += count;
                    }
                }
                else
                {
                    if (inventoryInfoList.Count >= maxCapacity)
                    {
                        Debug.Log("无法添加物品，背包容量已满");
                        return 0;
                    }
                    inventoryInfoList.Add(new InventoryInfo(itemInfo, count));
                }
                return count;
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    inventoryInfoList.Add(new InventoryInfo(itemInfo, 1));
                    if (inventoryInfoList.Count >= maxCapacity)
                    {
                        Debug.Log($"背包已满，已成功添加{i + 1}件物品，剩余{count - i - 1}件物品无法添加");
                        return i+1;
                    }
                }
                return count;
            }
        }

        public void UseItem(int id)
        {

        }

        public int RemoveItem(int id,int count)
        {
            ItemInfo itemInfo = GetItemInfo(id);
            if (itemInfo == null)
            {
                return 0;
            }
            int itemInInventoryIndex = GetItemInInventoryIndex(id,-1);
            if (itemInInventoryIndex != -1)
            {
                if (itemInfo.CanStack)
                {
                    if(inventoryInfoList[itemInInventoryIndex].Count<count)
                    {
                        int res=inventoryInfoList[itemInInventoryIndex].Count;
                        inventoryInfoList.RemoveAt(itemInInventoryIndex);
                        Debug.Log($"已移除{res}件物品，物品数量已为0无法继续移除");
                        return res;
                    }
                    else
                    {
                        inventoryInfoList[itemInInventoryIndex].Count -= count;
                        return count;
                    }
                }
                else
                {
                    for(int i=0;i<count; i++)
                    {
                        inventoryInfoList.RemoveAt(itemInInventoryIndex);
                        itemInInventoryIndex= GetItemInInventoryIndex(id, -1);
                        if(itemInInventoryIndex == -1)
                        {
                            Debug.Log($"已移除{i + 1}件物品，物品数量已为0无法继续移除");
                            return i+1;
                        }
                    }
                    return count;
                }
            }
            else
            {
                Debug.Log($"在此背包并未找到{itemInfo.Name}，无法移除");
                return 0;
            }
        }

        public ItemInfo GetItemInfo(int id)
        {
            if(itemData.TryGetValue(id, out var info))
            {
                return info;
            }
            else
            {
                Debug.LogWarning($"并未从数据库找到id为{id}的物品信息");
                return null;
            }
        }

        /// <summary>
        /// 根据物品ID来寻找物品在背包中所在的格子
        /// </summary>
        /// <param name="id">物品ID</param>
        /// <param name="sort">参数1为从前往后找，参数-1为从后往前找，默认值为1</param>
        /// <returns></returns>
        public int GetItemInInventoryIndex(int id,int sort=1)
        {
            if (GetItemInfo(id) == null)
            {
                return -1;
            }
            if (sort==-1)
            {
                for(int i=inventoryInfoList.Count-1;i>=0;i--)
                {
                    if (inventoryInfoList[i].Item.Id == id)
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (int i = 0; i < inventoryInfoList.Count; i++)
                {
                    if (inventoryInfoList[i].Item.Id == id)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public void UpdateInventoryUI()
        {

        }

        public void InitItemInfo()
        {

        }

        public void DebugInventory()
        {
            foreach(InventoryInfo info in inventoryInfoList)
            {
                Debug.Log($"ID:{info.Item.Id}  名字：{info.Item.Name}  数量：{info.Count}");
            }
        }
    }

    public class InventoryInfo
    {
        private int count;
        private ItemInfo item;

        public InventoryInfo(ItemInfo item,int count = 1)
        {
            this.count = count;
            this.item = item;
        }

        public int Count { 
            get => count; 
            set {
                count = value < 0 ? 0 : value;
            }
        }
        public ItemInfo Item { get => item;}
    }

    public class ItemInfo
    {
        private int id;
        private string name;
        private string description;
        private Sprite icon;
        private bool canStack;//此物品是否可以堆叠 

        public ItemInfo(int id, string name, string description, Sprite icon, bool canStack)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.icon = icon;
            this.canStack = canStack;
        }

        public int Id { get => id;}
        public string Name { get => name;}
        public string Description { get => description; }
        public Sprite Icon { get => icon; }
        public bool CanStack { get => canStack;}
    }
    
}


