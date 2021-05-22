using CmdLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CmdLike
{
    [DataContract]
    public class Config
    {
        [DataMember]
        /// <summary>
        /// 热键字典
        /// </summary>
        public Dictionary<string, HotkeyData> hotkeyDic = new Dictionary<string, HotkeyData>();

        [DataMember]
        /// <summary>
        /// 自定义cmd快捷字典
        /// </summary>
        public Dictionary<string, CmdData> cmdDic = new Dictionary<string, CmdData>();

        /// <summary>
        /// 预制cmd快捷字典
        /// </summary>
        public Dictionary<string, CmdData> systemDic = new Dictionary<string, CmdData>();

        [DataMember]
        /// <summary>
        /// 是否是开机启动
        /// </summary>
        public bool IsAutoOpen;
    }

    [DataContract]
    public class HotkeyData
    {
        [DataMember]
        // 唯一key, 对应的方法命
        public string funkey;

        [DataMember]
        // 普通键
        public uint key;

        // 功能键
        [DataMember]
        public uint keyFlag;

        // 操作
        public Action operation;

        // 描述
        public string DescriptionBinding { get; set; }

        [DataMember]
        // 按钮显示
        public string KeyBinding { get; set; }

        // 当前注册的原子ID
        public uint atom;

        public HotkeyData() { }

        public HotkeyData(string funName, Action operation)
        {
            this.funkey = funName;
            this.operation = operation;
        }

        /// <summary>
        /// 当前数据是否有效
        /// </summary>
        /// <returns></returns>
        public bool IsVaild()
        {
            return key > 0;
        }
    }

    [DataContract]
    public class CmdData : IComparable<CmdData>
    {
        // 当前是第几个添加的，用于排序
        public int sortIndex;

        // 操作类型
        public CmdCommandType type;

        // 快捷指令
        [DataMember]
        public string KeyBinding { get; set; }

        // 描述
        [DataMember]
        public string DescriptionBinding { get; set; }

        // cmd 命令
        public string cmdCommand;

        // 预设命令
        public Action function;

        public CmdData() { }
        public CmdData(string key)
        {
            KeyBinding = key;
        }

        public void DoCommand()
        {
            switch (type)
            {
                case CmdCommandType.OpenFile:
                    CmdLikeTool.Instance.OpenFile(DescriptionBinding);
                    break;
                case CmdCommandType.CmdCommand:
                    CmdTool.Instance.CallCommand(cmdCommand);
                    break;
                case CmdCommandType.ToolFunction:
                    function.Invoke();
                    break;
            }
        }

        public int CompareTo(CmdData other)
        {
            return other.sortIndex.CompareTo(sortIndex);
        }

        public CmdData Clone()
        {
            return new CmdData()
            {
                sortIndex = sortIndex,
                type = type,
                KeyBinding = KeyBinding,
                DescriptionBinding = DescriptionBinding,
                cmdCommand = cmdCommand,
                function = function
            };
        }

        public bool IsSame(CmdData data)
        {
            return data.sortIndex == sortIndex;
        }
    }

    public enum CmdCommandType
    {
        /// <summary>
        /// 打开文件或文件夹
        /// </summary>
        OpenFile,
        /// <summary>
        /// 执行CMD命令
        /// </summary>
        CmdCommand,
        /// <summary>
        /// 内部预设方法
        /// </summary>
        ToolFunction,
    }
}
