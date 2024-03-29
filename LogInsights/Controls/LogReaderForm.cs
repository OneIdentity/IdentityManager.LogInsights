﻿using LogInsights.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogInsights.LogReader;

namespace LogInsights.Controls
{
    public partial class LogReaderForm : Form
    {
        public LogReaderForm()
        {
            InitializeComponent();

            CheckValid();
        }

        public string ConnectionString
        {
            get { return SelectedProvider.ConnectionString; }
        }


        public ILogReader ConnectToReader()
        {
            return SelectedProvider.ConnectToReader();
        }

        private LogReaderControl SelectedProvider
        {
            get
            {
                if (tabControl.SelectedIndex == 0)
                    return ctlLogFile;

                if (tabControl.SelectedIndex == 1)
                    return ctlAppInsights;

                throw new Exception("Invalid provider selection.");
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckValid();
        }

        private void CheckValid()
        {
            try
            {
                btnOk.Enabled = SelectedProvider.IsValid;
            }
            catch (Exception e)
            {
                ExceptionHandler.Instance.HandleException(e);
            }
        }

        private void SelectedProvider_IsValidChanged(object sender, EventArgs e)
        {
           CheckValid();
        }

        public void StoreCredentials()
        {
            SelectedProvider.StoreCredentials();
        }
    }
}
