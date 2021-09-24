using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LogInsights.Helpers
{
    public static class GuiHelper
    {
        public static void SetGuiSave(Control GuiElem, Action GuiAction)
        {
         if (GuiElem.InvokeRequired)
                GuiElem.BeginInvoke(GuiAction);
            else
            {
                try
                {
                    GuiAction.Invoke();                    
                }
                catch (InvalidOperationException E)
                {
                    if (string.Compare(E.Message, "Thread", true) >= 0)
                        GuiElem.BeginInvoke(GuiAction);
                    else
                        throw;
                }
                catch
                {
                    throw;
                }
            }
        }
    }

    public class DropdownItem<T>
    {
        public T data;
        public string display;

        public DropdownItem(string display, T data)
        {
            this.display = display;
            this.data = data;
        }

        public override string ToString()
        {
            return display;
        }
    }


}
