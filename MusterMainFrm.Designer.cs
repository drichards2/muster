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
            this.GoBells = new System.Windows.Forms.Button();
            this.holePunchIP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.GoInternet = new System.Windows.Forms.Button();
            this.holePunchPort = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // GoBells
            // 
            this.GoBells.Location = new System.Drawing.Point(316, 136);
            this.GoBells.Name = "GoBells";
            this.GoBells.Size = new System.Drawing.Size(75, 23);
            this.GoBells.TabIndex = 0;
            this.GoBells.Text = "Go Bells";
            this.GoBells.UseVisualStyleBackColor = true;
            this.GoBells.Click += new System.EventHandler(this.Go_Click);
            // 
            // holePunchIP
            // 
            this.holePunchIP.Location = new System.Drawing.Point(189, 229);
            this.holePunchIP.Name = "holePunchIP";
            this.holePunchIP.Size = new System.Drawing.Size(100, 22);
            this.holePunchIP.TabIndex = 1;
            this.holePunchIP.Text = "54.154.238.252";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(103, 229);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "HolePunch";
            // 
            // GoInternet
            // 
            this.GoInternet.Location = new System.Drawing.Point(189, 277);
            this.GoInternet.Name = "GoInternet";
            this.GoInternet.Size = new System.Drawing.Size(100, 23);
            this.GoInternet.TabIndex = 3;
            this.GoInternet.Text = "Holepunch";
            this.GoInternet.UseVisualStyleBackColor = true;
            this.GoInternet.Click += new System.EventHandler(this.GoInternet_Click);
            // 
            // holePunchPort
            // 
            this.holePunchPort.Location = new System.Drawing.Point(189, 249);
            this.holePunchPort.Name = "holePunchPort";
            this.holePunchPort.Size = new System.Drawing.Size(100, 22);
            this.holePunchPort.TabIndex = 4;
            this.holePunchPort.Text = "51243";
            // 
            // Muster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.holePunchPort);
            this.Controls.Add(this.GoInternet);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.holePunchIP);
            this.Controls.Add(this.GoBells);
            this.KeyPreview = true;
            this.Name = "Muster";
            this.Text = "Muster";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Muster_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button GoBells;
        private System.Windows.Forms.TextBox holePunchIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button GoInternet;
        private System.Windows.Forms.TextBox holePunchPort;
    }
}

