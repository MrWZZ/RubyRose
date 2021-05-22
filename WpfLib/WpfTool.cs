using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfLib
{
    public class WpfTool
    {
        #region 单例

        private WpfTool() { }
        private static WpfTool instance;
        public static WpfTool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WpfTool();
                }
                return instance;
            }
        }

        #endregion

        /// <summary>  
        /// 递归查找该元素下所有指定类型的元素
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="obj"></param>  
        /// <returns></returns>  
        public List<T> GetItemsInChildren<T>(DependencyObject obj) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);

                if (child is T)
                {
                    childList.Add((T)child);
                }
                childList.AddRange(GetItemsInChildren<T>(child));
            }
            return childList;
        }

        /// <summary>
        /// 递归查找该元素下所有指定类型、指定名称的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<T> GetItemsInChildren<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);

                var typeChild = child as T;
                if (typeChild != null && typeChild.Name == name)
                {
                    childList.Add((T)child);
                }
                childList.AddRange(GetItemsInChildren<T>(child));
            }
            return childList;
        }

        /// <summary>
        /// 获取该元素下的子元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public List<T> GetChild<T>(DependencyObject obj) where T : FrameworkElement
        {
            List<T> childList = new List<T>();

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                {
                    childList.Add((T)child);
                }
            }
            return childList;
        }
    }
}
