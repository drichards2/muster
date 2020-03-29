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
            this.holePunchIP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.holePunchPort = new System.Windows.Forms.TextBox();
            this.connectionList = new System.Windows.Forms.DataGridView();
            this.NameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LocationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IPColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PortColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StatusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Connect = new System.Windows.Forms.Button();
            this.Disconnect = new System.Windows.Forms.Button();
            this.Test = new System.Windows.Forms.Button();
            this.bandID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.JoinBand = new System.Windows.Forms.Button();
            this.MakeNewBand = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.NameInput = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.LocationInput = new System.Windows.Forms.TextBox();
            this.ContactServer = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.connectionList)).BeginInit();
            this.SuspendLayout();
            // 
            // holePunchIP
            // 
            this.holePunchIP.Location = new System.Drawing.Point(82, 245);
            this.holePunchIP.Margin = new System.Windows.Forms.Padding(2);
            this.holePunchIP.Name = "holePunchIP";
            this.holePunchIP.Size = new System.Drawing.Size(76, 20);
            this.holePunchIP.TabIndex = 1;
            this.holePunchIP.Text = "54.154.238.252";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 245);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "HolePunch";
            // 
            // holePunchPort
            // 
            this.holePunchPort.Location = new System.Drawing.Point(82, 262);
            this.holePunchPort.Margin = new System.Windows.Forms.Padding(2);
            this.holePunchPort.Name = "holePunchPort";
            this.holePunchPort.Size = new System.Drawing.Size(76, 20);
            this.holePunchPort.TabIndex = 4;
            this.holePunchPort.Text = "51243";
            // 
            // connectionList
            // 
            this.connectionList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.connectionList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NameColumn,
            this.LocationColumn,
            this.IPColumn,
            this.PortColumn,
            this.StatusColumn});
            this.connectionList.Location = new System.Drawing.Point(11, 11);
            this.connectionList.Margin = new System.Windows.Forms.Padding(2);
            this.connectionList.Name = "connectionList";
            this.connectionList.RowHeadersWidth = 51;
            this.connectionList.RowTemplate.Height = 24;
            this.connectionList.Size = new System.Drawing.Size(653, 166);
            this.connectionList.TabIndex = 5;
            // 
            // NameColumn
            // 
            this.NameColumn.HeaderText = "Name";
            this.NameColumn.MinimumWidth = 6;
            this.NameColumn.Name = "NameColumn";
            this.NameColumn.Width = 125;
            // 
            // LocationColumn
            // 
            this.LocationColumn.HeaderText = "Location";
            this.LocationColumn.MinimumWidth = 6;
            this.LocationColumn.Name = "LocationColumn";
            this.LocationColumn.Width = 125;
            // 
            // IPColumn
            // 
            this.IPColumn.HeaderText = "IP";
            this.IPColumn.MinimumWidth = 6;
            this.IPColumn.Name = "IPColumn";
            this.IPColumn.Width = 125;
            // 
            // PortColumn
            // 
            this.PortColumn.HeaderText = "Port";
            this.PortColumn.MinimumWidth = 6;
            this.PortColumn.Name = "PortColumn";
            // 
            // StatusColumn
            // 
            this.StatusColumn.HeaderText = "Status";
            this.StatusColumn.MinimumWidth = 6;
            this.StatusColumn.Name = "StatusColumn";
            this.StatusColumn.ReadOnly = true;
            this.StatusColumn.Width = 125;
            // 
            // Connect
            // 
            this.Connect.Location = new System.Drawing.Point(176, 245);
            this.Connect.Margin = new System.Windows.Forms.Padding(2);
            this.Connect.Name = "Connect";
            this.Connect.Size = new System.Drawing.Size(123, 24);
            this.Connect.TabIndex = 6;
            this.Connect.Text = "Connect";
            this.Connect.UseVisualStyleBackColor = true;
            this.Connect.Click += new System.EventHandler(this.Connect_Click);
            // 
            // Disconnect
            // 
            this.Disconnect.Location = new System.Drawing.Point(461, 245);
            this.Disconnect.Margin = new System.Windows.Forms.Padding(2);
            this.Disconnect.Name = "Disconnect";
            this.Disconnect.Size = new System.Drawing.Size(123, 24);
            this.Disconnect.TabIndex = 7;
            this.Disconnect.Text = "Disconnect";
            this.Disconnect.UseVisualStyleBackColor = true;
            this.Disconnect.Click += new System.EventHandler(this.Disconnect_Click);
            // 
            // Test
            // 
            this.Test.Location = new System.Drawing.Point(318, 245);
            this.Test.Margin = new System.Windows.Forms.Padding(2);
            this.Test.Name = "Test";
            this.Test.Size = new System.Drawing.Size(123, 24);
            this.Test.TabIndex = 8;
            this.Test.Text = "Test connection";
            this.Test.UseVisualStyleBackColor = true;
            this.Test.Click += new System.EventHandler(this.Test_Click);
            // 
            // bandID
            // 
            this.bandID.Location = new System.Drawing.Point(82, 194);
            this.bandID.Margin = new System.Windows.Forms.Padding(2);
            this.bandID.Name = "bandID";
            this.bandID.Size = new System.Drawing.Size(76, 20);
            this.bandID.TabIndex = 9;
            this.bandID.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 197);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Band ID";
            // 
            // JoinBand
            // 
            this.JoinBand.Location = new System.Drawing.Point(176, 191);
            this.JoinBand.Margin = new System.Windows.Forms.Padding(2);
            this.JoinBand.Name = "JoinBand";
            this.JoinBand.Size = new System.Drawing.Size(123, 24);
            this.JoinBand.TabIndex = 11;
            this.JoinBand.Text = "Join/refresh band";
            this.JoinBand.UseVisualStyleBackColor = true;
            this.JoinBand.Click += new System.EventHandler(this.JoinBand_Click);
            // 
            // MakeNewBand
            // 
            this.MakeNewBand.Location = new System.Drawing.Point(318, 191);
            this.MakeNewBand.Margin = new System.Windows.Forms.Padding(2);
            this.MakeNewBand.Name = "MakeNewBand";
            this.MakeNewBand.Size = new System.Drawing.Size(123, 24);
            this.MakeNewBand.TabIndex = 12;
            this.MakeNewBand.Text = "Make new band";
            this.MakeNewBand.UseVisualStyleBackColor = true;
            this.MakeNewBand.Click += new System.EventHandler(this.MakeNewBand_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 315);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Name";
            // 
            // NameInput
            // 
            this.NameInput.AcceptsTab = true;
            this.NameInput.Location = new System.Drawing.Point(82, 312);
            this.NameInput.Margin = new System.Windows.Forms.Padding(2);
            this.NameInput.Name = "NameInput";
            this.NameInput.Size = new System.Drawing.Size(76, 20);
            this.NameInput.TabIndex = 14;
            this.NameInput.Text = "Frank";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 342);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Location";
            // 
            // LocationInput
            // 
            this.LocationInput.AcceptsTab = true;
            this.LocationInput.Location = new System.Drawing.Point(82, 339);
            this.LocationInput.Margin = new System.Windows.Forms.Padding(2);
            this.LocationInput.Name = "LocationInput";
            this.LocationInput.Size = new System.Drawing.Size(76, 20);
            this.LocationInput.TabIndex = 16;
            this.LocationInput.Text = "Cambridge";
            // 
            // ContactServer
            // 
            this.ContactServer.Location = new System.Drawing.Point(461, 190);
            this.ContactServer.Margin = new System.Windows.Forms.Padding(2);
            this.ContactServer.Name = "ContactServer";
            this.ContactServer.Size = new System.Drawing.Size(123, 24);
            this.ContactServer.TabIndex = 17;
            this.ContactServer.Text = "Contact server";
            this.ContactServer.UseVisualStyleBackColor = true;
            this.ContactServer.Click += new System.EventHandler(this.ContactServer_Click);
            // 
            // Muster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(677, 381);
            this.Controls.Add(this.ContactServer);
            this.Controls.Add(this.LocationInput);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.NameInput);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.MakeNewBand);
            this.Controls.Add(this.JoinBand);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.bandID);
            this.Controls.Add(this.Test);
            this.Controls.Add(this.Disconnect);
            this.Controls.Add(this.Connect);
            this.Controls.Add(this.connectionList);
            this.Controls.Add(this.holePunchPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.holePunchIP);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Muster";
            this.Text = "Muster";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Muster_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.connectionList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox holePunchIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox holePunchPort;
        private System.Windows.Forms.DataGridView connectionList;
        private System.Windows.Forms.Button Connect;
        private System.Windows.Forms.Button Disconnect;
        private System.Windows.Forms.Button Test;
        private System.Windows.Forms.TextBox bandID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button JoinBand;
        private System.Windows.Forms.Button MakeNewBand;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn LocationColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn IPColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn PortColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn StatusColumn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox NameInput;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox LocationInput;
        private System.Windows.Forms.Button ContactServer;
    }
}
