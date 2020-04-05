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
            this.components = new System.ComponentModel.Container();
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
            this.abelConnect = new System.Windows.Forms.Timer(this.components);
            this.abelConnectLabel = new System.Windows.Forms.Label();
            this.bandDetails = new System.Windows.Forms.DataGridView();
            this.NameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LocationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StatusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.bandDetails)).BeginInit();
            this.SuspendLayout();
            // 
            // Connect
            // 
            this.Connect.Location = new System.Drawing.Point(30, 179);
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
            this.Disconnect.Location = new System.Drawing.Point(310, 179);
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
            this.Test.Location = new System.Drawing.Point(170, 179);
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
            this.bandID.Location = new System.Drawing.Point(74, 25);
            this.bandID.Margin = new System.Windows.Forms.Padding(2);
            this.bandID.Name = "bandID";
            this.bandID.Size = new System.Drawing.Size(76, 20);
            this.bandID.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 28);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Band ID";
            // 
            // JoinBand
            // 
            this.JoinBand.Location = new System.Drawing.Point(169, 22);
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
            this.MakeNewBand.Location = new System.Drawing.Point(310, 22);
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
            this.label3.Location = new System.Drawing.Point(10, 56);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Name";
            // 
            // NameInput
            // 
            this.NameInput.AcceptsTab = true;
            this.NameInput.Location = new System.Drawing.Point(74, 53);
            this.NameInput.Margin = new System.Windows.Forms.Padding(2);
            this.NameInput.Name = "NameInput";
            this.NameInput.Size = new System.Drawing.Size(76, 20);
            this.NameInput.TabIndex = 14;
            this.NameInput.Text = "Frank";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 83);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Location";
            // 
            // LocationInput
            // 
            this.LocationInput.AcceptsTab = true;
            this.LocationInput.Location = new System.Drawing.Point(74, 80);
            this.LocationInput.Margin = new System.Windows.Forms.Padding(2);
            this.LocationInput.Name = "LocationInput";
            this.LocationInput.Size = new System.Drawing.Size(76, 20);
            this.LocationInput.TabIndex = 16;
            // 
            // abelConnect
            // 
            this.abelConnect.Enabled = true;
            this.abelConnect.Interval = 1000;
            this.abelConnect.Tick += new System.EventHandler(this.AbelConnect_Tick);
            // 
            // abelConnectLabel
            // 
            this.abelConnectLabel.AutoSize = true;
            this.abelConnectLabel.Location = new System.Drawing.Point(175, 57);
            this.abelConnectLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.abelConnectLabel.Name = "abelConnectLabel";
            this.abelConnectLabel.Size = new System.Drawing.Size(35, 13);
            this.abelConnectLabel.TabIndex = 19;
            this.abelConnectLabel.Text = "label1";
            // 
            // bandDetails
            // 
            this.bandDetails.AllowUserToAddRows = false;
            this.bandDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.bandDetails.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NameColumn,
            this.LocationColumn,
            this.StatusColumn});
            this.bandDetails.Location = new System.Drawing.Point(30, 216);
            this.bandDetails.Name = "bandDetails";
            this.bandDetails.RowHeadersVisible = false;
            this.bandDetails.Size = new System.Drawing.Size(403, 206);
            this.bandDetails.TabIndex = 20;
            // 
            // NameColumn
            // 
            this.NameColumn.HeaderText = "Name";
            this.NameColumn.Name = "NameColumn";
            this.NameColumn.ReadOnly = true;
            this.NameColumn.Width = 130;
            // 
            // LocationColumn
            // 
            this.LocationColumn.HeaderText = "Location";
            this.LocationColumn.Name = "LocationColumn";
            this.LocationColumn.ReadOnly = true;
            this.LocationColumn.Width = 150;
            // 
            // StatusColumn
            // 
            this.StatusColumn.HeaderText = "Status";
            this.StatusColumn.Name = "StatusColumn";
            this.StatusColumn.ReadOnly = true;
            this.StatusColumn.Width = 120;
            // 
            // Muster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 464);
            this.Controls.Add(this.bandDetails);
            this.Controls.Add(this.abelConnectLabel);
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
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Muster";
            this.Text = "Muster";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Muster_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button Connect;
        private System.Windows.Forms.Button Disconnect;
        private System.Windows.Forms.Button Test;
        private System.Windows.Forms.TextBox bandID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button JoinBand;
        private System.Windows.Forms.Button MakeNewBand;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox NameInput;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox LocationInput;
        private System.Windows.Forms.Timer abelConnect;
        private System.Windows.Forms.Label abelConnectLabel;
        private System.Windows.Forms.DataGridView bandDetails;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn LocationColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn StatusColumn;
    }
}
