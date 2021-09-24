using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;


namespace LogInsights.Helpers
{
    public class TreeNodeCollectionHelper
    {
        public static TreeNode CreateNode(TreeNodeCollection tnc, string fullname, string display, string imageKeyname, Color bgCol)
        {
            if (tnc == null)
                throw new ArgumentNullException("TreeNodeCollection tnc");

            if (string.IsNullOrEmpty(fullname))
                throw new ArgumentNullException("fullname");


            string[] fullpathparts = fullname.Split('/');

            if (fullpathparts.Length == 0)
                throw new ArgumentException($"wrong fullname {fullname}");

#if DEBUG
            display = $"{display} ({fullname})";
#endif


            TreeNode tn = new TreeNode()
            {
                BackColor = bgCol,
                ImageKey = imageKeyname,
                SelectedImageKey = imageKeyname,
                Text = display,
                Name = fullname
            };


            if (fullpathparts.Length == 1)
                tnc.Add(tn);
            else
            {
                string parentNodeFullName = string.Join("/", fullpathparts.Take(fullpathparts.Length - 1));
                string topNodeName = fullpathparts[0];

                TreeNode parentNode = FindNode(tnc, topNodeName, parentNodeFullName);
                if (parentNode == null)
                    throw new ArgumentException($"there is no such element in the TreeNodeCollection: {parentNodeFullName}");

                parentNode.Nodes.Add(tn);
            }

            return tn;
        }

        private static TreeNode FindNode(TreeNodeCollection tnc, string firstLevelName, string fullname)
        {
            TreeNode res = null;

            if (tnc.ContainsKey(firstLevelName))
            {
                res = tnc[firstLevelName];

                foreach (TreeNode tn in tnc[firstLevelName].Nodes)
                {
                    var res2 = FindNode(tn, fullname);
                    if (res2 != null)
                        return (res2);
                }
            }

            return (res);
        }

        private static TreeNode FindNode(TreeNode tn, string locator)
        {
            if (tn.Name == locator)
                return (tn);

            foreach(TreeNode child in tn.Nodes)
            {
                if (child.Name == locator)
                    return child;

                var subres = FindNode(child, locator);

                if (subres != null)
                    return (subres);
            }

            return null;
        }
    }
}
