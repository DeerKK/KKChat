namespace WindowsFormsApp1
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("");
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.FriendNameLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.SendBox = new System.Windows.Forms.RichTextBox();
            this.fileSystemWatcher1 = new System.IO.FileSystemWatcher();
            this.SendBotton = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.UserNameLabel = new System.Windows.Forms.Label();
            this.SearchBar = new System.Windows.Forms.TextBox();
            this.FriendsFlow = new System.Windows.Forms.FlowLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.ReceiveBox = new System.Windows.Forms.RichTextBox();
            this.button9 = new System.Windows.Forms.Button();
            this.FriendListView = new System.Windows.Forms.ListView();
            this.testColumnHead = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.UDPChatButton = new System.Windows.Forms.Button();
            this.TCPChatButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.pictureBox1.Location = new System.Drawing.Point(272, -8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(615, 91);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // FriendNameLabel
            // 
            this.FriendNameLabel.AutoSize = true;
            this.FriendNameLabel.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.FriendNameLabel.Font = new System.Drawing.Font("微软雅黑", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FriendNameLabel.Location = new System.Drawing.Point(287, 9);
            this.FriendNameLabel.Name = "FriendNameLabel";
            this.FriendNameLabel.Size = new System.Drawing.Size(282, 36);
            this.FriendNameLabel.TabIndex = 1;
            this.FriendNameLabel.Text = "Welcome to KKChat";
            this.FriendNameLabel.Click += new System.EventHandler(this.FriendNameLabel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.label2.Location = new System.Drawing.Point(290, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "状态：在线";
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.pictureBox2.Location = new System.Drawing.Point(272, 406);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(615, 185);
            this.pictureBox2.TabIndex = 6;
            this.pictureBox2.TabStop = false;
            // 
            // SendBox
            // 
            this.SendBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.SendBox.Location = new System.Drawing.Point(272, 456);
            this.SendBox.Name = "SendBox";
            this.SendBox.Size = new System.Drawing.Size(606, 78);
            this.SendBox.TabIndex = 7;
            this.SendBox.Text = "";
            // 
            // fileSystemWatcher1
            // 
            this.fileSystemWatcher1.EnableRaisingEvents = true;
            this.fileSystemWatcher1.SynchronizingObject = this;
            // 
            // SendBotton
            // 
            this.SendBotton.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.SendBotton.Location = new System.Drawing.Point(794, 541);
            this.SendBotton.Name = "SendBotton";
            this.SendBotton.Size = new System.Drawing.Size(71, 25);
            this.SendBotton.TabIndex = 8;
            this.SendBotton.Text = "发送";
            this.SendBotton.UseVisualStyleBackColor = false;
            this.SendBotton.Click += new System.EventHandler(this.SendBotton_Click);
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.button3.Location = new System.Drawing.Point(272, 425);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(71, 25);
            this.button3.TabIndex = 9;
            this.button3.Text = "文件";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            this.pictureBox3.Location = new System.Drawing.Point(7, 6);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(63, 63);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 10;
            this.pictureBox3.TabStop = false;
            // 
            // UserNameLabel
            // 
            this.UserNameLabel.AutoSize = true;
            this.UserNameLabel.Font = new System.Drawing.Font("微软雅黑", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.UserNameLabel.Location = new System.Drawing.Point(76, 13);
            this.UserNameLabel.Name = "UserNameLabel";
            this.UserNameLabel.Size = new System.Drawing.Size(99, 31);
            this.UserNameLabel.TabIndex = 11;
            this.UserNameLabel.Text = "DeerKK";
            this.UserNameLabel.Click += new System.EventHandler(this.UserNameLabel_Click);
            // 
            // SearchBar
            // 
            this.SearchBar.Location = new System.Drawing.Point(7, 75);
            this.SearchBar.Name = "SearchBar";
            this.SearchBar.Size = new System.Drawing.Size(259, 25);
            this.SearchBar.TabIndex = 12;
            this.SearchBar.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // FriendsFlow
            // 
            this.FriendsFlow.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.FriendsFlow.Location = new System.Drawing.Point(7, 137);
            this.FriendsFlow.Name = "FriendsFlow";
            this.FriendsFlow.Size = new System.Drawing.Size(259, 397);
            this.FriendsFlow.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(79, 49);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 15);
            this.label4.TabIndex = 13;
            this.label4.Text = "状态：在线";
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.button4.Location = new System.Drawing.Point(195, 44);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(71, 25);
            this.button4.TabIndex = 14;
            this.button4.Text = "查找";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.button5.Location = new System.Drawing.Point(7, 541);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(108, 25);
            this.button5.TabIndex = 15;
            this.button5.Text = "好友管理";
            this.button5.UseVisualStyleBackColor = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button7
            // 
            this.button7.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.button7.Location = new System.Drawing.Point(7, 106);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(108, 25);
            this.button7.TabIndex = 17;
            this.button7.Text = "单人会话";
            this.button7.UseVisualStyleBackColor = false;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.button8.Location = new System.Drawing.Point(158, 106);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(108, 25);
            this.button8.TabIndex = 18;
            this.button8.Text = "多人会话";
            this.button8.UseVisualStyleBackColor = false;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button6
            // 
            this.button6.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.button6.Location = new System.Drawing.Point(158, 541);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(108, 25);
            this.button6.TabIndex = 16;
            this.button6.Text = "群组文件";
            this.button6.UseVisualStyleBackColor = false;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // ReceiveBox
            // 
            this.ReceiveBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.ReceiveBox.Location = new System.Drawing.Point(272, 75);
            this.ReceiveBox.Name = "ReceiveBox";
            this.ReceiveBox.Size = new System.Drawing.Size(615, 344);
            this.ReceiveBox.TabIndex = 19;
            this.ReceiveBox.Text = "";
            // 
            // button9
            // 
            this.button9.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.button9.Location = new System.Drawing.Point(794, 12);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(71, 25);
            this.button9.TabIndex = 20;
            this.button9.Text = "退出";
            this.button9.UseVisualStyleBackColor = false;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // FriendListView
            // 
            this.FriendListView.AllowColumnReorder = true;
            this.FriendListView.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.FriendListView.CheckBoxes = true;
            this.FriendListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.testColumnHead});
            listViewItem1.StateImageIndex = 0;
            this.FriendListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.FriendListView.Location = new System.Drawing.Point(340, 106);
            this.FriendListView.Name = "FriendListView";
            this.FriendListView.Size = new System.Drawing.Size(253, 325);
            this.FriendListView.TabIndex = 8;
            this.FriendListView.UseCompatibleStateImageBehavior = false;
            this.FriendListView.SelectedIndexChanged += new System.EventHandler(this.FriendListView_SelectedIndexChanged);
            // 
            // testColumnHead
            // 
            this.testColumnHead.Text = "好友列表";
            this.testColumnHead.Width = 150;
            // 
            // UDPChatButton
            // 
            this.UDPChatButton.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.UDPChatButton.Location = new System.Drawing.Point(340, 406);
            this.UDPChatButton.Name = "UDPChatButton";
            this.UDPChatButton.Size = new System.Drawing.Size(108, 25);
            this.UDPChatButton.TabIndex = 22;
            this.UDPChatButton.Text = "UDP会话";
            this.UDPChatButton.UseVisualStyleBackColor = false;
            this.UDPChatButton.Click += new System.EventHandler(this.UDPChatButton_Click);
            // 
            // TCPChatButton
            // 
            this.TCPChatButton.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.TCPChatButton.Location = new System.Drawing.Point(485, 406);
            this.TCPChatButton.Name = "TCPChatButton";
            this.TCPChatButton.Size = new System.Drawing.Size(108, 25);
            this.TCPChatButton.TabIndex = 23;
            this.TCPChatButton.Text = "TCP会话";
            this.TCPChatButton.UseVisualStyleBackColor = false;
            this.TCPChatButton.Click += new System.EventHandler(this.TCPChatButton_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.button1.Location = new System.Drawing.Point(522, 106);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(71, 25);
            this.button1.TabIndex = 24;
            this.button1.Text = "关闭";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.button2.Location = new System.Drawing.Point(415, 406);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(108, 25);
            this.button2.TabIndex = 25;
            this.button2.Text = "确认删除";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button10
            // 
            this.button10.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.button10.Location = new System.Drawing.Point(794, 43);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(71, 25);
            this.button10.TabIndex = 26;
            this.button10.Text = "语音";
            this.button10.UseVisualStyleBackColor = false;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.ClientSize = new System.Drawing.Size(884, 578);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.TCPChatButton);
            this.Controls.Add(this.UDPChatButton);
            this.Controls.Add(this.FriendListView);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.ReceiveBox);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.FriendsFlow);
            this.Controls.Add(this.SearchBar);
            this.Controls.Add(this.UserNameLabel);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.SendBotton);
            this.Controls.Add(this.SendBox);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.FriendNameLabel);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Main";
            this.Text = "KKChat";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label FriendNameLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.RichTextBox SendBox;
        private System.IO.FileSystemWatcher fileSystemWatcher1;
        private System.Windows.Forms.Label UserNameLabel;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button SendBotton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.FlowLayoutPanel FriendsFlow;
        private System.Windows.Forms.TextBox SearchBar;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.RichTextBox ReceiveBox;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button TCPChatButton;
        private System.Windows.Forms.Button UDPChatButton;
        private System.Windows.Forms.ListView FriendListView;
        private System.Windows.Forms.ColumnHeader testColumnHead;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button10;
    }
}