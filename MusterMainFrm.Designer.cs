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
            this.bandDetails = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // Connect
            // 
            this.Connect.Location = new System.Drawing.Point(293, 241);
            this.Connect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Connect.Name = "Connect";
            this.Connect.Size = new System.Drawing.Size(164, 30);
            this.Connect.TabIndex = 6;
            this.Connect.Text = "Connect";
            this.Connect.UseVisualStyleBackColor = true;
            this.Connect.Click += new System.EventHandler(this.Connect_Click);
            // 
            // Disconnect
            // 
            this.Disconnect.Location = new System.Drawing.Point(667, 241);
            this.Disconnect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Disconnect.Name = "Disconnect";
            this.Disconnect.Size = new System.Drawing.Size(164, 30);
            this.Disconnect.TabIndex = 7;
            this.Disconnect.Text = "Disconnect";
            this.Disconnect.UseVisualStyleBackColor = true;
            this.Disconnect.Click += new System.EventHandler(this.Disconnect_Click);
            // 
            // Test
            // 
            this.Test.Location = new System.Drawing.Point(480, 241);
            this.Test.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Test.Name = "Test";
            this.Test.Size = new System.Drawing.Size(164, 30);
            this.Test.TabIndex = 8;
            this.Test.Text = "Test connection";
            this.Test.UseVisualStyleBackColor = true;
            this.Test.Click += new System.EventHandler(this.Test_Click);
            // 
            // bandID
            // 
            this.bandID.Location = new System.Drawing.Point(99, 31);
            this.bandID.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bandID.Name = "bandID";
            this.bandID.Size = new System.Drawing.Size(100, 22);
            this.bandID.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 17);
            this.label2.TabIndex = 10;
            this.label2.Text = "Band ID";
            // 
            // JoinBand
            // 
            this.JoinBand.Location = new System.Drawing.Point(225, 27);
            this.JoinBand.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.JoinBand.Name = "JoinBand";
            this.JoinBand.Size = new System.Drawing.Size(164, 30);
            this.JoinBand.TabIndex = 11;
            this.JoinBand.Text = "Join/refresh band";
            this.JoinBand.UseVisualStyleBackColor = true;
            this.JoinBand.Click += new System.EventHandler(this.JoinBand_Click);
            // 
            // MakeNewBand
            // 
            this.MakeNewBand.Location = new System.Drawing.Point(414, 27);
            this.MakeNewBand.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MakeNewBand.Name = "MakeNewBand";
            this.MakeNewBand.Size = new System.Drawing.Size(164, 30);
            this.MakeNewBand.TabIndex = 12;
            this.MakeNewBand.Text = "Make new band";
            this.MakeNewBand.UseVisualStyleBackColor = true;
            this.MakeNewBand.Click += new System.EventHandler(this.MakeNewBand_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 180);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 17);
            this.label3.TabIndex = 13;
            this.label3.Text = "Name";
            // 
            // NameInput
            // 
            this.NameInput.AcceptsTab = true;
            this.NameInput.Location = new System.Drawing.Point(99, 176);
            this.NameInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.NameInput.Name = "NameInput";
            this.NameInput.Size = new System.Drawing.Size(100, 22);
            this.NameInput.TabIndex = 14;
            this.NameInput.Text = "Frank";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 213);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 17);
            this.label4.TabIndex = 15;
            this.label4.Text = "Location";
            // 
            // LocationInput
            // 
            this.LocationInput.AcceptsTab = true;
            this.LocationInput.Location = new System.Drawing.Point(99, 209);
            this.LocationInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.LocationInput.Name = "LocationInput";
            this.LocationInput.Size = new System.Drawing.Size(100, 22);
            this.LocationInput.TabIndex = 16;
            this.LocationInput.Text = "Cambridge";
            // 
            // ContactServer
            // 
            this.ContactServer.Location = new System.Drawing.Point(605, 26);
            this.ContactServer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ContactServer.Name = "ContactServer";
            this.ContactServer.Size = new System.Drawing.Size(164, 30);
            this.ContactServer.TabIndex = 17;
            this.ContactServer.Text = "Contact server";
            this.ContactServer.UseVisualStyleBackColor = true;
            this.ContactServer.Click += new System.EventHandler(this.ContactServer_Click);
            // 
            // bandDetails
            // 
            this.bandDetails.FormattingEnabled = true;
            this.bandDetails.ItemHeight = 16;
            this.bandDetails.Location = new System.Drawing.Point(293, 276);
            this.bandDetails.MultiColumn = true;
            this.bandDetails.Name = "bandDetails";
            this.bandDetails.Size = new System.Drawing.Size(538, 228);
            this.bandDetails.TabIndex = 18;
            // 
            // Muster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(903, 571);
            this.Controls.Add(this.bandDetails);
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
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
        private System.Windows.Forms.Button ContactServer;
        private System.Windows.Forms.ListBox bandDetails;
    }
}
