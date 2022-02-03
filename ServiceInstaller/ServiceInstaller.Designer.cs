
namespace ServiceInstaller
{
    partial class ServiceInstaller
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.butInstall = new System.Windows.Forms.Button();
            this.butStart = new System.Windows.Forms.Button();
            this.butStop = new System.Windows.Forms.Button();
            this.butUninstall = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // butInstall
            // 
            this.butInstall.Location = new System.Drawing.Point(12, 12);
            this.butInstall.Name = "butInstall";
            this.butInstall.Size = new System.Drawing.Size(199, 47);
            this.butInstall.TabIndex = 0;
            this.butInstall.Text = "Install";
            this.butInstall.UseVisualStyleBackColor = true;
            this.butInstall.Click += new System.EventHandler(this.butInstall_Click);
            // 
            // butStart
            // 
            this.butStart.Location = new System.Drawing.Point(234, 12);
            this.butStart.Name = "butStart";
            this.butStart.Size = new System.Drawing.Size(199, 47);
            this.butStart.TabIndex = 1;
            this.butStart.Text = "Start";
            this.butStart.UseVisualStyleBackColor = true;
            this.butStart.Click += new System.EventHandler(this.butStart_Click);
            // 
            // butStop
            // 
            this.butStop.Location = new System.Drawing.Point(453, 12);
            this.butStop.Name = "butStop";
            this.butStop.Size = new System.Drawing.Size(199, 47);
            this.butStop.TabIndex = 2;
            this.butStop.Text = "Stop";
            this.butStop.UseVisualStyleBackColor = true;
            this.butStop.Click += new System.EventHandler(this.butStop_Click);
            // 
            // butUninstall
            // 
            this.butUninstall.Location = new System.Drawing.Point(671, 12);
            this.butUninstall.Name = "butUninstall";
            this.butUninstall.Size = new System.Drawing.Size(199, 47);
            this.butUninstall.TabIndex = 3;
            this.butUninstall.Text = "Uninstall";
            this.butUninstall.UseVisualStyleBackColor = true;
            this.butUninstall.Click += new System.EventHandler(this.butUninstall_Click);
            // 
            // ServiceInstaller
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(885, 77);
            this.Controls.Add(this.butUninstall);
            this.Controls.Add(this.butStop);
            this.Controls.Add(this.butStart);
            this.Controls.Add(this.butInstall);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ServiceInstaller";
            this.Text = "Service Installer";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button butInstall;
        private System.Windows.Forms.Button butStart;
        private System.Windows.Forms.Button butStop;
        private System.Windows.Forms.Button butUninstall;
    }
}

