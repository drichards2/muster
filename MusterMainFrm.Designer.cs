namespace Muster
{
    partial class Muster
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Holepunch = new System.Windows.Forms.Button();
            this.holePunchIP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.holePunchPort = new System.Windows.Forms.TextBox();
            this.connectionList = new System.Windows.Forms.DataGridView();
            this.IP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Port = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Connect = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.connectionList)).BeginInit();
            this.SuspendLayout();
            // 
            // Holepunch
            // 
            this.Holepunch.Location = new System.Drawing.Point(262, 302);
            this.Holepunch.Name = "Holepunch";
            this.Holepunch.Size = new System.Drawing.Size(164, 29);
            this.Holepunch.TabIndex = 0;
            this.Holepunch.Text = "Holepunch";
            this.Holepunch.UseVisualStyleBackColor = true;
            this.Holepunch.Click += new System.EventHandler(this.Go_Click);
            // 
            // holePunchIP
            // 
            this.holePunchIP.Location = new System.Drawing.Point(109, 302);
            this.holePunchIP.Name = "holePunchIP";
            this.holePunchIP.Size = new System.Drawing.Size(100, 22);
            this.holePunchIP.TabIndex = 1;
            this.holePunchIP.Text = "54.154.238.252";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 302);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "HolePunch";
            // 
            // holePunchPort
            // 
            this.holePunchPort.Location = new System.Drawing.Point(109, 322);
            this.holePunchPort.Name = "holePunchPort";
            this.holePunchPort.Size = new System.Drawing.Size(100, 22);
            this.holePunchPort.TabIndex = 4;
            this.holePunchPort.Text = "51243";
            // 
            // connectionList
            // 
            this.connectionList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.connectionList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.IP,
            this.Port,
            this.Status});
            this.connectionList.Location = new System.Drawing.Point(26, 12);
            this.connectionList.Name = "connectionList";
            this.connectionList.RowHeadersWidth = 51;
            this.connectionList.RowTemplate.Height = 24;
            this.connectionList.Size = new System.Drawing.Size(745, 204);
            this.connectionList.TabIndex = 5;
            // 
            // IP
            // 
            this.IP.HeaderText = "IP";
            this.IP.MinimumWidth = 6;
            this.IP.Name = "IP";
            this.IP.Width = 125;
            // 
            // Port
            // 
            this.Port.HeaderText = "Port";
            this.Port.MinimumWidth = 6;
            this.Port.Name = "Port";
            this.Port.Width = 125;
            // 
            // Status
            // 
            this.Status.HeaderText = "Status";
            this.Status.MinimumWidth = 6;
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            this.Status.Width = 125;
            // 
            // Connect
            // 
            this.Connect.Location = new System.Drawing.Point(468, 302);
            this.Connect.Name = "Connect";
            this.Connect.Size = new System.Drawing.Size(164, 29);
            this.Connect.TabIndex = 6;
            this.Connect.Text = "Connect";
            this.Connect.UseVisualStyleBackColor = true;
            this.Connect.Click += new System.EventHandler(this.Connect_Click);
            // 
            // Muster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Connect);
            this.Controls.Add(this.connectionList);
            this.Controls.Add(this.holePunchPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.holePunchIP);
            this.Controls.Add(this.Holepunch);
            this.Name = "Muster";
            this.Text = "Muster";
            ((System.ComponentModel.ISupportInitialize)(this.connectionList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Holepunch;
        private System.Windows.Forms.TextBox holePunchIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox holePunchPort;
        private System.Windows.Forms.DataGridView connectionList;
        private System.Windows.Forms.DataGridViewTextBoxColumn IP;
        private System.Windows.Forms.DataGridViewTextBoxColumn Port;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.Button Connect;
    }
}

