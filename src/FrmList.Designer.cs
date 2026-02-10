namespace KeyboardNinja
{
    partial class FrmList
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ListBox = new ListBox();
			SuspendLayout();
			// 
			// ListBox
			// 
			ListBox.Dock = DockStyle.Fill;
			ListBox.FormattingEnabled = true;
			ListBox.Location = new Point(0, 0);
			ListBox.Name = "ListBox";
			ListBox.Size = new Size(212, 342);
			ListBox.TabIndex = 0;
			// 
			// FrmList
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			AutoSizeMode = AutoSizeMode.GrowAndShrink;
			ClientSize = new Size(212, 342);
			ControlBox = false;
			Controls.Add(ListBox);
			FormBorderStyle = FormBorderStyle.None;
			Name = "FrmList";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.Manual;
			Text = "Keyboard Ninja";
			TopMost = true;
			Deactivate += FrmList_Deactivate;
			Shown += FrmList_Shown;
			ResumeLayout(false);
		}

		#endregion

		public ListBox ListBox;
	}
}
